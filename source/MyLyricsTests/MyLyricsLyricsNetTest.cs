using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyricsNetTest
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
        public void TestLyricsNet()
        {
            var site = new LyricsNet("Van Halen", "Ice Cream Man", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Now", splitLyrics[0]);
            Assert.AreEqual("satis-uh-fy", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsNet2()
        {
            var site = new LyricsNet("Eric Clapton", "Baby What's wrong", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Oh", splitLyrics[0]);
            Assert.AreEqual("verse", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsNet3()
        {
            var site = new LyricsNet("Barry Manilow", "I Write The Songs", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I", splitLyrics[0]);
            Assert.AreEqual("worldwide", splitLyrics[splitLyrics.Length - 3]);
        }

        [TestMethod]
        public void TestLyricsNetNotFound()
        {
            var site = new LyricsNet("Foo", "Bar", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Not", splitLyrics[0]);
            Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
        }
    }
}
