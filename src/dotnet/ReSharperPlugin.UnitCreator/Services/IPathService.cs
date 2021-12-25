using JetBrains.Annotations;
using JetBrains.ProjectModel;

namespace ReSharperPlugin.UnitCreator.Services {
    /// <summary>
    ///
    /// Service which is used to work with paths like
    /// ProjectName\Folder1\Folder2\FileName.
    /// 
    /// </summary>
    public interface IPathsService {
        string GetUniqueFileName([NotNull] IProjectFolder folder, [NotNull] string proposedName);
        [NotNull] string GetExpectedClassName([NotNull] string filePath);
        [NotNull] string Combine([NotNull] IProject project, [NotNull] string fileName);
    }
}