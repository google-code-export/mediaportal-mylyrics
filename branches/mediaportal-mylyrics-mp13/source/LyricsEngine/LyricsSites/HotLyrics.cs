using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using Timer=System.Timers.Timer;

namespace LyricsEngine.LyricsSites
{
    internal class HotLyrics
    {
        private bool complete;
        private string lyric = "";
        private int timeLimit;
        private Timer timer;

        public HotLyrics(string artist, string title, ManualResetEvent m_EventStop_SiteSearches, int timeLimit)
        {
            this.timeLimit = timeLimit/2;
            timer = new Timer();

            artist = LyricUtil.RemoveFeatComment(artist);
            artist = LyricUtil.CapatalizeString(artist);

            artist = artist.Replace(" ", "_");
            artist = artist.Replace(",", "_");
            artist = artist.Replace(".", "_");
            artist = artist.Replace("'", "_");
            artist = artist.Replace("(", "%28");
            artist = artist.Replace(")", "%29");
            artist = artist.Replace(",", "");
            artist = artist.Replace("#", "");
            artist = artist.Replace("%", "");
            artist = artist.Replace("+", "%2B");
            artist = artist.Replace("=", "%3D");
            artist = artist.Replace("-", "_");

            // French letters
            artist = artist.Replace("�", "%E9");

            title = LyricUtil.TrimForParenthesis(title);
            title = LyricUtil.CapatalizeString(title);

            title = title.Replace(" ", "_");
            title = title.Replace(",", "_");
            title = title.Replace(".", "_");
            title = title.Replace("'", "_");
            title = title.Replace("(", "%28");
            title = title.Replace(")", "%29");
            title = title.Replace(",", "_");
            title = title.Replace("#", "_");
            title = title.Replace("%", "_");
            title = title.Replace("?", "_");
            title = title.Replace("+", "%2B");
            title = title.Replace("=", "%3D");
            title = title.Replace("-", "_");
            title = title.Replace(":", "_");

            // German letters
            artist = artist.Replace("�", "%FC");
            artist = artist.Replace("�", "%DC");
            artist = artist.Replace("�", "%E4");
            artist = artist.Replace("�", "%C4");
            artist = artist.Replace("�", "%F6");
            artist = artist.Replace("�", "%D6");
            artist = artist.Replace("�", "%DF");

            // Danish letters
            title = title.Replace("�", "%E5");
            title = title.Replace("�", "%C5");
            title = title.Replace("�", "%E6");
            title = title.Replace("�", "%F8");

            // French letters
            title = title.Replace("�", "%E9");

            if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
            {
                return;
            }

            string firstLetter = artist[0].ToString();

            string urlString = "http://www.hotlyrics.net/lyrics/" + firstLetter + "/" + artist + "/" + title + ".html";

            LyricsWebClient client = new LyricsWebClient();

            timer.Enabled = true;
            timer.Interval = timeLimit;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();

            Uri uri = new Uri(urlString);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(callbackMethod);
            client.OpenReadAsync(uri);

            while (complete == false)
            {
                if (m_EventStop_SiteSearches.WaitOne(1, true))
                {
                    complete = true;
                }
                else
                {
                    Thread.Sleep(300);
                }
            }
        }

        public string Lyric
        {
            get { return lyric; }
        }

        private void callbackMethod(object sender, OpenReadCompletedEventArgs e)
        {
            bool thisMayBeTheCorrectLyric = true;
            StringBuilder lyricTemp = new StringBuilder();

            LyricsWebClient client = (LyricsWebClient) sender;
            Stream reply = null;
            StreamReader sr = null;

            try
            {
                reply = (Stream) e.Result;
                sr = new StreamReader(reply, Encoding.Default);

                string line = "";

                while (line.IndexOf("GOOGLE END") == -1)
                {
                    if (sr.EndOfStream)
                    {
                        thisMayBeTheCorrectLyric = false;
                        break;
                    }
                    else
                    {
                        line = sr.ReadLine();
                    }
                }

                if (thisMayBeTheCorrectLyric)
                {
                    lyricTemp = new StringBuilder();
                    line = sr.ReadLine();

                    while (line.IndexOf("<script type") == -1)
                    {
                        lyricTemp.Append(line);
                        if (sr.EndOfStream)
                        {
                            thisMayBeTheCorrectLyric = false;
                            break;
                        }
                        else
                        {
                            line = sr.ReadLine();
                        }
                    }

                    lyricTemp.Replace("?s", "'s");
                    lyricTemp.Replace("?t", "'t");
                    lyricTemp.Replace("?m", "'m");
                    lyricTemp.Replace("?l", "'l");
                    lyricTemp.Replace("?v", "'v");
                    lyricTemp.Replace("<br>", "\r\n");
                    lyricTemp.Replace("<br />", "\r\n");
                    lyricTemp.Replace("&quot;", "\"");
                    lyricTemp.Replace("</p>", "");
                    lyricTemp.Replace("<BR>", "");
                    lyricTemp.Replace("<br/>", "\r\n");
                    lyricTemp.Replace("&amp;", "&");

                    lyric = lyricTemp.ToString().Trim();

                    if (lyric.Contains("<td"))
                    {
                        lyric = "Not found";
                    }
                }
            }
            catch
            {
                lyric = "Not found";
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }

                if (reply != null)
                {
                    reply.Close();
                }

                if (timer != null)
                {
                    timer.Stop();
                    timer.Close();
                }
                complete = true;
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();
            timer.Dispose();

            lyric = "Not found";
            complete = true;
            Thread.CurrentThread.Abort();
        }
    }
}