using System.Collections.Generic;
// dpc
using WPFSpyn.Model;

namespace WPFSpyn.Library
{
    public interface ISyncPairRepository
    {
        /// <summary>
        /// Collection of Sync Pairs.
        /// </summary>
        List<SyncPair> SyncPairs { get; }

        /// <summary>
        /// Places the specified syncpair into the repository.
        /// If the syncpair is already in the repository, an
        /// event is not triggered.
        /// </summary>
        /// <param name="p_syncPair"></param>
        void AddSyncPair(SyncPair p_syncPair);

        /// <summary>
        /// Removes the syncpair from the repository and
        /// if successful, raise an event.
        /// </summary>
        /// <param name="p_syncPair"></param>
        void DeleteSyncPair(SyncPair p_syncPair);

        /// <summary>
        /// Returns true if the specified syncpair exists in the
        /// repository, or false if it is not.
        /// </summary>
        /// <param name="syncpair"></param>
        /// <returns></returns>
        bool ContainsSyncPair(SyncPair syncpair);

        /// <summary>
        /// Returns a shallow-copied list of all syncpairs in the repository.
        /// </summary>
        /// <returns>List<SyncPair></returns>
        List<SyncPair> GetSyncPairs();
    }
}