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
  // This class searches www.shiron.net for Hebrew lyrics
  public class Shironet
  {
    // artist/title for validation
    private readonly string _artist;
    private readonly string _title;

    // Lyrics
    private string _lyric = "";

    // step 1 output
    private string _prfid;
    private string _wrkid;
    private bool _firstStepComplete;
    private bool _complete;
    
    private readonly Timer _timer;

    public Shironet(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit)
    {
      _artist = artist;
      _title = title;

      _timer = new Timer();

      // Escape characters
      artist = FixEscapeCharacters(artist);
      title = FixEscapeCharacters(title);

      // Hebrew letters
      artist = FixHebrew(artist);
      title = FixHebrew(title);

      // timer
      _timer.Enabled = true;
      _timer.Interval = timeLimit;
      _timer.Elapsed += TimerElapsed;
      _timer.Start();

      // 1st step - find lyrics page
      var firstUrlString = "http://www.shiron.net/searchSongs?type=lyrics&q=" + artist + "%20" + title;

      var findLyricsPageWebClient = new LyricsWebClient();
      findLyricsPageWebClient.OpenReadCompleted += FirstCallbackMethod;
      findLyricsPageWebClient.OpenReadAsync(new Uri(firstUrlString));

      while (_firstStepComplete == false)
      {
        if (mEventStopSiteSearches.WaitOne(1, true))
        {
          _firstStepComplete = true;
        }
        else
        {
          Thread.Sleep(100);
        }
      }

      // 2nd step - find lyrics
      var secondUrlString = "http://www.shiron.net/artist?type=lyrics&lang=1&prfid=" + _prfid + "&wrkid=" + _wrkid;

      var findLyricsWebClient = new LyricsWebClient(firstUrlString);
      findLyricsWebClient.OpenReadCompleted += SecondCallbackMethod;
      findLyricsWebClient.OpenReadAsync(new Uri(secondUrlString));

      while (_complete == false)
      {
        if (mEventStopSiteSearches.WaitOne(1, true))
        {
          _complete = true;
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }

    public string Lyric
    {
      get { return _lyric; }
    }

    // Finds lyrics page
    private void FirstCallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      var thisMayBeTheCorrectPage = false;

      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        // RegEx to find lyrics page
        const string findLyricsPagePattern = "<a href=\\\"/artist\\?type=lyrics&lang=1&prfid=(?<prfid>\\d+)&wrkid=(?<wrkid>\\d+)\\\" class=\\\"search_link_name_big\\\">";

        while (!thisMayBeTheCorrectPage)
        {
          // Read line
          if (reader.EndOfStream)
          {
            break;
          }
          var line = reader.ReadLine() ?? "";

          // Try to find match in line
          var findLyricsPageMatch = Regex.Match(line, findLyricsPagePattern, RegexOptions.IgnoreCase);

          if (findLyricsPageMatch.Groups.Count == 3)
          {
            _prfid = findLyricsPageMatch.Groups[1].Value;
            _wrkid = findLyricsPageMatch.Groups[2].Value;

            if (Convert.ToUInt32(_prfid) > 0 && Convert.ToUInt32(_wrkid) > 0)
            {
              // Found page
              thisMayBeTheCorrectPage = true;
            }
          }
        }

        // Not found
        if (!thisMayBeTheCorrectPage)
        {
          _lyric = "Not found";
        }

      }
      catch
      {
        _lyric = "Not found";
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
        _firstStepComplete = true;
      }
    }

    // Find lyrics
    private void SecondCallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      var thisMayBeTheCorrectLyric = false;
      var lyricTemp = new StringBuilder();

      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        // Title RegEx
        const string titleSearchPattern = "<title>(?<title>.*?)</title>";
        // Lyrics start RegEx
        const string lyricsStartSearchPattern = "<span class=\\\"artist_lyrics_text\\\">(?<lyricsStart>.*)";
        // Lyrics end RegEx
        const string lyricsEndSearchPattern = "(?<lyricsEnd>.*?)</span>";

        var titleLine = "";
        var foundStart = false;

        while (!_complete)
        {
          // Read line
          if (reader.EndOfStream)
          {
            break;
          }
          var line = reader.ReadLine() ?? "";

          // Find artist + title in <title> line and validate correct artist/title
          if (titleLine == "")
          {
            var findLyricsPageMatch = Regex.Match(line, titleSearchPattern, RegexOptions.IgnoreCase);
            if (findLyricsPageMatch.Groups.Count == 2)
            {
              titleLine = findLyricsPageMatch.Groups[1].Value;
            }

            // validation
            if (!ValidateArtistAndTitle(titleLine))
            {
              throw new ArgumentException("Cannot find exact match");
            }
          }

          if (!foundStart)
          {
            // Try to find lyrics start in line
            var findLyricsPageMatch = Regex.Match(line, lyricsStartSearchPattern, RegexOptions.IgnoreCase);

            if (findLyricsPageMatch.Groups.Count == 2)
            {
              foundStart = true;

              // Initialize with first line
              lyricTemp.Append(findLyricsPageMatch.Groups[1].Value).Append(Environment.NewLine);
            }

          }
          else // already found start
          {
            // Try to find lyrics end in line
            var findLyricsPageMatch = Regex.Match(line, lyricsEndSearchPattern, RegexOptions.IgnoreCase);
            if (findLyricsPageMatch.Groups.Count == 2)
            {
              // Add last line
              lyricTemp.Append(findLyricsPageMatch.Groups[1].Value).Append(Environment.NewLine);
              thisMayBeTheCorrectLyric = true;
              break;
            }

            // Add line to lyrics
            lyricTemp.Append(line).Append(Environment.NewLine);
          }
        }

        if (thisMayBeTheCorrectLyric)
        {
          // Clean lyrics
          _lyric = CleanLyrics(lyricTemp);

          if (_lyric.Length == 0 || (_lyric.Contains("<") || _lyric.Contains(">") || _lyric.Contains("a href")))
          {
            _lyric = "Not found";
          }
        }
      }
      catch
      {
        _lyric = "Not found";
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

    private bool ValidateArtistAndTitle(string titleLine)
    {
      bool validated = true;
      string[] strings = titleLine.Split('-');
      if (strings.Length == 3)
      {
        // check artist
        if (!IgnoreSpecialChars(_artist).Equals(IgnoreSpecialChars(strings[0])))
        {
          validated = false;
        }
        // check title
        if (!IgnoreSpecialChars(_title).Equals(IgnoreSpecialChars(strings[1])))
        {
          validated = false;
        }
      }
      return validated;
    }

    private static string IgnoreSpecialChars(string orig)
    {
      return orig.Replace("\'", "").Replace("\"", "").Trim();
    }

    private static string CleanLyrics(StringBuilder lyricTemp)
    {
      lyricTemp.Replace("<br>", "");
      lyricTemp.Replace("<br/>", "");
      lyricTemp.Replace("&quot;", "\"");

      return lyricTemp.ToString().Trim();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      _timer.Stop();
      _timer.Close();
      _timer.Dispose();

      _lyric = "Not found";
      _complete = true;
      Thread.CurrentThread.Abort();
    }

    private static string FixEscapeCharacters(string text)
    {
      text = text.Replace("(", "");
      text = text.Replace(")", "");
      text = text.Replace("#", "");
      text = text.Replace("/", "");

      text = text.Replace("%", "%25");

      text = text.Replace(" ", "%20");
      text = text.Replace("$", "%24");
      text = text.Replace("&", "%26");
      text = text.Replace("'", "%27");
      text = text.Replace("+", "%2B");
      text = text.Replace(",", "%2C");
      text = text.Replace(":", "%3A");
      text = text.Replace(";", "%3B");
      text = text.Replace("=", "%3D");
      text = text.Replace("?", "%3F");
      text = text.Replace("@", "%40");
      text = text.Replace("&amp;", "&");

      return text;
    }

    private static string FixHebrew(string text)
    {
      text = text.Replace("\uC3A0", "%d7%90"); // à
      text = text.Replace("\uC3A1", "%D7%91");
      text = text.Replace("\uC3A2", "%D7%92");
      text = text.Replace("\uC3A3", "%D7%93");
      text = text.Replace("\uC3A4", "%D7%94");
      text = text.Replace("\uC3A5", "%D7%95");
      text = text.Replace("\uC3A6", "%D7%96");
      text = text.Replace("\uC3A7", "%D7%97");
      text = text.Replace("\uC3A8", "%D7%98");
      text = text.Replace("\uC3A9", "%D7%99");
      text = text.Replace("\uC3AA", "%D7%9A");
      text = text.Replace("\uC3AB", "%D7%9B");
      text = text.Replace("\uC3AC", "%D7%9C");
      text = text.Replace("\uC3AD", "%D7%9D");
      text = text.Replace("\uC3AE", "%D7%9E");
      text = text.Replace("\uC3AF", "%D7%9F");
      text = text.Replace("\uC3B0", "%D7%A0");
      text = text.Replace("\uC3B1", "%D7%A1");
      text = text.Replace("\uC3B2", "%D7%A2");
      text = text.Replace("\uC3B3", "%D7%A3");
      text = text.Replace("\uC3B4", "%D7%A4");
      text = text.Replace("\uC3B5", "%D7%A5");
      text = text.Replace("\uC3B6", "%D7%A6");
      text = text.Replace("\uC3B7", "%D7%A7");
      text = text.Replace("\uC3B8", "%D7%A8");
      text = text.Replace("\uC3B9", "%D7%A9");
      text = text.Replace("\uC3BA", "%D7%AA"); // ú

      text = text.Replace("\uD790", "%d7%90"); // à
      text = text.Replace("\uD791", "%D7%91");
      text = text.Replace("\uD792", "%D7%92");
      text = text.Replace("\uD793", "%D7%93");
      text = text.Replace("\uD794", "%D7%94");
      text = text.Replace("\uD795", "%D7%95");
      text = text.Replace("\uD796", "%D7%96");
      text = text.Replace("\uD797", "%D7%97");
      text = text.Replace("\uD798", "%D7%98");
      text = text.Replace("\uD799", "%D7%99");
      text = text.Replace("\uD79A", "%D7%9A");
      text = text.Replace("\uD79B", "%D7%9B");
      text = text.Replace("\uD79C", "%D7%9C");
      text = text.Replace("\uD79D", "%D7%9D");
      text = text.Replace("\uD79E", "%D7%9E");
      text = text.Replace("\uD79F", "%D7%9F");
      text = text.Replace("\uD7A0", "%D7%A0");
      text = text.Replace("\uD7A1", "%D7%A1");
      text = text.Replace("\uD7A2", "%D7%A2");
      text = text.Replace("\uD7A3", "%D7%A3");
      text = text.Replace("\uD7A4", "%D7%A4");
      text = text.Replace("\uD7A5", "%D7%A5");
      text = text.Replace("\uD7A6", "%D7%A6");
      text = text.Replace("\uD7A7", "%D7%A7");
      text = text.Replace("\uD7A8", "%D7%A8");
      text = text.Replace("\uD7A9", "%D7%A9");
      text = text.Replace("\uD7AA", "%D7%AA"); // ú

      return text;
    }
  }
}
