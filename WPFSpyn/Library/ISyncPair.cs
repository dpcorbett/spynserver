namespace WPFSpyn.Library
{
    public interface ISyncPair
    {
        /// <summary>
        /// Gets/Sets the display name of the pair
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets/Sets public status of SyncPair
        /// </summary>
        bool IsFullSync { get; set; }

        /// <summary>
        /// Gets/Sets the type of the sync 
        /// 
        /// TODO Define the type
        /// </summary>
        string SyncType { get; set; }

        /// <summary>
        /// Gets/sets the root directory of data source.
        /// </summary>
        string SrcRoot { get; set; }

        /// <summary>
        /// Gets/sets the root directory of data destination.
        /// </summary>
        string DstRoot { get; set; }

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        bool IsValid { get; }
    }
}
