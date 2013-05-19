using System;
using System.Collections.Generic;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MyLyrics
{
  internal class TagReaderUtil
  {
    private static string CurrentTitle = "";

    public static bool WriteLyrics(string fileName, string lyrics)
    {
      return TagReader.WriteLyrics(fileName, lyrics);
    }

    public static bool WriteLyrics(string artist, string title, string lyrics)
    {
      MusicDatabase mDB = MusicDatabase.Instance;
      List<Song> songs = new List<Song>();
      mDB.GetSongsByArtist(artist, ref songs);

      CurrentTitle = title;

      List<Song> rightSongs = songs.FindAll(RightTitle);

      bool test = false;

      foreach (Song song in rightSongs)
      {
        test = (TagReader.WriteLyrics(song.FileName, lyrics) ? true : test);
      }
      return test;
    }

    private static bool RightTitle(Song song)
    {
      if (song.Title.Equals(CurrentTitle, StringComparison.CurrentCultureIgnoreCase))
        return true;
      else
        return false;
    }
  }
}