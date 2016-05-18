//using ClassLibrarySharpTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using WPFSpyn.Model;


namespace WPFSpyn.DataAccess
{
    /// <summary>
    /// Represents a source of syncpairs in the application.
    /// </summary>
     [Serializable()]
    public class SyncPairRepository
    {

        #region Fields

        readonly List<SyncPair> _syncPairs;
        
        public List<SyncPair> syncPairs
        {
            get
            {
                return _syncPairs;
            }
        }

        #endregion // Fields


        #region Constructor

        /// <summary>
        /// Creates a repository with an empty SyncPair list.
        /// </summary>
        public SyncPairRepository()
        {
            _syncPairs = new List<SyncPair>();
        }


        /// <summary>
        /// Creates a new repository of syncpairs.
        /// </summary>
        /// <param name="syncPairDataFile">The relative path to an XML resource file that contains syncpair data.</param>
        public SyncPairRepository(string syncPairDataFile)
        {
            _syncPairs = LoadSyncPairs(syncPairDataFile);
        }

        #endregion // Constructor


        #region Destructor

      //   ~SyncPairRepository()
       // {
        //    SaveSyncPairs(  this._syncPairs);
       // }

        #endregion


        #region Public Interface

        /// <summary>
        /// Raised when a syncpair is placed into the repository.
        /// </summary>
        public event EventHandler<SyncPairAddedEventArgs> SyncPairAdded;

        /// <summary>
        /// Raised when a syncpair is removed into the repository.
        /// </summary>
        public event EventHandler<SyncPairRemovedEventArgs> SyncPairRemoved;


        /// <summary>
        /// Places the specified syncpair into the repository.
        /// If the syncpair is already in the repository, an
        /// exception is not thrown.
        /// </summary>
        /// <param name="p_syncPair"></param>
        public void AddSyncPair(SyncPair p_syncPair)
        {
            if (p_syncPair == null)
                throw new ArgumentNullException("syncpair");

            if (!_syncPairs.Contains(p_syncPair))
            {
                _syncPairs.Add(p_syncPair);

                if (this.SyncPairAdded != null)
                    this.SyncPairAdded(this, new SyncPairAddedEventArgs(p_syncPair));
            }
        }


        public void DeleteSyncPair(SyncPair p_syncPair)
        {
            if (p_syncPair == null)
                throw new ArgumentNullException("syncpair");

            if (_syncPairs.Contains(p_syncPair))
            {
                _syncPairs.Remove(p_syncPair);

                if (this.SyncPairRemoved != null)
                    this.SyncPairRemoved(this, new SyncPairRemovedEventArgs(p_syncPair));
            }
        }


        /// <summary>
        /// Returns true if the specified syncpair exists in the
        /// repository, or false if it is not.
        /// </summary>
        /// <param name="syncpair"></param>
        /// <returns></returns>
        public bool ContainsSyncPair(SyncPair syncpair)
        {
            if (syncpair == null)
                throw new ArgumentNullException("syncpair");

            return _syncPairs.Contains(syncpair);
        }


        /// <summary>
        /// Returns a shallow-copied list of all syncpairs in the repository.
        /// </summary>
        /// <returns></returns>
        public List<SyncPair> GetSyncPairs()
        {
            return new List<SyncPair>(_syncPairs);
        }

        #endregion // Public Interface


        #region Private Helpers

         /// <summary>
         /// Returns a list of SyncPairs retrieved from file.
         /// </summary>
         /// <param name="p_syncPairDataFile"></param>
         /// <returns></returns>
        static List<SyncPair> LoadSyncPairs(string p_syncPairDataFile)
        {
            // TODO Handle no file present
            using (XmlReader xmlRdr = new XmlTextReader(p_syncPairDataFile))
            {

                XmlSerializer x = new XmlSerializer(typeof(SyncPairRepository));
                SyncPairRepository myRepo = (SyncPairRepository)x.Deserialize(xmlRdr);
                return myRepo._syncPairs;
            }
        }


         /// <summary>
         /// Returns true if writing SyncPairRepository is successful.
         /// </summary>
         /// <param name="syncPairRep"></param>
         /// <param name="p_syncPairDataFile"></param>
         /// <returns></returns>
        public static bool SaveSyncPairs(SyncPairRepository syncPairRep, string p_syncPairDataFile)
        {
            bool saveSuccessful = false;

            try
            {
                FileStream fstTargetFile = File.Create(p_syncPairDataFile);
                XmlSerializer xsrOutput = new XmlSerializer(syncPairRep.GetType());
                xsrOutput.Serialize(fstTargetFile, syncPairRep);
                fstTargetFile.Close();
                saveSuccessful = true;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }

            return saveSuccessful;
        }

        #endregion // Private Helpers
    }
}