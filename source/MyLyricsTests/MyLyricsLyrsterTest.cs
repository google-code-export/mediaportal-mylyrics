using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyrsterTest
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
        public void TestLyrster()
        {
            var site = new Lyrster("Van Halen", "Ice Cream Man", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Dedicate", splitLyrics[4].Trim());
            Assert.AreEqual("satis-uh-fy", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyrster2()
        {
            var site = new Lyrster("Eric Clapton", "I Shot the Sheriff", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I", splitLyrics[0]);
            Assert.AreEqual("deputy", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyrster3()
        {
            var site = new Lyrster("Barry Manilow", "I Write The Songs", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I've", splitLyrics[0]);
            Assert.AreEqual("cry", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyrsterNotFound()
        {
            var site = new Lyrster("Foo", "Bar", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Not", splitLyrics[0]);
            Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
        }
    }
}
