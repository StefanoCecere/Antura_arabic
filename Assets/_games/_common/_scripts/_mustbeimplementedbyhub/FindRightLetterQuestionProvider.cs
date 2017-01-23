﻿using System.Collections.Generic;

namespace EA4S.MinigamesAPI
{

    /// <summary>
    /// Default IQuestionProvider that find the right letter question.
    /// </summary>
    /// <seealso cref="IQuestionProvider" />
    // refactor: this is used in all minigames as the core application reasons only in terms of question packs
    // refactor: this should be renamed to SequentialQuestionProvider, as it just moves to the next question
    public class FindRightLetterQuestionProvider : IQuestionProvider {

        #region properties
        List<IQuestionPack> questions = new List<IQuestionPack>();
        int currentQuestion;
        #endregion

        public FindRightLetterQuestionProvider(List<IQuestionPack> _questionsPack) {
            currentQuestion = 0;

            questions.AddRange(_questionsPack);
        }

        /// <summary>
        /// Provide me another question.
        /// </summary>
        /// <returns></returns>
        IQuestionPack IQuestionProvider.GetNextQuestion() {
            currentQuestion++;

            if (currentQuestion >= questions.Count)
                currentQuestion = 0;

            return questions[currentQuestion];
        }

    }
}