using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.UnitCreator.CreateTests
{
    public sealed class CreateTestsDataModel
    {
        public CreateTestsDataModel()
        {
        }

        public string TargetFilePath { get; set; }
        public IProjectFile TestClassFile { get; set; }

        public IDeclaration Declaration { get; set; }
        public IProjectFile SourceFile { get; set; }
        public IProjectFolder DefaultTargetProject { get; set; }
        public IProject TargetProject { get; set; }
        public bool IncludeTestSetup { get; set; }
        public ProjectFileImpl SourceProject { get; set; }

        // public IList<IProjectFolder> SelectionScope { get; set; }
        // public Func<IProjectFile, bool> SuggestFilter{ get; set; }

        [NotNull]
        public ISolution GetSolution()
        {
            return SourceFile.GetSolution();
        }
    }
}