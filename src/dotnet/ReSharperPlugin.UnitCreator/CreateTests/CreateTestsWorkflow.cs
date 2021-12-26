using System;
using System.IO;
using JetBrains.Annotations;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Diagnostics;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Transactions;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using ReSharperPlugin.UnitCreator.Builders;
using ReSharperPlugin.UnitCreator.Extensions;
using ReSharperPlugin.UnitCreator.Services;

namespace ReSharperPlugin.UnitCreator.CreateTests
{
    public class CreateTestsWorkflow : DrivenRefactoringWorkflow2<CreateTestsHelper>
    {
        [NotNull] private readonly IPathsService _pathsService;
        public CreateTestsDataModel Model { get; private set; }

        public CreateTestsWorkflow([NotNull] ISolution solution, [CanBeNull] string actionId = null)
            : base(solution, actionId)
        {
            _pathsService = solution.GetComponent<IPathsService>();
        }

        public override bool Initialize(IDataContext context)
        {
            var declaration = IsAvailableCore(context);
            Assertion.Assert(declaration != null, "declaration != null");

            var sourceFile = declaration.GetContainingFile()?.GetSourceFile()?.ToProjectFile();
            Assertion.Assert(sourceFile != null, "sourceFile != null");

            var sourceProject = context.GetData(ProjectModelDataConstants.PROJECT);
            Assertion.Assert(sourceProject != null, "sourceProject != null");
            var sourceProjectName = sourceProject.Name;
            var targetProject = Solution.GetProjectByName($"{sourceProjectName}.Tests");
            Assertion.Assert(targetProject != null, "targetProject != null");
            var sourceFileLocation = Path.GetDirectoryName(sourceFile.Location.FullPath);
            Assertion.Assert(sourceFileLocation != null, "sourceFileLocation != null");

            var localLocation = GetLocationInNamespace(sourceFileLocation, sourceProjectName);

            Model = new CreateTestsDataModel
            {
                Declaration = declaration,
                SourceFile = sourceFile,
                SourceProject = sourceProject,
                TargetProject = targetProject,
                TargetFilePath = Path.Combine(localLocation, $"{declaration.DeclaredName}Tests.cs"),
                IncludeTestSetup = true,
            };

            return true;
        }

        public override bool PreExecute(IProgressIndicator pi)
        {
            using (var transactionCookie = Solution.CreateTransactionCookie(DefaultAction.Commit, "Create Tests",
                       NullProgressIndicator.Create()))
            {
                try
                {
                    IProjectFolder projectFolder = Model.TargetProject;

                    var fileName = GenerateFolders(transactionCookie, ref projectFolder);

                    var path = projectFolder.Location.Combine(fileName);
                    Model.TestClassFile = transactionCookie.AddFile(projectFolder, path);

                    GenerateCodeAndFile(transactionCookie);
                }
                catch
                {
                    transactionCookie.Rollback();
                    throw;
                }
            }

            return true;
        }

        private string GenerateFolders(IProjectModelEditor transactionCookie, ref IProjectFolder projectFolder)
        {
            var pathParts = Model.TargetFilePath.TrimStart('\\', '/').Split('\\', '/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var n = 0; n < pathParts.Length - 1; n++)
            {
                try
                {
                    projectFolder = transactionCookie.AddFolder(projectFolder, pathParts[n]);
                }
                catch (Exception)
                {
                    projectFolder = projectFolder.GetSubFolders(pathParts[n]).First();
                }
            }

            return fileName;
        }

        private void GenerateCodeAndFile(IProjectModelTransactionCookie transactionCookie)
        {
            if (ExecuteTransaction(transactionCookie).Succeded)
                transactionCookie.Commit(NullProgressIndicator.Create());
            else
            {
                transactionCookie.Rollback();
            }
        }

        private TransactionResult ExecuteTransaction(IProjectModelTransactionCookie transactionCookie)
        {
            return Solution.GetComponent<IPsiTransactions>().Execute("Create Tests", () =>
            {
                var primaryPsiFile = (ICSharpFile)Model.TestClassFile.GetPrimaryPsiFile().NotNull();
                var className = _pathsService.GetExpectedClassName(Model.TargetFilePath);

                var targetProject = Model.TargetProject.GetProject().NotNull();
                var sourceProject = Model.SourceFile.GetProject().NotNull();

                var psiFileBuilder = new PsiFileBuilder(primaryPsiFile);

                if (Model.IncludeTestSetup)
                {
                    psiFileBuilder
                        .AddUsingDirective("Atc.Test")
                        .AddUsingDirective("FluentAssertions")
                        .AddUsingDirective("Xunit");
                }

                psiFileBuilder
                    .AddExpectedNamespace()
                    .AddClass(className, AccessRights.PUBLIC)
                    .WithMembers();

                transactionCookie.AddProjectReference(targetProject, sourceProject);
            });
        }

        public override bool IsAvailable(IDataContext context)
        {
            var declaration = IsAvailableCore(context);
            return declaration != null;
        }
        
        public override IRefactoringExecuter CreateRefactoring(IRefactoringDriver driver) =>
            new CreateTestsRefactoring(this, Solution, driver);
        
        public override IRefactoringPage FirstPendingRefactoringPage => new CreateTestsPageStartPage(this);

        public override string Title => "Create Tests";

        public override string HelpKeyword => "Refactorings__Create_Tests";

        public override bool MightModifyManyDocuments => true;

        public override RefactoringActionGroup ActionGroup => RefactoringActionGroup.Blessed;
        
        protected override CreateTestsHelper CreateUnsupportedHelper() => new CreateTestsHelper();

        protected override CreateTestsHelper CreateHelper(IRefactoringLanguageService service) =>
            new CreateTestsHelper();
        
        private static string GetLocationInNamespace(string sourceFileLocation, string sourceProjectName) =>
            sourceFileLocation
                .Substring(sourceFileLocation.LastIndexOf(sourceProjectName, StringComparison.Ordinal) +
                           sourceProjectName.Length + 1);

        private IClassDeclaration IsAvailableCore([NotNull] IDataContext context)
        {
            var declaredElement =
                context.GetData(RefactoringDataConstants.DeclaredElementWithoutSelection);
            return declaredElement == null
                ? null
                : (IClassDeclaration)Helper[declaredElement.PresentationLanguage].GetTypeDeclaration(context);
        }
    }
}