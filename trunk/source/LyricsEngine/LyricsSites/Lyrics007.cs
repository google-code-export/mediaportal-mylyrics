using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LyricsEngine.LyricsSites
{
    public class Lyrics007
    {
        # region const

        // base url
        private const string Lyrics007Com = "http://www.lyrics007.com/";
        // Not found
        private const string NotFound = "Not found";

        # endregion const

        # region patterns

        // artist/title for validation

        // lyrics mark pattern 
        private const string LyricsMarkPattern = @"data-artist=""(?<artist>.*)"" data-song=""(?<song>.*)"" data-search";
        private readonly string _artist;
        private readonly string _title;

        # endregion patterns

        private readonly Timer _timer;
        private bool _complete;
        private string _lyric = "";
        private readonly int _timeLimit;

        public Lyrics007(string artist, string title, ManualResetEvent mEventStopSiteSearches, int timeLimit)
        {
            _artist = artist;
            _title = title;

            _timeLimit = timeLimit/2;
            _timer = new Timer();

            artist = LyricUtil.RemoveFeatComment(artist);
            artist = artist.Replace("#", "");
            title = LyricUtil.TrimForParenthesis(title);
            title = title.Replace("#", "");

            // Cannot find lyrics contaning non-English letters!

            var urlString = Lyrics007Com + artist + " Lyrics/" + title + " Lyrics.html";

            _timer.Enabled = true;
            _timer.Interval = _timeLimit;
            _timer.Elapsed += timer_Elapsed;
            _timer.Start();

            var uri = new Uri(urlString);
            var client = new LyricsWebClient();
            client.OpenReadCompleted += CallbackMethod;
            client.OpenReadAsync(uri);

            while (_complete == false)
            {
                if (mEventStopSiteSearches.WaitOne(500, true))
                {
                    _complete = true;
                }
            }
        }

        public string Lyric
        {
            get { return _lyric; }
        }


        private void CallbackMethod(object sender, OpenReadCompletedEventArgs e)
        {
            var lyricTemp = new StringBuilder();
            // ReSharper disable UnusedVariable
            var client = (LyricsWebClient) sender;
            // ReSharper restore UnusedVariable
            Stream reply = null;
            StreamReader reader = null;

            try
            {
                // ReSharper disable RedundantCast
                reply = (Stream)e.Result;
                // ReSharper restore RedundantCast
                reader = new StreamReader(reply, Encoding.Default);

                // Look for start
                var inLyrics = false;
                while (true)
                {
                    if (reader.EndOfStream)
                    {
                        break;
                    }

                    // Read next line
                    var line = reader.ReadLine() ?? "";

                    // Find lyrics mark
                    var match = Regex.Match(line, LyricsMarkPattern, RegexOptions.IgnoreCase);

                    // Handle line
                    if (!inLyrics) // before lyrics started
                    {
                        // mark start of lyrics
                        if (match.Success)
                        {
                            if (match.Groups.Count == 3)
                            {
                                if (match.Groups[1].Value.Equals(_artist, StringComparison.OrdinalIgnoreCase) &&
                                    match.Groups[2].Value.Equals(_title, StringComparison.OrdinalIgnoreCase))
                                {
                                    inLyrics = true;
                                }
                                else
                                {
                                    _lyric = NotFound;
                                    break;
                                }
                            }
                        }
                    }
                    else // after lyrics started
                    {
                        // end of lyrics
                        if (match.Success)
                        {
                            break;
                        }

                        // Add line
                        lyricTemp.Append(line);
                    }
                }

                _lyric = lyricTemp.ToString();

                if (_lyric.Length > 0)
                {
                    CleanLyrics();
                }
                else
                    _lyric = NotFound;
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

        // Cleans the lyrics
        private void CleanLyrics()
        {
            _lyric = _lyric.Replace("</script>", "");
            _lyric = _lyric.Replace("??s", "'s");
            _lyric = _lyric.Replace("??t", "'t");
            _lyric = _lyric.Replace("??m", "'m");
            _lyric = _lyric.Replace("??l", "'l");
            _lyric = _lyric.Replace("??v", "'v");
            _lyric = _lyric.Replace("?s", "'s");
            _lyric = _lyric.Replace("?t", "'t");
            _lyric = _lyric.Replace("?m", "'m");
            _lyric = _lyric.Replace("?l", "'l");
            _lyric = _lyric.Replace("?v", "'v");
            _lyric = _lyric.Replace("<br>", "\r\n");
            _lyric = _lyric.Replace("<br />", "\r\n");
            _lyric = _lyric.Replace("<BR>", "\r\n");
            _lyric = _lyric.Replace("&amp;", "&");
            _lyric = Regex.Replace(_lyric, @"<span.*</span>", "", RegexOptions.Singleline);
            _lyric = Regex.Replace(_lyric, @"<.*?>", "", RegexOptions.Singleline);
            _lyric = Regex.Replace(_lyric, @"<!--.*-->", "", RegexOptions.Singleline);
            _lyric = _lyric.Trim();
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