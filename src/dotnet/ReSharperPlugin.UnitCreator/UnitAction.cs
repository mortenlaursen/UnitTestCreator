using System;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.Application.UI.Actions.ActionManager;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.CSharp.ContextActions;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using ReSharperPlugin.UnitCreator.CreateTests;
using ReSharperPlugin.UnitCreator.Utils;

namespace ReSharperPlugin.UnitCreator
{
    [ContextAction(Name = "Create Tests", Description = "Creates a test fixture for a class", Group = "C#")]
    public sealed class UnitAction : ContextActionBase
    {
        [NotNull] private readonly ICSharpContextActionDataProvider dataProvider;

        public UnitAction([NotNull] ICSharpContextActionDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public override string Text => "Create unit test";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            var declaration = ContextActionUtil.GetClassLikeDeclaration(dataProvider);
            return declaration is IClassDeclaration;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            return _ =>
            {
                using (var lifetimeDefinition = Lifetime.Define(Lifetime.Eternal))
                {
                    RefactoringActionUtil.ExecuteRefactoring(
                        solution
                            .GetComponent<IActionManager>().DataContexts
                            .CreateOnActiveControl(lifetimeDefinition.Lifetime), 
                        new CreateTestsWorkflow(solution));
                }
            };
        }
    }
}