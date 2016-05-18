using SharpTools.MVVM.Mediator;
using SharpTools.MVVM.RelayCommand;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using WPFSpyn.DataAccess;
using WPFSpyn.Model;
using WPFSpyn.Library;
using WPFSpyn.Properties;
using WPFSpyn.ViewModel;

namespace WPFSpyn.ViewModel
{
    public class MainWindowViewModel : WorkspaceViewModel, IWorkspaceCommands
    {
        
        #region Fields

        ReadOnlyCollection<CommandViewModel> _commands;
        readonly SyncPairRepository _syncPairRepository;
        ObservableCollection<WorkspaceViewModel> _workspaces;
        private ObservableCollection<string> m_obcLog;
        private ObservableCollection<FileInfo> m_obcPath1Files;
        private ObservableCollection<FileInfo> m_obcPath2Files;

        #endregion


        #region Properties

        public SharpToolsMVVMRelayCommand GetTestCommand { get; set; }
     
        public ObservableCollection<string> Log
        {
            get
            {
                return m_obcLog;
            }
            set
            {
                if (m_obcLog != value)
                {
                    m_obcLog = value;
                    OnPropertyChanged("Log");
                }
            }
        }

        public ObservableCollection<FileInfo> Path1Files
        {
            get
            {
                return m_obcPath1Files;
            }
            set
            {
                if (m_obcPath1Files != value)
                {
                    m_obcPath1Files = value;
                    OnPropertyChanged("Path1Files");
                }
            }
        }

        public ObservableCollection<FileInfo> Path2Files
        {
            get
            {
                return m_obcPath2Files;
            }
            set
            {
                if (m_obcPath2Files != value)
                {
                    m_obcPath2Files = value;
                    OnPropertyChanged("Path2Files");
                }
            }
        }

        
        #endregion // Properties


        #region Constructors

        public MainWindowViewModel(string syncPairDataFile)
        {
            base.DisplayName = Strings.MainWindowViewModel_DisplayName;
            _syncPairRepository = new SyncPairRepository(syncPairDataFile);
            GetTestCommand = new SharpToolsMVVMRelayCommand(GetTest);
            
            //SharpToolsMVVMMediator.Register("update", AddUpdate ); // Listener for change events

        }

        #endregion


        #region Junk

        /// <summary>
        /// Checks source and destination directories existence for pairing
        /// Created: 12-Mar-2016
        /// </summary>
        private bool CheckPairFolders(string p_strPath1, string p_strPath2)
        {
            bool m_booPairValid = false;
            DirectoryInfo directory1 = new DirectoryInfo(p_strPath1);
            DirectoryInfo directory2 = new DirectoryInfo(p_strPath2);

            if ((directory1.Exists) && (directory2.Exists))
                m_booPairValid = true;

           return m_booPairValid;
        }


        private void SetUpDummyFolders()
        { /*
            DirectoryInfo directory1 = new DirectoryInfo(FOLDER1);
            DirectoryInfo directory2 = new DirectoryInfo(FOLDER2);

            if (!directory1.Exists)
                Directory.CreateDirectory(FOLDER1);
            if (!directory2.Exists)
                Directory.CreateDirectory(FOLDER2);
            
            //clear out old files
            foreach (System.IO.FileInfo file in directory1.GetFiles()) file.Delete();
            foreach (System.IO.FileInfo file in directory2.GetFiles()) file.Delete();

            int fileNumber = 1;
            for (var a = 0; a < 5; a++)
                File.Create(System.IO.Path.Combine(directory1.ToString(), "File_" + fileNumber++ + ".txt"));
            for (var a = 0; a < 5; a++)
                File.Create(System.IO.Path.Combine(directory2.ToString(), "File_" + fileNumber++ + ".txt"));
            File.Create(System.IO.Path.Combine(directory2.ToString(), "File_" + fileNumber++ + ".pete"));

            Path1 = directory1.FullName;
            Path2 = directory2.FullName;
        */}


        private void ReloadFileLists()
        {/*
            var di = new DirectoryInfo(FOLDER1);
            Path1Files = new ObservableCollection<FileInfo>(di.GetFiles());
            di = new DirectoryInfo(FOLDER2);
            Path2Files = new ObservableCollection<FileInfo>(di.GetFiles());
        */}


