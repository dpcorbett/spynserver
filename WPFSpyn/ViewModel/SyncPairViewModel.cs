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
        // Store preview sync state.
        private SyncOperationStatistics _previewStats;
        // Used to run explorer for browse.
        System.Diagnostics.Process sdp;
        // Used for progress meter increments.
        private double _progressStep;
        // The number of changes for sync type.
        private int _changes;
        // The current change count.
        private int _currentStep;

        #endregion // Fields


        #region Properties

        // Directory path update event handler.
        public event EventHandler UpdateDirectoryPath;

        //public event EventHandler UpdateProgressBar;

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
        /// Update progress meter through binding.
        /// </summary>
        public double CurrentProgress
        {
            get { return _currentProgress; }
            private set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnPropertyChanged("CurrentProgress");
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
        public SyncPairViewModel(SyncPair p_syncPair, SyncPairRepository p_syncPairRepository, IWorkspaceCommands p_wsCommands)
        {
            //_currentProgress = 0;
            _previewWorker.WorkerReportsProgress = true;
            _syncWorker.WorkerReportsProgress = true;
            _previewWorker.WorkerSupportsCancellation = true;
            _syncWorker.WorkerSupportsCancellation = true;
            _previewWorker.DoWork += new DoWorkEventHandler(PreviewWorker_DoWork);
            _syncWorker.DoWork += new DoWorkEventHandler(SyncWorker_DoWork);
            _previewWorker.ProgressChanged += new ProgressChangedEventHandler(PreviewWorker_ProgressChanged);
            _syncWorker.ProgressChanged += new ProgressChangedEventHandler(SyncWorker_ProgressChanged);
            //bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            // Create local commands.
            _wsCommands = p_wsCommands;

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
            
            SharpToolsMVVMMediator.Register("update", AddUpdate); // Listener for change events.
            // LOG
            _log.Debug("Update Mediator Registered");
            //
            ResultLog = new ObservableCollection<string>();

            SharpToolsMVVMMediator.Register("progress", ProgressUpdate); // Listener for change events.
            // LOG
            _log.Debug("Progress Mediator Registered");
            //
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
        /// Create sync view from sync pair supplied on a background worker.
        /// </summary>
        /// <param name="syncpair"></param>
        public void Sync(object syncpair)
        {
            if (_syncWorker.IsBusy != true)
            {
                _currentProgress = 0;
                _currentStep = 0;
                _syncWorker.RunWorkerAsync(syncpair);
                UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
                IsSynchronising = false;
            }
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

        //System.Diagnostics.Process sdp = new System.Diagnostics.Process();

        /// <summary>
        /// Use explorer to edit source.
        /// </summary>
        /// <param name="param"></param>
        private void BrowseSrcRoot(object param)
        {
            sdp = new System.Diagnostics.Process();
            sdp.StartInfo.Arguments = _syncPair.SrcRoot;
            sdp.StartInfo.FileName = "explorer.exe";
            sdp.EnableRaisingEvents = true;
            sdp.Exited += new EventHandler(Browse_Exited);
            sdp.Start();
        }

        /// <summary>
        /// Use explorer to edit destination.
        /// </summary>
        /// <param name="param"></param>
        private void BrowseDstRoot(object param)
        {
            sdp = new System.Diagnostics.Process();
            sdp.StartInfo.Arguments = _syncPair.DstRoot;
            sdp.StartInfo.FileName = "explorer.exe";
            sdp.EnableRaisingEvents = true;
            sdp.Exited += new EventHandler(Browse_Exited);
            sdp.Start();
        }

        /// <summary>
        /// Open Modal message box and do update on it's close.
        /// </summary>
        /// <param name="sender">Process that is exiting.</param>
        /// <param name="e"></param>
        private void Browse_Exited(object sender, System.EventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                System.Windows.MessageBox.Show(System.Windows.Application.Current.MainWindow, "If changes were made while browsing.", "Close to Refresh", MessageBoxButton.OK, MessageBoxImage.Stop);
                UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
            });
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

        /// <summary>
        /// Update progress with integer(default is 1).
        /// </summary>
        /// <param name="param"></param>
        void ProgressUpdate(object param)
        {
            // Dispatch update for result log.
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                // Add string to result log.
                _currentStep += (Int32)param;
                CurrentProgress = CurrentProgress + (_progressStep * _currentStep);
            }));
        }



        /// <summary>
        /// Run and report preview sync of data.
        /// </summary>
        /// <param name="sender">Background worker as object.</param>
        /// <param name="e">SyncPairViewModel passed in.</param>
        private void PreviewWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if ((worker.CancellationPending == true))
            {
                e.Cancel = true;
            }
            else
            {
                bool isFullSync = false;
                SyncPairViewModel spvm = (SyncPairViewModel)e.Argument;
                if (spvm != null)
                {
                    ResultLog = new ObservableCollection<string>(); //reset log
                    isFullSync = spvm.IsFullSync;
                    // Set progress to half for started,
                    // and full for done. This can't be
                    // reported accurately at runtime.
                    worker.ReportProgress(50);
                    _previewStats = SharpToolsSynch.PreviewSync(_syncPair.SrcRoot, _syncPair.DstRoot, isFullSync);
                    worker.ReportProgress(100);
                    if (_previewStats != null)
                    {
                        _changes = _previewStats.UploadChangesTotal;
                        if (isFullSync)
                            _changes += _previewStats.DownloadChangesTotal;

                        // Prevent divide by zero and give full progress to an empty sync.
                        _progressStep = _changes > 0 ? ((100 /_changes) + 1) : 100;

                        // Pass update to UI.
                        App.Current.Dispatcher.Invoke((Action)delegate 
                        {
                            ResultLog.Add(_previewStats.DownloadChangesApplied + " update(s) to source pending.\n" +
                            _previewStats.DownloadChangesFailed + " update(s) to source will fail.\n" +
                            _previewStats.UploadChangesApplied + " update(s) to destination pending.\n" +
                            _previewStats.UploadChangesFailed + " update(s) to destination will fail.");
                            UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Run and report sync of data.
        /// </summary>
        /// <param name="sender">Background worker as object.</param>
        /// <param name="e">SyncPairViewModel passed in.</param>
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
                    worker.ReportProgress(1);
                    SharpToolsSynch.Sync(_syncPair.SrcRoot, _syncPair.DstRoot, isFullSync);
                    if (_changes == 0)
                        worker.ReportProgress(100);
                    // Update treeviews.
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        UpdateDirectoryPath?.Invoke(this, EventArgs.Empty);
                    });
                    return;
                }
            }
        }

        /// <summary>
        /// Updates preview progress on changed event.
        /// </summary>
        /// <param name="sender">Object that reported the change.</param>
        /// <param name="pcea">The new value for preview progress.</param>
        private void PreviewWorker_ProgressChanged(object sender, ProgressChangedEventArgs pcea)
        {
            this.CurrentProgress = (this.CurrentProgress != pcea.ProgressPercentage) ? pcea.ProgressPercentage : this.CurrentProgress;
        }

        /// <summary>
        /// Updates sync progress on changed event.
        /// </summary>
        /// <param name="sender">Object that reported the change.</param>
        /// <param name="pcea">The new value for sync progress.</param>
        private void SyncWorker_ProgressChanged(object sender, ProgressChangedEventArgs pcea)
        {
            //System.Windows.MessageBox.Show($"param: {pcea.ProgressPercentage}");
            this.CurrentProgress = (this.CurrentProgress != pcea.ProgressPercentage) ? pcea.ProgressPercentage : this.CurrentProgress;
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
