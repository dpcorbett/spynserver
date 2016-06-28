﻿using SharpTools.MVVM.RelayCommand;
using SharpTools.Synch;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Forms;
using WPFSpyn.DataAccess;
using WPFSpyn.Library;
using WPFSpyn.Model;
using WPFSpyn.Properties;

namespace WPFSpyn.ViewModel
{

    /// <summary>
    /// A UI-friendly wrapper for a SyncPair object.
    /// </summary>
    public class SyncPairViewModel : WorkspaceViewModel, IDataErrorInfo
    {

        #region Fields

        // Create a sync pair.
        private readonly SyncPair _syncPair;
        // Create a sync pair repository.
        private readonly SyncPairRepository _syncPairRepository;
        // True if synchronizing.
        private bool m_booIsSynchronising;
        // Create a sync pair type.
        private string _syncPairType;
        // Create possible sync pair types.
        private string[] _syncPairTypeOptions;
        // Create selected state.
        private bool _isSelected;
        // Create a save relay command.
        private SharpToolsMVVMRelayCommand _saveCommand;
        // Create a delete relay command.
        private SharpToolsMVVMRelayCommand _deleteCommand;
        // Create a delete sync pair relay command.
        private SharpToolsMVVMRelayCommand _deleteSyncPairCommand;
        // Create a sync relay command.
        private SharpToolsMVVMRelayCommand _syncCommand;
        // Create a pair relay command
        private SharpToolsMVVMRelayCommand _pairCommand;
        // Create commands.
        private IWorkspaceCommands _wsCommands;
        // Create an observable collection of source directories and files.
        private ObservableCollection<FileInfo> _srcTree;
        // Create an observable collection of destination directories and files.
        private ObservableCollection<FileInfo> _dstTree;

        #endregion // Fields


        #region Properties

        // Directory path update event handler.
        public event EventHandler UpdateDirectoryPath;

        /// <summary>
        /// Exposes get source root command.
        /// </summary>
        public SharpToolsMVVMRelayCommand GetSrcRootCommand { get; set; }

        /// <summary>
        /// Exposes get destination root command.
        /// </summary>
        public SharpToolsMVVMRelayCommand GetDstRootCommand { get; set; }

        /// <summary>
        /// Exposes delete sync pair command.
        /// </summary>
        public SharpToolsMVVMRelayCommand DeleteSyncPairCommand
        {
            get { return _deleteSyncPairCommand; }
            set
            {
                if (_deleteSyncPairCommand != value)
                {
                    _deleteSyncPairCommand = value;
                    base.OnPropertyChanged("DeleteSyncPairCommand");
                }
            }
        }

        /// <summary>
        /// Exposes sync command.
        /// </summary>
        public SharpToolsMVVMRelayCommand SyncCommand
        {
            get { return _syncCommand; }
            set
            {
                if (_syncCommand != value)
                {
                    _syncCommand = value;
                    base.OnPropertyChanged("SyncCommand");
                }
            }
        }

        /// <summary>
        /// Exposes description.
        /// </summary>
        public string Description
        {
            get { return _syncPair.Description; }
            set
            {
                if (value == _syncPair.Description)
                    return;
                _syncPair.Description = value;
                base.OnPropertyChanged("Description");
            }
        }

        /// <summary>
        /// Exposes source root directory.
        /// </summary>
        public string SrcRoot
        {
            get { return _syncPair.SrcRoot; }
            set
            {
                if (value == _syncPair.SrcRoot)
                    return;
                _syncPair.SrcRoot = value;
                base.OnPropertyChanged("SrcRoot");
            }
        }

        /// <summary>
        /// Exposes destination root directory.
        /// </summary>
        public string DstRoot
        {
            get { return _syncPair.DstRoot; }
            set
            {
                if (value == _syncPair.DstRoot)
                    return;
                _syncPair.DstRoot = value;
                base.OnPropertyChanged("DstRoot");
            }
        }

