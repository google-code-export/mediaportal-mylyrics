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
      var splitUwowy = u2WithOrWithoutYou.Lyric.Split(' ');
      Assert.AreEqual("See", splitUwowy[0]);
      Assert.AreEqual("you", splitUwowy[splitUwowy.Length - 1]);

      var u2StaringAtTheSun = new Lyrics007("U2", "Staring At The Sun", new ManualResetEvent(false), 10000);
      var splitUsats = u2StaringAtTheSun.Lyric.Split(' ');
      Assert.AreEqual("Staring", splitUsats[0]);
      Assert.AreEqual("blind", splitUsats[splitUsats.Length - 1]);
    }

    [TestMethod]
    public void TestLyrics007NotFound()
    {
      var notFound = new Lyrics007("Foo", "Bar", new ManualResetEvent(false), 10000);
      var splitNf = notFound.Lyric.Split(' ');
      Assert.AreEqual("Not", splitNf[0]);
      Assert.AreEqual("found", splitNf[splitNf.Length - 1]);
    }
  }
}
