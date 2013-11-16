using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLrcFinderTest
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
        public void TestLrcFinder()
        {
            var site = new LrcFinder("U2", "With Or Without You", new ManualResetEvent(false), 100000);

            if (site.SiteActive())
            {
                site.FindLyrics();
                var lrcLyrics = site.Lyric;

                const string lrcPattern = @"\[.*?\]";
                const string lrcPatternReplacement = "";
                var lrcRegex = new Regex(lrcPattern);
                var lyrics = lrcRegex.Replace(lrcLyrics, lrcPatternReplacement);

                const string pattern = @"\s+";
                const string patternReplacement = " ";
                var regex = new Regex(pattern);
                lyrics = regex.Replace(lyrics, patternReplacement);

                var lyricsSpit = lyrics.Trim().Split(' ');

                Assert.AreEqual("See", lyricsSpit[0]);
                Assert.AreEqual("you", lyricsSpit[lyricsSpit.Length - 1]);
            }
        }
    }
}
