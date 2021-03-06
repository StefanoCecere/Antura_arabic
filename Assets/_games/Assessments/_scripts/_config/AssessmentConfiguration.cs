using EA4S.MinigamesAPI;
using EA4S.MinigamesCommon;
using EA4S.Teacher;
using System;
using UnityEngine;

namespace EA4S.Assessment
{
    public class AssessmentConfiguration : IAssessmentConfiguration
    {
        /// <summary>
        /// Externally provided Context: Inject all subsystems needed by this minigame
        /// </summary>
        public IGameContext Context { get; set; }

        /// <summary>
        /// Configured externally: which assessment we need to start.
        /// </summary>
        public AssessmentCode assessmentType = AssessmentCode.Unsetted;

        /// <summary>
        /// Externally provided Question provider
        /// </summary>
        private IQuestionProvider questionProvider;
        public IQuestionProvider Questions
        {
            get
            {
                return GetQuestionProvider();
            }
            set
            {
                questionProvider = value;
            }
        }

        private IQuestionProvider GetQuestionProvider()
        {
            return questionProvider;
        }

        /// <summary>
        /// Setted externally: assessments will scale quantity of content (number of questions
        /// and answers mostly) linearly with this value. It is assumed that Difficulty
        /// start with 0 and increase up to 1 as long as the Child progress in the world.
        /// The difficulty should be different for each assessmentType.
        /// </summary>
        public float Difficulty { get; set; }

        public bool TutorialEnabled { get; set; }


        /// <summary>
        /// How many questions showed simultaneously on the screen.
        /// </summary>
        public int SimultaneosQuestions { get; private set; }

        /// <summary>
        /// How many answers should each question have. In Categorize assessments
        /// (The ones where the child should put something in the right category,
        /// like Sun/Moon) this is used to show maximum number of answers even when
        /// each question has a different number of answers (there could be 2 words
        /// to be putted in Moon, and 3 in Sun, in this case 3 placeholders are
        /// showed anyway).
        /// </summary>
        public int Answers { get; private set; } // number of answers in category questions

        /// <summary>
        /// Number of rounds, mostly fixed for each game, this value is provided externally
        /// </summary>
        public int NumberOfRounds { get { return _rounds; }  set { _rounds = value; } }
        private int _rounds = 0;

        /////////////////
        // Singleton Pattern
        static AssessmentConfiguration instance;
        public static AssessmentConfiguration Instance
        {
            get
            {
                if (instance == null)
                    instance = new AssessmentConfiguration();
                return instance;
            }
        }
        /////////////////

        /// <summary>
        /// This is called by MiniGameAPI to create QuestionProvider, that means that if I start game
        /// from debug scene, I need a custom test Provider.
        /// </summary>
        /// <returns>Custom question data for the assessment</returns>
        public IQuestionBuilder SetupBuilder()
        {
            switch (assessmentType)
            {
                case AssessmentCode.LetterForm:
                    return Setup_LetterForm_Builder();

                case AssessmentCode.MatchLettersToWord:
                    return Setup_MatchLettersToWord_Builder();

                case AssessmentCode.WordsWithLetter:
                    return Setup_WordsWithLetter_Builder();

                case AssessmentCode.SunMoonWord:
                    return Setup_SunMoonWords_Builder();

                case AssessmentCode.SunMoonLetter:
                    return Setup_SunMoonLetter_Builder();

                case AssessmentCode.QuestionAndReply:
                    return Setup_QuestionAnReply_Builder();

                case AssessmentCode.SelectPronouncedWord:
                    return Setup_SelectPronuncedWord_Builder();

                case AssessmentCode.SingularDualPlural:
                    return Setup_SingularDualPlural_Builder();

                case AssessmentCode.WordArticle:
                    return Setup_WordArticle_Builder();

                case AssessmentCode.MatchWordToImage:
                    return Setup_MatchWordToImage_Builder();

                case AssessmentCode.CompleteWord:
                    return Setup_CompleteWord_Builder();

                case AssessmentCode.OrderLettersOfWord:
                    return Setup_OrderLettersOfWord_Builder();

                case AssessmentCode.CompleteWord_Form:
                    return Setup_CompleteWord_Form_Builder();

                case AssessmentCode.MatchLettersToWord_Form:
                    return Setup_MatchLettersToWord_Form_Builder();

                default:
                    throw new NotImplementedException( "NotImplemented Yet!");
            }
        }

