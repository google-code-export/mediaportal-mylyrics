namespace MyLyrics
{
    public class MyLyricsUtils
    {
        // Old Settings filename
        public static readonly string OldSettingsFileName = "MediaPortal.xml";

        // New Settings filename
        public static readonly string SettingsFileName = "MyLyrics.xml";

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
