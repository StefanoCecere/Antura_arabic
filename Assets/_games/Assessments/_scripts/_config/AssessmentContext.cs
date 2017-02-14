using EA4S.Database;
using EA4S.MinigamesCommon;

namespace EA4S.Assessment
{
    /// <summary>
    /// Used to pass managers around. Try to avoid passing it around apart
    /// using it in composition roots.
    /// </summary>
    public class AssessmentContext
    {
        public AssessmentConfiguration Configuration;
        public AssessmentEvents Events;
        public IGameContext Utils;
        public ICheckmarkWidget CheckmarkWidget;
        public IQuestionGenerator QuestionGenerator;
        public IDragManager DragManager;
        public ILogicInjector LogicInjector;
        public IQuestionPlacer QuestionPlacer;
        public IAnswerPlacer AnswerPlacer;
        public AnswerChecker AnswerChecker;
        public AssessmentAudioManager AudioManager;
        public LocalizationDataId GameDescription;
        public AssessmentGame Game;
    }
}