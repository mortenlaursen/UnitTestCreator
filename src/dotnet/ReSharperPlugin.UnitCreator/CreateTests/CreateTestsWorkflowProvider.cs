using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Feature.Services.Refactorings;

namespace ReSharperPlugin.UnitCreator.CreateTests {
    [RefactoringWorkflowProvider]
    public class CreateTestsWorkflowProvider : IRefactoringWorkflowProvider {
        public IEnumerable<IRefactoringWorkflow> CreateWorkflow(IDataContext dataContext) {
            var solution = dataContext.GetData(ProjectModelDataConstants.SOLUTION);

            return new IRefactoringWorkflow[] {
                new CreateTestsWorkflow(solution, "Create Tests")
            };
        }
    }
}