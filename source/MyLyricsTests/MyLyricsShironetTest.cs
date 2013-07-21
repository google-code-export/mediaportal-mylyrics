using System.Diagnostics;
using System.Threading;
using LyricsEngine.LyricsSites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyLyricsTests
{
    [TestClass]
    public class MyLyricsShironetTest
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
        public void TestShironet1()
        {
            var site = new Shironet("משינה", "עתיד מתוק", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("כולם", splitLyrics[0]);
                Assert.AreEqual("טוב...", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestShironet2()
        {
            var site = new Shironet("שבק ס", "נופל וקם", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("הדרך", splitLyrics[0]);
                Assert.AreEqual("ומפותלת...", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestShironet3()
        {
            var site = new Shironet("אריק ברמן", "ג'ט לג", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("אל", splitLyrics[0]);
                Assert.AreEqual("לסדין.", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestShironet4()
        {
            var site = new Shironet("אפרת גוש", "תמיד כשאתה בא", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("תמיד", splitLyrics[0]);
                Assert.AreEqual("באמת", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestLoadShironet()
        {
            for (var i = 0; i < 100; i++)
            {
                var site = new Shironet("משינה", "עתיד מתוק", new ManualResetEvent(false), 3000);
                if (site.SiteActive())
                {
                    site.FindLyrics();
                    var splitLyrics = site.Lyric.Split(' ');
                    Assert.AreEqual("כולם", splitLyrics[0]);
                    Assert.AreEqual("טוב...", splitLyrics[splitLyrics.Length - 1]);
                }
            }
        }

        [TestMethod]
        public void TestShironetNotFound()
        {
            var site = new Shironet("Foo", "Bar", new ManualResetEvent(false), 10000);
            if (site.SiteActive())
            {
                site.FindLyrics();
                var splitLyrics = site.Lyric.Split(' ');
                Assert.AreEqual("Not", splitLyrics[0]);
                Assert.AreEqual("found", splitLyrics[splitLyrics.Length - 1]);
            }
        }

        [TestMethod]
        public void TestHebrewInput()
        {
            const string hebrewText = "אבגדהוזחטיכךלמםנןסעפףצץקרשת";
            const string englishText = "abcdefghijklmnopqrstuvwxyz";
            const string mixedText = "abcdeאfghijk";

            Assert.IsTrue(Shironet.IsHebrew(hebrewText));
            Assert.IsFalse(Shironet.IsHebrew(englishText));
            Assert.IsTrue(Shironet.IsHebrew(mixedText));
        }
    }
}
