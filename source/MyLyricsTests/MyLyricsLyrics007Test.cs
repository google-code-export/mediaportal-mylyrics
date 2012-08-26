using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsLyrics007Test
    {
        [TestMethod]
        public void TestLyrics007()
        {
          var u2WithOrWithoutYou = new Lyrics007("U2", "With Or Without You", new ManualResetEvent(false), 10000);
            var splitMam = u2WithOrWithoutYou.Lyric.Split(' ');
            Assert.AreEqual("See", splitMam[0]);
            Assert.AreEqual("you", splitMam[splitMam.Length - 1]);
        }

        [TestMethod]
        public void TestLyrics007NotFound()
        {
            var notFound = new Lyrics007("Foo", "Bar", new ManualResetEvent(false), 10000);
            var splitMam = notFound.Lyric.Split(' ');
            Assert.AreEqual("Not", splitMam[0]);
            Assert.AreEqual("found", splitMam[splitMam.Length - 1]);
        }
    }
}
