﻿using System.Collections.Generic;
using EA4S.Teacher;
using System.Linq;
using System;

namespace EA4S.Log {
    /// <summary>
    /// Concrete implementation of log manager to store log data on db.
    /// </summary>
    public class MinigameLogManager : ILogManager {

        #region Runtime variables
        // Minigame
        string session;
        string playSession;
        MiniGameCode miniGameCode;
        List<ILivingLetterAnswerData> logLearnBuffer = new List<ILivingLetterAnswerData>();
        List<LogIntelligence.PlayResultParameters> logPlayBuffer = new List<LogIntelligence.PlayResultParameters>();
        #endregion

        #region Initializations

        public void InitLogSession() {
            session = DateTime.Now.Ticks.ToString();
        }

        public void InitPlaySession() {
            playSession = DateTime.Now.Ticks.ToString();
        }

        /// <summary>
        /// Initializes the single minigame gameplay log session.
        /// </summary>
        public void InitGameplayLogSession(MiniGameCode _minigameCode) {
            miniGameCode = _minigameCode;
        }



        #endregion

        #region API       

        /// <summary>
        /// To be called to any action of player linked to learnig objective and with positive or negative vote.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_isPositiveResult"></param>
        public void OnAnswer(ILivingLetterData _data, bool _isPositiveResult) {
            ILivingLetterAnswerData newILivingLetterAnswerData = new ILivingLetterAnswerData();
            newILivingLetterAnswerData._data = _data;
            newILivingLetterAnswerData._isPositiveResult = _isPositiveResult;
            bufferizeLogLearnData(newILivingLetterAnswerData);
        }

        /// <summary>
        /// Called when minigame is finished.
        /// </summary>
        /// <param name="_valuation">The valuation.</param>
        public void OnGameplaySessionResult(int _valuation) {
            //MinigameResultData newGameplaySessionResultData = new MinigameResultData();
            //newGameplaySessionResultData._valuation = _valuation;
            flushLogLearn();
            flushLogPlay();
            AppManager.Instance.LogManager.LogMinigameScore(miniGameCode, _valuation);
        }

        /// <summary>
        /// Logs the play session score.
        /// </summary>
        /// <param name="_playSessionId">The play session identifier.</param>
        /// <param name="_score">The score.</param>
        public void LogPlaySessionScore(string _playSessionId, float _score) {
            AppManager.Instance.LogManager.LogPlaySessionScore(_playSessionId, _score);
        }

        /// <summary>
        /// Logs the learning block score.
        /// </summary>
        /// <param name="_learningBlock">The learning block.</param>
        /// <param name="_score">The score.</param>
        public void LogLearningBlockScore(int _learningBlock, float _score) {
            AppManager.Instance.LogManager.LogLearningBlockScore(_learningBlock, _score);
        }

        /// <summary>
        /// Called when player perform a [gameplay skill action] action during gameplay. .
        /// </summary>
        /// <param name="_ability">The ability.</param>
        /// <param name="_score">The score.</param>
        public void OnGameplaySkillAction(PlaySkill _ability, float _score) {
            bufferizeLogPlayData(new LogIntelligence.PlayResultParameters() {
                playEvent = PlayEvent.Skill,
                skill = _ability,
                score = _score,
            });
        }

        /// <summary>
        /// Log a generic info data.
        /// </summary>
        /// <param name="_event">The event.</param>
        /// <param name="_data">The data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void LogInfoData(InfoEvent _event, string _data = "") {
            AppManager.Instance.LogManager.LogInfo(session, _event, _data);
            
        }

        public void LogMood(int _mood) {
            AppManager.Instance.LogManager.LogMood(_mood);
        }

        #endregion

        #region Gameplay        
        /// <summary>
            /// Bufferizes the log play data.
            /// </summary>
            /// <param name="_playResultParameters">The play result parameters.</param>
        void bufferizeLogPlayData(LogIntelligence.PlayResultParameters _playResultParameters) {
            logPlayBuffer.Add(_playResultParameters);
        }
        /// <summary>
        /// Flushes the log play to app teacher log intellingence.
        /// </summary>
        void flushLogPlay() {
            AppManager.Instance.LogManager.LogPlay(session, playSession, miniGameCode, logPlayBuffer);
        }
        #endregion

        #region Learn        
        /// <summary>
        /// Bufferizes the log learn data.
        /// </summary>
        /// <param name="_iLivingLetterAnswerData">The i living letter answer data.</param>
        void bufferizeLogLearnData(ILivingLetterAnswerData _iLivingLetterAnswerData) {
            logLearnBuffer.Add(_iLivingLetterAnswerData);
        }

        /// <summary>
        /// Flushes the log learn data to app teacher log intellingence.
        /// </summary>
        void flushLogLearn() {
            List<LogIntelligence.LearnResultParameters> resultsList = new List<LogIntelligence.LearnResultParameters>();
            ILivingLetterData actualData = null;
            LogIntelligence.LearnResultParameters actualLearnResult = new LogIntelligence.LearnResultParameters();

            foreach (var l in logLearnBuffer) {
                if (actualData != l._data) {
                    // Is a different learn objective 
                    actualData = l._data;
                    if (actualData != null) {
                        // save actualLearnResult to data log list to send, if exist...
                        resultsList.Add(actualLearnResult);
                        // ...and reset actualLearnResult for new learn objective with new properties
                        actualLearnResult = new LogIntelligence.LearnResultParameters();
                        switch (l._data.DataType) {
                            case LivingLetterDataType.Letter:
                                actualLearnResult.table = Db.DbTables.Letters;
                                break;
                            case LivingLetterDataType.Word:
                                actualLearnResult.table = Db.DbTables.Words;
                                break;
                            case LivingLetterDataType.Image:
                                actualLearnResult.table = Db.DbTables.Words;
                                break;
                            default:
                                // data type not found. Make soft exception.
                                break;
                        }
                        actualLearnResult.elementId = l._data.Key;
                    }
                }
                // update learn objective log...
                if (l._isPositiveResult)
                    actualLearnResult.nCorrect ++;
                else
                    actualLearnResult.nWrong ++;
            }

            AppManager.Instance.LogManager.LogLearn(session, playSession, miniGameCode, resultsList);

        }
        #endregion

        #region Journey Scores



        #endregion

        #region Mood
        // direct into API
        #endregion

        #region internal data structures and interfaces
        public interface iBufferizableLog {
            string CachedType { get; }
        }

        public struct ILivingLetterAnswerData : iBufferizableLog {
            public string CachedType { get { return "ILivingLetterAnswerData"; } }
            public ILivingLetterData _data;
            public bool _isPositiveResult;
        }
        #endregion
    }
}