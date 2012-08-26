﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LyricsEngine.LyricsSites
{
  internal class LyricWiki
  {
    private bool complete;
    private string lyric = "";
    private int timeLimit;
    private Timer timer;

    public LyricWiki(string artist, string title, ManualResetEvent m_EventStop_SiteSearches, int timeLimit)
    {
      this.timeLimit = timeLimit / 2;
      timer = new Timer();

      artist = LyricUtil.RemoveFeatComment(artist);
      artist = LyricUtil.CapatalizeString(artist);
      artist = artist.Replace(" ", "_");

      title = LyricUtil.TrimForParenthesis(title);
      title = LyricUtil.CapatalizeString(title);
      title = title.Replace(" ", "_");
      title = title.Replace("?", "%3F");

      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
      {
        return;
      }

      string urlString = "http://lyricwiki.org/" + artist + ":" + title;

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

      LyricsWebClient client = (LyricsWebClient)sender;
      Stream reply = null;
      StreamReader sr = null;

      try
      {
        reply = (Stream)e.Result;
        sr = new StreamReader(reply, Encoding.Default);

        string line = "";

        while (line.IndexOf("class='lyricbox' >") == -1)
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
          string cutString = "class='lyricbox' >";

          int cutIndex = line.IndexOf(cutString);

          if (cutIndex != -1)
          {
            lyric = line.Substring(cutIndex + cutString.Length);
          }

          Encoding iso8859 = Encoding.GetEncoding("ISO-8859-1");
          lyric = Encoding.UTF8.GetString(iso8859.GetBytes(lyric));

          lyric = lyric.Replace("<br />", "\r\n");
          lyric = lyric.Replace("<i>", "");
          lyric = lyric.Replace("</i>", "");
          lyric = lyric.Replace("<b>", "");
          lyric = lyric.Replace("</b>", "");
          lyric = lyric.Replace("&amp;", "&");

          lyric = lyric.Trim();

          if (lyric.Contains("<"))
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