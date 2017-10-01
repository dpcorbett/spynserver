using SharpTools.MVVM.RelayCommand;
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
using Microsoft.Synchronization;
using SharpTools.MVVM.Mediator;
using System.Windows.Threading;
using SharpTools.Log;
using Microsoft.Synchronization.Files;
using System.Threading.Tasks;
using System.Windows;

namespace WPFSpyn.ViewModel
{

    /// <summary>
    /// A UI-friendly wrapper for a SyncPair object.
    /// </summary>
    public class SyncPairViewModel : WorkspaceViewModel, IDataErrorInfo
    {

        #region Fields

        // Create logger.
        private static readonly log4net.ILog _log = SharpToolsLog.GetLogger();
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
        // Create a delete sync pair relay command.
        private SharpToolsMVVMRelayCommand _deleteSyncPairCommand;
        // Create a sync relay command.
        private SharpToolsMVVMRelayCommand _syncCommand;
        // Create a preview relay command
        private SharpToolsMVVMRelayCommand _previewCommand;
        // Create commands.
        private IWorkspaceCommands _wsCommands;
        // Create an observable collection of source directories and files.
        private ObservableCollection<FileInfo> _srcTree;
        // Create an observable collection of destination directories and files.
        private ObservableCollection<FileInfo> _dstTree;
        // Create an observable collection for preview sync results.
        ObservableCollection<string> _resultLog;
        // Store value for progress meter.
        private double _currentProgress;
        // To run preview from.
        private BackgroundWorker _previewWorker = new BackgroundWorker();
        // To run sync from.
        private BackgroundWorker _syncWorker = new BackgroundWorker();

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
        /// Exposes get destination root command.
        /// </summary>
        public SharpToolsMVVMRelayCommand BrowseSrcRootCommand { get; set; }

        /// <summary>
        /// Exposes get destination root command.
        /// </summary>
        public SharpToolsMVVMRelayCommand BrowseDstRootCommand { get; set; }

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

        /// <summary>
        /// Exposes sync results observable collection.
        /// </summary>
        public ObservableCollection<string> ResultLog
        {
            get
            {
                return _resultLog;
            }
            set
            {
                if (_resultLog != value)
                {
                    _resultLog = value;
                    OnPropertyChanged("ResultLog");
                }
            }
        }

