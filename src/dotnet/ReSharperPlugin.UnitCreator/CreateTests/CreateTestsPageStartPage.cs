using JetBrains.Annotations;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Feature.Services.UI.Validation;
using JetBrains.Rider.Model.UIAutomation;
using ReSharperPlugin.UnitCreator.Builders;
using ReSharperPlugin.UnitCreator.Extensions;

namespace ReSharperPlugin.UnitCreator.CreateTests
{
    public sealed class CreateTestsPageStartPage : SingleBeRefactoringPage
    {
        [NotNull] private readonly CreateTestsDataModel model;
        [NotNull] private readonly PageBuilder pageBuilder;

        public CreateTestsPageStartPage([NotNull] CreateTestsWorkflow workflow)
            : base(workflow.WorkflowExecuterLifetime)
        {
            model = workflow.Model;
            pageBuilder = new PageBuilder(Lifetime);

            TargetFilePath = this.Property(nameof(model.TargetFilePath), model.TargetFilePath);
            IncludeTestMethod = this.Property(nameof(IncludeTestMethod), model.IncludeTestSetup);
        }

        public IProperty<string> TargetFilePath { get; }
        public IProperty<bool> IncludeTestMethod { get; }

        public override BeControl GetPageContent()
        {
            return pageBuilder.TextBox(TargetFilePath, "N_ame:", x =>
                {
                    var solution = model.SourceFile.GetSolution();

                    x.WithTextNotEmpty(Lifetime, null)
                        .WithAllowedExtensions(model.SourceFile.ExtensionWithDot(), Lifetime, null);
                    // .WithValidationRule(Lifetime, p => new FileAlreadyExistsRule(p, solution, model))
                    // .WithValidationRule(Lifetime, p => new FileCannotBeCreatedRule(p, solution, model));
                    // .WithProjectItemCompletion(solution, Lifetime, model.Declaration?.Language ?? KnownLanguage.ANY, model.SelectionScope.ToList(p => (IProjectModelElement)p), model.SuggestFilter);
                })
                .StartGroupBox()
                .CheckBox(IncludeTestMethod, "Add Xunit, Atc and FluentAssertions")
                .EndGroupBox("Methods")
                .Content();
        }

        public override void Commit()
        {
            model.TargetFilePath = TargetFilePath.Value;
            model.IncludeTestSetup = IncludeTestMethod.Value;
        }

        public override string Title => "Customize your tests-class";

        public override string Description => "Specify options you want to see";
    }
}