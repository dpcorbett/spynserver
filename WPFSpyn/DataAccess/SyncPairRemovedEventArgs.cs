using System;
using WPFSpyn.Library;
using WPFSpyn.Model;

namespace WPFSpyn.DataAccess
{
    /// <summary>
    /// Event arguments used by SyncPairRepository's SyncPairDeleted event.
    /// </summary>
    public class SyncPairRemovedEventArgs : EventArgs
    {
        public SyncPairRemovedEventArgs(ISyncPair p_oldSyncPair) => OldSyncPair = (SyncPair)p_oldSyncPair;

        public SyncPair OldSyncPair { get; private set; }

    }
}
