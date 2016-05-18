//using Microsoft.Synchronization;
//using Microsoft.Synchronization.Files;
using SharpTools.MVVM.RelayCommand;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        readonly SyncPair _syncPair;
        readonly SyncPairRepository _syncPairRepository;
        private ObservableCollection<string> m_obcLog;
        private string m_strSrcPath;
        private string m_strDstPath;
        private bool m_booIsSynchronising;
        string _syncPairType;
        string[] _syncPairTypeOptions;
        bool _isSelected;
        SharpToolsMVVMRelayCommand _saveCommand;
        SharpToolsMVVMRelayCommand _deleteCommand;


        #endregion // Fields


        private SharpToolsMVVMRelayCommand _deleteSyncPairCommand;

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

        IWorkspaceCommands _wsCommands;


        #region Properties

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

        }

        #endregion // Constructor


       // public void DeleteSyncPair(object syncpair)
  //      {
            //SyncPairViewModel customerVM = syncpair as SyncPairViewModel;
            //SyncPairViewModel customerVM = new  SyncPairViewModel(_syncPair,_syncPairRepository, _wsCommands);

         //   _wsCommands.RemoveWorkspace(this);

//        }


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
            //_syncPair.SyncType = 
            if (!_syncPair.IsValid)
                throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);

            if (this.IsNewSyncPair)
                _syncPairRepository.AddSyncPair(_syncPair);
            
            base.OnPropertyChanged("DisplayName");
            SyncPairRepository.SaveSyncPairs(this._syncPairRepository, MainWindow.DATA_PATH);
        }


        /// <summary>
        /// Deletes the Syncpair from the repository.  This method is invoked by the DeleteCommand.
        /// </summary>
        public void Delete(object syncpair)
        {
            if (!_syncPair.IsValid)
            {
                //throw new InvalidOperationException(Strings.SyncPairViewModel_Exception_CannotSave);
                MessageBox.Show("Not Saved");
                return;
            }

            if (!this.IsNewSyncPair)
            {
                _syncPairRepository.DeleteSyncPair(_syncPair);
                _wsCommands.RemoveWorkspace(this);
            }
            base.OnPropertyChanged("DisplayName");
            SyncPairRepository.SaveSyncPairs(this._syncPairRepository, MainWindow.DATA_PATH);
            base.Dispose();
        }


        private void GetSrcRoot(object param)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            SrcRoot = dialog.SelectedPath;
        }


        private void GetDstRoot(object param)
        {
            var dialog = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            DstRoot = dialog.SelectedPath;
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

    }
}
