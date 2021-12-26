using System;
using System.IO;
using System.Linq;
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
using ReSharperPlugin.UnitCreator.Components;
using ReSharperPlugin.UnitCreator.Extensions;
using ReSharperPlugin.UnitCreator.Services;
using ReSharperPlugin.UnitCreator.Utils;

namespace ReSharperPlugin.UnitCreator.CreateTests
{
    public class CreateTestsWorkflow : DrivenRefactoringWorkflow2<CreateTestsHelper>
    {
        [NotNull] private readonly IPathsService pathsService;

        public CreateTestsWorkflow([NotNull] ISolution solution, [CanBeNull] string actionId = null)
            : base(solution, actionId)
        {
            pathsService = solution.GetComponent<IPathsService>();
        }

        public CreateTestsDataModel Model { get; private set; }

        public override bool Initialize(IDataContext context)
        {
            var (declaration, declaredElement) = IsAvailableCore(context);
            Assertion.Assert(declaration != null, "declaration != null");
            Assertion.Assert(declaredElement != null, "declaredElement != null");
            
            var sourceFile = declaration.GetContainingFile()?.GetSourceFile()?.ToProjectFile();
            Assertion.Assert(sourceFile != null, "sourceFile != null");
            
            var project = context.GetData(ProjectModelDataConstants.PROJECT);
            Assertion.Assert(project != null, "PROJECT != null");
            
            var testProject = Solution.GetProjectByName($"{project.Name}.Tests");
            Assertion.Assert(testProject != null, "defaultTestProject != null");
            
            var projectName = project.Location.Name;
            var projectFileImpl = (ProjectFileImpl)context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);
            // Assertion.Assert(projectFileImpl != null, "projectFileImpl != null");
            
            Model = new CreateTestsDataModel
            {
                Declaration = declaration,
                SourceFile = sourceFile,
                DefaultTargetProject = testProject,
                SourceProject = projectFileImpl,
                IncludeTestSetup = true,
            };
            
            var testProjectTarget = Solution.GetProjectsByName($"{projectName}.Tests");
            // Assertion.Assert(testProjectTarget != null, "testProjectTarget != null");
            
            var targetProjectFullPath = Path.GetDirectoryName(projectFileImpl.Location.FullPath);
            // Assertion.Assert(targetProjectFullPath != null, "targetProjectFullPath != null");
            
            Model.TargetFilePath = Path.Combine(
                targetProjectFullPath.Substring(
                    targetProjectFullPath.LastIndexOf(projectName, StringComparison.Ordinal) + projectName.Length + 1),
                $"{declaredElement.ShortName}Tests.cs");
            
            Model.TargetProject = testProjectTarget.First();
            return true;
        }

        public override bool IsAvailable(IDataContext context)
        {
            var (declaration, _) = IsAvailableCore(context);
            return declaration != null;
        }

        private (IDeclaration, IDeclaredElement) IsAvailableCore([NotNull] IDataContext context)
        {
            var declaredElement =
                context.GetData(RefactoringDataConstants.DeclaredElementWithoutSelection);
            return declaredElement == null
                ? (null, null)
                : (Helper[declaredElement.PresentationLanguage].GetTypeDeclaration(context), declaredElement);
        }

        public override IRefactoringExecuter CreateRefactoring(IRefactoringDriver driver)
        {
            return new CreateTestsRefactoring(this, Solution, driver);
        }

        protected override CreateTestsHelper CreateUnsupportedHelper()
        {
            return new CreateTestsHelper();
        }

        protected override CreateTestsHelper CreateHelper(IRefactoringLanguageService service)
        {
            return new CreateTestsHelper();
        }

        public override IRefactoringPage FirstPendingRefactoringPage => new CreateTestsPageStartPage(this);

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

        private string GenerateFolders(
            IProjectModelEditor transactionCookie,
            ref IProjectFolder projectFolder)
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
            if (Solution.GetComponent<IPsiTransactions>().Execute("Create Tests", () =>
                {
                    var primaryPsiFile = (ICSharpFile)Model.TestClassFile.GetPrimaryPsiFile().NotNull();
                    var className = pathsService.GetExpectedClassName(Model.TargetFilePath);

                    var targetProject = Model.TestClassFile.GetProject().NotNull();
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

                    // transactionCookie.AddProjectReference(targetProject, sourceProject);
                }).Succeded)
                transactionCookie.Commit(NullProgressIndicator.Create());
            else
            {
                transactionCookie.Rollback();
            }
        }

        public override void SuccessfulFinish(IProgressIndicator pi)
        {
            if (Model.TestClassFile != null) NavigationUtil.NavigateTo(Solution, Model.TestClassFile);
            base.SuccessfulFinish(pi);
        }

        public override string Title
        {
            get { return "Create Tests"; }
        }

        public override string HelpKeyword
        {
            get { return "Refactorings__Create_Tests"; }
        }

        public override bool MightModifyManyDocuments
        {
            get { return true; }
        }

        public override RefactoringActionGroup ActionGroup
        {
            get { return RefactoringActionGroup.Blessed; }
        }
    }
}