        /// <summary>
        /// Exposes full sync status.
        /// </summary>
        public bool IsFullSync
        {
            get { return _syncPair.IsFullSync; }
        }
        
        /// <summary>
        /// Exposes source observable collection.
        /// </summary>
        public ObservableCollection<FileInfo> SrcTree
        {
            get { return _srcTree; }
            set
            {
                if (_srcTree != value)
                {
                    _srcTree = value;
                    base.OnPropertyChanged("PathFilesSrc");
                }
            }
        }

        /// <summary>
        /// Exposes destination observable collection.
        /// </summary>
        public ObservableCollection<FileInfo> DstTree
        {
            get { return _dstTree; }
            set
            {
                if (_dstTree != value)
                {
                    _dstTree = value;
                    base.OnPropertyChanged("PathFilesDst");
                }
            }
        }

        /// <summary>
        /// Exposes sync state.
        /// </summary>
        public bool IsSynchronising
        {
            get { return m_booIsSynchronising; }
            set
            {
                if (m_booIsSynchronising != value)
                {
                    m_booIsSynchronising = value;
                    OnPropertyChanged("IsSynchronising");
                }
            }
        }

        #endregion // Properties


        #region Constructor

        /// <summary>
        /// Constructor created from supplied args.
        /// </summary>
        /// <param name="p_syncPair"></param>
        /// <param name="p_syncPairRepository"></param>
        /// <param name="wsCommands"></param>
        public SyncPairViewModel(SyncPair p_syncPair, SyncPairRepository p_syncPairRepository, IWorkspaceCommands wsCommands)
        {
            // Create local commands.
            _wsCommands = wsCommands;

            // Check sync pair exists.
            if (p_syncPair == null)
                throw new ArgumentNullException("p_syncPair");

            // Check sync repository exists.
            if (p_syncPairRepository == null)
                throw new ArgumentNullException("p_syncPairRepository");

            // Set sync pair.
            _syncPair = p_syncPair;
            // Set sync pair repository.
            _syncPairRepository = p_syncPairRepository;

            // Set sync type if specified.
            if (_syncPair.SyncType == null)
            {
                _syncPairType = Strings.SyncPairViewModel_SyncPairTypeOption_NotSpecified;
            }
            else
            {
                _syncPairType = _syncPair.SyncType;
            }

            // Initialise source root retrieval.
            GetSrcRootCommand = new SharpToolsMVVMRelayCommand(GetSrcRoot);

            // Initialise destination root retrieval.
            GetDstRootCommand = new SharpToolsMVVMRelayCommand(GetDstRoot);
            
            // Initialise delete sync pair.
            DeleteSyncPairCommand = new SharpToolsMVVMRelayCommand(Delete);
            
            // Initialise sync.
            SyncCommand = new SharpToolsMVVMRelayCommand(Sync);
        }

        #endregion // Constructor


        #region Presentation Properties

        /// <summary>
        /// Gets/sets a value that indicates what type of SyncPair this is.
        /// This property maps to the IsFullSync property of the SyncPair class,
        /// but also has support for an 'unselected' state.
        /// </summary>
        public string SyncPairType
        {
            get { return _syncPairType; }
            set
            {
                // Break if type is already selected or is empty.
                if (value == _syncPairType || String.IsNullOrEmpty(value))
                    return;

                // Set type.
                _syncPairType = value;
                // Assign to sync pair.
                _syncPair.SyncType = _syncPairType;

                // Modify full sync status according to selected type.
                if (_syncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_FullSync)
                {
                    _syncPair.IsFullSync = true;
                }
                else 
                {
                    _syncPair.IsFullSync = false;
                }

                // Raise type changed.
                base.OnPropertyChanged("SyncPairType");

                // Since this ViewModel object has knowledge of how to translate
                // a SyncPair type (i.e. text) to a SyncPair object's IsFullSync property,
                // it also must raise a property change notification when it changes
                // the value of IsFullSync.  The SyncPairType property is validated 
                // differently based on whether the Sync is a both ways or not,
                // so the validation for the IsFullSync property must execute now.
                base.OnPropertyChanged("IsFullSync");
            }
        }

