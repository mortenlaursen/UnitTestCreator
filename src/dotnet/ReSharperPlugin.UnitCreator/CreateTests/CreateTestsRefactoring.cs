using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Refactorings.Workflow;

namespace ReSharperPlugin.UnitCreator.CreateTests {
    public sealed class CreateTestsRefactoring : DrivenRefactoring<CreateTestsWorkflow, RefactoringExecBase<CreateTestsWorkflow, CreateTestsRefactoring>> {
        public CreateTestsRefactoring(
            [NotNull] CreateTestsWorkflow workflow,
            [NotNull] ISolution solution,
            [NotNull] IRefactoringDriver driver)
            : base(workflow, solution, driver) {
        }
        public override bool Execute(IProgressIndicator pi) {
            return true;
        }
    }
}