        private IQuestionBuilder Setup_CompleteWord_Form_Builder()
        {
            SimultaneosQuestions = 2;
            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.sortPacksByDifficulty = false;

            return new LetterFormsInWordsQuestionBuilder(
                nPacksPerRound: SimultaneosQuestions,
                nRounds: NumberOfRounds,
                forceUnseparatedLetters: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_MatchLettersToWord_Form_Builder()
        {
            SimultaneosQuestions = 2;
            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.sortPacksByDifficulty = false;

            return new LetterFormsInWordsQuestionBuilder(
                nPacksPerRound: SimultaneosQuestions,
                nRounds: NumberOfRounds,
                forceUnseparatedLetters: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_OrderLettersOfWord_Builder()
        {
            SimultaneosQuestions = 1;

            var builderParams = new QuestionBuilderParameters();
            builderParams.wordFilters.requireDrawings = true;
            builderParams.sortPacksByDifficulty = false;

            // Maximum number of letters depends on the screen.
            float screenRatio = Screen.width / Screen.height;
            int maxLetters = 7;

            if (screenRatio > 1.4999f)
                maxLetters = 8;

            if (screenRatio > 1.7777f)
                maxLetters = 9;

            return new LettersInWordQuestionBuilder(
                NumberOfRounds,
                nCorrect:2,
                useAllCorrectLetters: true,
                parameters: builderParams,
                maximumWordLength: maxLetters
                );
        }

        private IQuestionBuilder Setup_CompleteWord_Builder()
        {
            SimultaneosQuestions = 1;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = true;
            builderParams.wordFilters.requireDrawings = true;
            builderParams.sortPacksByDifficulty = false;

            return new LettersInWordQuestionBuilder(

                SimultaneosQuestions * NumberOfRounds,  // Total Answers
                nCorrect:1,            // Always one!
                nWrong:4,            // WrongAnswers
                useAllCorrectLetters: false,
                forceUnseparatedLetters: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_MatchWordToImage_Builder()
        {
            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = false;
            builderParams.wordFilters.requireDrawings = true;
            builderParams.sortPacksByDifficulty = false;
            SimultaneosQuestions = 1;

            int nCorrect = 1;
            int nWrong = 3;

            return new RandomWordsQuestionBuilder(
                SimultaneosQuestions * NumberOfRounds,
                nCorrect,
                nWrong,
                firstCorrectIsQuestion: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_WordArticle_Builder()
        {
            SimultaneosQuestions = 2;

            Answers = 2;
            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wordFilters.excludeArticles = false;
            builderParams.sortPacksByDifficulty = false;

            return new WordsByArticleQuestionBuilder(
                Answers * NumberOfRounds * 3,
                builderParams);
        }

        private IQuestionBuilder Setup_SingularDualPlural_Builder()
        {
            SimultaneosQuestions = 3;
            Answers = 2;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wordFilters.excludePluralDual = false;
            builderParams.sortPacksByDifficulty = false;

            return new WordsByFormQuestionBuilder(
                SimultaneosQuestions* NumberOfRounds * 4,
                builderParams);
        }

        private IQuestionBuilder Setup_SelectPronuncedWord_Builder()
        {
            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = false;
            builderParams.sortPacksByDifficulty = false;

            SimultaneosQuestions = 1;
            int nCorrect = 1;
            int nWrong = 3;
            return new RandomWordsQuestionBuilder(
                SimultaneosQuestions* NumberOfRounds,
                nCorrect,
                nWrong,
                firstCorrectIsQuestion: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_QuestionAnReply_Builder()
        {
            var builderParams = new QuestionBuilderParameters();
            builderParams.sortPacksByDifficulty = false;

            SimultaneosQuestions = 1;
            int nWrongs = 4;

            return new  PhraseQuestionsQuestionBuilder(
                        SimultaneosQuestions * NumberOfRounds, // totale questions
                        nWrongs,     // wrong additional answers
                parameters:builderParams);
        }

        private IQuestionBuilder Setup_SunMoonLetter_Builder()
        {
            SimultaneosQuestions = 2;
            Answers = 2;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.sortPacksByDifficulty = false;

            return new LettersBySunMoonQuestionBuilder( 
                        SimultaneosQuestions * NumberOfRounds * 2,
                        builderParams
            );
        }

        private IQuestionBuilder Setup_WordsWithLetter_Builder()
        {
            // This assessment changes behaviour based on the current stage
            var jp = AppManager.I.Player.CurrentJourneyPosition;
            switch (jp.Stage)
            {
                case 1:
                    SimultaneosQuestions = 1;
                    break;
                case 2:
                case 3:
                    SimultaneosQuestions = 2;
                    break;
                case 4:
                case 5:
                case 6:
                default:
                    SimultaneosQuestions = 3;
                    break;
            }
            int nWrong = 6 - SimultaneosQuestions;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = false;
            builderParams.sortPacksByDifficulty = false;

            return new WordsWithLetterQuestionBuilder(
                NumberOfRounds,
                SimultaneosQuestions,
                1,                  // Correct Answers
                nWrong,             // Wrong Answers
                parameters: builderParams
                );     

        }

        private IQuestionBuilder Setup_SunMoonWords_Builder()
        {
            var builderParams = new QuestionBuilderParameters();
            builderParams.sortPacksByDifficulty = false;

            SimultaneosQuestions = 2;
            Answers = 2;

            return new WordsBySunMoonQuestionBuilder( SimultaneosQuestions * NumberOfRounds * 2, parameters: builderParams);
        }

        private IQuestionBuilder Setup_MatchLettersToWord_Builder()
        {
            // This assessment changes behaviour based on the current stage
            var jp = AppManager.I.Player.CurrentJourneyPosition;
            switch (jp.Stage)
            {
                case 1:
                    SimultaneosQuestions = 1;
                    break;
                case 2:
                case 3:
                    SimultaneosQuestions = 2;
                    break;
                case 4:
                case 5:
                case 6:
                default:
                    SimultaneosQuestions = 3;
                    break;
            }
            int nWrong = 6 - SimultaneosQuestions;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = false;
            builderParams.sortPacksByDifficulty = false;

            return new LettersInWordQuestionBuilder(
                NumberOfRounds,
                SimultaneosQuestions,
                nCorrect:1,   
                nWrong:nWrong,
                useAllCorrectLetters: false,
                forceUnseparatedLetters: true,
                parameters: builderParams);
        }

        private IQuestionBuilder Setup_LetterForm_Builder()
        {
            SimultaneosQuestions = 1;

            var builderParams = new QuestionBuilderParameters();
            builderParams.correctChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongChoicesHistory = PackListHistory.RepeatWhenFull;
            builderParams.wrongSeverity = SelectionSeverity.MayRepeatIfNotEnough;
            builderParams.useJourneyForWrong = false;
            builderParams.sortPacksByDifficulty = false;

            return new RandomLettersQuestionBuilder(
                SimultaneosQuestions * NumberOfRounds,  // Total Answers
                1,                              // CorrectAnswers
                4,                              // WrongAnswers
                firstCorrectIsQuestion:true,
                parameters:builderParams);
        }

        public MiniGameLearnRules SetupLearnRules()
        {
            switch (assessmentType)
            {
                case AssessmentCode.LetterForm:
                    return Setup_LetterForm_LearnRules();

                case AssessmentCode.MatchLettersToWord:
                    return Setup_MatchLettersToWord_LearnRules();

                case AssessmentCode.WordsWithLetter:
                    return Setup_WordsWithLetter_LearnRules();

                case AssessmentCode.SunMoonWord:
                    return Setup_SunMoonWords_LearnRules();

                case AssessmentCode.SunMoonLetter:
                    return Setup_SunMoonLetter_LearnRules();

                case AssessmentCode.QuestionAndReply:
                    return Setup_QuestionAnReply_LearnRules();

                case AssessmentCode.SelectPronouncedWord:
                    return Setup_SelectPronuncedWord_LearnRules();

                case AssessmentCode.SingularDualPlural:
                    return Setup_SingularDualPlural_LearnRules();

                case AssessmentCode.WordArticle:
                    return Setup_WordArticle_LearnRules();

                case AssessmentCode.MatchWordToImage:
                    return Setup_MatchWordToImage_LearnRules();

                case AssessmentCode.CompleteWord:
                    return Setup_CompleteWord_LearnRules();

                case AssessmentCode.OrderLettersOfWord:
                    return Setup_OrderLettersOfWord_LearnRules();

                default:
                    throw new NotImplementedException( "NotImplemented Yet!");
            }
        }

        private MiniGameLearnRules Setup_OrderLettersOfWord_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_CompleteWord_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_MatchWordToImage_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_WordArticle_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_SingularDualPlural_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_SelectPronuncedWord_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_QuestionAnReply_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_SunMoonLetter_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_SunMoonWords_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_WordsWithLetter_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_MatchLettersToWord_LearnRules()
        {
            return new MiniGameLearnRules();
        }

        private MiniGameLearnRules Setup_LetterForm_LearnRules()
        {
            return new MiniGameLearnRules();
        }

    }
}
