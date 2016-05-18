using System;
using WPFSpyn.Model;

namespace WPFSpyn.DataAccess
{
    /// <summary>
    /// Event arguments used by SyncPairRepository's SyncPairDeleted event.
    /// </summary>
    public class SyncPairRemovedEventArgs : EventArgs
    {
        public SyncPairRemovedEventArgs(SyncPair oldSyncPair)
        {
            this.OldSyncPair = oldSyncPair;
        }

        public SyncPair OldSyncPair { get; private set; }

    }
}
