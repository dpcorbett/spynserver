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
using System.Linq;
using System.Security.Principal;
using System.Windows.Data;
using System.Windows.Threading;
using WPFSpyn.DataAccess;
using WPFSpyn.Model;
using WPFSpyn.Library;
using WPFSpyn.Properties;

namespace WPFSpyn.ViewModel
{
    /// <summary>
    /// Parent window for program.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel, IWorkspaceCommands
    {
        
        #region Fields

        // Create command collection.
        ReadOnlyCollection<CommandViewModel> _commands;
        // Create sync pair collection.
        readonly SyncPairRepository _syncPairRepository;
        // Create worksapce collection.
        ObservableCollection<WorkspaceViewModel> _workspaces;

        #endregion


        #region Properties

        // Retrieve program path from system.
        public static string APP_PATH = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        // Add data file location to app path.
        public static string DATA_PATH = APP_PATH += "\\Data\\syncpairs.xml";

        /// <summary>
        /// Test command for Settings button.
        /// </summary>
        public SharpToolsMVVMRelayCommand GetTestCommand { get; set; }
             
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
                // Check list doesn't already exist.
                if (_commands == null)
                {
                    // Create command list.
                    List<CommandViewModel> cmds = CreateCommands();
                    // Set field.
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        } 

        /// <summary>
        /// Creates command view models specified.
        /// </summary>
        /// <returns>list of command view models</returns>
        List<CommandViewModel> CreateCommands()
        {
            // Create and return list.
            return new List<CommandViewModel>
            {
                // All sync pairs view model.
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_ViewAllSyncPairs,
                    new SharpToolsMVVMRelayCommand(param => ShowAllSyncPairs())),

                // New sync pair view model.
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_CreateNewSyncPair,
                    new SharpToolsMVVMRelayCommand(param => CreateNewSyncPair())),
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
                // Check if list exists.
                if (_workspaces == null)
                {
                    // Create list.
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    // Attach worspace changed event.
                    _workspaces.CollectionChanged += OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        /// <summary>
        /// Handles workspace changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Check for new workspaces.
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    // Attach workspace close request event to each one.
                    workspace.RequestClose += OnWorkspaceRequestClose;

            // Check for new workspaces.
            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    // Attach workspace close request event to each one.
                    workspace.RequestClose -= OnWorkspaceRequestClose;
        }

        /// <summary>
        /// Handles workspace request close event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            // Store workspace view model address locally.
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            // Destroy workspace.
            workspace.Dispose();
            // Remove reference to workspace.
            Workspaces.Remove(workspace);
        }

        #endregion // Workspaces


        #region Public Methods

        /// <summary>
        /// USED FOR TESTING ONLY.
        /// </summary>
        /// <param name="param"></param>
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
            // Create sync pair.
            SyncPair newSyncPair = SyncPair.CreateNewSyncPair();
            // Create new sync pair model passing sync pair, repository and itself??? TODO Fix this.
            SyncPairViewModel workspace = new SyncPairViewModel(newSyncPair, _syncPairRepository, this);
            // Add sync pair view model to workspaces.
            Workspaces.Add(workspace);
            // Give workspace focus.
            SetActiveWorkspace(workspace);
        }

        /// <summary>
        /// Adds all Sync Pair workspaces.
        /// </summary>
        void ShowAllSyncPairs()
        {
            // Select workspace if it is already open.
            AllSyncPairsViewModel workspace =
                Workspaces.FirstOrDefault(vm => vm is AllSyncPairsViewModel)
                as AllSyncPairsViewModel;

            // Create and add an all sync pairs view model.
            if (workspace == null)
            {
                workspace = new AllSyncPairsViewModel(_syncPairRepository, this);
                Workspaces.Add(workspace);
            }
            // Give workspace focus.
            SetActiveWorkspace(workspace);
        }

        /// <summary>
        /// Sets active workspace.
        /// </summary>
        /// <param name="workspace">Workspace to select</param>
        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            // Ascertain existance for debugging.
            Debug.Assert(Workspaces.Contains(workspace));

            // Create collection view of workspaces.
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(Workspaces);

            // Set supplied workspace as active.
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }
                 

        #endregion // Private Helpers


        #region IWorkspaceCommands Members

        /// <summary>
        /// Add supplied workspace view model if it doesn't exist,
        /// and set active.
        /// </summary>
        /// <param name="view"></param>
        public void AddWorkspace(WorkspaceViewModel view)
        {
            // Check that workspace doesn't exist.
            if (!Workspaces.Contains(view))
            {
                Workspaces.Add(view);
            }
            SetActiveWorkspace(view);
        }

        /// <summary>
        /// Delete supplied workspace view model if it exists.
        /// </summary>
        /// <param name="view"></param>
        public void RemoveWorkspace(WorkspaceViewModel view)
        {
            // Check that workspace exists.
            if (Workspaces.Contains(view))
            {
                Workspaces.Remove(view);
            }
        }

        #endregion

    }
}
