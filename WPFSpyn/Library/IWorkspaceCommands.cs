// dpc
using WPFSpyn.ViewModel;

namespace WPFSpyn.Library
{
    /// <summary>
    /// Base commands for classes extended from WorkspaceViewModel.
    /// </summary>
    public interface IWorkspaceCommands
    {
        /// <summary>
        /// Define adding a workspace.
        /// </summary>
        /// <param name="view"></param>
        void AddWorkspace(WorkspaceViewModel view);

        /// <summary>
        /// Define removing a workspace.
        /// </summary>
        /// <param name="view"></param>
        void RemoveWorkspace(WorkspaceViewModel view);
    }
}
