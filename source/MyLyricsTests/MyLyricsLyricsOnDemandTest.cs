using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyricsOnDemandTest
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
        public void TestLyricsOnDemand()
        {
            var site = new LyricsOnDemand("Van Halen", "I Can't Stop Lovin' You", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("There's", splitLyrics[0]);
            Assert.AreEqual("YOU", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsOnDemand2()
        {
            var site = new LyricsOnDemand("Eric Clapton", "I Shot the Sheriff", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I", splitLyrics[0]);
            Assert.AreEqual("no.", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsOnDemand3()
        {
            var site = new LyricsOnDemand("Barry Manilow", "I Write The Songs", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I've", splitLyrics[0]);
            Assert.AreEqual("songs", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsOnDemandNotFound()
        {
            var site = new LyricsOnDemand("Foo", "Bar", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Not", splitLyrics[0]);
            Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
        }
    }
}
