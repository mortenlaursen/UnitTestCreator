using JetBrains.Annotations;
using JetBrains.Application.UI.PopupLayout;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Resources.Shell;

namespace ReSharperPlugin.UnitCreator.Utils {
    public static class NavigationUtil {
        public static void NavigateTo([NotNull] ISolution solution, [NotNull] IProjectFile projectFile) {
            Guard.IsNotNull(solution, nameof(solution));
            Guard.IsNotNull(projectFile, nameof(projectFile));

            NavigationOptions options = NavigationOptions.FromWindowContext(Shell.Instance.GetComponent<IMainWindowPopupWindowContext>().Source, "");
            NavigationManager.GetInstance(solution).Navigate(new ProjectFileNavigationPoint(projectFile), options);
        }
    }
}