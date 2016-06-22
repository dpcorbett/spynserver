using SharpTools.MVVM.RelayCommand;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

        private readonly SyncPair _syncPair;
        private readonly SyncPairRepository _syncPairRepository;
        private bool m_booIsSynchronising;
        private string _syncPairType;
        private string[] _syncPairTypeOptions;
        private bool _isSelected;
        private SharpToolsMVVMRelayCommand _saveCommand;
        private SharpToolsMVVMRelayCommand _deleteCommand;
        private SharpToolsMVVMRelayCommand _deleteSyncPairCommand;
        private SharpToolsMVVMRelayCommand _syncCommand;
        private IWorkspaceCommands _wsCommands;
        private ObservableCollection<FileInfo> _pathFilesSrc;
        private ObservableCollection<FileInfo> _pathFilesDst;


        #endregion // Fields


        #region Properties

        public event EventHandler UpdateDirectoryPath;

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
        
        public SharpToolsMVVMRelayCommand GetSrcRootCommand { get; set; }

        public SharpToolsMVVMRelayCommand GetDstRootCommand { get; set; }

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

        public bool IsFullSync
        {
            get { return _syncPair.IsFullSync; }
        }
        
        public ObservableCollection<FileInfo> PathFilesSrc
        {
            get
            {
                return _pathFilesSrc;
            }
            set
            {
                if (_pathFilesSrc != value)
                {
                    _pathFilesSrc = value;
                    base.OnPropertyChanged("PathFilesSrc");
                }
            }
        }

        public ObservableCollection<FileInfo> PathFilesDst
        {
            get
            {
                return _pathFilesDst;
            }
            set
            {
                if (_pathFilesDst != value)
                {
                    _pathFilesDst = value;
                    base.OnPropertyChanged("PathFilesDst");
                }
            }
        }


        //public ObservableCollection<string> Log
        //{
        //    get
        //   {
        //         return m_obcLog;
        //     }
        //      set
        //      {
        //         if (m_obcLog != value)
        //         {
        //             m_obcLog = value;
        //             OnPropertyChanged("Log");
        //         }
        //    }
        //  }

        public bool IsSynchronising
        {
            get
            {
                return m_booIsSynchronising;
            }
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

        public SyncPairViewModel(SyncPair p_syncPair, SyncPairRepository p_syncPairRepository, IWorkspaceCommands wsCommands)
        {
            _wsCommands = wsCommands;

            if (p_syncPair == null)
                throw new ArgumentNullException("p_syncPair");

            if (p_syncPairRepository == null)
                throw new ArgumentNullException("p_syncPairRepository");

            _syncPair = p_syncPair;
            _syncPairRepository = p_syncPairRepository;

            if (_syncPair.SyncType == null)
            {
                _syncPairType = Strings.SyncPairViewModel_SyncPairTypeOption_NotSpecified;
            }
            else
            {
                _syncPairType = _syncPair.SyncType;
            }

            GetSrcRootCommand = new SharpToolsMVVMRelayCommand(GetSrcRoot);
            GetDstRootCommand = new SharpToolsMVVMRelayCommand(GetDstRoot);
            DeleteSyncPairCommand = new SharpToolsMVVMRelayCommand(Delete);
            SyncCommand = new SharpToolsMVVMRelayCommand(Sync);

           // UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
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
            get 
            { 
                return _syncPairType; 
            }

            set
            {
                if (value == _syncPairType || String.IsNullOrEmpty(value))
                    return;

                _syncPairType = value;
                _syncPair.SyncType = _syncPairType;

                if (_syncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_FullSync)
                {
                    _syncPair.IsFullSync = true;
                }
                else 
                {
                    _syncPair.IsFullSync = false;
                }

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


        public override string DisplayName
        {
            get
            {
                if (this.IsNewSyncPair)
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
                if (value == _isSelected)
                    return;

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
                        param => this.Save(),
                        param => this.CanSave
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
                        param => this.Delete(this),
                        param => this.CanDelete
                        );
                }
                return _deleteCommand;
            }
        }

        #endregion // Presentation Properties


        #region Public Methods




        /// <summary>
        /// Saves the Syncpair to the repository.  This method is invoked by the SaveCommand.
        /// </summary>
        public void Save()
        {
            if (!_syncPair.IsValid)
                throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);

            if (this.IsNewSyncPair)
                _syncPairRepository.AddSyncPair(_syncPair);
            
            base.OnPropertyChanged("DisplayName");
            SyncPairRepository.SaveSyncPairs(this._syncPairRepository, MainWindowViewModel.DATA_PATH);
        }


        /// <summary>
        /// Deletes the Syncpair from the repository.  This method is invoked by the DeleteCommand.
        /// </summary>
        public void Delete(object syncpair)
        {
            if (!_syncPair.IsValid)
            {
                //throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);
                System.Windows.MessageBox.Show("Not Saved");
                return;
            }

            if (!this.IsNewSyncPair)
            {
                _syncPairRepository.DeleteSyncPair(_syncPair);
                _wsCommands.RemoveWorkspace(this);
            }
            base.OnPropertyChanged("DisplayName");
            SyncPairRepository.SaveSyncPairs(this._syncPairRepository, MainWindowViewModel.DATA_PATH);
            base.Dispose();
        }


        public void Sync(object syncpair)
        {
            if (!_syncPair.IsValid)
            {
                //throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);
                System.Windows.MessageBox.Show("Not Saved");
                return;
            }

            var syncVM = new SyncViewModel(_syncPair, _syncPairRepository, _wsCommands);
            _wsCommands.AddWorkspace(syncVM);

        }


        private void GetSrcRoot(object param)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            SrcRoot = dialog.SelectedPath;
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

        }


        private void GetDstRoot(object param)
        {
            var dialog = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            DstRoot = dialog.SelectedPath;
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
        }


        /*     private void FillTreeView(TreeViewItem parentItem, string path)
             {
                 foreach (string str in Directory.EnumerateDirectories(path))
                 {
                     TreeViewItem item = new TreeViewItem();
                     item.Header = str.Substring(str.LastIndexOf('\\') + 1);
                     item.Tag = str;
                     item.FontWeight = FontWeights.Normal;
                     parentItem.Items.Add(item);
                     fillFiles(item, str);
                     FillTreeView(item, str);
                 }


             }

             private void fillFiles(TreeViewItem parentItem, string path)
             {
                 foreach (string str in Directory.EnumerateFiles(path))
                 {
                     TreeViewItem item = new TreeViewItem();
                     item.Header = str.Substring(str.LastIndexOf('\\') + 1);
                     item.Tag = str;
                     item.FontWeight = FontWeights.Normal;
                     parentItem.Items.Add(item);

               }  }*/

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
            get { return String.IsNullOrEmpty(this.ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        /// <summary>
        /// Returns true if the SyncPair is valid and can be deleted.
        /// </summary>
        bool CanDelete
        {
            get { return String.IsNullOrEmpty(this.ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        #endregion // Private Helpers


        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_syncPair as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                if (propertyName == "SyncPairType")
                {
                    // The IsCompany property of the SyncPair class 
                    // is Boolean, so it has no concept of being in
                    // an "unselected" state.  The SyncPairViewModel
                    // class handles this mapping and validation.
                    error = this.ValidateSyncPairType();
                }
                else
                {
                    error = (_syncPair as IDataErrorInfo)[propertyName];
                }

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        string ValidateSyncPairType()
        {
            if (this.SyncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_FullSync || this.SyncPairType == Strings.SyncPairViewModel_SyncPairTypeOption_PushSync)
                return null;

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
            var viewModel = new SyncViewModel(e.NewSyncPair, _syncPairRepository, _wsCommands);
            _wsCommands.AddWorkspace(viewModel);
        }


        #endregion // Event Handling Methods


    }
}
