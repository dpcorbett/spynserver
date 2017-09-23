using System;
using System.Collections.Generic;
using WPFSpyn.DataAccess;
using WPFSpyn.Model;

namespace WPFSpyn.Library
{
    public interface ISyncPairRepository
    {
        List<SyncPair> SyncPairs { get;}

    }
}
