namespace MyLyrics
{
    internal class MyLyricsSettings
    {
        // Database settings
        internal static string LogName = "MyLyrics.log";
        internal static LyricsDatabase LyricsDB;
        internal static string LyricsDBName = "LyricsDatabaseV2.db";
        internal static LyricsDatabase LyricsMarkedDB;
        internal static string LyricsMarkedDBName = "LyricsMarkedDatabaseV2.db";

        #region Nested type: Screen

        internal enum Screen
        {
            LYRICS = 0,
            LRC = 1,
            LRC_EDITOR = 2,
            LRC_PICK = 3
        }

        #endregion

        // log information
    }
}