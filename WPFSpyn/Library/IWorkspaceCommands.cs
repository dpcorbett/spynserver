using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFSpyn.ViewModel;

namespace WPFSpyn.Library
{
    public interface IWorkspaceCommands
    {
        void AddWorkspace(WorkspaceViewModel view);
        void RemoveWorkspace(WorkspaceViewModel view);
    }
}