        /// <summary>
        /// Returns a list of strings used to populate the SyncPair Type selector.
        /// </summary>
        public string[] SyncPairTypeOptions
        {
            get
            {
                // Return an existing array, or create a new one.
                if (_syncPairTypeOptions == null)
                {
                    _syncPairTypeOptions = new string[]
                    {
                        Strings.SyncPairViewModel_SyncPairTypeOption_NotSpecified,
                        Strings.SyncPairViewModel_SyncPairTypeOption_FullSync,
                        Strings.SyncPairViewModel_SyncPairTypeOption_PushSync
                    };
                }
                return _syncPairTypeOptions;
            }
        }

        /// <summary>
        /// Override base display name to update.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (IsNewSyncPair)
                {
                    return Strings.SyncPairViewModel_DisplayName;
                }

                else 
                {
                    return _syncPair.Description;
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this Sync Pair is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                // Exit if state is already set.
                if (value == _isSelected) return;

                // Set value and raise property changed event.
                _isSelected = value;
                base.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Returns a command that saves the SyncPair.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new SharpToolsMVVMRelayCommand(
                        param => Save(),
                        param => CanSave
                        );
                }
                return _saveCommand;
            }
        }

        /// <summary>
        /// Returns a command that deletes the SyncPair.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new SharpToolsMVVMRelayCommand(
                        param => Delete(this),
                        param => CanDelete
                        );
                }
                return _deleteCommand;
            }
        }

        /// <summary>
        /// Returns a command that pairs the SyncPair.
        /// </summary>
        public ICommand PairCommand
        {
            get
            {
                if (_pairCommand == null)
                {
                    _pairCommand = new SharpToolsMVVMRelayCommand(
                        param => Pair(),
                        param => CanPair
                        );
                }
                return _pairCommand;
            }
        }

        #endregion // Presentation Properties


        #region Public Methods

        /// <summary>
        /// Saves the Syncpair to the repository.  This method is invoked by the SaveCommand.
        /// </summary>
        public void Save()
        {
            // Check state for saving.
            if (!_syncPair.IsValid)
                throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);

            // Save sync pair.
            if (IsNewSyncPair)
                _syncPairRepository.AddSyncPair(_syncPair);
            
            // Raise event that display name property has changed.
            base.OnPropertyChanged("DisplayName");

            // Update data store.
            SyncPairRepository.SaveSyncPairs(_syncPairRepository, MainWindowViewModel.DATA_PATH);
        }

        /// <summary>
        /// Adds the sync metadata. This method is invoked by the PairCommand. 
        /// </summary>
        public void Pair()
        {
            SharpToolsSynch.SetSync(_syncPair.SrcRoot, _syncPair.DstRoot);
        }

        /// <summary>
        /// Deletes the Syncpair from the repository.  This method is invoked by the DeleteCommand.
        /// </summary>
        public void Delete(object syncpair)
        {
            // Check state for deletion.
            if (!_syncPair.IsValid)
            {
                // TODO throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);
                System.Windows.MessageBox.Show("Not Saved");
                return;
            }

            // Check if sync pair has been saved.
            if (!IsNewSyncPair)
            {
                // Remove sync pair.
                _syncPairRepository.DeleteSyncPair(_syncPair);
                // Remove workspace.
                _wsCommands.RemoveWorkspace(this);
            }

            // Update data store.
            SyncPairRepository.SaveSyncPairs(_syncPairRepository, MainWindowViewModel.DATA_PATH);

            // Destroy base.
            base.Dispose();
        }

        /// <summary>
        /// Create sync view from sync pair supplied.
        /// </summary>
        /// <param name="syncpair"></param>
        public void Sync(object syncpair)
        {
            // Check sync pair is ready to sync.
            if (!_syncPair.IsValid)
            {
                // TODO throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);
                System.Windows.MessageBox.Show("Not Saved");
                return;
            }

            // Create sync pair view model and add it to workspaces.
            var syncVM = new SyncViewModel(_syncPair, _syncPairRepository, _wsCommands);
            _wsCommands.AddWorkspace(syncVM);
        }

        /// <summary>
        /// Use folder browser dialog to collect path for source root directory,
        /// and notify tree view.
        /// </summary>
        /// <param name="param"></param>
        private void GetSrcRoot(object param)
        {
            // Create dialog window.
            var dialog = new FolderBrowserDialog();
            // Open dialog window.
            DialogResult result = dialog.ShowDialog();
            // Retrieve selected path.
            SrcRoot = dialog.SelectedPath;
            // Raise event for directory tree view.
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

        }

        /// <summary>
        /// Use folder browser dialog to collect path for destination
        /// root directory, and notify tree view.
        /// </summary>
        /// <param name="param"></param>
        private void GetDstRoot(object param)
        {
            // Create dialog window.
            var dialog = new FolderBrowserDialog();
            // Open dialog window.
            DialogResult result = dialog.ShowDialog();
            // Retrieve selected path.
            DstRoot = dialog.SelectedPath;
            // Raise event for directory tree view.
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
        }

        #endregion // Public Methods


        #region Private Helpers

        /// <summary>
        /// Returns true if this SyncPair was created by the user and it has not yet
        /// been saved to the SyncPair repository.
        /// </summary>
        bool IsNewSyncPair
        {
            get { return !_syncPairRepository.ContainsSyncPair(_syncPair); }
        }

        /// <summary>
        /// Returns true if the SyncPair is valid and can be saved.
        /// </summary>
        bool CanSave
        {
            get { return String.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        /// <summary>
        /// Returns true if the SyncPair is valid and can be deleted.
        /// </summary>
        bool CanDelete
        {
            get { return String.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }
        
        /// <summary>
        /// Returns true if the SyncPair is valid and can be paired.
        /// </summary>
        bool CanPair
        {
            get { return String.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        #endregion // Private Helpers


        #region IDataErrorInfo Members

        /// <summary>
        /// Overrides base command.
        /// </summary>
        string IDataErrorInfo.Error
        {
            get { return (_syncPair as IDataErrorInfo).Error; }
        }

        /// <summary>
        /// Overrides base command.
        /// </summary>
        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                if (propertyName == "SyncPairType")
                {
                    // The IsFullSync property of the SyncPair class 
                    // is Boolean, so it has no concept of being in
                    // an "unselected" state.  The SyncPairViewModel
                    // class handles this mapping and validation.
                    error = ValidateSyncPairType();
                }
                else
                {
                    error = (_syncPair as IDataErrorInfo)[propertyName];
                }

                // Dirty the commands registered with CommandManager,
                // such as Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        /// <summary>
        /// Check that a valid sync pair type is set.
        /// </summary>
        /// <returns>null or string error message</returns>
        string ValidateSyncPairType()
        {
            // Check that one of the two valid options have been chosen, and return null.
            if (SyncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_FullSync || SyncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_PushSync)
                return null;

            // Return defined error message.
            return Strings.SyncPairViewModel_Error_MissingSyncPairType;
        }

        #endregion // IDataErrorInfo Members


        #region Event Handling Methods


        /// <summary>
        /// Handles OnSyncPairAddedToRepository Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairAddedToRepository(object sender, SyncPairAddedEventArgs e)
        {
            // Create new view model.
            var viewModel = new SyncViewModel(e.NewSyncPair, _syncPairRepository, _wsCommands);

            // Add to workspaces.
            _wsCommands.AddWorkspace(viewModel);
        }


        #endregion // Event Handling Methods

    }
}
