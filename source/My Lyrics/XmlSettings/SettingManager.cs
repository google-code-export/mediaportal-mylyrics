namespace MyLyrics.XmlSettings
{
    class SettingManager
    {
        // Prefix
        private const string Prefix = "mylyrics";

        #region param names
        public static string PluginsName = "pluginsName";
        public static string Guid = "Guid";

        public static string SitesMode = "sitesMode";
        public static string DefaultSitesModeValue = "defaultSitesModeValue";
        public static string SitePrefix = "use";

        public static string UploadLrcToLrcFinder = "uploadLrcToLrcFinder";
        public static string AlwaysAskUploadLrcToLrcFinder = "alwaysAskUploadLrcToLrcFinder";
        public static string LrcTaggingOffset = "LrcTaggingOffset";
        public static string LrcTaggingName = "LrcTaggingName";

        public static string TranslationLanguage = "translationLanguage";

        public static string UseAutoscroll = "useAutoscroll";

        public static string AutomaticReadFromMusicTag = "automaticReadFromMusicTag";
        public static string AutomaticWriteToMusicTag = "automaticWriteToMusicTag";

        public static string AutomaticFetch = "automaticFetch";
        public static string AutomaticUpdateWhenFirstFound = "automaticUpdateWhenFirstFound";
        public static string MoveLyricFromMarkedDatabase = "moveLyricFromMarkedDatabase";

        public static string ConfirmedNoUploadLrcToLrcFinder = "confirmedNoUploadLrcToLrcFinder";
        public static string UseAutoOnLyricLength = "useAutoOnLyricLength";

        public static string Find = "find";
        public static string Replace = "replace";

        public static string Limit = "limit";

        #endregion

        #region default values

        // Default values
        public static string DefaultPluginName = "My Lyrics";
        public static string DefaultTranslationLanguage = "English (en)";
        public static string DefaultLrcTaggingOffset = "500";

        #endregion

        #region public get methods

        /// <summary>
        /// Get param as string
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as string</returns>
        public static string GetParamAsString(string paramName, string defaultValue)
        {
            using (var xmlreader = MyLyricsCore.MediaPortalSettings)
            {
                return xmlreader.GetValueAsString(Prefix, paramName, defaultValue);
            }
        }

        /// <summary>
        /// Get param as bool
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as bool</returns>
        public static bool GetParamAsBool(string paramName, bool defaultValue)
        {
            using (var xmlreader = MyLyricsCore.MediaPortalSettings)
            {
                return xmlreader.GetValueAsBool(Prefix, paramName, defaultValue);
            }
        }

        /// <summary>
        /// Get param as int
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as int</returns>
        public static int GetParamAsInt(string paramName, int defaultValue)
        {
            using (var xmlreader = MyLyricsCore.MediaPortalSettings)
            {
                return xmlreader.GetValueAsInt(Prefix, paramName, defaultValue);
            }
        }

        #endregion public get methods

        #region public set methods

        /// <summary>
        /// Set param value
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="value">param value</param>
        public static void SetParam(string paramName, string value)
        {
            using (var xmlwriter = MyLyricsCore.MediaPortalSettings)
            {
                xmlwriter.SetValue(Prefix, paramName, value);
            }
        }

        /// <summary>
        /// Set bool param value to "True" or "False"
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="value">param value</param>
        public static void SetParamAsBool(string paramName, bool value)
        {
            SetParam(paramName, value ? "True" : "False");
        }

        #endregion public get methods
    }
}
