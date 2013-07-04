using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyrDbTest
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        [TestInitialize]
        public void SetUp()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        [TestCleanup]
        public void TearDown()
        {
            _stopwatch.Stop();
            Debug.WriteLine("Test duration: " + _stopwatch.Elapsed);
        }

        [TestMethod]
        public void TestLyrDb()
        {
            var withOrWithoutYou = new LyrDb("U2", "With Or Without You", new ManualResetEvent(false), 30000);
            withOrWithoutYou.FindLyrics();
            var splitMam = withOrWithoutYou.Lyric.Split(' ');
            Assert.AreEqual("See", splitMam[0]);
            Assert.AreEqual("you", splitMam[splitMam.Length - 1]);
        }

        [TestMethod]
        public void TestLyrDbNotFound()
        {
            var notFound = new LyrDb("Foo", "Bar", new ManualResetEvent(false), 30000);
            notFound.FindLyrics();
            var splitNf = notFound.Lyric.Split(' ');
            Assert.AreEqual("Not", splitNf[0]);
            Assert.AreEqual("found", splitNf[splitNf.Length - 1]);
        }
    }
}