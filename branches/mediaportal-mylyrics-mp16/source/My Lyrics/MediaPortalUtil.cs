using System;
using LyricsEngine;
using MyLyrics.XmlSettings;

namespace MyLyrics
{
  internal class MediaPortalUtil
  {
      // Returns latests settings from the MediaPortal.xml file. Reload on access to ensure any changes made while the program runs are honored.

      public static string[] GetStrippedPrefixStringArray()
      {
          string strippedPrefixes;
          using (var xmlreader = SettingManager.MediaPortalSettings)
          {
              strippedPrefixes = (xmlreader.GetValueAsString("musicfiles", "artistprefixes", "the,les,die"));
          }
          var strippedPrefixesArray = strippedPrefixes.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

          for (var i = 0; i < strippedPrefixesArray.Length; i++)
          {
              var temp = strippedPrefixesArray[i];
              strippedPrefixesArray[i] = LyricUtil.CapatalizeString(", " + temp.Trim());
          }

          return strippedPrefixesArray;
      }

      public static string GetStrippedPrefixArtist(string artist, string[] strippedPrefixStringArray)
    {
      foreach (var s in strippedPrefixStringArray)
      {
        var index = artist.IndexOf(s, StringComparison.Ordinal);
        if (index != -1)
        {
          var prefix = artist.Substring(index + 2);
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