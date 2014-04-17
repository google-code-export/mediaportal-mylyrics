using LyricsEngine.LyricsDatabase;

namespace MyLyrics
{
    public class MyLyricsUtils
    {
        // Settings filename
        public static readonly string SettingsFileName = "MediaPortal.xml";

        // Log filename
        public static string LogFileName = "MyLyrics.log";
        public static string OldLogFileName = "MyLyrics.log.bak";

        // Database settings
        internal static LyricsDatabase LyricsDB;
        internal static string LyricsDBName = "LyricsDatabaseV2.db";
        internal static LyricsDatabase LyricsMarkedDB;
        internal static string LyricsMarkedDBName = "LyricsMarkedDatabaseV2.db";
    }
}
