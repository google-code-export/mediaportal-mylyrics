using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LyricsEngine.LyricsSites
{
    public abstract class AbstractSite : ILyricSite
    {
        #region const

        // Not found
        protected const string NotFound = "Not found";

        #endregion const

        #region members

        // Artist
        protected readonly string Artist;
        // Title
        protected readonly string Title;

        // Lyrics
        protected string LyricText = "";

        // Complete
        protected bool _complete;

        // Timer
        protected readonly Timer SearchTimer;
        // Stop event
        protected WaitHandle MEventStopSiteSearches;

        #endregion members


        protected AbstractSite(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit)
        {
            // artist
            Artist = artist;
            // title
            Title = title;
            
            // timer
            SearchTimer = new Timer {Enabled = false, Interval = timeLimit};
            SearchTimer.Elapsed += TimerElapsed;
            
            MEventStopSiteSearches = mEventStopSiteSearches;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            SearchTimer.Stop();
            SearchTimer.Close();
            SearchTimer.Dispose();

            LyricText = NotFound;
            _complete = true;
            Thread.CurrentThread.Abort();
        }

        protected abstract void FindLyricsWithTimer();


        #region interface abstract methods

        public string Lyric
        {
            get { return LyricText; }
        }

        public abstract string Name { get; }
        public abstract string BaseUrl { get; }
        public void FindLyrics()
        {
            SearchTimer.Start();
            FindLyricsWithTimer();
        }
        
        public abstract LyricType GetLyricType();
        public abstract SiteType GetSiteType();
        public abstract SiteComplexity GetSiteComplexity();

        #endregion interface abstract methods
    }
}
