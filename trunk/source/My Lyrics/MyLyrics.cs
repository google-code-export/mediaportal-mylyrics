using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using LyricsEngine;
using LyricsEngine.LRC;
using LyricsEngine.LyricsDatabase;
using LyricsEngine.LyricsSites;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.TagReader;
using MyLyrics.XmlSettings;
using NLog;
using Timer = System.Timers.Timer;

namespace MyLyrics
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [PluginIcons("MyLyrics.Resources.MyLyrics_icon_enabled.png", "MyLyrics.Resources.MyLyrics_icon_disabled.png")]
    public partial class GUIMyLyrics : GUIWindow, ILyricForm, ISetupForm
    {
        // Make sure everything initialize
        private readonly MyLyricsCore _core = MyLyricsCore.GetInstance();
        // Logger
        private static readonly Logger PluginLogger = LogManager.GetCurrentClassLogger();

        #region Fields related to the MyLyrics in general

        internal const string MLyricsDbName = "LyricsDatabaseV2.db";
        internal const string MLyricsMarkedDbName = "LyricsMarkedDatabaseV2.db";
        internal static LyricsDatabase LyricsDb;
        internal static LyricsDatabase LyricsMarkedDb;
        public static int WindowMylyrics = 90478;
        private readonly ManualResetEvent _eventStopThread;

        private readonly object _imageMutex;
        private readonly List<String> _imagePathContainer;
        private readonly PlayListPlayer _playlistPlayer;
        internal int ControlLyricSelected = 20;
        private bool _alreadyValidLRC;

        // Track info
        private string _artist = "";
        private bool _automaticReadFromMusicTag = true;
        private bool _automaticWriteToMusicTag = true;

        private string _currentThumbFileName = string.Empty;
        private string _currentTrackFileName = string.Empty;
        private string _nextTrackFileName = string.Empty;
        private MusicTag _currentTrackTag;
        private MusicTag _nextTrackTag;

        private string _find = string.Empty;
        private Guid _guid;
        private string _guidString;
        private Timer _imageChangeTimer;
        private string _lastStreamFile = string.Empty;
        private LyricsController _lyricsController;
        private DataTable _lrcTable;

        private bool _lyriccontrollerIsWorking;
        private Thread _lyricControllerThread;
        private bool _lyricsFound;

        private List<string> _nonLrcSitesToSearch = new List<string>();
        private const string LyricsScreenXml = "MyLyrics.xml";
        private List<string[]> _lyricsToWriteToTag;
        private string _lyricText = "";
        private bool _newTrack;
        private string _replace = string.Empty;
        private int _searchingState;
        private int _selectedScreen;
        private int _startingScrollSpeedVertical;
        private string _statusText = "";
        private string[] _strippedPrefixStrings;
        private string _title = "";
        private string _translationLanguage = string.Empty;
        private string _translationLanguageCode = string.Empty;

        private Timer _writeTagTimer;

        private int _selectedinLRCPicker;
        private Timer _lrcPickTimer;
        private bool _isInTranslation;
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _settingsRead;
        private IDictionary _defines;

        #endregion

        [SkinControl((int) GUILRCControls.ControlArtAlbumArt)] protected GUIImage GuIelementImgCoverArt;
        [SkinControl((int) GUILRCControls.ControlArtProgress)] protected GUIProgressControl GuIelementProgTrack;

        private int _searchType;

        #region Fields related to LRC mode

        internal bool ConfirmedNoUploadLrcToLrcFinder;
        internal string LrcTaggingName;
        internal string LrcTaggingOffset;

        private SimpleLRCTimeAndLineCollection _lrcTimeCollection;
        private SimpleLRC _simpleLrc;
        private Stopwatch _stopwatch;
        internal bool UploadLrcToLrcFinder;
        internal bool UseAutoOnLyricLength;
        internal bool UseAutoScrollAsDefault;
        internal bool AlwaysAskUploadLrcToLrcFinder;

        #endregion

        #region Fields releated to the editor mode

        internal const int TagInRound = 13;
        internal int NextLRCLineIndex;
        private string[] _lines;
        internal int LRCLinesTotal;
        internal int Min;
        internal int Msec;
        internal int Sec;
        internal int TagRoundFinished;

        #endregion

        public GUIMyLyrics()
        {
            _eventStopThread = new ManualResetEvent(false);

            _imagePathContainer = new List<string>();
            _imageMutex = new Object();
            _playlistPlayer = PlayListPlayer.SingletonPlayer;

            GetID = WindowMylyrics;
        }

        public override bool Init()
        {
            _selectedScreen = (int) MyLyricsSettings.Screen.LYRICS;

            _startingScrollSpeedVertical = GUIGraphicsContext.ScrollSpeedVertical;
            GUIGraphicsContext.ScrollSpeedVertical = 0;

            _worker.DoWork += SaveLyricToTagLists;

            LoadSkinSettings(GUIGraphicsContext.Skin + @"\" + LyricsScreenXml);

            return Load(GUIGraphicsContext.Skin + @"\" + LyricsScreenXml);
        }


        private void UpdateNextTrackInfo()
        {
            if (_nextTrackTag != null)
            {
                GUIPropertyManager.SetProperty("#Play.Next.Title", _nextTrackTag.Title);
                GUIPropertyManager.SetProperty("#Play.Next.Track", _nextTrackTag.Track > 0 ? _nextTrackTag.Track.ToString(CultureInfo.InvariantCulture) : string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Album", _nextTrackTag.Album);
                GUIPropertyManager.SetProperty("#Play.Next.Artist", _nextTrackTag.Artist);
                GUIPropertyManager.SetProperty("#Play.Next.Genre", _nextTrackTag.Genre);
                GUIPropertyManager.SetProperty("#Play.Next.Year", _nextTrackTag.Year > 1900 ? _nextTrackTag.Year.ToString(CultureInfo.InvariantCulture) : string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Rating", (System.Convert.ToDecimal(2*_nextTrackTag.Rating + 1)).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                GUIPropertyManager.SetProperty("#Play.Next.Title", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Track", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Album", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Artist", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Genre", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Year", string.Empty);
                GUIPropertyManager.SetProperty("#Play.Next.Rating", "0");
            }
        }

        public override void Process()
        {
            if ((_newTrack || _searchingState != (int) SearchState.NotSearching)
                && (!g_Player.IsRadio || !string.IsNullOrEmpty(_artist)))
                //&& (!g_Player.IsRadio || _CurrentTrackTag!=null))
            {

                if (_newTrack)
                {
                    HandleNewTrack();
                }

                if (_currentTrackTag != null || g_Player.IsRadio)
                {
                    if (!g_Player.IsRadio)
                    {
                        if (!string.IsNullOrEmpty(_currentTrackTag.Artist))
                        {
                            UpdateGuiPropertiesFromCurrentTrackTag();
                        }

                        _currentTrackTag.Lyrics = LyricUtil.FixLyrics(_currentTrackTag.Lyrics);

                        UpdateNextTrackInfo();
                    }

                    if (_selectedScreen == (int) MyLyricsSettings.Screen.LYRICS
                        || _selectedScreen == (int) MyLyricsSettings.Screen.LRC
                        || _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                    {
                        // Get lyric
                        if (_artist.Length != 0 && _title.Length != 0)
                        {
                            GetLyrics();
                        }
                        else if ((_artist.Length == 0 && _title.Length > 0) || (_title.Length == 0 && _artist.Length > 0))
                        {
                            ResetLrcFields();
                            ResetGUI(_selectedScreen);

                            _statusText = "Not enough data for lyric search";
                            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                        }
                        else
                        {
                            _artist = "";
                            _title = "";
                            _lyricText = "";
                            _imagePathContainer.Clear();
                            GuIelementImgCoverArt.Dispose();

                            ResetLrcFields();

                            ResetGUI(_selectedScreen);

                            _statusText = "No music file is playing";
                            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                        }
                    }
                    else if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
                    {
                        _newTrack = false;
                        NextLRCLineIndex = 0;
                        LRCLinesTotal = 0;
                        TagRoundFinished = 0;

                        _artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
                        _title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));

                        if (!_lyricText.Equals(string.Empty))
                        {
                            ShowLRCtoEdit();
                        }
                        else if (
                            DatabaseUtil.IsSongInLyricsDatabase(LyricsDb, _artist, _title).Equals(
                                DatabaseUtil.LyricFound))
                        {
                            var item = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)];
                            _lyricText = item.Lyrics;
                            ShowLRCtoEdit();
                        }
                        else
                        {
                            _lyricsFound = false;

                            if (!string.IsNullOrEmpty(_artist) && !string.IsNullOrEmpty(_title))
                            {
                                StopThread();
                                _searchType = (int) SearchTypes.BothLRCsAndLyrics;
                                ResetGUI((int) MyLyricsSettings.Screen.LRC);
                                _newTrack = true;
                                //FindLyric();
                            }
                            else if ((_artist.Length == 0 && _title.Length > 0) ||
                                     (_title.Length == 0 && _artist.Length > 0))
                            {
                                //_ImagePathContainer.Clear();
                                //GUIelement_ImgCoverArt.Dispose();

                                ResetLrcFields();
                                ResetGUI(_selectedScreen);

                                _statusText = "Not enough data for lyric search";
                                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                            }
                            else
                            {
                                _artist = "";
                                _title = "";
                                _lyricText = "";
                                _imagePathContainer.Clear();
                                GuIelementImgCoverArt.Dispose();

                                ResetGUI(_selectedScreen);

                                _statusText = "No music file is playing";
                                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                            }
                        }
                    }
                }
            }

            if (_lyricsFound)
            {
                if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Lyrics")))
                {
                    GUIPropertyManager.SetProperty("#Play.Current.Lyrics", _lyricText);
                }

                if (_selectedScreen == (int) MyLyricsSettings.Screen.LRC
                    || _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                {
                    CalculateNextInterval();
                }
            }

            //GUIGraphicsContext.ResetLastActivity();

            base.Process();
        }

        private void GetLyrics()
        {
            if (_searchingState == (int) SearchState.NotSearching)
            {
                var lrcActiveSites =
                    LyricsSiteFactory.LrcLyricsSiteNames()
                        .Where(lrcSite => Setup.GetInstance().ActiveSites.Contains(lrcSite))
                        .ToList();
                if (lrcActiveSites.Count > 0 &&
                    (_searchType == (int) SearchTypes.BothLRCsAndLyrics
                     || _searchType == (int) SearchTypes.OnlyLRCs))
                {
                    var lrcFoundInTagOrLyricDb = FindLrc();
                    if (lrcFoundInTagOrLyricDb)
                    {
                        _searchingState = (int) SearchState.NotSearching;
                    }
                    else
                    {
                        _searchingState = (int) SearchState.SearchingForLRC;
                    }
                }
                else
                {
                    var lyricFoundInTagOrLyricDb = FindLyric();
                    if (lyricFoundInTagOrLyricDb)
                    {
                        _searchingState = (int) SearchState.NotSearching;
                    }
                    else
                    {
                        _searchingState = (int) SearchState.SearchingForLyric;
                    }
                }
            }
            else if (_searchingState == (int) SearchState.SearchingForLRC &&
                     !_lyriccontrollerIsWorking)
            {
                if (_searchType == (int) SearchTypes.BothLRCsAndLyrics
                    || _searchType == (int) SearchTypes.OnlyLYRICS)
                {
                    FindLyric();
                    _searchingState = (int) SearchState.SearchingForLyric;
                }
                else
                {
                    _searchingState = (int) SearchState.NotSearching;
                }
            }
            else if (_searchingState == (int) SearchState.SearchingForLyric &&
                     !_lyriccontrollerIsWorking)
            {
                _searchingState = (int) SearchState.NotSearching;
            }
        }

        private void UpdateGuiPropertiesFromCurrentTrackTag()
        {
            GUIPropertyManager.SetProperty("#Play.Current.Artist", _currentTrackTag.Artist);
            GUIPropertyManager.SetProperty("#Play.Current.Title", _currentTrackTag.Title);
            GUIPropertyManager.SetProperty("#Play.Current.Track", _currentTrackTag.Track > 0 ? _currentTrackTag.Track.ToString(CultureInfo.InvariantCulture) : string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Album", _currentTrackTag.Album);
            GUIPropertyManager.SetProperty("#Play.Current.Genre", _currentTrackTag.Genre);
            GUIPropertyManager.SetProperty("#Play.Current.Year", _currentTrackTag.Year > 1900 ? _currentTrackTag.Year.ToString(CultureInfo.InvariantCulture) : string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Rating", (System.Convert.ToDecimal(2*_currentTrackTag.Rating + 1)).ToString(CultureInfo.InvariantCulture));
            GUIPropertyManager.SetProperty("#duration", MediaPortal.Util.Utils.SecondsToHMSString(_currentTrackTag.Duration));
        }

        private void HandleNewTrack()
        {
            _alreadyValidLRC = false;

            _imagePathContainer.Clear();

            _lyricsFound = false;
            StopThread();
            _newTrack = false;

            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, "");

            if (g_Player.IsRadio == false)
            {
                _currentTrackFileName = g_Player.CurrentFile;
                _nextTrackFileName = PlayListPlayer.SingletonPlayer.GetNext();
                //_CurrentTrackTag = mDB.GetTag(g_Player.CurrentFile);
                GetTrackTags();

                if (_currentTrackTag != null)
                {
                    GetAlbumArt();
                    GetAlbumArt(_artist);

                    // Outcommented some code that led to a serious bug were first found lyrics always was saved in lyrics database for all following tracks and accordingly always shown on screen!
                    //if (currentLyrics != null && currentFile != null && _CurrentTrackTag.FileName == currentFile)
                    //{
                    //    if (_CurrentTrackTag.Lyrics != currentLyrics)
                    //    {
                    //        _CurrentTrackTag.Lyrics = currentLyrics;
                    //    }
                    //}
                }
                else
                {
                    _artist = "";
                    _title = "";
                    _lyricText = "";
                    _imagePathContainer.Clear();
                    GuIelementImgCoverArt.Dispose();

                    ResetLrcFields();
                    ResetGUI(_selectedScreen);

                    _statusText = "No music file is playing";
                    GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                }
            }
            else
            {
                GetAlbumArt(_artist);
            }
        }

        private void UpdateArtistAndTitleFromTrackTag()
        {
            _artist = _currentTrackTag.Artist.Trim();
            _artist = _artist.Replace("| ", "");
            _artist = _artist.Replace(" |", "");
            _artist = _artist.Replace("''", "'");
            _artist = LyricUtil.CapatalizeString(_artist);

            _title = _currentTrackTag.Title.Trim();
            _title = _title.Replace("''", "'");
            _title = LyricUtil.CapatalizeString(_title);
        }

        private bool CheckReallyEditLRCBeforeEdit()
        {
            var lrc = new SimpleLRC(_artist, _title, _lyricText);

            if (lrc.IsValid)
            {
                var dlgYesNo = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
                if (dlgYesNo != null)
                {
                    dlgYesNo.Reset();

                    dlgYesNo.SetHeading("Edit existing LRC?");

                    dlgYesNo.SetLine(1, "This song already has a valid LRC lyric.");
                    dlgYesNo.SetLine(2, "Do you really want to edit it?");

                    dlgYesNo.DoModal(GetID);

                    if (dlgYesNo.IsConfirmed)
                    {
                        _lyricText = lrc.LyricAsPlainLyric;
                        return true;
                    }
                    return false;
                }
                //_alreadyValidLRC = true;
                //_lines = new string[1] { "This song already has a valid LRC lyric" };
                //_StatusText = "This song already has a valid LRC lyric";
                //GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _lines[0]);
            }
            return true;
        }

        private void ShowLRCtoEdit()
        {
            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, "");

            if (!string.IsNullOrEmpty(_lyricText))
            {
                _lyricText = _lyricText.Trim();

                //else
                {
                    ResetLrcFields();

                    _lines = _lyricText.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

                    try
                    {
                        for (var i = TagRoundFinished * TagInRound;
                             i < (TagRoundFinished + 1) * TagInRound;
                             i++)
                        {
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                            //GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, _lines[i]);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, _lines[i]);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, "[xx:xx.xx]");
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                _statusText = "No valid lyric found";
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
            }

            if (_currentTrackTag == null)
            {
                ResetGUI(_selectedScreen);

                _statusText = "No music file is playing";
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
            }
        }

        private void LoadSkinSettings(string skinXMLFile)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(skinXMLFile);
                if (doc.DocumentElement != null)
                {
                    _defines = LoadDefines(doc);
                }
            }
            catch
            {
                ;
            }
        }

        private IDictionary LoadDefines(XmlDocument document)
        {
            var table = new Hashtable();
            try
            {
                foreach (XmlNode node in document.SelectNodes("/window/define"))
                {
                    var tokens = node.InnerText.Split(':');
                    if (tokens.Length < 2)
                    {
                        continue;
                    }
                    table[tokens[0]] = tokens[1];
                }
            }
            catch
            {
                ;
            }
            return table;
        }

        private bool EnableMouseControl()
        {
            var enableMouseControl = String.Empty;
            if (_defines != null && _defines.Contains("#MyLyrics.EnableMouseControl"))
                enableMouseControl = (string) _defines["#MyLyrics.EnableMouseControl"];
            try
            {
                if (enableMouseControl.ToUpper() == "TRUE" || enableMouseControl.ToUpper() == "YES")
                    return true;
            }
            catch
            {
            }
            return false;
        }

        private bool UseEditControlsOnLRCPick()
        {
            var useEditControlsOnLRCPick = String.Empty;
            if (_defines != null && _defines.Contains("#MyLyrics.UseEditControlsOnLRCPick"))
                useEditControlsOnLRCPick = (string) _defines["#MyLyrics.UseEditControlsOnLRCPick"];
            try
            {
                if (useEditControlsOnLRCPick.ToUpper() == "TRUE" || useEditControlsOnLRCPick.ToUpper() == "YES")
                    return true;
            }
            catch
            {
            }
            return false;
        }

        private bool IsFocusedBlacklisted()
        {
            var focusID = GetFocusControlId();
            var blacklistedControlIDs = String.Empty;
            if (_defines != null && _defines.Contains("#MyLyrics.BlacklistedControlIDs"))
            {
				blacklistedControlIDs = (string) _defines["#MyLyrics.BlacklistedControlIDs"];
            }
            foreach (var cID in blacklistedControlIDs.Split(new[] {','}))
            {
                try
                {
                    if (focusID == int.Parse(cID))
                        return true;
                }
                catch
                {
                    ;
                }
            }
            return false;
        }

        private void LoadSettings()
        {
            _newTrack = true;
            _searchingState = (int) SearchState.NotSearching;
            _searchType = (int) SearchTypes.BothLRCsAndLyrics;

            _lyricsToWriteToTag = new List<string[]>();

            ResetGUI(_selectedScreen);

            // Get MediaPortal internal configuration
            using (var xmlreader = SettingManager.MediaPortalSettings)
            {
                xmlreader.GetValueAsBool("musicfiles", "showid3", true);

                xmlreader.GetValueAsString("skin", "name", "Blue3");

                xmlreader.GetValueAsInt("audioplayer", "crossfade", 2000);
            }

            Setup.GetInstance().ActiveSites.Clear();
            foreach (var site in from site in LyricsSiteFactory.LyricsSitesNames() let active = SettingManager.GetParamAsBool(SettingManager.SitePrefix + site, false) where active select site)
            {
                Setup.GetInstance().ActiveSites.Add(site);
            }

            _automaticWriteToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, false);
            _automaticReadFromMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticReadFromMusicTag, false);

            UseAutoScrollAsDefault = SettingManager.GetParamAsBool(SettingManager.UseAutoscroll, true);
            UseAutoOnLyricLength = SettingManager.GetParamAsBool(SettingManager.UseAutoOnLyricLength, false);
            LrcTaggingOffset = SettingManager.GetParamAsString(SettingManager.LrcTaggingOffset, SettingManager.DefaultLrcTaggingOffset);

            var strButtonText = SettingManager.GetParamAsString(SettingManager.PluginsName, SettingManager.DefaultPluginName);
            GUIPropertyManager.SetProperty("#currentmodule", strButtonText);

            LrcTaggingName = SettingManager.GetParamAsString(SettingManager.LrcTaggingName, "");

            UploadLrcToLrcFinder = SettingManager.GetParamAsBool(SettingManager.UploadLrcToLrcFinder, false);
            ConfirmedNoUploadLrcToLrcFinder = SettingManager.GetParamAsBool(SettingManager.ConfirmedNoUploadLrcToLrcFinder, false);
            AlwaysAskUploadLrcToLrcFinder = SettingManager.GetParamAsBool(SettingManager.AlwaysAskUploadLrcToLrcFinder, false);

            var translationString = SettingManager.GetParamAsString(SettingManager.TranslationLanguage, SettingManager.DefaultTranslationLanguage);

            var strings = translationString.Split(new[] {"("}, StringSplitOptions.None);
            _translationLanguage = strings[0].Trim();
            _translationLanguageCode = strings[1].Replace(")", string.Empty);

            _guidString = SettingManager.GetParamAsString(SettingManager.Guid, "");

            _find = SettingManager.GetParamAsString(SettingManager.Find, "");
            _replace = SettingManager.GetParamAsString(SettingManager.Replace, "");

            if (!_settingsRead) // only first time
            {
                if (UseAutoScrollAsDefault)
                {
                    ControlLyricSelected = (int) GUILyricsControls.ControlLyricScroll;

                    GUIControl.HideControl(GetID, (int) GUILyricsControls.ControlLyric);
                }
                else
                {
                    ControlLyricSelected = (int) GUILyricsControls.ControlLyric;

                    GUIControl.HideControl(GetID, (int) GUILyricsControls.ControlLyricScroll);
                }
            }

            if (string.IsNullOrEmpty(_guidString))
            {
                _guid = Guid.NewGuid();
                _guidString = _guid.ToString("P");
                
                SettingManager.SetParam(SettingManager.Guid, _guidString);
            }
            else
            {
                _guid = new Guid(_guidString);
            }


            _strippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            _nonLrcSitesToSearch = Setup.GetInstance().ActiveSites.Where(site => !LyricsSiteFactory.LrcLyricsSiteNames().Contains(site)).ToList();

            _settingsRead = true;

            // Deserialize lyrics and marked database, and save references in LyricsDB
            try
            {
                var path = Config.GetFile(Config.Dir.Database, MLyricsDbName);
                var fs = new FileStream(path, FileMode.Open);
                var bf = new BinaryFormatter();
                LyricsDb = (LyricsDatabase) bf.Deserialize(fs);
                fs.Close();

                path = Config.GetFile(Config.Dir.Database, MLyricsMarkedDbName);
                fs = new FileStream(path, FileMode.Open);
                LyricsMarkedDb = (LyricsDatabase) bf.Deserialize(fs);
                fs.Close();
            }
            catch
            {
                var dlg = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                dlg.SetHeading("No lyric database found");
                dlg.SetLine(1, "Please run the MyLyrics configuration");
                dlg.SetLine(2, "before running the plugin.");
                dlg.SetLine(3, String.Empty);
                dlg.DoModal(GUIWindowManager.ActiveWindow);
            }
        }


        protected override void OnPageLoad()
        {
            LoadSettings();

            GUIPropertyManager.OnPropertyChanged += trackChangeHandler;

            //_OkToCallPreviousMenu = false;

            base.OnPageLoad();

            if (_imageChangeTimer == null)
            {
                _imageChangeTimer = new Timer();
                _imageChangeTimer.Interval = 15*1000;
                _imageChangeTimer.Elapsed += OnImageTimerTickEvent;
                _imageChangeTimer.Start();
            }

            if (_lrcPickTimer == null)
            {
                _lrcPickTimer = new Timer();
                _lrcPickTimer.Interval = 3*1000;
                _lrcPickTimer.Elapsed += OnLRCPickTimerTickEvent;
                _lrcPickTimer.Stop();
            }
            
            PluginLogger.Info("MyLyrics opens");
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            StopThread();
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }
            ResetAll();
            ResetGUI((int) MyLyricsSettings.Screen.LYRICS);

            GUIGraphicsContext.ScrollSpeedVertical = _startingScrollSpeedVertical;

            if (_writeTagTimer != null)
            {
                _writeTagTimer.Stop();
                _writeTagTimer.Close();
                _writeTagTimer.Dispose();
                _writeTagTimer = null;
            }

            if (_automaticWriteToMusicTag)
            {
                foreach (var pair in _lyricsToWriteToTag)
                {
                    TagReaderUtil.WriteLyrics(pair[0], pair[1]);
                }
            }
            
            PluginLogger.Info("MyLyrics closes");

            //deregister the handler!
            GUIPropertyManager.OnPropertyChanged -= trackChangeHandler;
        }

        public override bool OnMessage(GUIMessage message)
        {
            // Check if the message was ment for this control.
            if ((
                    message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET ||
                    message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_RESET
                ) &&
                message.TargetWindowId == GetID &&
                message.TargetControlId == (int) GUILRCControls.ControlLrcPickStatus)
            {
                _lrcPickTimer.Stop();
            }
            return base.OnMessage(message);
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action)
        {
            if (!EnableMouseControl())
            {
                if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOUSE_MOVE)
                {
                    return;
                }
            }

            base.OnAction(action);

            switch (action.wID)
            {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                    {
                        //_OkToCallPreviousMenu = true;
                        //GUIWindowManager.ShowPreviousWindow();
                        //return;
                        break;
                    }
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:
                    {
                        if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                        {
                            if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT)
                            {
                                _selectedinLRCPicker--;
                            }
                            if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT)
                            {
                                _selectedinLRCPicker++;
                            }

                            if (_selectedinLRCPicker < 0)
                            {
                                _selectedinLRCPicker = _lrcTable.Rows.Count - 1;
                            }
                            if (_selectedinLRCPicker > _lrcTable.Rows.Count - 1)
                            {
                                _selectedinLRCPicker = 0;
                            }


                            var status = string.Format("LRC {0} of {1} shown", _selectedinLRCPicker + 1, _lrcTable.Rows.Count);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);

                            _lyricText = _lrcTable.Rows[_selectedinLRCPicker]["Lyrics"] as string;
                            _simpleLrc = new SimpleLRC(null, null, _lyricText);
                            StartShowingLrc(_lyricText, true);
                        }
                        break;
                    }
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
                    {
                        if (!IsFocusedBlacklisted())
                        {
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick && !action.m_key.KeyChar.Equals(42))
                            {
                                ShowLrcPick();
                            }
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
                            {
                                TagLine();
                            }
                        }
                        break;
                    }
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_KEY_PRESSED:
                    {
                        if ( /*action.m_key.KeyChar.Equals(13) || */action.m_key.KeyChar.Equals(35) ||
                                                                    action.m_key.KeyChar.Equals(41))
                            // 'Enter' or '#' or ')'
                        {
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick && !action.m_key.KeyChar.Equals(35))
                            {
                                ShowLrcPick();
                            }
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
                            {
                                TagLine();
                            }
                        }
                        else if (action.m_key.KeyChar.Equals(48) || action.m_key.KeyChar.Equals(101)) // '0' or 'E'
                        {
                            // Don't use a stream to create a LRC
                            if (g_Player.IsRadio || String.IsNullOrEmpty(_artist))
                            {
                                break;
                            }

                            _lyricsFound = false;
                            if (_selectedScreen != (int) MyLyricsSettings.Screen.LRCEditor)
                            {
                                if (CheckReallyEditLRCBeforeEdit())
                                {
                                    ResetGUI((int) MyLyricsSettings.Screen.LRCEditor);
                                    ShowLRCtoEdit();
                                    Process();
                                }
                            }
                            else
                            {
                                // parameter could be anything but LRC_EDITOR. Will find correct type when running findLyric().
                                if (_searchType == (int) SearchTypes.BothLRCsAndLyrics ||
                                    _searchType == (int) SearchTypes.OnlyLRCs)
                                {
                                    ResetGUI((int) MyLyricsSettings.Screen.LRC);
                                }
                                else
                                {
                                    ResetGUI((int) MyLyricsSettings.Screen.LYRICS);
                                }
                                //resetGUI((int) MyLyricsSettings.Screen.LYRICS);
                                _newTrack = true;
                                Process();
                            }
                        }
                        else if (action.m_key.KeyChar.Equals(115)) // 'S' 
                        {
                            if (GUIGraphicsContext.ScrollSpeedVertical >= 10)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 0;
                            }
                            else if (GUIGraphicsContext.ScrollSpeedVertical >= 8)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 10;
                            }
                            else if (GUIGraphicsContext.ScrollSpeedVertical >= 6)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 8;
                            }
                            else if (GUIGraphicsContext.ScrollSpeedVertical >= 4)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 7;
                            }
                            else if (GUIGraphicsContext.ScrollSpeedVertical >= 2)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 4;
                            }
                            else if (GUIGraphicsContext.ScrollSpeedVertical >= 0)
                            {
                                GUIGraphicsContext.ScrollSpeedVertical = 2;
                            }
                        }
                        else if (action.m_key.KeyChar.Equals(102)) // 'F'
                        {
                            var lyricText = String.Empty;
                            if (LyricsDb.ContainsKey(DatabaseUtil.CorrectKeyFormat(_artist, _title)))
                            {
                                lyricText = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)].Lyrics;
                            }
                            if (lyricText != null && (new SimpleLRC(_artist, _title, lyricText)).IsValid)
                            {
                            }

                            var shouldNewTrack = true;
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor ||
                                _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick || _isInTranslation)
                            {
                                //shouldNewTrack = true;
                            }
                            else
                            {
                                if (_searchType == (int) SearchTypes.BothLRCsAndLyrics)
                                {
                                    if (_selectedScreen == (int) MyLyricsSettings.Screen.LRC)
                                    {
                                        _searchType = (int) SearchTypes.OnlyLYRICS;
                                    }
                                    else
                                    {
                                        _searchType = (int) SearchTypes.OnlyLRCs;
                                    }
                                }
                                else if (_searchType == (int) SearchTypes.OnlyLRCs)
                                {
                                    _searchType = (int) SearchTypes.OnlyLYRICS;
                                }
                                else
                                {
                                    _searchType = (int) SearchTypes.OnlyLRCs;
                                }

                                //if (hasValidLRC && _SearchType == (int)SEARCH_TYPES.ONLY_LYRICS)
                                //    _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;

                                var lrcActiveSites = LyricsSiteFactory.LrcLyricsSiteNames().Where(lrcSite => Setup.GetInstance().ActiveSites.Contains(lrcSite)).ToList();
                                if (_searchType != (int)SearchTypes.OnlyLYRICS && lrcActiveSites.Count == 0)
                                {
                                    _searchType = (int) SearchTypes.OnlyLYRICS;

                                    var dlg2 =
                                        (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                                    dlg2.SetHeading("LRC screen disabled");
                                    dlg2.SetLine(1, "You haven't enabled LRCFinder");
                                    dlg2.SetLine(2, "in configuration.");
                                    dlg2.SetLine(3, "LRC screen is disabled!");
                                    dlg2.DoModal(GUIWindowManager.ActiveWindow);

                                    return;
                                }

                                //resetGUI((int)MyLyricsSettings.Screen.LRC);

                                shouldNewTrack = true;
                            }

                            if (_searchType == (int) SearchTypes.BothLRCsAndLyrics ||
                                _searchType == (int) SearchTypes.OnlyLRCs)
                            {
                                ResetGUI((int) MyLyricsSettings.Screen.LRC);
                            }
                            else
                            {
                                ResetGUI((int) MyLyricsSettings.Screen.LYRICS);
                            }

                            _newTrack = shouldNewTrack;
                        }
                        else if (action.m_key.KeyChar.Equals(112)) // 'P'
                        {
                            ShowLrcPick();
                        }
                        else if (49 <= action.m_key.KeyChar && action.m_key.KeyChar <= 57) // '1'-'9'
                        {
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                            {
                                var index = action.m_key.KeyChar - 49;
                                var noOfRows = index + 1;

                                if (noOfRows <= _lrcTable.Rows.Count)
                                {
                                    var status = string.Format("LRC {0} of {1} shown", noOfRows, _lrcTable.Rows.Count);
                                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus,
                                                               status);

                                    _lyricText = _lrcTable.Rows[index]["Lyrics"] as string;
                                    _simpleLrc = new SimpleLRC(null, null, _lyricText);
                                    StartShowingLrc(_lyricText, true);
                                }
                            }
                        }
                        else if (action.m_key.KeyChar.Equals(8) || action.m_key.KeyChar.Equals(42) || action.m_key.KeyChar.Equals(40)) // 'Backslash' or '*'
                        {
                            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
                            {
                                RemoveLatestTagLine();
                            }
                        }
                        else if (action.m_key.KeyChar.Equals(98)) // 'B' (stop playing media)
                        {
                            ResetLrcFields();
                            ResetGUI(_selectedScreen);
                        }
                        break;
                    }
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_REWIND:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_FORWARD:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_FORWARD:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_PLAY:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_MUSIC_REWIND:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PAUSE:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PLAY:
                    {
                        break;
                    }
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_EXIT:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_END:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_SHUTDOWN:
                    {
                        break;
                    }
            }
        }

        private void ShowLrcPick()
        {
            if (String.IsNullOrEmpty(_artist)) return;

            if (_selectedScreen != (int) MyLyricsSettings.Screen.LRCPick)
            {
                var lrcFinder = new LrcFinder(_artist, _title, null, 0);
                _lrcTable = lrcFinder.FindLRCs();

                if (_lrcTable != null && _lrcTable.Rows.Count > 0)
                {
                    _selectedinLRCPicker = 0;
                    if (_simpleLrc != null)
                    {
                        for (var i = 0; i < _lrcTable.Rows.Count; i++)
                        {
                            var simpleLrcTemp = new SimpleLRC(null, null, _lrcTable.Rows[i]["Lyrics"] as string);
                            if (_simpleLrc.LyricAsLRC == simpleLrcTemp.LyricAsLRC)
                            {
                                _selectedinLRCPicker = i;
                                break;
                            }
                        }
                    }
                    _lyricText = _lrcTable.Rows[_selectedinLRCPicker]["Lyrics"] as string;
                    _simpleLrc = new SimpleLRC(null, null, _lyricText);
                    StartShowingLrc(_lyricText, true);

                    string status;

                    if (_lrcTable.Rows.Count == 1)
                    {
                        status = "One LRC file found";
                    }
                    else
                    {
                        status = string.Format("LRC {0} of {1} shown", _selectedinLRCPicker + 1, _lrcTable.Rows.Count);
                    }

                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);
                }
                else if (_lrcTable == null)
                {
                    //resetGUI((int) MyLyricsSettings.Screen.LRC_PICK);

                    const string status = "LrcFinder could not be reached";
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);
                    _lrcPickTimer.Stop();
                    _lrcPickTimer.Start();
                }
                else
                {
                    //resetGUI((int)MyLyricsSettings.Screen.LRC_PICK);

                    const string status = "No LRC found";
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);
                    _lrcPickTimer.Stop();
                    _lrcPickTimer.Start();

                    //string lyricInfo = "Press the 'P' key to return";
                    //GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, lyricInfo);
                }
            }
            else
            {
                SaveLyricToDatabase(_artist, _title, _lyricText, "MyLyrics LRC Pick", true);

                if (_currentTrackTag != null)
                {
                    if (_worker.IsBusy)
                    {
                        _worker.CancelAsync();
                    }
                    var data = new SaveLyricToTagListsData(_lyricText, _artist, _title, _currentTrackTag.FileName);
                    //worker.RunWorkerAsync(data);
                    //SaveLyricToTagLists(_CurrentTrackTag.FileName, _LyricText);
                    SaveLyricToTagLists(this, new DoWorkEventArgs(data));
                }

                _lrcTable = null;
                _newTrack = true;
                _searchingState = (int) SearchState.NotSearching;

                Process();
            }
        }


        protected override void OnShowContextMenu()
        {
            var dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return;
            }

            dlg.Reset();
            dlg.SetHeading(498); // menu

            dlg.Add("Find LRC");
            dlg.Add("Find lyric");
            dlg.Add("Make LRC");

            var translateLabelString = "Translate to " + _translationLanguage.ToLower();
            dlg.Add(translateLabelString);

            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
            {
                dlg.Add("Use picked LRC");
            }
            else
            {
                dlg.Add("Pick LRC");
            }

            if (ControlLyricSelected == (int) GUILyricsControls.ControlLyric)
            {
                dlg.Add("Show scrolling lyric");
            }
            else
            {
                dlg.Add("Show static lyric");
            }


            dlg.DoModal(GetID);
            if (dlg.SelectedLabel == -1)
            {
                if (ControlLyricSelected == (int) GUILyricsControls.ControlLyric)
                {
                    GUIControl.FocusControl(GetID, ControlLyricSelected);
                }
                else if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                {
                    GUIControl.FocusControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                }
                else
                {
                    return;
                }
            }
            switch (dlg.SelectedLabelText)
            {
                case "Find LRC":
                    var lrcActiveSites = LyricsSiteFactory.LrcLyricsSiteNames().Where(lrcSite => Setup.GetInstance().ActiveSites.Contains(lrcSite)).ToList();
                    if (lrcActiveSites.Count > 0)
                    {
                        _searchType = (int) SearchTypes.OnlyLRCs;

                        //_selectedScreen = (int) MyLyricsSettings.Screen.LRC;
                        ResetGUI((int) MyLyricsSettings.Screen.LRC);

                        _newTrack = true;
                    }
                    else
                    {
                        var dlg2 = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                        dlg2.SetHeading("LRC screen disabled");
                        dlg2.SetLine(1, "You haven't enabled LRCFinder");
                        dlg2.SetLine(2, "in configuration.");
                        dlg2.SetLine(3, "LRC screen is disabled!");
                        dlg2.DoModal(GUIWindowManager.ActiveWindow);
                    }
                    break;

                case "Find lyric":
                    _searchType = (int) SearchTypes.OnlyLYRICS;

                    //_selectedScreen = (int) MyLyricsSettings.Screen.LYRICS;
                    ResetGUI((int) MyLyricsSettings.Screen.LYRICS);

                    _newTrack = true;
                    break;

                case "Make LRC":
                    // Don't use a stream to create a LRC
                    if (g_Player.IsRadio)
                    {
                        var dlg2 = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                        dlg2.SetHeading("LRC editor error");
                        dlg2.SetLine(1, "You cannot tag a streaming");
                        dlg2.SetLine(2, "media due to inproper timestamps.");
                        dlg2.SetLine(3, String.Empty);
                        dlg2.DoModal(GUIWindowManager.ActiveWindow);
                        return;
                    }
                    if (String.IsNullOrEmpty(_artist))
                        break;

                    //GUIControl.HideControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll);
                    //CONTROL_LYRIC_SELECTED = (int) GUI_Lyrics_Controls.CONTROL_Lyric;

                    //_lyricsFound = false;
                    if (_selectedScreen != (int) MyLyricsSettings.Screen.LRCEditor)
                    {
                        if (CheckReallyEditLRCBeforeEdit())
                        {
                            ResetGUI((int) MyLyricsSettings.Screen.LRCEditor);
                            ShowLRCtoEdit();
                            Process();
                        }
                    }
                    break;


                case "Pick LRC":
                case "Use picked LRC":
                    ShowLrcPick();
                    break;

                case "Show scrolling lyric":
                case "Show static lyric":

                    _searchType = (int) SearchTypes.OnlyLYRICS;

                    //_selectedScreen = (int)MyLyricsSettings.Screen.LYRICS;
                    ResetGUI((int) MyLyricsSettings.Screen.LYRICS);

                    _newTrack = true;

                    if (ControlLyricSelected == (int) GUILyricsControls.ControlLyric)
                    {
                        ControlLyricSelected = (int) GUILyricsControls.ControlLyricScroll;
                        GUIControl.ShowControl(GetID, (int) GUILyricsControls.ControlLyricScroll);
                        GUIControl.HideControl(GetID, (int) GUILyricsControls.ControlLyric);
                    }
                    else
                    {
                        ControlLyricSelected = (int) GUILyricsControls.ControlLyric;
                        GUIControl.HideControl(GetID, (int) GUILyricsControls.ControlLyricScroll);
                        GUIControl.ShowControl(GetID, (int) GUILyricsControls.ControlLyric);
                    }

                    break;
            }

            if (dlg.SelectedLabelText.Equals(translateLabelString))
            {
                TranslateProvider.TranslateProvider translate = null;

                string lyricToTranslate;

                var lrc = new SimpleLRC(_artist, _title, _lyricText);
                if (lrc.IsValid)
                {
                    lyricToTranslate = lrc.LyricAsPlainLyric;
                }
                else
                {
                    lyricToTranslate = _lyricText;
                }

                if (String.IsNullOrEmpty(lyricToTranslate)) return;

                _statusText = string.Empty;
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);

                try
                {
                    translate = new TranslateProvider.TranslateProvider("www.team-mediaportal.com/MyLyrics");
                }
                catch (FileNotFoundException)
                {
                    var dlg3 = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                    dlg3.SetHeading("File not found");
                    dlg3.SetLine(1, "The TranslateProvider.dll assembly");
                    dlg3.SetLine(2, "could not be located!");
                    dlg3.SetLine(3, String.Empty);
                    dlg3.DoModal(GUIWindowManager.ActiveWindow);
                }

                lyricToTranslate = lyricToTranslate.Replace("\n", "\n ");

                string translation;
                try
                {
                    string detectedLanguage;
                    bool reliable;
                    double confidence;
                    translation = translate.Translate(lyricToTranslate, _translationLanguageCode,
                                                      out detectedLanguage, out reliable, out confidence, "\n");
                }
                catch
                {
                    var dlg3 = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
                    dlg3.SetHeading("Error");
                    dlg3.SetLine(1, "Error occured while trying to translate!");
                    dlg3.SetLine(2, String.Empty);
                    dlg3.SetLine(3, String.Empty);
                    dlg3.DoModal(GUIWindowManager.ActiveWindow);
                    return;
                }

                translation = translation.Replace("\n ", "\n");

                translation = MediaPortalUtil.MakePlainLyricPerfectToShow(translation);

                ResetGUI((int) MyLyricsSettings.Screen.LYRICS);
                _isInTranslation = true;
                GUIControl.SetControlLabel(GetID, ControlLyricSelected, translation);
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlTitle,
                                           string.Format("{0} translation", _translationLanguage));

                GUIControl.FocusControl(GetID, ControlLyricSelected);
            }
        }


        //event driven handler to detect track change
        private void trackChangeHandler(string tag2, string value)
        {
            if (tag2.Contains("#percentage") || tag2.Contains("#currentplaytime") || tag2.Contains("#currentremaining"))
            {
                return;
            }

            if (tag2.Equals("#Play.Current.Title") || tag2.Equals("#Play.Current.Artist") ||
                tag2.Equals("#Play.Current.File")) // track has changed
            {
                if (value.Length != 0) // additional check        
                {
                    ResetLrcFields();
                    ResetGUI(_selectedScreen);
                    StopThread();
                    ResetAll();
                    _newTrack = true;
                    _searchType = (int) SearchTypes.BothLRCsAndLyrics;
                    _searchingState = (int) SearchState.NotSearching;
                }
                else
                {
                    _artist = "";
                    _title = "";
                    _lyricText = "";
                    _imagePathContainer.Clear();
                    GuIelementImgCoverArt.Dispose();

                    ResetGUI(_selectedScreen);

                    _statusText = "No music file is playing";
                    GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                }
            }
            else if (g_Player.IsRadio)
            {
                var newArtist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
                var newTitle = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));

                if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Artist"))
                    || string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Title")))
                {
                    _statusText = "Stream info not complete";
                    GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                }
                else if (!g_Player.CurrentFile.Equals(_lastStreamFile) || !newArtist.Equals(_artist) || !newTitle.Equals(_title))
                {
                    ResetGUI(_selectedScreen);
                    StopThread();
                    ResetAll();

                    _searchType = (int) SearchTypes.BothLRCsAndLyrics;
                    _searchingState = (int) SearchState.NotSearching;

                    _lastStreamFile = g_Player.CurrentFile;

                    _artist = newArtist;
                    _title = newTitle;

                    _newTrack = true;
                }
            }
        }


        //event driven handler to detect track change. Only used in LRC_Editor mode.
        private void TimeHandler(string tag2, string value)
        {
            if (tag2.Equals("#currentplaytime"))
            {
                if (_stopwatch != null && _stopwatch.IsRunning)
                {
                    _stopwatch.Stop();
                }
                _stopwatch = new Stopwatch();
                _stopwatch.Start();

                var timeStrings = value.Split(':');
                Min = int.Parse(timeStrings[0]);
                Sec = int.Parse(timeStrings[1]);
            }
        }

        /// <summary>
        /// FindLrc searches for a lyric related to the given tag.
        /// </summary>
        private bool FindLrc()
        {
            _eventStopThread.Reset();

            GUIControl.ClearControl(GetID, ControlLyricSelected);
            GUIControl.SetControlLabel(GetID, ControlLyricSelected, "");

            ResetGUI((int) MyLyricsSettings.Screen.LRC);

            if ((_currentTrackTag != null && _currentTrackTag.Artist != "") || g_Player.IsRadio)
            {
                PluginLogger.Info("FindLrc({0}, {1})", _artist, _title);

                /* The prioritized search order is:
                   1) LRC in music tag
                   2) LRC in database
                   3) Search for LRC
                   4) Lyric in music tag
                   5) Lyric in database
                   6) Search for lyric 
                 */

                // (1 of 2) Search LRCS
                if (_searchType != (int) SearchTypes.OnlyLYRICS)
                {
                    _statusText = "Searching for a matching LRC...";
                    GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);

                    #region 1) LRC in music tag

                    if (g_Player.IsRadio == false
                        && _currentTrackTag != null && ((_currentTrackTag.Lyrics.Length != 0
                                                         &&
                                                         (_simpleLrc =
                                                          new SimpleLRC(_artist, _title, _currentTrackTag.Lyrics)).
                                                             IsValid)))
                    {
                        if (_simpleLrc.IsValid)
                        {
                            StartShowingLrc(_currentTrackTag.Lyrics, false);
                            SaveLyricToDatabase(_artist, _title, _currentTrackTag.Lyrics, "music tag", true);
                            return true;
                        }
                    }

                    #endregion

                    #region 2) LRC in lyrics Database

                    string lyricText = null;

                    if (LyricsDb.ContainsKey(DatabaseUtil.CorrectKeyFormat(_artist, _title)))
                    {
                        lyricText = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)].Lyrics;
                    }

                    if (lyricText != null && (_simpleLrc = new SimpleLRC(_artist, _title, lyricText)).IsValid)
                    {
                        if (_simpleLrc.IsValid)
                        {
                            StartShowingLrc(lyricText, false);
                            return true;
                        }
                    }

                    #endregion

                    #region 3) Search the Internet for a LRC

                    var lrcActiveSites = LyricsSiteFactory.LrcLyricsSiteNames().Where(lrcSite => Setup.GetInstance().ActiveSites.Contains(lrcSite)).ToList();
                    if (lrcActiveSites.Count > 0)
                    {
                        _lyricsFound = false;

                        _lyricsController = new LyricsController(this, _eventStopThread, lrcActiveSites.ToArray(), false, false, _find, _replace);

                        // create worker thread instance
                        ThreadStart job = delegate { _lyricsController.Run(); };

                        _lyricControllerThread = new Thread(job);
                        _lyricControllerThread.Name = "LRC search";
                        _lyricControllerThread.Start();

                        _lyricsController.AddNewLyricSearch(_artist, _title, MediaPortalUtil.GetStrippedPrefixArtist(_artist, _strippedPrefixStrings));

                        _lyriccontrollerIsWorking = true;
                    }
                    else
                    {
                        _searchType = (int) SearchTypes.OnlyLYRICS;
                        _statusText = "No matching LRC found";
                        GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                    }

                    #endregion

                    return false;
                }
                return false;
            }
            return false;
        }

        private bool FindLyric()
        {
            _eventStopThread.Reset();

            if ((_currentTrackTag != null && _currentTrackTag.Artist != "") || g_Player.IsRadio)
            {
                PluginLogger.Info("FindLyric({0}, {1})", _artist, _title);

                var lyricText = string.Empty;

                if (LyricsDb.ContainsKey(DatabaseUtil.CorrectKeyFormat(_artist, _title)))
                {
                    lyricText = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)].Lyrics;
                }

                if (_searchType != (int) SearchTypes.OnlyLRCs)
                {
                    #region 4) Lyric in music tag

                    if (_automaticReadFromMusicTag && g_Player.IsRadio == false
                        && _currentTrackTag != null && _currentTrackTag.Lyrics.Length != 0
                        && !(new SimpleLRC(_artist, _title, _currentTrackTag.Lyrics).IsValid))
                    {
                        var lyric = LyricUtil.FixLyrics(_currentTrackTag.Lyrics);
                        _currentTrackTag.Lyrics = lyric;
                        ShowLyricOnScreen(lyric, "music tag");
                        SaveLyricToDatabase(_currentTrackTag.Artist, _currentTrackTag.Title, lyric, "music tag", false);
                        return true;
                    }

                    #endregion

                    #region 5) Lyric in music database

                    if (lyricText.Length != 0 &&
                        !(( /*!_useLrcFinder && */ new SimpleLRC(_artist, _title, lyricText).IsValid)))
                    {
                        LyricFound = new Object[] {lyricText, _artist, _title, "lyrics database"};
                        return true;
                    }
                        #endregion

                        #region 6) Search the Internet for a lyric

                    if (_nonLrcSitesToSearch.Count > 0)
                    {
                        _statusText = "Searching for a matching lyric...";
                        GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);

                        ResetGUI((int) MyLyricsSettings.Screen.LYRICS);

                        _lyricsFound = false;

                        _lyricsController = new LyricsController(this, _eventStopThread, _nonLrcSitesToSearch.ToArray(), false, false,
                            _find, _replace);
                        // create worker thread instance
                        ThreadStart job = delegate { _lyricsController.Run(); };

                        _lyricControllerThread = new Thread(job);
                        _lyricControllerThread.Name = "lyricSearch Thread"; // looks nice in Output window
                        _lyricControllerThread.Start();

                        _lyricsController.AddNewLyricSearch(_artist, _title,
                            MediaPortalUtil.GetStrippedPrefixArtist(_artist, _strippedPrefixStrings));

                        _lyriccontrollerIsWorking = true;

                        return false;
                    }
                    return false;

                    #endregion
                }
                return false;
            }
            _imagePathContainer.Clear();
            GuIelementImgCoverArt.Dispose();

            ResetGUI(_selectedScreen);
            ResetLrcFields();

            _statusText = "No music file is playing";
            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
            return false;
        }

        private void StartShowingLrc(string lyricText, bool showLrcPickScreen)
        {
            _lyricsFound = true;

            _lrcTimeCollection = _simpleLrc.SimpleLRCTimeAndLineCollectionWithOffset;
            _lines = _lrcTimeCollection.Copy();
            
            PluginLogger.Info("LRC found: {0} - {1}.", _artist, _title);

            if (showLrcPickScreen)
            {
                ResetGUI((int) MyLyricsSettings.Screen.LRCPick);
            }
            else
            {
                ResetGUI((int) MyLyricsSettings.Screen.LRC);
            }

            _searchType = (int) SearchTypes.OnlyLRCs;
            _statusText = "";
            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);

            _lyricText = lyricText;

            try
            {
                for (var i = TagRoundFinished*TagInRound; i < (TagRoundFinished + 1)*TagInRound; i++)
                {
                    if (UseEditControlsOnLRCPick() && _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                    {
                        ShowLrcLine((int) GUILRCControls.ControlEditLine + i, _lines[i]);
                        ShowLrcLine((int) GUILRCControls.ControlEditLineDone + i, _lines[i]);
                    }
                    else
                    {
                        ShowLrcLine((int) GUILRCControls.ControlViewLine + i, _lines[i]);
                        ShowLrcLine((int) GUILRCControls.ControlViewLineDone + i, _lines[i]);
                    }
                }

                if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                {
                    for (var i = TagRoundFinished*TagInRound; i < (TagRoundFinished + 1)*TagInRound; i++)
                    {
                        var currentLine = _lrcTimeCollection[i];
                        GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i,
                                                   currentLine.TimeString);
                    }
                }
            }
            catch
            {
            }
        }

        private void TagLine()
        {
            if (_alreadyValidLRC == false && _selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
            {
                if (_stopwatch == null)
                    return;

                if (LRCLinesTotal < _lines.Length)
                {
                    var time = "[" + Min + ":" + (Sec.ToString(CultureInfo.InvariantCulture).Length == 2 ? Sec.ToString(CultureInfo.InvariantCulture) : "0" + Sec) +
                                  "." +
                                  (_stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture).Length >= 2
                                       ? _stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture).Substring(0, 2)
                                       : _stopwatch.ElapsedMilliseconds + "0") + "]";
                    _lines[LRCLinesTotal] = time + _lines[LRCLinesTotal];
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + NextLRCLineIndex,
                                               time);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + NextLRCLineIndex);
                    GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLineDone + NextLRCLineIndex);
                }

                if (++LRCLinesTotal < _lines.Length)
                {
                    // If a new round has to start
                    if (++NextLRCLineIndex == TagInRound)
                    {
                        NextLRCLineIndex = 0;
                        ++TagRoundFinished;


                        for (var i = 0; i < TagInRound; i++)
                        {
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, "");
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, "");
                        }

                        try
                        {
                            for (var i = 0; i < TagInRound && LRCLinesTotal + i < _lines.Length; i++)
                            {
                                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i,
                                                           _lines[TagRoundFinished*TagInRound + i]);
                                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i,
                                                           _lines[TagRoundFinished*TagInRound + i]);
                                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i,
                                                           "[xx:xx.xx]");
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    var lyric = new StringBuilder();

                    var artist = LyricUtil.CapatalizeString(_artist);
                    var title = LyricUtil.CapatalizeString(_title);

                    lyric.AppendLine(string.Format("[ar:{0}]", artist));
                    lyric.AppendLine(string.Format("[ti:{0}]", title));
                    if (!string.IsNullOrEmpty(LrcTaggingName))
                    {
                        lyric.AppendLine(string.Format("[by:{0}]", LrcTaggingName));
                    }
                    if (!string.IsNullOrEmpty(LrcTaggingOffset))
                    {
                        lyric.AppendLine(string.Format("[offset:{0}]", LrcTaggingOffset));
                    }
                    lyric.AppendLine("[ap:MediaPortal]");

                    for (var i = 0; i < _lines.Length; i++)
                    {
                        lyric.AppendLine(_lines[i] + "");
                    }

                    lyric.Replace("\r", "");

                    var lyricAsString = lyric.ToString();

                    //lyricAsString = lyricAsString.Substring(0, lastLineShift);
                    _lyricText = lyricAsString;

                    SaveLyricToDatabase(artist, title, _lyricText, "MyLyrics LRC Editor", true);

                    if (_currentTrackTag != null)
                    {
                        if (_worker.IsBusy)
                            _worker.CancelAsync();
                        var data = new SaveLyricToTagListsData(_lyricText, artist, title,
                                                                                   _currentTrackTag.FileName);
                        //worker.RunWorkerAsync(data);
                        //SaveLyricToTagLists(_CurrentTrackTag.FileName, _LyricText);
                        SaveLyricToTagLists(this, new DoWorkEventArgs(data));
                    }

                    _selectedScreen = (int) MyLyricsSettings.Screen.LRC;
                    ShowLyricOnScreen(_lyricText, "MediaPortal");

                    // Upload LRC to LrcFinder if user has accepted in configuration
                    if (UploadLrcToLrcFinder && !AlwaysAskUploadLrcToLrcFinder)
                    {
                        UploadLrcFile(_lyricText);
                    }
                    else if (AlwaysAskUploadLrcToLrcFinder || !ConfirmedNoUploadLrcToLrcFinder)
                    {
                        var dlgYesNo =
                            (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
                        if (dlgYesNo != null)
                        {
                            dlgYesNo.Reset();

                            dlgYesNo.SetHeading("Upload to LRCFinder?");

                            dlgYesNo.SetLine(1, "Upload this and future created");
                            dlgYesNo.SetLine(2, "LRC files to LrcFinder?");

                            dlgYesNo.DoModal(GetID);

                            if (dlgYesNo.IsConfirmed)
                            {
                                UploadLrcFile(_lyricText);

                                UploadLrcToLrcFinder = true;
                                
                                SettingManager.SetParamAsBool(SettingManager.UploadLrcToLrcFinder, true);
                            }
                            else
                            {
                                ConfirmedNoUploadLrcToLrcFinder = true;
                                
                                SettingManager.SetParamAsBool(SettingManager.ConfirmedNoUploadLrcToLrcFinder, true);
                            }
                        }
                    }
                }
            }
        }


        private void UploadLrcFile(string lrcFile)
        {
            var lrcUploaded = LrcFinder.SaveLrcWithGuid(lrcFile, _guid);

            if (lrcUploaded)
            {
                const string status = "Your LRC was successfully uploaded";
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);
            }
            else
            {
                const string status = "LrcFinder could not be reached";
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, status);
            }
        }

        private void RemoveLatestTagLine()
        {
            --LRCLinesTotal;

            if (LRCLinesTotal < 0)
            {
                LRCLinesTotal = 0;
                return;
            }

            _lines[LRCLinesTotal] = _lines[LRCLinesTotal].Substring(9);

            if (--NextLRCLineIndex < 0)
            {
                NextLRCLineIndex = TagInRound - 1;
                --TagRoundFinished;

                try
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, _lines[TagRoundFinished*TagInRound + i].Substring(0, 9));
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, _lines[TagRoundFinished*TagInRound + i].Substring(9));
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, _lines[TagRoundFinished*TagInRound + i].Substring(9));
                        GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                    }
                }
                catch
                {
                    ;
                }

                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + NextLRCLineIndex);
                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + NextLRCLineIndex);
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + NextLRCLineIndex, "[xx:xx.xx]");

                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + NextLRCLineIndex, _lines[TagRoundFinished*TagInRound + NextLRCLineIndex]);
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + NextLRCLineIndex, _lines[TagRoundFinished*TagInRound + NextLRCLineIndex]);
            }
            else
            {
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + NextLRCLineIndex);
                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + NextLRCLineIndex);
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + NextLRCLineIndex, "[xx:xx.xx]");
            }
        }


        private void CalculateNextInterval()
        {
            if (_lrcTimeCollection != null)
            {
                var trackTime = (int) (g_Player.CurrentPosition*1000);
                NextLRCLineIndex = _lrcTimeCollection.GetSimpleLRCTimeAndLineIndex(trackTime);

                var currentLine = _lrcTimeCollection[NextLRCLineIndex];

                TagRoundFinished = NextLRCLineIndex/TagInRound;
                var localIndex = (NextLRCLineIndex%TagInRound);

                if (NextLRCLineIndex == _lrcTimeCollection.Count - 1)
                {
                    if (currentLine.Time - trackTime < 500)
                    {
                        ++localIndex;
                    }
                }

                #region Show LRC lines in LRC mini labels

                var currentLRCLineIndex = NextLRCLineIndex > 0 ? NextLRCLineIndex - 1 : 0;

                if (_lrcTimeCollection[NextLRCLineIndex].Time < trackTime)
                {
                    ++currentLRCLineIndex;
                }


                // 1. The two previous lines
                if (currentLRCLineIndex >= 2)
                {
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 0, _lines[currentLRCLineIndex - 2]);
                }
                if (currentLRCLineIndex >= 1)
                {
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 1, _lines[currentLRCLineIndex - 1]);
                }

                // 2. The current line
                ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 2, _lines[currentLRCLineIndex + 0]);

                // 3. The two future lines
                // If last, then show empty lines for fourth and fifth label (showing the future LRC lines)
                if (currentLRCLineIndex + 1 == _lrcTimeCollection.Count)
                {
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 3, string.Empty);
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 4, string.Empty);
                }
                    // If second last then clear the last label (only one future LRC lines left)
                else if (currentLRCLineIndex + 2 == _lrcTimeCollection.Count)
                {
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 3, _lines[currentLRCLineIndex + 1]);
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 4, string.Empty);
                }
                else
                {
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 3, _lines[currentLRCLineIndex + 1]);
                    ShowLrcLine((int) GUILRCControls.ControlMiniViewLine + 4, _lines[currentLRCLineIndex + 2]);
                }

                #endregion

                if (TagRoundFinished > 0 && localIndex == 0)
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        if (UseEditControlsOnLRCPick() && _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                        {
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                        }
                        else
                        {
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        if (UseEditControlsOnLRCPick() && _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                        {
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, "");
                        }
                        else
                        {
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLineDone + i, "");
                        }
                    }

                    if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                    {
                        for (var i = 0; i < TagInRound; i++)
                        {
                            var currentLineTime = _lrcTimeCollection[TagRoundFinished*TagInRound + i];
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, 
                                (currentLineTime != null ? currentLineTime.TimeString : string.Empty));
                        }
                    }

                    try
                    {
                        for (var i = 0; i < TagInRound; i++)
                        {
                            if (UseEditControlsOnLRCPick() && _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                            {
                                ShowLrcLine((int) GUILRCControls.ControlEditLine + i, _lines[TagRoundFinished*TagInRound + i]);
                                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                                ShowLrcLine((int) GUILRCControls.ControlEditLineDone + i, _lines[TagRoundFinished*TagInRound + i]);
                            }
                            else
                            {
                                ShowLrcLine((int) GUILRCControls.ControlViewLine + i, _lines[TagRoundFinished*TagInRound + i]);
                                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                                ShowLrcLine((int) GUILRCControls.ControlViewLineDone + i, _lines[TagRoundFinished*TagInRound + i]);
                            }
                        }
                    }
                    catch
                    {
                        ;
                    }

                    // Highlight the lines that have been passed in the current interval
                    for (var i = 0; i < localIndex; i++)
                    {
                        if (UseEditControlsOnLRCPick() && _selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
                        {
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                        }
                        else
                        {
                            GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                        }
                    }
                }
            }
        }

        private void ShowLrcLine(int controlID, string text)
        {
            text = MediaPortalUtil.MakeLRCLinePerfectToShow(text);
            GUIControl.SetControlLabel(GetID, controlID, text);
        }


        private void ShowLyricOnScreen(string lyricText, string source)
        {
            _lyricsFound = true;
            _lyricText = lyricText;
            
            PluginLogger.Info("Lyric found: {0} - {1}. Place: {2}", _artist, _title, source);

            _searchType = (int) SearchTypes.OnlyLYRICS;

            _statusText = "";
            GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);

            if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
            {
                ShowLRCtoEdit();
            }
            else
            {
                // If LRC that has been found, then show lyric in LRC-mode
                _simpleLrc = new SimpleLRC(null, null, _lyricText);

                if (_simpleLrc.IsValid)
                {
                    _searchType = (int) SearchTypes.OnlyLRCs;
                    StartShowingLrc(_lyricText, false);
                }
                    // else show plain lyric
                else
                {
                    _searchType = (int) SearchTypes.OnlyLYRICS;

                    _lyricText = MediaPortalUtil.MakePlainLyricPerfectToShow(_lyricText);

                    ResetGUI((int) MyLyricsSettings.Screen.LYRICS);
                    GUIControl.SetControlLabel(GetID, ControlLyricSelected, _lyricText);
                    GUIControl.FocusControl(GetID, ControlLyricSelected);
                }
            }
        }

        private void SaveLyricToDatabase(string artist, string title, string lyric, string site, bool lrc)
        {
            var capArtist = LyricUtil.CapatalizeString(artist);
            var capTitle = LyricUtil.CapatalizeString(title);

            if (DatabaseUtil.IsSongInLyricsDatabase(LyricsDb, capArtist, capTitle).Equals(DatabaseUtil.LyricNotFound))
            {
                LyricsDb.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                             new LyricsItem(capArtist, capTitle, lyric, site));
                SaveDatabase(MLyricsDbName, LyricsDb);
            }
            else if (
                !DatabaseUtil.IsSongInLyricsDatabaseAsLRC(LyricsDb, capArtist, capTitle).Equals(
                    DatabaseUtil.LRCFound))
            {
                LyricsDb[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)] = new LyricsItem(capArtist, capTitle,
                                                                                              lyric, site);
                SaveDatabase(MLyricsDbName, LyricsDb);
            }
            else if (lrc)
            {
                LyricsDb[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)] = new LyricsItem(capArtist, capTitle,
                                                                                              lyric, site);
                SaveDatabase(MLyricsDbName, LyricsDb);
            }

            if (
                DatabaseUtil.IsSongInLyricsMarkedDatabase(LyricsMarkedDb, capArtist, capTitle).Equals(
                    DatabaseUtil.LyricMarked))
            {
                LyricsMarkedDb.Remove(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle));
                SaveDatabase(MLyricsMarkedDbName, LyricsMarkedDb);
            }
        }

        private void SaveDatabase(string dbName, LyricsDatabase lyricsDatabase)
        {
            var path = Config.GetFile(Config.Dir.Database, dbName);
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                lyricsDatabase.SetLastModified();
                bf.Serialize(fs, lyricsDatabase);
                fs.Close();
            }
        }

        // Stop worker thread if it is running.
        // Called when user presses Stop button of form is closed.
        private void StopThread()
        {
            if (_lyricControllerThread != null && _lyricControllerThread.IsAlive) // thread is active
            {
                // set event "Stop"
                _eventStopThread.Set();

                while (_lyricControllerThread.IsAlive)
                {
                    Thread.Sleep(100);
                }

                _lyricControllerThread = null;
                _lyricsController = null;
            }

            _lyriccontrollerIsWorking = false;
        }


        private void ResetAll()
        {
            _newTrack = false;
            _artist = "";
            _title = "";
            _statusText = "";
            _lyricText = "";
            _lyricsFound = false;
            _lyricControllerThread = null;
            _lyricsController = null;

            ResetLrcFields();
        }

        private void ResetLrcFields()
        {
            if (_stopwatch != null)
            {
                _stopwatch.Reset();
            }

            _simpleLrc = null;

            _lrcTimeCollection = null;
            
            NextLRCLineIndex = 0;
            LRCLinesTotal = 0;
            TagRoundFinished = 0;
            Min = 0;
            Sec = 0;
            Msec = 0;
            _lines = null;
        }

        private void ResetGUI(int screenID)
        {
            _selectedScreen = screenID;

            _isInTranslation = false;

            GUIPropertyManager.OnPropertyChanged -= TimeHandler;

            if (_selectedScreen == (int) MyLyricsSettings.Screen.LYRICS)
            {
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlTitle, "Lyrics screen");

                GUIControl.ClearControl(GetID, ControlLyricSelected);
                GUIControl.FocusControl(GetID, ControlLyricSelected);

                // Reset general and lyrics controls
                GUIControl.ShowControl(GetID, (int) GUIGeneralControls.ControlLbStatus);
                GUIControl.HideControl(GetID, ControlLyricSelected);
                GUIControl.ShowControl(GetID, ControlLyricSelected);
                GUIControl.SetControlLabel(GetID, ControlLyricSelected, "");

                // album art only visible for basic screen
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtCurrently);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtDuration);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbum);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtYear);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtNowPlayingBack);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbumArt);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgress);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgressImage);

                // Hide LRC controls
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlTagPickButton, "");
                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

                for (var i = 0; i < TagInRound; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);

                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                }

                for (var i = 0; i < 5; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlMiniViewLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlMiniViewLine + i, "");
                }
            }

            else if (_selectedScreen == (int) MyLyricsSettings.Screen.LRC)
            {
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlTitle, "LRC screen");

                GUIControl.FocusControl(GetID, ControlLyricSelected);

                // Lyrics controls
                GUIControl.ShowControl(GetID, (int) GUIGeneralControls.ControlLbStatus);
                //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
                GUIControl.HideControl(GetID, ControlLyricSelected);

                // album art only visible for basic screen
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtCurrently);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtDuration);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbum);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtYear);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtNowPlayingBack);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbumArt);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgress);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgressImage);

                // LRC controls
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlTagPickButton, "");
                GUIControl.HideControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

                for (var i = 0; i < TagInRound; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);

                    GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLineDone + i, "");
                }

                if (string.IsNullOrEmpty(_artist))
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                    }
                }

                for (var i = 0; i < 5; i++)
                {
                    GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlMiniViewLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlMiniViewLine + i, "");
                }
            }


            else if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCEditor)
            {
                GUIPropertyManager.OnPropertyChanged += TimeHandler;

                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlTitle, "LRC editor");

                // Lyrics controls
                GUIControl.ShowControl(GetID, (int) GUIGeneralControls.ControlLbStatus);
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, "");
                //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
                GUIControl.HideControl(GetID, ControlLyricSelected);

                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtCurrently);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtDuration);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbum);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtYear);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtNowPlayingBack);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbumArt);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgress);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgressImage);

                // LRC controls
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlTagPickButton, "Tag");
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                GUIControl.FocusControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

                for (var i = 0; i < TagInRound; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, "");
                    //GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, "");
                    GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");

                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                }

                if (string.IsNullOrEmpty(_artist))
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLineDone + i, "");
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, "");
                    }
                    ResetGUI((int) MyLyricsSettings.Screen.LYRICS);
                }

                for (var i = 0; i < 5; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlMiniViewLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlMiniViewLine + i, "");
                }
            }

            else if (_selectedScreen == (int) MyLyricsSettings.Screen.LRCPick)
            {
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlTitle, "LRC pick");

                // Lyrics controls
                GUIControl.ShowControl(GetID, (int) GUIGeneralControls.ControlLbStatus);
                //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
                GUIControl.HideControl(GetID, ControlLyricSelected);

                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtCurrently);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtDuration);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbum);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtYear);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtNowPlayingBack);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtAlbumArt);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgress);
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlArtProgressImage);

                // LRC controls
                GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlTagPickButton, "Pick");
                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                GUIControl.FocusControl(GetID, (int) GUILRCControls.ControlTagPickButton);
                //GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "");
                //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);

                GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlLrcPickStatus);

                //if (prevSelectedScreen != (int) MyLyricsSettings.Screen.LRC)
                //{
                for (var i = 0; i < TagInRound; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditTime + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditTime + i, " ");
                    if (UseEditControlsOnLRCPick())
                    {
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);

                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");
                        GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLineDone + i, "");
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);
                    }
                    else
                    {
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLineDone + i);

                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                        GUIControl.ShowControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLineDone + i, "");
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLineDone + i);
                    }
                }
                //}

                if (string.IsNullOrEmpty(_artist))
                {
                    for (var i = 0; i < TagInRound; i++)
                    {
                        GUIControl.HideControl(GetID, (int) GUILRCControls.ControlViewLine + i);
                        GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlViewLine + i, "");
                        if (UseEditControlsOnLRCPick())
                        {
                            GUIControl.HideControl(GetID, (int) GUILRCControls.ControlEditLine + i);
                            GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlEditLine + i, "");
                        }
                    }
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlLrcPickStatus, "");
                    ResetGUI((int) MyLyricsSettings.Screen.LRC);
                }

                for (var i = 0; i < 5; i++)
                {
                    GUIControl.HideControl(GetID, (int) GUILRCControls.ControlMiniViewLine + i);
                    GUIControl.SetControlLabel(GetID, (int) GUILRCControls.ControlMiniViewLine + i, "");
                }
            }
        }

        #region ILyricForm properties

        public Object[] UpdateString
        {
            set
            {
                var line1 = (string) value[0];
                var line2 = (string) value[1];
                _statusText = line1 + "\r\n" + line2;
                GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
            }
        }

        public Object[] UpdateStatus
        {
            set { }
        }

        public Object[] LyricFound
        {
            set
            {
                if (_lyricsFound == false)
                {
                    var lyricText = (String) value[0];
                    var artist = (String) value[1];
                    var title = (String) value[2];
                    var site = (String) value[3];

                    // If the lrc or lyric found matches the currently playing show the lyric else don't!
                    if (_artist.Equals(artist) && _title.Equals(title))
                    {
                        ShowLyricOnScreen(lyricText, site);

                        SaveLyricToDatabase(artist, title, lyricText, site, site.Equals("LrcFinder"));

                        if (_currentTrackTag != null)
                        {
                            if (_worker.IsBusy)
                                _worker.CancelAsync();
                            var data = new SaveLyricToTagListsData(lyricText, _artist, _title,
                                                                                       _currentTrackTag.FileName);
                            _worker.RunWorkerAsync(data);
                            //SaveLyricToTagLists(_CurrentTrackTag.FileName, lyricText);
                        }
                    }

                    _searchingState = (int) SearchState.NotSearching;
                    _lyriccontrollerIsWorking = false;
                }
            }
        }

        public Object[] LyricNotFound
        {
            set { }
        }

        public Object[] ThreadFinished
        {
            set
            {
                var artist = (String) value[0];
                var title = (String) value[1];
                var message = (String) value[2];
                var site = (String) value[3];

                if (!_lyricsFound && _artist.Equals(artist) && _title.Equals(title))
                {
                    if (site.Equals("LrcFinder"))
                    {
                        if (_searchType == (int) SearchTypes.BothLRCsAndLyrics)
                            _searchType = (int) SearchTypes.OnlyLYRICS;
                        _statusText = "No matching LRC found";
                        
                        PluginLogger.Info("No matching LRC found for {0} - {1}. Place: {2}", artist, title, site);
                    }
                    else
                    {
                        _statusText = "No matching lyric found";
                        
                        PluginLogger.Info("No matching lyric found for {0} - {1}. Place: {2}", artist, title, site);
                    }
                    GUIControl.SetControlLabel(GetID, (int) GUIGeneralControls.ControlLbStatus, _statusText);
                }

                GUIControl.ShowControl(GetID, ControlLyricSelected);

                _lyriccontrollerIsWorking = false;
            }
        }

        public string ThreadException
        {
            set { var message = value; }
        }

        //private void SaveLyricToTagLists(string fileName, string lyric)
        private void SaveLyricToTagLists(object sender, DoWorkEventArgs e)
        {
            var data = (SaveLyricToTagListsData) e.Argument;
            if (!string.IsNullOrEmpty(data.FileName) && !string.IsNullOrEmpty(data.Lyrics))
            {
                lock (_lyricsToWriteToTag)
                {
                    var currentTrackTag = _currentTrackTag;
                    // if lyric is a LRC, then always add
                    if (new SimpleLRC(data.Artist, data.Title, data.Lyrics).IsValid)
                    {
                        if (currentTrackTag == _currentTrackTag)
                            //only if track didnt change - i think it's unsafe to lock it
                        {
                            _currentTrackTag.Lyrics = data.Lyrics;
                        }
                        _lyricsToWriteToTag.Add(new[] {data.FileName, data.Lyrics});
                    }
                        // if 'lyric' is plain lyric and lyric in musictag is LRC, then DON'T add
                    else if (new SimpleLRC(data.FileName).IsValid)
                    {
                    }
                        // if lyric in musictag is not LRC, then add plain lyric
                    else
                    {
                        if (currentTrackTag == _currentTrackTag)
                            //only if track didnt change - i think it's unsafe to lock it
                        {
                            _currentTrackTag.Lyrics = data.Lyrics;
                        }
                        _lyricsToWriteToTag.Add(new[] {data.FileName, data.Lyrics});
                    }
                }
            }
        }

        #endregion

        #region ISetup Interface methods

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = (SettingManager.GetParamAsString(SettingManager.PluginsName, SettingManager.DefaultPluginName));
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_my lyrics.png";
            return true;
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public bool CanEnable() // Indicates whether plugin can be enabled/disabled
        {
            return true;
        }

        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public bool DefaultEnabled()
        {
            return false;
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public int GetWindowId()
        {
            return WindowMylyrics;
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public string PluginName()
        {
            return "My Lyrics";
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public string Description()
        {
            return "Plugin used to manage and show lyrics";
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public string Author()
        {
            return "yoavain";
        }

        /// <summary>
        /// See ISetupForm interface
        /// </summary>
        public void ShowPlugin()
        {
            _core.Initialize();
            var dlg = new MyLyricsSetup();
            dlg.ShowDialog();
        }

        #endregion

        #region Nested type: GUI_General_Controls

        private enum GUIGeneralControls
        {
            ControlBackground = 1,
            ControlTitle = 2,
            ControlLbStatus = 11,
            ControlUpNext = 22,
            ControlAlbum = 26,
            ControlYear = 27,
            ControlTrackTitle = 30,
            ControlTrackArtist = 32,
            ControlNumberDuration = 33,
            ControlNextTrack = 121,
            ControlNextArtist = 123,
        }

        #endregion

        #region Nested type: GUI_LRC_Controls

        private enum GUILRCControls
        {
            ControlArtCurrently = 24,
            ControlArtDuration = 25,
            ControlArtAlbum = 26,
            ControlArtYear = 27,
            ControlArtNowPlayingBack = 31,
            ControlArtAlbumArt = 112,
            ControlArtProgress = 118,
            ControlArtProgressImage = 117,

            ControlTagPickButton = 50,

            ControlEditTime = 600,
            ControlEditLine = 200,
            ControlEditLineDone = 300,

            ControlViewLine = 400,
            ControlViewLineDone = 500,

            ControlLrcPickStatus = 1011,

            ControlMiniViewLine = 1400
        }

        #endregion

        #region Nested type: GUI_Lyrics_Controls

        private enum GUILyricsControls
        {
            ControlLyric = 20,
            ControlLyricScroll = 1020,
        }

        #endregion

        #region Nested type: SEARCH_STATE

        private enum SearchState
        {
            NotSearching = 0,
            SearchingForLRC = 1,
            SearchingForLyric = 2
        }

        #endregion

        #region Nested type: SEARCH_TYPES

        private enum SearchTypes
        {
            BothLRCsAndLyrics = 0,
            OnlyLRCs = 1,
            OnlyLYRICS = 2
        }

        #endregion
    }

    public class SaveLyricToTagListsData
    {
        public string Lyrics { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }

        public SaveLyricToTagListsData(string lyrics, string artist, string title, string filename)
        {
            Lyrics = lyrics;
            Artist = artist;
            Title = title;
            FileName = filename;
        }
    }
}