        void AddUpdate(object param)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                Log.Add(param as string);
            }));
        }


        public static void FindFileSystemReplicaChanges(string replicaRootPath, FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider provider = null;

            try
            {
                provider = new FileSyncProvider(replicaRootPath, filter, options);
                provider.DetectChanges();
            }
            finally
            {
                if (provider != null)
                    provider.Dispose();
            }
        }


        public static void OneWaySyncFileSystemReplicas(string sourceReplicaRootPath, string destinationReplicaRootPath, FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider path1Provider = null;
            FileSyncProvider path2Provider = null;

            try
            {
                path1Provider = new FileSyncProvider(sourceReplicaRootPath, filter, options);
                path2Provider = new FileSyncProvider(destinationReplicaRootPath, filter, options);

                path2Provider.SkippedChange += OnSkippedChange;
                path2Provider.AppliedChange += OnAppliedChange;

                SyncOrchestrator manager = new SyncOrchestrator();
                manager.LocalProvider = path1Provider;
                manager.RemoteProvider = path2Provider;
                manager.Direction = SyncDirectionOrder.Upload;
                manager.Synchronize();
            }
            finally
            {
                if (path1Provider != null)
                    path1Provider.Dispose();
                if (path2Provider != null)
                    path2Provider.Dispose();
            }
        }


        public static void OnAppliedChange(object sender, AppliedChangeEventArgs args)
        {
            switch (args.ChangeType)
            {
                case ChangeType.Create:
                    SharpToolsMVVMMediator.NotifyColleagues("update", "File created: " + args.NewFilePath);
                    break;
                case ChangeType.Delete:
                    SharpToolsMVVMMediator.NotifyColleagues("update", "Deleted File: " + args.OldFilePath);
                    break;
                case ChangeType.Update:
                    SharpToolsMVVMMediator.NotifyColleagues("update", "Overwrote file: " + args.OldFilePath);
                    break;
                case ChangeType.Rename:
                    SharpToolsMVVMMediator.NotifyColleagues("update", "Renamed file: " + args.OldFilePath + " to " + args.NewFilePath);
                    break;
            }
        }


        public static void OnSkippedChange(object sender, SkippedChangeEventArgs args)
        {
            SharpToolsMVVMMediator.NotifyColleagues("update", "Error! Skipped file: " + args.ChangeType.ToString().ToUpper() + " for "
                + (!string.IsNullOrEmpty(args.CurrentFilePath) ? args.CurrentFilePath : args.NewFilePath));

            if (args.Exception != null)
                SharpToolsMVVMMediator.NotifyColleagues("update", "Error: " + args.Exception.Message);
        }

        #endregion


        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewAllSyncPairs,
                    new SharpToolsMVVMRelayCommand(param => this.ShowAllSyncPairs())),

                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CreateNewSyncPair,
                    new SharpToolsMVVMRelayCommand(param => this.CreateNewSyncPair())),
                    
                //new CommandViewModel(
                 //   Strings.MainWindowViewModel_Command_EditSyncPair,
                //    new SharpToolsMVVMRelayCommand(param => this.EditSyncPair())                    )
            };
        }

        #endregion // Commands
        

        #region Workspaces

        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                    
                }
                return _workspaces;
            }
        }



        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }



        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.Dispose();
            this.Workspaces.Remove(workspace);
        }

        #endregion // Workspaces


        #region Public Methods

        private void GetTest(object param)
        {
            SharpTools.System.User.SystemUser sus = new SharpTools.System.User.SystemUser();
            //SharpTools.File.Access.SharpToolsFileAccess fas = new SharpTools.File.Access.SharpToolsFileAccess();

            System.Windows.Forms.MessageBox.Show("Token number is: " + sus.CurAccountToken.ToString());
            string nameWithoutDomain = sus.WII.Name.Split('\\').Last();
            System.Windows.Forms.MessageBox.Show(sus.WII.Name);
            System.Windows.Forms.MessageBox.Show((SharpTools.File.Access.SharpToolsFileAccess.checkDirectoryAuth("e:\\Dave", WindowsIdentity.GetCurrent().Name)).ToString());
            System.Windows.Forms.MessageBox.Show(nameWithoutDomain);
        }

        #endregion


        #region Private Helpers

        /// <summary>
        /// Adds a workspace for creating a Sync Pair
        /// </summary>
        void CreateNewSyncPair()
        {
            SyncPair newSyncPair = SyncPair.CreateNewSyncPair();
            SyncPairViewModel workspace = new SyncPairViewModel(newSyncPair, _syncPairRepository, this);
            this.Workspaces.Add(workspace);
            this.SetActiveWorkspace(workspace);
        }


        /// <summary>
        /// Adds all Sync Pair workspace.
        /// </summary>
        void ShowAllSyncPairs()
        {
            AllSyncPairsViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is AllSyncPairsViewModel)
                as AllSyncPairsViewModel;

            if (workspace == null)
            {
                workspace = new AllSyncPairsViewModel(_syncPairRepository, this);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }


     //   void DeleteSyncPair()
       // {
      //      SyncPair newSyncPair = SyncPair.CreateNewSyncPair();
//SyncPairViewModel workspace = new SyncPairViewModel(newSyncPair, _syncPairRepository, this);
       //     this.Workspaces.Remove(workspace);
      //  }

        /// <summary>
        /// Sets active workspace.
        /// </summary>
        /// <param name="workspace">Workspace to select</param>
        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }
                 

        #endregion // Private Helpers


        #region IWorkspaceCommands Members

        public void AddWorkspace(WorkspaceViewModel view)
        {
            if (!Workspaces.Contains(view))
            {
                Workspaces.Add(view);
            }
            SetActiveWorkspace(view);
        }

        public void RemoveWorkspace(WorkspaceViewModel view)
        {
            if (Workspaces.Contains(view))
            {
                Workspaces.Remove(view);
            }
        }

        #endregion

    }
}