        /// <summary>
        /// Update progress meter.
        /// </summary>
        public double CurrentProgress
        {
            get { return _currentProgress; }
            private set
            {
                _currentProgress = value;
                OnPropertyChanged("CurrentProgress");
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
            _previewWorker.WorkerReportsProgress = true;
            _syncWorker.WorkerReportsProgress = true;
            _previewWorker.WorkerSupportsCancellation = true;
            _syncWorker.WorkerSupportsCancellation = true;
            _previewWorker.DoWork += new DoWorkEventHandler(PreviewWorker_DoWork);
            _syncWorker.DoWork += new DoWorkEventHandler(SyncWorker_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            
            // Create local commands.
            _wsCommands = wsCommands;

            // Set sync pair.
            _syncPair = p_syncPair ?? throw new ArgumentNullException("p_syncPair");

            // Set sync pair repository.
            _syncPairRepository = p_syncPairRepository ?? throw new ArgumentNullException("p_syncPairRepository");

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

            // Initialise source root browser.
            BrowseSrcRootCommand = new SharpToolsMVVMRelayCommand(BrowseSrcRoot);

            // Initialise destination root browser.
            BrowseDstRootCommand = new SharpToolsMVVMRelayCommand(BrowseDstRoot);

            // Initialise delete sync pair.
            DeleteSyncPairCommand = new SharpToolsMVVMRelayCommand(Delete);
            
            SharpToolsMVVMMediator.Register("update", AddUpdate); // Listener for change events
            // LOG
            _log.Debug("Mediator Registered");
            //
            ResultLog = new ObservableCollection<string>();
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
                if (value == _syncPairType || string.IsNullOrEmpty(value))
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
        /// Returns a command that previews the sync pair.
        /// </summary>
        public ICommand PreviewCommand
        {
            get
            {
                if (_previewCommand == null)
                {
                    _previewCommand = new SharpToolsMVVMRelayCommand(
                        param => Preview(this),
                        param => CanPreview
                        );
                }
                return _previewCommand;
            }
        }

        /// <summary>
        /// Returns a command that syncs the sync pair.
        /// </summary>
        public ICommand SyncCommand
        {
            get
            {
                if (_syncCommand == null)
                {
                    _syncCommand = new SharpToolsMVVMRelayCommand(
                        param => Sync(this),
                        param => CanSync
                        );
                }
                return _syncCommand;
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
        /// Run preview on background thread if available. This method is invoked by the PreviewCommand. 
        /// </summary>
        public void Preview(object syncpair)
        {
            if (_previewWorker.IsBusy != true)
            {
                _previewWorker.RunWorkerAsync(syncpair);
                UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Deletes the Syncpair from the repository.  This method is invoked by the DeleteCommand.
        /// </summary>
        public void Delete(object syncpair)
        {
            // Check if sync pair has been saved.
            if (!IsNewSyncPair)
            {
                // Check if user is certain.
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    // Remove sync pair.
                    _syncPairRepository.DeleteSyncPair(_syncPair);
                    // Remove workspace.
                    _wsCommands.RemoveWorkspace(this);
                }
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
            if (_syncWorker.IsBusy != true)
            {
                _syncWorker.RunWorkerAsync(syncpair);
                UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
            }


            /*
            // Check sync pair is ready to sync.
            if (_syncPair.IsValid)
            {
                ResultLog = new ObservableCollection<string>(); //reset log

                bool isFullSync = false;
                SyncPairViewModel spvm = (SyncPairViewModel)syncpair;
                if (spvm != null)
                    isFullSync = spvm.IsFullSync;
                SharpToolsSynch.Sync(_syncPair.SrcRoot, _syncPair.DstRoot, isFullSync);            
                // Put sync on background thread
                //Task.Factory.StartNew(() => { SharpToolsSynch.Sync(_syncPair.SrcRoot, _syncPair.DstRoot); }).ContinueWith(_ => { IsSynchronising = false; });


                UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

                // TODO throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);

                return;
            }*/
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

        /// <summary>
        /// Use explorer to edit source.
        /// </summary>
        /// <param name="param"></param>
        private void BrowseSrcRoot(object param)
        {
            // Create dialog window.
            System.Diagnostics.Process.Start("explorer.exe", _syncPair.SrcRoot);
            // Raise event for directory tree view.
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

        }

        /// <summary>
        /// Use explorer to edit destination.
        /// </summary>
        /// <param name="param"></param>
        private void BrowseDstRoot(object param)
        {
            // Create dialog window.
            System.Diagnostics.Process.Start("explorer.exe", _syncPair.DstRoot);
            // Raise event for directory tree view.
            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

        }


        #endregion // Public Methods


        #region Private Helpers

        /// <summary>
        /// Returns true if this sync pair was created by the user and it has not yet
        /// been saved to the sync pair repository.
        /// </summary>
        bool IsNewSyncPair
        {
            get { return !_syncPairRepository.ContainsSyncPair(_syncPair); }
        }

        /// <summary>
        /// Returns true if the sync pair is valid and can be saved.
        /// </summary>
        bool CanSave
        {
            get { return string.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }
        
        /// <summary>
        /// Returns true if the sync pair is valid and can be paired.
        /// </summary>
        bool CanPreview
        {
            get { return string.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        /// <summary>
        /// Returns true if the sync pair is valid and can be refreshed.
        /// </summary>
        bool CanSync
        {
            get { return string.IsNullOrEmpty(ValidateSyncPairType()) && _syncPair.IsValid; }
        }

        /// <summary>
        /// Updates result log with supplied string.
        /// </summary>
        /// <param name="param"></param>
        void AddUpdate(object param)
        {
            // Dispatch update for result log.
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                // Add string to result log.
                ResultLog.Add(param as string);
            }));
        }


        private void PreviewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
            }
            else
            {
                // Perform a time consuming operation and report progress.
                //System.Threading.Thread.Sleep(500);
                //worker.ReportProgress((i * 10));
                bool isFullSync = false;
                SyncPairViewModel spvm = (SyncPairViewModel)e.Argument;

                if (spvm != null)
                {
                    isFullSync = spvm.IsFullSync;
                    //CurrentProgress = 50;
                    worker.ReportProgress(50);
                    SyncOperationStatistics sos = SharpToolsSynch.PreviewSync(_syncPair.SrcRoot, _syncPair.DstRoot, isFullSync);
                    //CurrentProgress = 100;
                    worker.ReportProgress(100);
                    string msg;
                    if (sos != null)
                    {
                        //UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
                        // Display statistics for the synchronization operation.
                        msg = "Synchronization analysis...\n\n" +
                            sos.DownloadChangesApplied + " update(s) to source pending.\n" +
                            sos.DownloadChangesFailed + " update(s) to source will fail.\n" +
                            sos.UploadChangesApplied + " update(s) to destination pending.\n" +
                            sos.UploadChangesFailed + " update(s) to destination will fail.";
                        System.Windows.MessageBox.Show(msg, "Synchronization Results");
                    }
                }
            }
        }


        private void SyncWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
            }
            else
            {
                // Check sync pair is ready to sync.
                if (_syncPair.IsValid)
                {
                    ResultLog = new ObservableCollection<string>(); //reset log

                    bool isFullSync = false;
                    SyncPairViewModel spvm = (SyncPairViewModel)e.Argument;
                    if (spvm != null)
                        isFullSync = spvm.IsFullSync;
                    SharpToolsSynch.Sync(_syncPair.SrcRoot, _syncPair.DstRoot, isFullSync);
                    // Put sync on background thread
                    //Task.Factory.StartNew(() => { SharpToolsSynch.Sync(_syncPair.SrcRoot, _syncPair.DstRoot); }).ContinueWith(_ => { IsSynchronising = false; });


                   // UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);

                    // TODO throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);

                    return;
                }

            }
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

    }
}
