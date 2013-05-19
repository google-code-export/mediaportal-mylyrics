using System;
using LyricsEngine;
using MediaPortal.Profile;

namespace MyLyrics
{
  internal class MediaPortalUtil
  {
    public static string[] GetStrippedPrefixStringArray()
    {
      string strippedPrefixes = "";
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        strippedPrefixes = (xmlreader.GetValueAsString("musicfiles", "artistprefixes", "the,les,die"));
      }
      string[] strippedPrefixesArray = strippedPrefixes.Split(new string[1] { "," },
                                                              StringSplitOptions.RemoveEmptyEntries);

      for (int i = 0; i < strippedPrefixesArray.Length; i++)
      {
        string temp = strippedPrefixesArray[i];
        strippedPrefixesArray[i] = LyricUtil.CapatalizeString(", " + temp.Trim());
      }

      return strippedPrefixesArray;
    }

    public static string GetStrippedPrefixArtist(string artist, string[] strippedPrefixStringArray)
    {
      foreach (string s in strippedPrefixStringArray)
      {
        int index = artist.IndexOf(s);
        if (index != -1)
        {
          string prefix = artist.Substring(index + 2);
          artist = prefix + " " + artist.Replace(s, "");
          break;
        }
      }
      return artist;
    }

    public static string MakePlainLyricPerfectToShow(string text)
    {
      return text.Replace("\r\n", "\n");
    }

    public static string MakeLRCLinePerfectToShow(string text)
    {
      //string temp = lyric.Replace("\r\n", " ");
      //if (temp.Equals(string.Empty))
      //{
      //    return "-";
      //}
      //else
      //{
      //    return temp;
      //}

      return text.Replace("\r\n", string.Empty);
    }
  }
}