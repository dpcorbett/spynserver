using NUnit.Framework;
using WPFSpyn.Model;

namespace SpynTest
{
    [TestFixture]
    class SpynTest
    {
        #region SyncPair

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

        #endregion
    }
}
