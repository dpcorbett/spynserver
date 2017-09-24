using System;
// dpc
using WPFSpyn.Library;
using WPFSpyn.Model;

namespace WPFSpyn.DataAccess
{
    /// <summary>
    /// Event arguments used by SyncPairRepository's SyncPairAdded event.
    /// </summary>
    public class SyncPairAddedEventArgs : EventArgs
    {
        public SyncPairAddedEventArgs(ISyncPair p_newSyncPair) => NewSyncPair = (SyncPair)p_newSyncPair;

        public SyncPair NewSyncPair { get; private set; }
    }
}