using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets/Sets the display name of the pair 
        /// 
        /// TODO Define the variable
        /// </summary>
        string SyncType { get; set; }

    }
}
