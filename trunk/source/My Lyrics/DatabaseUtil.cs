using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LyricsEngine;
using LyricsEngine.LRC;
using MediaPortal.Configuration;

namespace MyLyrics
{
  internal class DatabaseUtil
  {
    internal const int LRC_FOUND = 2;
    internal const int LRC_NOT_FOUND = 3;
    internal const int LYRIC_FOUND = 1;
    internal const int LYRIC_MARKED = 0;
    internal const int LYRIC_NOT_FOUND = -1;
    internal const string MARK = "(no lyric attached)";

    internal static int IsSongInLyricsDatabase(LyricsDatabase lyricDB, string artist, string title)
    {
      string lyricText = "";

      string capatalizedArtist = LyricUtil.CapatalizeString(artist);
      string capatalizedTitle = LyricUtil.CapatalizeString(title);

      string key = CorrectKeyFormat(capatalizedArtist, capatalizedTitle);

      if (lyricDB.ContainsKey(key))
      {
        lyricText = lyricDB[key].Lyrics;

        if (lyricText.Equals(MARK))
        {
          return LYRIC_MARKED;
        }
        else
        {
          return LYRIC_FOUND;
        }
      }
      else
      {
        return LYRIC_NOT_FOUND;
      }
    }

    internal static int IsSongInLyricsMarkedDatabase(LyricsDatabase lyricMarkedDB, string artist, string title)
    {
      string key = CorrectKeyFormat(LyricUtil.CapatalizeString(artist), LyricUtil.CapatalizeString(title));

      if (lyricMarkedDB.ContainsKey(key))
      {
        return LYRIC_MARKED;
      }
      else
      {
        return LYRIC_NOT_FOUND;
      }
    }

    internal static int IsSongInLyricsDatabaseAsLRC(LyricsDatabase lyricDB, string artist, string title)
    {
      string lyricText = "";

      string capatalizedArtist = LyricUtil.CapatalizeString(artist);
      string capatalizedTitle = LyricUtil.CapatalizeString(title);

      string key = CorrectKeyFormat(capatalizedArtist, capatalizedTitle);

      if (lyricDB.ContainsKey(key))
      {
        lyricText = lyricDB[key].Lyrics;

        if (new SimpleLRC(capatalizedArtist, capatalizedTitle, lyricText).IsValid)
        {
          return LRC_FOUND;
        }
        else
        {
          return LYRIC_FOUND;
        }
      }
      else
      {
        return LRC_NOT_FOUND;
      }
    }


    public static string CorrectKeyFormat(string artist, string title)
    {
      return artist + "-" + title;
    }


    public static void WriteToLyricsDatabase(LyricsDatabase lyricsDB, LyricsDatabase lyricsMarkedDB,
                                             string capArtist, string capTitle, string lyric, string site)
    {
      if (IsSongInLyricsDatabase(lyricsDB, capArtist, capTitle).Equals(LYRIC_NOT_FOUND))
      {
        lyricsDB.Add(CorrectKeyFormat(capArtist, capTitle), new LyricsItem(capArtist, capTitle, lyric, site));
      }

      if (IsSongInLyricsMarkedDatabase(lyricsMarkedDB, capArtist, capTitle).Equals(LYRIC_MARKED))
      {
        lyricsMarkedDB.Remove(CorrectKeyFormat(capArtist, capTitle));
      }
    }

    public static void ReplaceInLyricsDatabase(LyricsDatabase currentLyricsDB, string capArtist, string capTitle,
                                               string lyric, string site)
    {
      LyricsDatabase otherDatabase = GetOtherLyricsDatabase(currentLyricsDB);

      string key = CorrectKeyFormat(capArtist, capTitle);
      if (IsSongInLyricsDatabase(currentLyricsDB, capArtist, capTitle).Equals(LYRIC_NOT_FOUND) == false)
      {
        currentLyricsDB.Remove(key);
      }

      currentLyricsDB.Add(key, new LyricsItem(capArtist, capTitle, lyric, site));

      if (IsSongInLyricsMarkedDatabase(otherDatabase, capArtist, capTitle).Equals(LYRIC_MARKED))
      {
        otherDatabase.Remove(CorrectKeyFormat(capArtist, capTitle));
      }
    }

    public static void SerializeDB(LyricsDatabase lyricsDatabase)
    {
      if (lyricsDatabase == MyLyricsSettings.LyricsDB)
      {
        SerializeLyricDB();
      }
      else
      {
        SerializeLyricMarkedDB();
      }
    }

    public static void SerializeDBs()
    {
      SerializeLyricDB();
      SerializeLyricMarkedDB();
    }

    public static void SerializeLyricDB()
    {
      string path = Config.GetFile(Config.Dir.Database, MyLyricsSettings.LyricsDBName);
      using (FileStream fs = new FileStream(path, FileMode.Open))
      {
        BinaryFormatter bf = new BinaryFormatter();
        MyLyricsSettings.LyricsDB.SetLastModified();
        bf.Serialize(fs, MyLyricsSettings.LyricsDB);
        fs.Close();
      }
    }

    public static void SerializeLyricMarkedDB()
    {
      string path = Config.GetFile(Config.Dir.Database, MyLyricsSettings.LyricsMarkedDBName);
      using (FileStream fs = new FileStream(path, FileMode.Open))
      {
        BinaryFormatter bf = new BinaryFormatter();
        MyLyricsSettings.LyricsMarkedDB.SetLastModified();
        bf.Serialize(fs, MyLyricsSettings.LyricsMarkedDB);
        fs.Close();
      }
    }

    public static LyricsDatabase GetOtherLyricsDatabase(LyricsDatabase currentDatabase)
    {
      if (currentDatabase.Equals(MyLyricsSettings.LyricsDB))
      {
        return MyLyricsSettings.LyricsMarkedDB;
      }
      else
      {
        return MyLyricsSettings.LyricsDB;
      }
    }
  }
}