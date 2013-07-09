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
        public void TestLyrsterLyrics()
        {
            var vanHalenCantStopLovinYou = new Lyrster("Van Halen", "Ice Cream Man", new ManualResetEvent(false), 10000);
            vanHalenCantStopLovinYou.FindLyrics();
            var splitVhcslu = vanHalenCantStopLovinYou.Lyric.Split(' ');
            Assert.AreEqual("Dedicate", splitVhcslu[4].Trim());
            Assert.AreEqual("satis-uh-fy", splitVhcslu[splitVhcslu.Length - 1]);
        }

        [TestMethod]
        public void TestLyrsterLyrics2()
        {
            var ericClaptonIShotTheSheriff = new Lyrster("Eric Clapton", "I Shot the Sheriff", new ManualResetEvent(false), 10000);
            ericClaptonIShotTheSheriff.FindLyrics();
            var splitEcists = ericClaptonIShotTheSheriff.Lyric.Split(' ');
            Assert.AreEqual("I", splitEcists[0]);
            Assert.AreEqual("deputy", splitEcists[splitEcists.Length - 1]);
        }

        [TestMethod]
        public void TestLyrsterLyrics3()
        {
            var barryManilowIWriteTheSongs = new Lyrster("Barry Manilow", "I Write The Songs", new ManualResetEvent(false), 10000);
            barryManilowIWriteTheSongs.FindLyrics();
            var splitBmiwts = barryManilowIWriteTheSongs.Lyric.Split(' ');
            Assert.AreEqual("I've", splitBmiwts[0]);
            Assert.AreEqual("cry", splitBmiwts[splitBmiwts.Length - 1]);
        }

        [TestMethod]
        public void TestLyrsterNotFound()
        {
            var notFound = new Lyrster("Foo", "Bar", new ManualResetEvent(false), 10000);
            notFound.FindLyrics();
            var splitNf = notFound.Lyric.Split(' ');
            Assert.AreEqual("Not", splitNf[0]);
            Assert.AreEqual("found", splitNf[splitNf.Length - 1]);
        }
    }
}
