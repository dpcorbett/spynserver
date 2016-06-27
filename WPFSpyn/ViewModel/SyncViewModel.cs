using SharpTools.MVVM.RelayCommand;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using WPFSpyn.DataAccess;
using WPFSpyn.Library;
using WPFSpyn.Model;

namespace WPFSpyn.ViewModel
{
    class SyncViewModel : WorkspaceViewModel
    {

        #region Fields

        // Create sync pair.
        private readonly SyncPair _syncPair;
        // Create sync pair repository.
        private readonly SyncPairRepository _syncPairRepository;
        // Create workspace commands.
        private IWorkspaceCommands _wsCommands;
        // Create log for actions.
        private ObservableCollection<string> m_obcLog;
        // Create collection for source.
        private ObservableCollection<FileInfo> m_obcSrcFiles;
        // Create collection for destination.
        private ObservableCollection<FileInfo> m_obcDstFiles;

        #endregion

        #region Constructors

        public SyncViewModel()
        {
            //SyncCommand = new SharpToolsMVVMRelayCommand(TrySync);
        }

        // TODO Needs to setup for display
        public SyncViewModel(SyncPair p_syncPair, SyncPairRepository p_syncPairRepository, IWorkspaceCommands wsCommands)
        {
            _wsCommands = wsCommands;

            if (p_syncPair == null)
                throw new ArgumentNullException("p_syncPair");

            if (p_syncPairRepository == null)
                throw new ArgumentNullException("p_syncPairRepository");

            _syncPair = p_syncPair;
            _syncPairRepository = p_syncPairRepository;
        }

        #endregion

        #region Properties

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
                return m_obcSrcFiles;
            }
            set
            {
                if (m_obcSrcFiles != value)
                {
                    m_obcSrcFiles = value;
                    OnPropertyChanged("Path1Files");
                }
            }
        }

        public ObservableCollection<FileInfo> Path2Files
        {
            get
            {
                return m_obcDstFiles;
            }
            set
            {
                if (m_obcDstFiles != value)
                {
                    m_obcDstFiles = value;
                    OnPropertyChanged("Path2Files");
                }
            }
        }



        public SharpToolsMVVMRelayCommand SyncCommand { get; set; }

        #endregion



        public override string DisplayName
        {
            get
            {
                    return _syncPair.Description;
            }
        }

        /// <summary>
        /// Checks source and destination directories existence for pairing
        /// Created: 12-Mar-2016
        /// Moved: 10-Jun-2016
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


        /*
                private void TrySync(object param)
                {
                    if (string.IsNullOrEmpty(SrcPath) || string.IsNullOrEmpty(m_strDstPath) || !Directory.Exists(m_strSrcPath) || !Directory.Exists(DstPath))
                    {
                        System.Windows.MessageBox.Show("Please supply two valid file paths");
                        return;
                    }

                    IsSynchronising = true;
                    // Put sync on background thread
                    Task.Factory.StartNew(() => { DoSync(); }).ContinueWith(_ => { IsSynchronising = false; });

                }


                public void DoSync()
                {
                    try
                    {
                        Log = new ObservableCollection<string>(); //reset log

                        FileSyncOptions options = FileSyncOptions.ExplicitDetectChanges
                            | FileSyncOptions.RecycleConflictLoserFiles
                            | FileSyncOptions.RecycleDeletedFiles
                            | FileSyncOptions.RecyclePreviousFileOnUpdates;

                        FileSyncScopeFilter filter = new FileSyncScopeFilter();
                        filter.FileNameExcludes.Add("*.pete");

                        // Avoid two change detection passes for the two-way sync
                        FindFileSystemReplicaChanges(Path1, filter, options);
                        FindFileSystemReplicaChanges(Path2, filter, options);

                        // Sync both ways
                        OneWaySyncFileSystemReplicas(Path1, Path2, null, options);
                        OneWaySyncFileSystemReplicas(Path2, Path1, null, options);

                        //           ReloadFileLists();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }
                }
                */

        void AddUpdate(object param)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                Log.Add(param as string);
            }));
        }

    }
}
