using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EA4S.Database;

namespace EA4S.Core
{
    public static class MiniGamesUtilities
    {

        public static List<MainMiniGame> GetMainMiniGameList()
        {
            Dictionary<string, MainMiniGame> dictionary = new Dictionary<string, MainMiniGame>();
            List<MiniGameInfo> minigameInfoList = AppManager.I.ScoreHelper.GetAllMiniGameInfo();
            foreach (var minigameInfo in minigameInfoList)
            {
                if (!dictionary.ContainsKey(minigameInfo.data.Main))
                {
                    dictionary[minigameInfo.data.Main] = new MainMiniGame
                    {
                        id = minigameInfo.data.Main,
                        variations = new List<MiniGameInfo>()
                    };
                }
                dictionary[minigameInfo.data.Main].variations.Add(minigameInfo);
            }

            List<MainMiniGame> outputList = new List<MainMiniGame>();
            foreach (var k in dictionary.Keys)
            {
                if (dictionary[k].id != "Assessment")
                {
                    outputList.Add(dictionary[k]);

                }
            }

            // Sort minigames and variations based on their minimum journey position
            Dictionary<MiniGameCode, JourneyPosition> minimumJourneyPositions =
                new Dictionary<MiniGameCode, JourneyPosition>();
            foreach (var mainMiniGame in outputList)
            {
                foreach (var miniGameInfo in mainMiniGame.variations)
                {
                    var miniGameCode = miniGameInfo.data.Code;
                    minimumJourneyPositions[miniGameCode] =
                        AppManager.I.JourneyHelper.GetMinimumJourneyPositionForMiniGame(miniGameCode);
                }
            }

            // First sort variations (so the first variation is in front)
            foreach (var mainMiniGame in outputList)
            {
                mainMiniGame.variations.Sort((g1, g2) => minimumJourneyPositions[g1.data.Code].IsMinor(
                    minimumJourneyPositions[g2.data.Code])
                    ? -1
                    : 1);
            }

            // Then sort minigames by their first variation
            outputList.Sort((g1, g2) => SortMiniGames(minimumJourneyPositions, g1, g2));

            return outputList;
        }

        public static int SortMiniGames(Dictionary<MiniGameCode, JourneyPosition> minimumJourneyPositions,
            MainMiniGame g1, MainMiniGame g2)
        {
            // MiniGames are sorted based on minimum play session
            var minPos1 = minimumJourneyPositions[g1.GetFirstVariationMiniGameCode()];
            var minPos2 = minimumJourneyPositions[g2.GetFirstVariationMiniGameCode()];

            if (minPos1.IsMinor(minPos2)) return -1;
            if (minPos2.IsMinor(minPos1)) return 1;

            // Check play session order
            var sharedPlaySessionData = AppManager.I.DB.GetPlaySessionDataById(minPos1.ToStringId());
            int ret = 0;
            switch (sharedPlaySessionData.Order)
            {
                case PlaySessionDataOrder.Random:
                    // No specific sorting
                    ret = 0;
                    break;
                    ;
                case PlaySessionDataOrder.Sequence:
                    // In case of a Sequence PS, two minigames with the same minimum play session are sorted based on the sequence order
                    var miniGameInPlaySession1 =
                        sharedPlaySessionData.Minigames.ToList()
                            .Find(x => x.MiniGameCode == g1.GetFirstVariationMiniGameCode());
                    var miniGameInPlaySession2 =
                        sharedPlaySessionData.Minigames.ToList()
                            .Find(x => x.MiniGameCode == g2.GetFirstVariationMiniGameCode());
                    ret = miniGameInPlaySession1.Weight - miniGameInPlaySession2.Weight;
                    break;
            }
            return ret;
        }

        public static Dictionary<MiniGameCode, float[]> GetMiniGameDifficultiesForTesting()
        {
            var difficultiesForTesting = new Dictionary<MiniGameCode, float[]>();
            difficultiesForTesting.Add(MiniGameCode.Assessment_CompleteWord, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_CompleteWord_Form, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_LetterForm, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_MatchLettersToWord, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_MatchLettersToWord_Form, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_OrderLettersOfWord, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_MatchWordToImage, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_QuestionAndReply, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_SelectPronouncedWord, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_SunMoonLetter, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_SingularDualPlural, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_VowelOrConsonant, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_SunMoonWord, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_WordArticle, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Assessment_WordsWithLetter, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.AlphabetSong_alphabet, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.AlphabetSong_letters, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.ReadingGame, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.Balloons_counting, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Balloons_letter, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Balloons_spelling, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Balloons_words, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.ColorTickle, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.DancingDots, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.Egg_letters, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Egg_sequence, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.HideSeek, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.FastCrowd_alphabet, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.FastCrowd_counting, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.FastCrowd_letter, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.FastCrowd_spelling, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.FastCrowd_words, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.Scanner, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Scanner_phrase, new[] { 0.0f });

            difficultiesForTesting.Add(MiniGameCode.MixedLetters_alphabet, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.MixedLetters_spelling, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.MakeFriends, new[] {0.0f, 1.0f});

            difficultiesForTesting.Add(MiniGameCode.Maze, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.MissingLetter, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.MissingLetter_forms, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.MissingLetter_phrases, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.Tobogan_letters, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.Tobogan_words, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.TakeMeHome, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.SickLetters, new[] {0.0f});

            difficultiesForTesting.Add(MiniGameCode.ThrowBalls_letterinword, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.ThrowBalls_letters, new[] {0.0f});
            difficultiesForTesting.Add(MiniGameCode.ThrowBalls_words, new[] {0.0f});

            return difficultiesForTesting;
        }
    }
}