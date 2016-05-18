using System;
using WPFSpyn.Model;

namespace WPFSpyn.DataAccess
{
    /// <summary>
    /// Event arguments used by SyncPairRepository's SyncPairAdded event.
    /// </summary>
    public class SyncPairAddedEventArgs : EventArgs
    {
        public SyncPairAddedEventArgs(SyncPair newSyncPair)
        {
            this.NewSyncPair = newSyncPair;
        }

        public SyncPair NewSyncPair { get; private set; }
    }
}