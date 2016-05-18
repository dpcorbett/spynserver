using SharpTools.MVVM.RelayCommand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFSpyn.DataAccess;
using WPFSpyn.Library;
using WPFSpyn.Properties;

namespace WPFSpyn.ViewModel
{
    /// <summary>
    /// Represents a container of ViewModel objects
    /// that has support for staying synchronized with the
    /// SyncPairRepository.  This class also provides information
    /// related to multiple selected SyncPairs.
    /// </summary>
    public class AllSyncPairsViewModel : WorkspaceViewModel
    {

        #region Fields

        readonly SyncPairRepository _syncPairRepository;

        #endregion // Fields


        #region Constructor

        IWorkspaceCommands _wsCommands;
        /// <summary>
        /// Constructor taking Sync Pair Repository.
        /// </summary>
        /// <param name="p_syncPairRepository"></param>
        public AllSyncPairsViewModel(SyncPairRepository p_syncPairRepository, IWorkspaceCommands wsCommands)
        {
            _wsCommands = wsCommands;
            if (p_syncPairRepository == null)
                throw new ArgumentNullException("syncPairRepository");

            base.DisplayName = Strings.AllSyncPairsViewModel_DisplayName;

            _syncPairRepository = p_syncPairRepository;

            // Subscribe for notifications of when a new SyncPair is saved.
            _syncPairRepository.SyncPairAdded += this.OnSyncPairAddedToRepository;
            // Subscribe for notifications of when a SyncPair is deleted
            _syncPairRepository.SyncPairRemoved += this.OnSyncPairRemovedFromRepository;
            // Populate the AllSyncPairs collection with SyncPairViewModels.
            this.CreateAllSyncPairs();
            EditSyncPairCommand = new SharpToolsMVVMRelayCommand(EditSyncPair);
            //DeleteSyncPairCommand = new SharpToolsMVVMRelayCommand(DeleteSyncPair);  

        }


        public void EditSyncPair(object syncpair)
        {
            SyncPairViewModel customerVM = syncpair as SyncPairViewModel;
            _wsCommands.AddWorkspace(customerVM);

        }


        /// <summary>
        /// Creates the list of Sync Pairs to display.
        /// </summary>
        void CreateAllSyncPairs()
        {
            List<SyncPairViewModel> all =
                (from syncpair in _syncPairRepository.GetSyncPairs()
                 select new SyncPairViewModel(syncpair, _syncPairRepository, _wsCommands)).ToList();

            foreach (SyncPairViewModel spvm in all)
                spvm.PropertyChanged += this.OnSyncPairViewModelPropertyChanged;

            this.AllSyncPairs = new ObservableCollection<SyncPairViewModel>(all);
            this.AllSyncPairs.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor


        #region Public Interface

        /// <summary>
        /// Returns a collection of all the SyncPairViewModel objects.
        /// </summary>
        public ObservableCollection<SyncPairViewModel> AllSyncPairs { get; private set; }

        /// <summary>
        /// Returns the display name of selected SyncPair.
        /// </summary>
        public string SyncPairSelected
        {
            get
            {
                IEnumerable<SyncPairViewModel> query = this.AllSyncPairs.Where(syncpair => syncpair.IsSelected == true);

                string strTmp = null;
                if (query.Count() > 0)
                    strTmp = query.ElementAt(0).Description;
                return strTmp;
            }
        }
        
        #endregion // Public Interface


        #region  Base Class Overrides

        /// <summary>
        /// Clean up.
        /// </summary>
        protected override void OnDispose()
        {
            foreach (SyncPairViewModel custVM in this.AllSyncPairs)
                custVM.Dispose();

            this.AllSyncPairs.Clear();
            this.AllSyncPairs.CollectionChanged -= this.OnCollectionChanged;
            _syncPairRepository.SyncPairAdded -= this.OnSyncPairAddedToRepository;
        }

        #endregion // Base Class Overrides


        #region Event Handling Methods

        /// <summary>
        /// Handles OnCollectionChanged Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (SyncPairViewModel synprVM in e.NewItems)
                    synprVM.PropertyChanged += this.OnSyncPairViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (SyncPairViewModel custVM in e.OldItems)
                    custVM.PropertyChanged -= this.OnSyncPairViewModelPropertyChanged;
        }


        /// <summary>
        /// Handles OnSyncPairViewModelPropertyChanged Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name referenced, is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as SyncPairViewModel).VerifyPropertyName(IsSelected);

            // When a SyncPair is selected or unselected, we must let the
            // world know that the TotalSelectedSales property has changed,
            // so that it will be queried again for a new value.
            if (e.PropertyName == IsSelected)
                this.OnPropertyChanged("SyncPairSelected");
        }


        /// <summary>
        /// Handles OnSyncPairAddedToRepository Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairAddedToRepository(object sender, SyncPairAddedEventArgs e)
        {
            var viewModel = new SyncPairViewModel(e.NewSyncPair, _syncPairRepository, _wsCommands);
            this.AllSyncPairs.Add(viewModel);
        }


        /// <summary>
        /// Handles OnSyncPairRemovedToRepository Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairRemovedFromRepository(object sender, SyncPairRemovedEventArgs e)
        { 
            foreach (var vwww in this.AllSyncPairs)
            {
                if (vwww.Description == e.OldSyncPair.Description)
                {
                    if (this.AllSyncPairs.Contains(vwww))
                    {
                        this.AllSyncPairs.Remove(vwww);
                        break;
                    }
                }
            }

            //var viewModel = new SyncPairViewModel(e.OldSyncPair, _syncPairRepository, _wsCommands);
            //if (this.AllSyncPairs.Contains(viewModel))
            //{
            //    this.AllSyncPairs.Remove(viewModel);
            //}

            // TODO Refresh AllSyncPairs list after delete this.AllSyncPairs.
            
        }


        private SharpToolsMVVMRelayCommand _editSyncPairCommand;

        public SharpToolsMVVMRelayCommand EditSyncPairCommand
        {
            get { return _editSyncPairCommand; }
            set
            {
                if (_editSyncPairCommand != value)
                {
                    _editSyncPairCommand = value;
                    base.OnPropertyChanged("EditSyncPairCommand");
                }
            }
        }

        #endregion // Event Handling Methods

    }
}
    