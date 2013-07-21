using System;
using System.Diagnostics;
using MediaPortal.Configuration;

namespace MyLyrics.XmlSettings
{
    public static class SettingManager
    {
        // Prefix
        private const string Prefix = "mylyrics";

        public static MediaPortal.Profile.Settings MediaPortalSettings
        {
            get
            {
                return new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, MyLyricsUtils.SettingsFileName));
            }
        }

        #region param names
        public static readonly string PluginsName = "pluginsName";
        public static readonly string Guid = "Guid";

        public static readonly string SitesMode = "sitesMode";
        public static readonly string DefaultSitesModeValue = "defaultSitesModeValue";
        public static readonly string SitePrefix = "use";

        public static readonly string UploadLrcToLrcFinder = "uploadLrcToLrcFinder";
        public static readonly string AlwaysAskUploadLrcToLrcFinder = "alwaysAskUploadLrcToLrcFinder";
        public static readonly string LrcTaggingOffset = "LrcTaggingOffset";
        public static readonly string LrcTaggingName = "LrcTaggingName";

        public static readonly string TranslationLanguage = "translationLanguage";

        public static readonly string UseAutoscroll = "useAutoscroll";

        public static readonly string AutomaticReadFromMusicTag = "automaticReadFromMusicTag";
        public static readonly string AutomaticWriteToMusicTag = "automaticWriteToMusicTag";

        public static readonly string AutomaticFetch = "automaticFetch";
        public static readonly string AutomaticUpdateWhenFirstFound = "automaticUpdateWhenFirstFound";
        public static readonly string MoveLyricFromMarkedDatabase = "moveLyricFromMarkedDatabase";

        public static readonly string ConfirmedNoUploadLrcToLrcFinder = "confirmedNoUploadLrcToLrcFinder";
        public static readonly string UseAutoOnLyricLength = "useAutoOnLyricLength";

        public static readonly string Find = "find";
        public static readonly string Replace = "replace";

        public static readonly string Limit = "limit";

        #endregion

        #region default values

        // Default values
        public static readonly string DefaultPluginName = "My Lyrics";
        public static readonly string DefaultTranslationLanguage = "English (en)";
        public static readonly string DefaultLrcTaggingOffset = "500";

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
            return MediaPortalSettings.GetValueAsString(Prefix, paramName, defaultValue);
        }

        /// <summary>
        /// Get param as bool
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as bool</returns>
        public static bool GetParamAsBool(string paramName, bool defaultValue)
        {
            return MediaPortalSettings.GetValueAsBool(Prefix, paramName, defaultValue);
        }

        /// <summary>
        /// Get param as int
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>param value as int</returns>
        public static int GetParamAsInt(string paramName, int defaultValue)
        {
            return MediaPortalSettings.GetValueAsInt(Prefix, paramName, defaultValue);
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
            MediaPortalSettings.SetValue(Prefix, paramName, value);
        }

        /// <summary>
        /// Set bool param value to "True" or "False"
        /// </summary>
        /// <param name="paramName">param name</param>
        /// <param name="value">param value</param>
        public static void SetParamAsBool(string paramName, bool value)
        {
            MediaPortalSettings.SetValueAsBool(Prefix, paramName, value);
        }

        #endregion public get methods
        
    }
}
