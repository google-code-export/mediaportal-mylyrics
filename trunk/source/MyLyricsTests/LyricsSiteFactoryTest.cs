using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class LyricsSiteFactoryTest
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
        public void TestLyricsSiteFactory()
        {
            var lyricsSitesNames = LyricsSiteFactory.LyricsSitesNames();
            foreach (var lyricsSitesName in lyricsSitesNames)
            {
                Debug.WriteLine("Executing lyrics site " + lyricsSitesName);
                var abstractSite = LyricsSiteFactory.Create(lyricsSitesName, "U2", "With Or Without You", new ManualResetEvent(false), 5000);
                abstractSite.FindLyrics();
                // No assertions
            }
        }
    }
}
