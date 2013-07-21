using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsActionextTest
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
        public void TestActionext1()
        {
            var site = new Actionext("U2", "With Or Without You", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("See", splitLyrics[0]);
                Assert.AreEqual("you", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestActionext2()
        {
            var site = new Actionext("U2", "Staring At The Sun", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("SUMMER", splitLyrics[0]);
                Assert.AreEqual("blind", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestActionextNotFound()
        {
            var site = new Actionext("Foo", "Bar", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("Not", splitLyrics[0]);
                Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
            }
        }
    }
}
