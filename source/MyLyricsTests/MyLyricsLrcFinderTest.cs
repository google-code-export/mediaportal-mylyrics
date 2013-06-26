using System.Text.RegularExpressions;
using LyricsEngine.lrcfinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLrcFinderTest
    {
        [TestMethod]
        public void TestLrcFinder()
        {
            var u2WithOrWithoutYou = new LrcFinder(); 
            var lrcLyrics = u2WithOrWithoutYou.FindLRC("U2", "With Or Without You");

            const string lrcPattern = @"\[.*?\]";
            const string lrcPatternReplacement = "";
            var lrcRegex = new Regex(lrcPattern);
            var lyrics = lrcRegex.Replace(lrcLyrics, lrcPatternReplacement);

            const string pattern = @"\s+";
            const string patternReplacement = " ";
            var regex = new Regex(pattern);
            lyrics = regex.Replace(lyrics, patternReplacement);

            lyrics = lyrics.Trim();

            var splitUwowy = lyrics.Split(' ');
            Assert.AreEqual("See", splitUwowy[0]);
            Assert.AreEqual("you", splitUwowy[splitUwowy.Length - 1]);
        }
    }
}
