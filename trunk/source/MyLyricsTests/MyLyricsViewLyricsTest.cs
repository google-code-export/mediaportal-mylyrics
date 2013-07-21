using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsViewLyricsTest
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
        public void TestViewLyrics()
        {
            var site = new ViewLyrics("U2", "With Or Without You", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("the", splitLyrics[9]); // 2nd word
                Assert.AreEqual("without", splitLyrics[146]); // disregard garbage in specific song}
            }
        }

        [TestMethod]
        public void TestViewLyrics2()
        {
            var site = new ViewLyrics("Eric Clapton", "I Shot the Sheriff", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("I", splitLyrics[0]);
                Assert.AreEqual("oh", splitLyrics[splitLyrics.Length - 16]);
            }
        }

        [TestMethod]
        public void TestViewLyricsNotFound()
        {
            var site = new ViewLyrics("Foo", "Bar", new ManualResetEvent(false), 10000);
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
