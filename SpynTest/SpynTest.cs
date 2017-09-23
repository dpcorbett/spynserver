using NUnit.Framework;
using WPFSpyn.Model;

namespace SpynTest
{
    [TestFixture]
    class SpynTest
    {
        #region SyncPair

        [Test]
        public void ReadDirectory() => Assert.That(new SyncPair
        {
            Description = "test"
        }.Description, Is.EqualTo("test"));

        [Test]
        public void ReadIsFullSync() => Assert.That(new SyncPair
        {
            IsFullSync = true 
        }.IsFullSync, Is.EqualTo(true));

        #endregion
    }
}
