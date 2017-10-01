using SharpTools.MVVM.RelayCommand;
using System;
using System.Windows.Input;

namespace WPFSpyn.ViewModel
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed 
    /// from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {

        #region Fields

        // Use relay to issue close command.
        SharpToolsMVVMRelayCommand _closeCommand;

        #endregion // Fields


        #region Constructor

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected WorkspaceViewModel()
        {
        }

        #endregion // Constructor


        #region CloseCommand

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new SharpToolsMVVMRelayCommand(param => OnRequestClose());

                return _closeCommand;
            }
        }

        #endregion // CloseCommand


        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]
    }
}