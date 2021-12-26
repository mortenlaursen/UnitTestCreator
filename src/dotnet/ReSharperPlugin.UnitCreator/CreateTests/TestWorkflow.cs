using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Refactorings;

namespace ReSharperPlugin.UnitCreator.CreateTests
{
    public class TestWorkflow : DrivenRefactoringWorkflow2<IWorkflowExec>
    {
        public TestWorkflow([NotNull] ISolution solution, string actionId) : base(solution, actionId)
        {
        }

        public override bool Initialize(IDataContext context)
        {
            throw new System.NotImplementedException();
        }

        public override string HelpKeyword { get; }
        public override IRefactoringPage FirstPendingRefactoringPage { get; }
        public override bool MightModifyManyDocuments { get; }
        public override string Title { get; }
        public override RefactoringActionGroup ActionGroup { get; }

        public override bool IsAvailable(IDataContext context)
        {
            throw new System.NotImplementedException();
        }

        public override IRefactoringExecuter CreateRefactoring(IRefactoringDriver driver)
        {
            throw new System.NotImplementedException();
        }

        protected override IWorkflowExec CreateUnsupportedHelper()
        {
            throw new System.NotImplementedException();
        }

        protected override IWorkflowExec CreateHelper(IRefactoringLanguageService service)
        {
            throw new System.NotImplementedException();
        }
    }
}