using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyricsmodeTest
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
        public void TestLyricsmode()
        {
            var site = new Lyricsmode("U2", "With Or Without You", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("See", splitLyrics[0]);
            Assert.AreEqual("you", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsmode2()
        {
            var site = new Lyricsmode("Eric Clapton", "I Shot the Sheriff", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I", splitLyrics[0]);
            Assert.AreEqual("no.", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsmode3()
        {
            var site = new Lyricsmode("Barry Manilow", "I Write The Songs", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("I've", splitLyrics[0]);
            Assert.AreEqual("songs.", splitLyrics[splitLyrics.Length - 1]);
        }

        [TestMethod]
        public void TestLyricsmodeNotFound()
        {
            var site = new Lyricsmode("Foo", "Bar", new ManualResetEvent(false), 10000);
            site.FindLyrics();
            var splitLyrics = site.Lyric.Split(' ');
            Assert.AreEqual("Not", splitLyrics[0]);
            Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
        }
    }
}
