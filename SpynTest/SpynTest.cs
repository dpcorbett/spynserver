using NUnit.Framework;
using System;
using WPFSpyn.DataAccess;
using WPFSpyn.Library;
using WPFSpyn.Model;

namespace SpynTest
{
    [TestFixture]
    class SyncPairTest
    {
        [Test]
        public void ReadDescription() => Assert.That(new SyncPair
        {
            Description = "test"
        }.Description, Is.EqualTo("test"));

        [Test]
        public void ReadIsFullSync() => Assert.That(new SyncPair
        {
            IsFullSync = true
        }.IsFullSync, Is.EqualTo(true));

        [Test]
        public void ReadSyncType() => Assert.That(new SyncPair
        {
            SyncType = "Full"
        }.SyncType, Is.EqualTo("Full"));

        [Test]
        public void ReadSourceRoot() => Assert.That(new SyncPair
        {
            SrcRoot = "c:\\src"
        }.SrcRoot, Is.EqualTo("c:\\src"));

        [Test]
        public void ReadDestinationRoot() => Assert.That(new SyncPair
        {
            DstRoot = "c:\\dst"
        }.DstRoot, Is.EqualTo("c:\\dst"));
    }

    [TestFixture]
    class SyncPairRepositoryTest
    { 
        [Test]
        public void TestNullAddSyncPair() 
        {
            ISyncPairRepository spr = new SyncPairRepository();
            var ex = Assert.Throws<ArgumentNullException>(() => spr.AddSyncPair(null));
        }

        [Test]
        public void TestAddSyncPair()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            int firstCount = spr.SyncPairs.Count;
            spr.AddSyncPair(new SyncPair());
            Assert.That(spr.SyncPairs.Count, Is.EqualTo(firstCount+1));
        }

        [Test]
        public void TestNullDeleteSyncPair()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            var ex = Assert.Throws<ArgumentNullException>(() => spr.DeleteSyncPair(null));
        }

        [Test]
        public void TestDeleteSyncPair()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            ISyncPair sp = new SyncPair();
            spr.AddSyncPair((SyncPair)sp);
            int firstCount = spr.SyncPairs.Count;
            spr.DeleteSyncPair((SyncPair)sp);
            Assert.That(spr.SyncPairs.Count, Is.EqualTo(firstCount - 1));
        }
        [Test]
        public void TestNullContainsSyncPair()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            var ex = Assert.Throws<ArgumentNullException>(() => spr.ContainsSyncPair(null));
        }

        [Test]
        public void TestContainsSyncPair()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            ISyncPair sp = new SyncPair();
            spr.AddSyncPair((SyncPair)sp);
            Assert.That(spr.ContainsSyncPair((SyncPair)sp), Is.EqualTo(true));
        }

        [Test]
        public void TestGetSyncPairs()
        {
            ISyncPairRepository spr = new SyncPairRepository();
            ISyncPair sp = new SyncPair();
            spr.AddSyncPair((SyncPair)sp);
            Assert.That(spr.GetSyncPairs(), Is.EqualTo(spr.SyncPairs));
        }
    }
}
