using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LyricsEngine.LyricsSites
{
    public class LyrDB
    {
        // Base url
        private const string LyrDbCom = "http://webservices.lyrdb.com";

        private const string Agent = "MediaPortal/MyLyrics";
        private const string NotFound = "Not found";

        private readonly Timer _timer;
        private readonly ManualResetEvent _mEventStopSiteSearches;

        private bool _complete;
        private string _lyric = "";
        private readonly int _timeLimit;

        public LyrDB(string artist, string title, ManualResetEvent eventStopSiteSearches, int timeLimit)
        {
            _timeLimit = timeLimit;
            _timer = new Timer();

            _mEventStopSiteSearches = eventStopSiteSearches;

            artist = LyricUtil.RemoveFeatComment(artist);
            title = LyricUtil.TrimForParenthesis(title);
            var urlString = string.Format(LyrDbCom + "/lookup.php?q={0}%7c{1}&for=match&agant={2}", artist, title, Agent);

            var client = new LyricsWebClient();

            _timer.Enabled = true;
            _timer.Interval = _timeLimit;
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();

            var uri = new Uri(urlString);
            client.OpenReadCompleted += CallbackMethodSearch;
            client.OpenReadAsync(uri);

            while (_complete == false)
            {
                if (_mEventStopSiteSearches.WaitOne(1, true))
                {
                    _complete = true;
                }
                else
                {
                    Thread.Sleep(300);
                }
            }
        }

        public string Lyric
        {
            get { return _lyric; }
        }

        private void CallbackMethodSearch(object sender, OpenReadCompletedEventArgs e)
        {
            var client = (LyricsWebClient) sender;
            Stream reply = null;
            StreamReader reader = null;

            try
            {
                reply = e.Result;
                reader = new StreamReader(reply, Encoding.Default);

                var result = reader.ReadToEnd();

                if (result.Equals(""))
                {
                    _lyric = NotFound;
                    return;
                }

                var id = result.Substring(0, result.IndexOf(@"\", StringComparison.Ordinal));

                var urlString = string.Format(LyrDbCom + "/getlyr.php?q={0}", id);

                var client2 = new LyricsWebClient();

                var uri = new Uri(urlString);
                client2.OpenReadCompleted += CallbackMethodGetLyric;
                client2.OpenReadAsync(uri);

                while (_complete == false)
                {
                    if (_mEventStopSiteSearches.WaitOne(1, true))
                    {
                        _complete = true;
                    }
                    else
                    {
                        Thread.Sleep(300);
                    }
                }
            }
            catch
            {
                _lyric = NotFound;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (reply != null)
                {
                    reply.Close();
                }
            }
        }


        private void CallbackMethodGetLyric(object sender, OpenReadCompletedEventArgs e)
        {
            var client = (LyricsWebClient) sender;
            Stream reply = null;
            StreamReader reader = null;

            try
            {
                reply = e.Result;
                reader = new StreamReader(reply, Encoding.Default);

                _lyric = reader.ReadToEnd().Trim();

                _lyric = _lyric.Replace("*", "");
                _lyric = _lyric.Replace("&amp;", "&");
            }
            catch
            {
                _lyric = NotFound;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (reply != null)
                {
                    reply.Close();
                }
                _complete = true;
            }
        }


        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Close();
            _timer.Dispose();

            _lyric = NotFound;
            _complete = true;
            Thread.CurrentThread.Abort();
        }
    }
}