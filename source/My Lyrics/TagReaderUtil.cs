using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MyLyrics
{
    internal class TagReaderUtil
    {
        private static string _currentTitle = "";

        public static bool WriteLyrics(string fileName, string lyrics)
        {
            return TagReader.WriteLyrics(fileName, lyrics);
        }

        public static bool WriteLyrics(string artist, string title, string lyrics)
        {
            var mDB = MusicDatabase.Instance;
            var songs = new List<Song>();
            mDB.GetSongsByArtist(artist, ref songs);

            _currentTitle = title;

            var rightSongs = songs.FindAll(RightTitle);

            return rightSongs.Aggregate(false, (current, song) => (TagReader.WriteLyrics(song.FileName, lyrics) || current));
        }

        private static bool RightTitle(Song song)
        {
            return song.Title.Equals(_currentTitle, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}