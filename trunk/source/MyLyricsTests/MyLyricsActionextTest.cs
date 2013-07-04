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
            var u2WithOrWithoutYou = new Actionext("U2", "With Or Without You", new ManualResetEvent(false), 10000);
            u2WithOrWithoutYou.FindLyrics();
            var splitUwowy = u2WithOrWithoutYou.Lyric.Split(' ');
            Assert.AreEqual("See", splitUwowy[0]);
            Assert.AreEqual("you", splitUwowy[splitUwowy.Length - 1]);
        }

        [TestMethod]
        public void TestActionext2()
        {
            var u2StaringAtTheSun = new Actionext("U2", "Staring At The Sun", new ManualResetEvent(false), 10000);
            u2StaringAtTheSun.FindLyrics();
            var splitUsats = u2StaringAtTheSun.Lyric.Split(' ');
            Assert.AreEqual("SUMMER", splitUsats[0]);
            Assert.AreEqual("blind", splitUsats[splitUsats.Length - 1]);
        }

        [TestMethod]
        public void TestActionextNotFound()
        {
            var notFound = new Actionext("Foo", "Bar", new ManualResetEvent(false), 10000);
            notFound.FindLyrics();
            var splitNf = notFound.Lyric.Split(' ');
            Assert.AreEqual("Not", splitNf[0]);
            Assert.AreEqual("found", splitNf[splitNf.Length - 1]);
        }
    }
}
