using System;
using System.IO;
using System.Reflection;
using MediaPortal.Configuration;
using MediaPortal.Services;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MyLyrics
{
    public class MyLyricsCore
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Singleton

        private MyLyricsCore()
        {
        }
        private static class MyLyricsCoreHolder
        {
            public static readonly MyLyricsCore Instance = new MyLyricsCore();
        }

        public static MyLyricsCore GetInstance()
        {
            return MyLyricsCoreHolder.Instance;
        }

        #endregion Singleton

        // Should be the first thing that is run whenever the plugin launches, either
        // from the GUI or the Config Screen.
        public void Initialize()
        {
            InitLogger();
            LogStartupBanner();
            // InitSettings();
        }
        
        // Initializes the logging system.
        private void InitLogger()
        {
            var fullLogFilePath = Config.GetFile(Config.Dir.Log, MyLyricsUtils.LogFileName);
            var fullOldLogFilePath = Config.GetFile(Config.Dir.Log, MyLyricsUtils.OldLogFileName);

            // backup the old log file if it exists
            try
            {
                if (File.Exists(fullLogFilePath)) File.Copy(fullLogFilePath, fullOldLogFilePath, true);
                File.Delete(fullLogFilePath);
            }
            catch (Exception e)
            {
                logger.ErrorException(String.Format("Error setting up logging paths{0}", Environment.NewLine), e);
            }

            var config = LogManager.Configuration ?? new LoggingConfiguration();

            // build logging rules for our logger
            var fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, MyLyricsUtils.LogFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss.fff} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";
            config.AddTarget("file", fileTarget);

            var logLevel = GetMediaportalLogLevel();

            // if the plugin was compiled in DEBUG mode, always default to debug logging
            #if DEBUG
            logLevel = LogLevel.Debug;
            #endif

            // add the previously defined rules and targets to the logging configuration
            var rule = new LoggingRule("MyLyrics*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        private LogLevel GetMediaportalLogLevel()
        {
            LogLevel logLevel;
            var xmlreader = MediaPortalSettings;
            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
            {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }
            return logLevel;
        }

        // Logs a startup message to the log files.
        private void LogStartupBanner()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info(String.Format("MyLyrics ({0}.{1}.{2}.{3})", ver.Major, ver.Minor, ver.Build, ver.Revision));
            logger.Info("Plugin launched");
        }

        public static MediaPortal.Profile.Settings MediaPortalSettings
        {
            get
            {
                return new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, MyLyricsUtils.OldSettingsFileName));
            }
        }

        /*
        private void InitSettings()
        {
        }
        */
    }
}
