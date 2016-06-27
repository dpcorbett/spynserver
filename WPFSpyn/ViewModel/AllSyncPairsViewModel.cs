using SharpTools.MVVM.RelayCommand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using WPFSpyn.DataAccess;
using WPFSpyn.Library;
using WPFSpyn.Properties;

namespace WPFSpyn.ViewModel
{
    /// <summary>
    /// Represents a container of SyncPairViewModel objects
    /// that has support for staying synchronized with the
    /// SyncPairRepository.
    /// </summary>
    public class AllSyncPairsViewModel : WorkspaceViewModel
    {

        #region Fields

        // Stores sync pairs.
        readonly SyncPairRepository _syncPairRepository;
        // Stores commands.
        private IWorkspaceCommands _wsCommands;
        // Create command to edit sync pair.
        private SharpToolsMVVMRelayCommand _editSyncPairCommand;

        #endregion // Fields


        #region Constructors

        /// <summary>
        /// Constructor taking sync pair repository, and workspace commands.
        /// </summary>
        /// <param name="p_syncPairRepository"></param>
        /// <param name="wsCommands"></param>
        public AllSyncPairsViewModel(SyncPairRepository p_syncPairRepository, IWorkspaceCommands wsCommands)
        {
            // Store commands locally.
            _wsCommands = wsCommands;

            // Throw exception if parameter supplied is null
            if (p_syncPairRepository == null)
                throw new ArgumentNullException("syncPairRepository");

            // Set name displayed on tab
            base.DisplayName = Strings.AllSyncPairsViewModel_DisplayName;

            // Store sync pair repository locally.
            _syncPairRepository = p_syncPairRepository;

            // Subscribe for notifications of when a new SyncPair is saved.
            _syncPairRepository.SyncPairAdded += OnSyncPairAddedToRepository;

            // Subscribe for notifications of when a SyncPair is deleted
            _syncPairRepository.SyncPairRemoved += OnSyncPairRemovedFromRepository;

            // Populate the AllSyncPairs collection with SyncPairViewModels.
            CreateAllSyncPairs();

            // Attach relay command.
            EditSyncPairCommand = new SharpToolsMVVMRelayCommand(EditSyncPair);

        }

        /// <summary>
        /// Open workspace to edit sync pair provided.
        /// </summary>
        /// <param name="syncpair"></param>
        public void EditSyncPair(object syncpair)
        {
            // Create sync pair view model.
            SyncPairViewModel syncpairVM = syncpair as SyncPairViewModel;

            // Use commands supplied by parent to add new workspace.
            _wsCommands.AddWorkspace(syncpairVM);

        }

        /// <summary>
        /// Creates the list of Sync Pairs to display.
        /// </summary>
        void CreateAllSyncPairs()
        {
            // For each sync pair found in repository, create sync pair view model and add to list
            List<SyncPairViewModel> syncPairList =
                (from syncpair in _syncPairRepository.GetSyncPairs()
                 select new SyncPairViewModel(syncpair, _syncPairRepository, _wsCommands)).ToList();

            // Attach event for property changed.
            foreach (SyncPairViewModel spvm in syncPairList)
                spvm.PropertyChanged += OnSyncPairViewModelPropertyChanged;

            // Create an observable collection out of sync pair view model list.
            AllSyncPairs = new ObservableCollection<SyncPairViewModel>(syncPairList);

            // Raise event to notify that collection has changed.
            AllSyncPairs.CollectionChanged += OnCollectionChanged;
        }

        #endregion // Constructors


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
                // Check if any sync pairs are selected.
                IEnumerable<SyncPairViewModel> query = AllSyncPairs.Where(syncpair => syncpair.IsSelected == true);

                // Set response to null as default.
                string strTmp = null;

                // If any syncpairs where selected, then return description of first one.
                if (query.Count() > 0)
                    strTmp = query.ElementAt(0).Description;

                // Return response.
                return strTmp;
            }
        }

        /// <summary>
        ///  Create edit sync pair relay command.
        /// </summary>
        public SharpToolsMVVMRelayCommand EditSyncPairCommand
        {
            get { return _editSyncPairCommand; }
            set
            {
                // Reset value if not current, and raise proprety changed event..
                if (_editSyncPairCommand != value)
                {
                    _editSyncPairCommand = value;
                    base.OnPropertyChanged("EditSyncPairCommand");
                }
            }
        }
        
        #endregion // Public Interface


        #region  Base Class Overrides

        /// <summary>
        /// Clean up.
        /// </summary>
        protected override void OnDispose()
        {
            // Destroy any sync pair view models.
            foreach (SyncPairViewModel custVM in AllSyncPairs)
                custVM.Dispose();

            // Remove sync pairs.
            AllSyncPairs.Clear();

            // Raise event to notify that collection has changed.
            AllSyncPairs.CollectionChanged -= OnCollectionChanged;

            // Detach event.
            _syncPairRepository.SyncPairAdded -= OnSyncPairAddedToRepository;
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
            // Attach property changed event to new view models.
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (SyncPairViewModel synprVM in e.NewItems)
                    synprVM.PropertyChanged += OnSyncPairViewModelPropertyChanged;

            // Detach property changed event from old view models.
            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (SyncPairViewModel custVM in e.OldItems)
                    custVM.PropertyChanged -= OnSyncPairViewModelPropertyChanged;
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
                OnPropertyChanged("SyncPairSelected");
        }


        /// <summary>
        /// Handles OnSyncPairAddedToRepository Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairAddedToRepository(object sender, SyncPairAddedEventArgs e)
        {
            // Create new sync pair view model with supplied args.
            var viewModel = new SyncPairViewModel(e.NewSyncPair, _syncPairRepository, _wsCommands);

            // Add view model to observable collection.
            AllSyncPairs.Add(viewModel);
        }


        /// <summary>
        /// Handles OnSyncPairRemovedToRepository Event
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Event Arguments</param>
        void OnSyncPairRemovedFromRepository(object sender, SyncPairRemovedEventArgs e)
        { 
            // Find sync pair view model by matching description.
            foreach (var syncPairViewModel in AllSyncPairs)
            {
                if (syncPairViewModel.Description == e.OldSyncPair.Description)
                {
                    // If it is the same view model, remove it from observable collection.
                    if (AllSyncPairs.Contains(syncPairViewModel))
                    {
                        AllSyncPairs.Remove(syncPairViewModel);
                        break;
                    }
                }
            }
        }

        #endregion // Event Handling Methods

    }
}
    