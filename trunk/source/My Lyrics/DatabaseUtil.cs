using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LyricsEngine;
using LyricsEngine.LRC;
using LyricsEngine.LyricsDatabase;
using MediaPortal.Configuration;

namespace MyLyrics
{
    internal class DatabaseUtil
    {
        internal const int LRCFound = 2;
        internal const int LRCNotFound = 3;
        internal const int LyricFound = 1;
        internal const int LyricMarked = 0;
        internal const int LyricNotFound = -1;
        internal const string Mark = "(no lyric attached)";

        internal static int IsSongInLyricsDatabase(LyricsDatabase lyricDB, string artist, string title)
        {
            var capatalizedArtist = LyricUtil.CapatalizeString(artist);
            var capatalizedTitle = LyricUtil.CapatalizeString(title);

            var key = CorrectKeyFormat(capatalizedArtist, capatalizedTitle);

            if (lyricDB.ContainsKey(key))
            {
                var lyricText = lyricDB[key].Lyrics;

                return lyricText.Equals(Mark) ? LyricMarked : LyricFound;
            }
            
            return LyricNotFound;
        }

        internal static int IsSongInLyricsMarkedDatabase(LyricsDatabase lyricMarkedDB, string artist, string title)
        {
            var key = CorrectKeyFormat(LyricUtil.CapatalizeString(artist), LyricUtil.CapatalizeString(title));

            return lyricMarkedDB.ContainsKey(key) ? LyricMarked : LyricNotFound;
        }

        internal static int IsSongInLyricsDatabaseAsLRC(LyricsDatabase lyricDB, string artist, string title)
        {
            var capatalizedArtist = LyricUtil.CapatalizeString(artist);
            var capatalizedTitle = LyricUtil.CapatalizeString(title);

            var key = CorrectKeyFormat(capatalizedArtist, capatalizedTitle);

            if (lyricDB.ContainsKey(key))
            {
                var lyricText = lyricDB[key].Lyrics;

                return new SimpleLRC(capatalizedArtist, capatalizedTitle, lyricText).IsValid ? LRCFound : LyricFound;
            }
            
            return LRCNotFound;
        }


        public static string CorrectKeyFormat(string artist, string title)
        {
            return artist + "-" + title;
        }


        public static void WriteToLyricsDatabase(LyricsDatabase lyricsDB, LyricsDatabase lyricsMarkedDB,
            string capArtist, string capTitle, string lyric, string site)
        {
            if (IsSongInLyricsDatabase(lyricsDB, capArtist, capTitle).Equals(LyricNotFound))
            {
                lyricsDB.Add(CorrectKeyFormat(capArtist, capTitle), new LyricsItem(capArtist, capTitle, lyric, site));
            }

            if (IsSongInLyricsMarkedDatabase(lyricsMarkedDB, capArtist, capTitle).Equals(LyricMarked))
            {
                lyricsMarkedDB.Remove(CorrectKeyFormat(capArtist, capTitle));
            }
        }

        public static void ReplaceInLyricsDatabase(LyricsDatabase currentLyricsDB, string capArtist, string capTitle,
            string lyric, string site)
        {
            var otherDatabase = GetOtherLyricsDatabase(currentLyricsDB);

            var key = CorrectKeyFormat(capArtist, capTitle);
            if (IsSongInLyricsDatabase(currentLyricsDB, capArtist, capTitle).Equals(LyricNotFound) == false)
            {
                currentLyricsDB.Remove(key);
            }

            currentLyricsDB.Add(key, new LyricsItem(capArtist, capTitle, lyric, site));

            if (IsSongInLyricsMarkedDatabase(otherDatabase, capArtist, capTitle).Equals(LyricMarked))
            {
                otherDatabase.Remove(CorrectKeyFormat(capArtist, capTitle));
            }
        }

        public static void SerializeDB(LyricsDatabase lyricsDatabase)
        {
            if (lyricsDatabase == MyLyricsUtils.LyricsDB)
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
            var path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsDBName);
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                MyLyricsUtils.LyricsDB.SetLastModified();
                bf.Serialize(fs, MyLyricsUtils.LyricsDB);
                fs.Close();
            }
        }

        public static void SerializeLyricMarkedDB()
        {
            var path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsMarkedDBName);
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                MyLyricsUtils.LyricsMarkedDB.SetLastModified();
                bf.Serialize(fs, MyLyricsUtils.LyricsMarkedDB);
                fs.Close();
            }
        }

        public static LyricsDatabase GetOtherLyricsDatabase(LyricsDatabase currentDatabase)
        {
            return currentDatabase.Equals(MyLyricsUtils.LyricsDB) ? MyLyricsUtils.LyricsMarkedDB : MyLyricsUtils.LyricsDB;
        }
    }
}