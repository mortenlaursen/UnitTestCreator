using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Refactorings.Workflow;

namespace ReSharperPlugin.UnitCreator.CreateTests {
    public class CreateTestsHelper : IWorkflowExec {
        public CreateTestsHelper() {
        }
        public IDeclaration GetTypeDeclaration(IDataContext context) {
            return RefactoringWorkflowUtil.GetTypeDeclaration<ITypeDeclaration, ITypeElement>(context, out bool _);
        }
        public bool CanSuggestProjectFile(IProjectFile projectFile) {
            return projectFile.LanguageType.Is<CSharpProjectFileType>();
        }
        public bool IsLanguageSupported { get { return false; } }
    }
}