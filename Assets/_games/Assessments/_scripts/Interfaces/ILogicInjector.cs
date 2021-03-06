using System.Collections;

namespace EA4S.Assessment
{
    /// <summary>
    /// This class adds game-specific logic to LivingLetters Objects
    /// </summary>
    public interface ILogicInjector
    {
        void Wire( IQuestion question, Answer[] answers);
        void EnableGamePlay();
        void CompleteWiring();
        void ResetRound();
        bool AllAnswersCorrect();
        void EnableDragOnly();
        void RemoveDraggables();
        void AnswersAdded();
        IEnumerator AllAnsweredEvent();
    }
}
