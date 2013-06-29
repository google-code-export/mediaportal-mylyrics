using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;
using LyricsEngine;
using LyricsEngine.LRC;
using LyricsEngine.LyricsSites;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using Timer = System.Timers.Timer;

namespace MyLyrics
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  [PluginIcons("MyLyrics.Resources.MyLyrics_icon_enabled.png", "MyLyrics.Resources.MyLyrics_icon_disabled.png")]
  public partial class GUIMyLyrics : GUIWindow, ILyricForm, ISetupForm
  {
    #region Fields related to the MyLyrics in general

    internal const string MLyricsDbName = "LyricsDatabaseV2.db";
    internal const string MLyricsMarkedDbName = "LyricsMarkedDatabaseV2.db";
    internal static LyricsDatabase LyricsDb;
    internal static LyricsDatabase LyricsMarkedDb;
    public static int WINDOW_MYLYRICS = 90478;
    private readonly ManualResetEvent _EventStopThread;

    private readonly object _imageMutex;
    private readonly List<String> _ImagePathContainer;
    private readonly PlayListPlayer _PlaylistPlayer;
    internal int CONTROL_LYRIC_SELECTED = 20;
    private string logFileName = "MyLyrics.log";
    private string logFullFileName = "";
    private bool _alreadyValidLRC;

    // Track info
    private string _artist = "";
    private bool _AutomaticReadFromMusicTag = true;
    private bool _AutomaticWriteToMusicTag = true;

    private int _crossfade;
    private string _CurrentThumbFileName = string.Empty;
    private string _CurrentTrackFileName = string.Empty;
    private string _NextTrackFileName = string.Empty;
    private MusicTag _CurrentTrackTag;
    private MusicTag _NextTrackTag;
    private MusicTag _PreviousTrackTag;
    private bool _enableLogging;

    private string _Find = string.Empty;
    private Guid _guid;
    private string _guidString;
    private Timer _ImageChangeTimer;
    private string _LastFileName = "";
    //private string _LastLyricText = "";
    private string _LastStreamFile = string.Empty;
    private LyricsController _lc;
    internal string _logName = "MyLyrics.log";
    private DataTable _LrcTable;

    private bool _LyriccontrollerIsWorking;
    private Thread _LyricControllerThread;
    private bool _lyricsFound;

    private String[] _LyricSitesTosearch;
    private string _lyricsScreenXML = "MyLyrics.xml";
    private List<string[]> _LyricsToWriteToTag;
    private string _LyricText = "";
    private bool _newTrack;
    private string _Replace = string.Empty;
    private int _SearchingState;
    private int _selectedScreen;
    private string _skin = "";
    private int _startingScrollSpeedVertical;
    private string _StatusText = "";
    private string[] _strippedPrefixStrings;
    private string _title = "";
    private string _TrackText = "";
    private string _translationLanguage = string.Empty;
    private string _translationLanguageCode = string.Empty;
    private bool _useActionext;
    private bool _useHotLyrics;
    private bool _UseID3;
    private bool _useLrcFinder;
    private bool _useLyrDB;

    private bool _useLyrics007,
                 _useLyricsOnDemand,
                 _useLyricsPluginSite,
                 _useShironet;

    private bool _ValidLrcLyric;
    // A valid LRC-lyric always overwrites a normal lyric in both Lyrics db and music tag (if allowed)

    private Timer _WriteTagTimer;

    private int _selectedinLRCPicker = 0;
    private Timer _LRCPickTimer;
    private string _lastLRCPickLabel = "";
    private bool _isInTranslation = false;
    private BackgroundWorker worker = new BackgroundWorker();
    private bool _settingsRead = false;
    private IDictionary defines = null;

    #endregion

    [SkinControl((int)GUI_LRC_Controls.CONTROL_ART_ALBUMART)]
    protected GUIImage GUIelement_ImgCoverArt;
    [SkinControl((int)GUI_LRC_Controls.CONTROL_ART_PROGRESS)]
    protected GUIProgressControl GUIelement_ProgTrack;

    private int _SearchType;

    #region Fields related to LRC mode

    internal bool _confirmedNoUploadLrcToLrcFinder;
    internal string _LrcTaggingName;
    internal string _LrcTaggingOffset;

    private SimpleLRCTimeAndLineCollection _LrcTimeCollection;
    private SimpleLRC _SimpleLrc;
    private Stopwatch _Stopwatch;
    internal bool _uploadLrcToLrcFinder;
    internal bool _useAutoOnLyricLength;
    internal bool _useAutoScrollAsDefault;
    internal bool _alwaysAskUploadLrcToLrcFinder;

    #endregion

    #region Fields releated to the editor mode

    internal const int _TAG_IN_ROUND = 13;
    internal int nextLRCLineIndex;
    private string[] _lines;
    internal int _LRCLinesTotal;
    internal int _min;
    internal int _msec;
    internal int _sec;
    internal int _tagRoundFinished;

    #endregion

    public GUIMyLyrics()
    {
      _EventStopThread = new ManualResetEvent(false);

      _ImagePathContainer = new List<string>();
      _imageMutex = new Object();
      _PlaylistPlayer = PlayListPlayer.SingletonPlayer;

      GetID = WINDOW_MYLYRICS;
    }

    public override bool Init()
    {
      _selectedScreen = (int)MyLyricsSettings.Screen.LYRICS;

      _startingScrollSpeedVertical = GUIGraphicsContext.ScrollSpeedVertical;
      GUIGraphicsContext.ScrollSpeedVertical = 0;

      worker.DoWork += SaveLyricToTagLists;

      loadSkinSettings(GUIGraphicsContext.Skin + @"\" + _lyricsScreenXML);

      return Load(GUIGraphicsContext.Skin + @"\" + _lyricsScreenXML);
    }


    private void UpdateNextTrackInfo()
    {
      if (_NextTrackTag != null)
      {
        string strNextTrack = String.Format("{0} {1}", GUILocalizeStrings.Get(435), _NextTrackTag.Track); //	"Track: "
        if (_NextTrackTag.Track <= 0)
        {
          strNextTrack = string.Empty;
        }

        string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), _NextTrackTag.Year); //	"Year: "
        if (_NextTrackTag.Year <= 1900)
        {
          strYear = string.Empty;
        }

        GUIPropertyManager.SetProperty("#Play.Next.Title", _NextTrackTag.Title);
        GUIPropertyManager.SetProperty("#Play.Next.Track", strNextTrack);
        GUIPropertyManager.SetProperty("#Play.Next.Album", _NextTrackTag.Album);
        GUIPropertyManager.SetProperty("#Play.Next.Artist", _NextTrackTag.Artist);
        GUIPropertyManager.SetProperty("#Play.Next.Genre", _NextTrackTag.Genre);
        GUIPropertyManager.SetProperty("#Play.Next.Year", strYear);
        GUIPropertyManager.SetProperty("#Play.Next.Rating", (Convert.ToDecimal(2 * _NextTrackTag.Rating + 1)).ToString());
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
      if ((_newTrack || _SearchingState != (int)SEARCH_STATE.NOT_SEARCHING)
          && (!g_Player.IsRadio || !string.IsNullOrEmpty(_artist)))
      //&& (!g_Player.IsRadio || _CurrentTrackTag!=null))
      {
        if (_newTrack)
        {
          _alreadyValidLRC = false;

          _ImagePathContainer.Clear();

          _lyricsFound = false;
          StopThread();
          _newTrack = false;

          MusicDatabase mDB = MusicDatabase.Instance;

          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, "");

          if (g_Player.IsRadio == false)
          {
            string currentFile = null;
            string currentLyrics = null;
            if (_CurrentTrackTag != null)
            {
              currentFile = _CurrentTrackTag.FileName;
              currentLyrics = _CurrentTrackTag.Lyrics;
            }

            _CurrentTrackFileName = g_Player.CurrentFile;
            _NextTrackFileName = PlayListPlayer.SingletonPlayer.GetNext();
            //_CurrentTrackTag = mDB.GetTag(g_Player.CurrentFile);
            GetTrackTags();

            if (_CurrentTrackTag != null)
            {
              _artist = _CurrentTrackTag.Artist.Trim();
              _artist = _artist.Replace("| ", "");
              _artist = _artist.Replace(" |", "");
              _artist = _artist.Replace("''", "'");
              _artist = LyricUtil.CapatalizeString(_artist);
              _title = _CurrentTrackTag.Title.Trim();
              _title = _title.Replace("''", "'");
              _title = LyricUtil.CapatalizeString(_title);

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
              _TrackText = "";
              _LyricText = "";
              _ImagePathContainer.Clear();
              GUIelement_ImgCoverArt.Dispose();

              resetLrcFields();
              resetGUI(_selectedScreen);

              _StatusText = "No music file is playing";
              GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
            }
          }
          else
          {
            GetAlbumArt(_artist);
          }
        }

        if (_CurrentTrackTag != null || g_Player.IsRadio)
        {
          if (!g_Player.IsRadio)
          {
            string strTrack = String.Format("{0} {1}", GUILocalizeStrings.Get(435), _CurrentTrackTag.Track); //	"Track"
            if (_CurrentTrackTag.Track <= 0)
            {
              strTrack = string.Empty;
            }

            string strYear = String.Format("{0} {1}", GUILocalizeStrings.Get(436), _CurrentTrackTag.Year); //	"Year: "
            if (_CurrentTrackTag.Year <= 1900)
            {
              strYear = string.Empty;
            }

            GUIPropertyManager.SetProperty("#Play.Current.Title", _CurrentTrackTag.Title);
            GUIPropertyManager.SetProperty("#Play.Current.Track", strTrack);
            GUIPropertyManager.SetProperty("#Play.Current.Album", _CurrentTrackTag.Album);
            GUIPropertyManager.SetProperty("#Play.Current.Artist", _CurrentTrackTag.Artist);
            GUIPropertyManager.SetProperty("#Play.Current.Genre", _CurrentTrackTag.Genre);
            GUIPropertyManager.SetProperty("#Play.Current.Year", strYear);
            GUIPropertyManager.SetProperty("#Play.Current.Rating", (Convert.ToDecimal(2 * _CurrentTrackTag.Rating + 1)).ToString());
            GUIPropertyManager.SetProperty("#duration", MediaPortal.Util.Utils.SecondsToHMSString(_CurrentTrackTag.Duration));

            //_CurrentTrackTag.Artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
            //_CurrentTrackTag.Title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));
            _CurrentTrackTag.Lyrics = LyricUtil.FixLyrics(_CurrentTrackTag.Lyrics);

            UpdateNextTrackInfo();
          }

          if (_selectedScreen == (int)MyLyricsSettings.Screen.LYRICS
              || _selectedScreen == (int)MyLyricsSettings.Screen.LRC
              || _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
          {
            // Get lyric
            if (_artist.Length != 0 && _title.Length != 0)
            {
              if (_SearchingState == (int)SEARCH_STATE.NOT_SEARCHING)
              {
                if (_useLrcFinder &&
                    (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS
                    || _SearchType == (int)SEARCH_TYPES.ONLY_LRCS))
                {
                  bool lrcFoundInTagOrLyricDb = FindLrc();
                  if (lrcFoundInTagOrLyricDb)
                  {
                    _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
                  }
                  else
                  {
                    _SearchingState = (int)SEARCH_STATE.SEARCHING_FOR_LRC;
                  }
                }
                else
                {
                  bool lyricFoundInTagOrLyricDb = FindLyric();
                  if (lyricFoundInTagOrLyricDb)
                  {
                    _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
                  }
                  else
                  {
                    _SearchingState = (int)SEARCH_STATE.SEARCHING_FOR_LYRIC;
                  }
                }
              }
              else if (_SearchingState == (int)SEARCH_STATE.SEARCHING_FOR_LRC &&
                       !_LyriccontrollerIsWorking)
              {
                if (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS
                    || _SearchType == (int)SEARCH_TYPES.ONLY_LYRICS)
                {
                  FindLyric();
                  _SearchingState = (int)SEARCH_STATE.SEARCHING_FOR_LYRIC;
                }
                else
                {
                  _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
                }
              }
              else if (_SearchingState == (int)SEARCH_STATE.SEARCHING_FOR_LYRIC &&
                       !_LyriccontrollerIsWorking)
              {
                _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
              }
            }
            else if ((_artist.Length == 0 && _title.Length > 0) || (_title.Length == 0 && _artist.Length > 0))
            {
              //_ImagePathContainer.Clear();
              //GUIelement_ImgCoverArt.Dispose();

              resetLrcFields();
              resetGUI(_selectedScreen);

              _StatusText = "Not enough data for lyric search";
              GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
            }
            else
            {
              _artist = "";
              _title = "";
              _TrackText = "";
              _LyricText = "";
              _ImagePathContainer.Clear();
              GUIelement_ImgCoverArt.Dispose();

              resetLrcFields();

              resetGUI(_selectedScreen);

              _StatusText = "No music file is playing";
              GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
            }
          }
          else if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
          {
            _newTrack = false;
            nextLRCLineIndex = 0;
            _LRCLinesTotal = 0;
            _tagRoundFinished = 0;

            _artist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
            _title = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));

            if (!_LyricText.Equals(string.Empty))
            {
              ShowLRCtoEdit();
            }
            else if (
                DatabaseUtil.IsSongInLyricsDatabase(LyricsDb, _artist, _title).Equals(
                    DatabaseUtil.LYRIC_FOUND))
            {
              LyricsItem item = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)];
              _LyricText = item.Lyrics;
              ShowLRCtoEdit();
            }
            else
            {
              _lyricsFound = false;

              if (!string.IsNullOrEmpty(_artist) && !string.IsNullOrEmpty(_title))
              {
                StopThread();
                _SearchType = (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS;
                resetGUI((int)MyLyricsSettings.Screen.LRC);
                _newTrack = true;
                //FindLyric();
              }
              else if ((_artist.Length == 0 && _title.Length > 0) || (_title.Length == 0 && _artist.Length > 0))
              {
                //_ImagePathContainer.Clear();
                //GUIelement_ImgCoverArt.Dispose();

                resetLrcFields();
                resetGUI(_selectedScreen);

                _StatusText = "Not enough data for lyric search";
                GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
              }
              else
              {
                _artist = "";
                _title = "";
                _TrackText = "";
                _LyricText = "";
                _ImagePathContainer.Clear();
                GUIelement_ImgCoverArt.Dispose();

                resetGUI(_selectedScreen);

                _StatusText = "No music file is playing";
                GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
              }
            }
          }
        }
      }

      if (_lyricsFound)
      {
        if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC
            || _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
        {
          CalculateNextInterval();
        }
      }

      //GUIGraphicsContext.ResetLastActivity();

      base.Process();
    }

    private bool CheckReallyEditLRCBeforeEdit()
    {
      SimpleLRC lrc = new SimpleLRC(_artist, _title, _LyricText);

      if (lrc.IsValid)
      {
        GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
        if (dlgYesNo != null)
        {
          dlgYesNo.Reset();

          dlgYesNo.SetHeading("Edit existing LRC?");

          dlgYesNo.SetLine(1, "This song already has a valid LRC lyric.");
          dlgYesNo.SetLine(2, "Do you really want to edit it?");

          dlgYesNo.DoModal(GetID);

          if (dlgYesNo.IsConfirmed)
          {
            _LyricText = lrc.LyricAsPlainLyric;
            return true;
          }
          else
          {
            return false;
          }

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
      GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, "");

      if (!string.IsNullOrEmpty(_LyricText))
      {
        _LyricText = _LyricText.Trim();

        //else
        {
          resetLrcFields();

          _lines = _LyricText.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);

          try
          {
            for (int i = _tagRoundFinished * _TAG_IN_ROUND;
                 i < (_tagRoundFinished + 1) * _TAG_IN_ROUND;
                 i++)
            {
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
              //GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, _lines[i]);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i,
                                         _lines[i]);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i, "[xx:xx.xx]");
            }
          }
          catch
          {
          }
        }
      }
      else
      {
        _StatusText = "No valid lyric found";
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
      }

      if (_CurrentTrackTag == null)
      {
        resetGUI(_selectedScreen);

        _StatusText = "No music file is playing";
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
        return;
      }
    }

    private void loadSkinSettings(string skinXMLFile)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(skinXMLFile);
        if (doc.DocumentElement != null)
        {
          defines = LoadDefines(doc);
        }
      }
      catch
      {
      }
    }

    private IDictionary LoadDefines(XmlDocument document)
    {
      Hashtable table = new Hashtable();
      try
      {
        foreach (XmlNode node in document.SelectNodes("/window/define"))
        {
          string[] tokens = node.InnerText.Split(':');
          if (tokens.Length < 2)
          {
            continue;
          }
          table[tokens[0]] = tokens[1];
        }
      }
      catch
      {
      }
      return table;
    }

    private bool enableMouseControl()
    {
      string enableMouseControl = String.Empty;
      if (defines != null && defines.Contains("#MyLyrics.EnableMouseControl"))
        enableMouseControl = (string)defines["#MyLyrics.EnableMouseControl"];
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

    private bool useEditControlsOnLRCPick()
    {
      string useEditControlsOnLRCPick = String.Empty;
      if (defines != null && defines.Contains("#MyLyrics.UseEditControlsOnLRCPick"))
        useEditControlsOnLRCPick = (string)defines["#MyLyrics.UseEditControlsOnLRCPick"];
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

    private bool isFocusedBlacklisted()
    {
      int focusID = GetFocusControlId();
      string blacklistedControlIDs = String.Empty;
      if (defines != null && defines.Contains("#MyLyrics.BlacklistedControlIDs"))
        blacklistedControlIDs = (string)defines["#MyLyrics.BlacklistedControlIDs"];
      foreach (string cID in blacklistedControlIDs.Split(new char[] { ',' }))
      {
        try
        {
          if (focusID == int.Parse(cID))
            return true;
        }
        catch
        {
        }
      }
      return false;
    }

    private void LoadSettings()
    {
      _newTrack = true;
      _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
      _SearchType = (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS;

      _LyricsToWriteToTag = new List<string[]>();

      resetGUI(_selectedScreen);

      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        _UseID3 = xmlreader.GetValueAsBool("musicfiles", "showid3", true);
        _useLrcFinder = (xmlreader.GetValueAsString("myLyrics", "useLrcFinder", "True")).Equals("True") ? true : false;
        _useLyrics007 = (xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).Equals("True") ? true : false;
        _useLyricsOnDemand = (xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).Equals("True") ? true : false;
        _useLyricsPluginSite = (xmlreader.GetValueAsString("myLyrics", "useLyricsPluginSite", "True")).Equals("True") ? true : false;
        _useShironet = (xmlreader.GetValueAsString("myLyrics", "useShironet", "True")).Equals("True") ? true : false;
        _useHotLyrics = (xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).Equals("True") ? true : false;
        _useActionext = (xmlreader.GetValueAsString("myLyrics", "useActionext", "True")).Equals("True") ? true : false;
        _useLyrDB = (xmlreader.GetValueAsString("myLyrics", "useLyrDB", "True")).Equals("True") ? true : false;

        _enableLogging = xmlreader.GetValue("myLyrics", "loggingEnabled").Equals("True");

        _AutomaticWriteToMusicTag = xmlreader.GetValue("myLyrics", "automaticWriteToMusicTag").Equals("yes");
        _AutomaticReadFromMusicTag = xmlreader.GetValue("myLyrics", "automaticReadFromMusicTag").Equals("yes");

        _useAutoScrollAsDefault =
            (xmlreader.GetValueAsString("myLyrics", "useAutoscroll", "True")).Equals("yes") ? true : false;
        _useAutoOnLyricLength =
            (xmlreader.GetValueAsString("myLyrics", "useAutoOnLyricLength", "False")).Equals("yes")
                ? true
                : false;
        _LrcTaggingOffset = (xmlreader.GetValueAsString("myLyrics", "LrcTaggingOffset", "´500"));

        string strButtonText = (xmlreader.GetValueAsString("myLyrics", "pluginsName", "My Lyrics"));
        GUIPropertyManager.SetProperty("#currentmodule", strButtonText);

        _LrcTaggingName = (xmlreader.GetValueAsString("myLyrics", "LrcTaggingName", ""));

        _uploadLrcToLrcFinder =
            (xmlreader.GetValueAsString("myLyrics", "uploadLrcToLrcFinder", "False")).Equals("yes")
                ? true
                : false;
        _confirmedNoUploadLrcToLrcFinder =
            (xmlreader.GetValueAsString("myLyrics", "confirmedNoUploadLrcToLrcFinder", "False")).Equals("yes")
                ? true
                : false;
        _alwaysAskUploadLrcToLrcFinder =
            (xmlreader.GetValueAsString("myLyrics", "alwaysAskUploadLrcToLrcFinder", "False")).Equals("yes")
                ? true
                : false;

        string translationString =
            (xmlreader.GetValueAsString("myLyrics", "translationLanguage", "English (en)"));

        string[] strings = translationString.Split(new string[1] { "(" }, StringSplitOptions.None);
        _translationLanguage = strings[0].Trim();
        _translationLanguageCode = strings[1].Replace(")", string.Empty);

        _skin = (xmlreader.GetValueAsString("skin", "name", "Blue3"));

        _guidString = (xmlreader.GetValueAsString("myLyrics", "Guid", ""));

        _crossfade = xmlreader.GetValueAsInt("audioplayer", "crossfade", 2000);

        _Find = xmlreader.GetValueAsString("myLyrics", "find", "");
        _Replace = xmlreader.GetValueAsString("myLyrics", "replace", "");
      }

      if (!_settingsRead) // only first time
      {
        if (_useAutoScrollAsDefault)
        {
          CONTROL_LYRIC_SELECTED = (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll;

          GUIControl.HideControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric);
        }
        else
        {
          CONTROL_LYRIC_SELECTED = (int)GUI_Lyrics_Controls.CONTROL_Lyric;

          GUIControl.HideControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll);
        }
      }

      if (string.IsNullOrEmpty(_guidString))
      {
        _guid = Guid.NewGuid();
        _guidString = _guid.ToString("P");

        using (Settings xmlwriter = new Settings("MediaPortal.xml"))
        {
          xmlwriter.SetValue("myLyrics", "Guid", _guidString);
        }
      }
      else
      {
        _guid = new Guid(_guidString);
      }


      _strippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

      ArrayList sitesToSearch = new ArrayList();
      //if (_useLrcFinder && Setup.IsMember("LrcFinder"))
      //{
      //    sitesToSearch.Add("LrcFinder");
      //}

      if (_useActionext && Setup.IsMember("Actionext"))
      {
        sitesToSearch.Add("Actionext");
      }
      if (_useActionext && Setup.IsMember("LyrDB"))
      {
        sitesToSearch.Add("LyrDB");
      }
      if (_useLyrics007 && Setup.IsMember("Lyrics007"))
      {
        sitesToSearch.Add("Lyrics007");
      }
      if (_useLyricsOnDemand && Setup.IsMember("LyricsOnDemand"))
      {
        sitesToSearch.Add("LyricsOnDemand");
      }
      if (_useHotLyrics && Setup.IsMember("HotLyrics"))
      {
        sitesToSearch.Add("HotLyrics");
      }
      if (_useLyricsPluginSite && Setup.IsMember("LyricsPluginSite"))
      {
        sitesToSearch.Add("LyricsPluginSite");
      }
      if (_useShironet && Setup.IsMember("Shironet"))
      {
        sitesToSearch.Add("Shironet");
      }

      _LyricSitesTosearch = (string[])sitesToSearch.ToArray(typeof(string));

      _settingsRead = true;

      // Deserialize lyrics and marked database, and save references in LyricsDB
      try
      {
        string path = Config.GetFile(Config.Dir.Database, MLyricsDbName);
        FileStream fs = new FileStream(path, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        LyricsDb = (LyricsDatabase)bf.Deserialize(fs);
        fs.Close();

        path = Config.GetFile(Config.Dir.Database, MLyricsMarkedDbName);
        fs = new FileStream(path, FileMode.Open);
        LyricsMarkedDb = (LyricsDatabase)bf.Deserialize(fs);
        fs.Close();
      }
      catch
      {
        GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
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

      if (_ImageChangeTimer == null)
      {
        _ImageChangeTimer = new Timer();
        _ImageChangeTimer.Interval = 15 * 1000;
        _ImageChangeTimer.Elapsed += OnImageTimerTickEvent;
        _ImageChangeTimer.Start();
      }

      if (_LRCPickTimer == null)
      {
        _LRCPickTimer = new Timer();
        _LRCPickTimer.Interval = 3 * 1000;
        _LRCPickTimer.Elapsed += OnLRCPickTimerTickEvent;
        _LRCPickTimer.Stop();
      }

      if (_enableLogging)
      {
        logFullFileName = Config.GetFile(Config.Dir.Log, logFileName);
        LyricDiagnostics.OpenLog(logFullFileName);
        LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Start, 0,
                                                LyricDiagnostics.ElapsedTimeString() + "MyLyrics opens");
      }
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      StopThread();
      if (worker.IsBusy)
        worker.CancelAsync();
      resetAll();
      resetGUI((int)MyLyricsSettings.Screen.LYRICS);

      GUIGraphicsContext.ScrollSpeedVertical = _startingScrollSpeedVertical;

      if (_WriteTagTimer != null)
      {
        _WriteTagTimer.Stop();
        _WriteTagTimer.Close();
        _WriteTagTimer.Dispose();
        _WriteTagTimer = null;
      }

      if (_AutomaticWriteToMusicTag)
      {
        foreach (string[] pair in _LyricsToWriteToTag)
        {
          TagReaderUtil.WriteLyrics(pair[0], pair[1]);
        }
      }

      if (_enableLogging)
      {
        LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Stop, 0,
                                                LyricDiagnostics.ElapsedTimeString() + "MyLyrics closes");
        LyricDiagnostics.Dispose();
      }

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
          message.TargetControlId == (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS)
      {
        _LRCPickTimer.Stop();
      }
      return base.OnMessage(message);
    }

    public override void OnAction(MediaPortal.GUI.Library.Action action)
    {
      if (!enableMouseControl())
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
            if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
            {
              if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_LEFT)
                _selectedinLRCPicker--;
              if (action.wID == MediaPortal.GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT)
                _selectedinLRCPicker++;

              if (_selectedinLRCPicker < 0)
                _selectedinLRCPicker = _LrcTable.Rows.Count - 1;
              if (_selectedinLRCPicker > _LrcTable.Rows.Count - 1)
                _selectedinLRCPicker = 0;


              string status = string.Format("LRC {0} of {1} shown", _selectedinLRCPicker + 1, _LrcTable.Rows.Count);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);

              _LyricText = _LrcTable.Rows[_selectedinLRCPicker]["Lyrics"] as string;
              _SimpleLrc = new SimpleLRC(null, null, _LyricText);
              StartShowingLrc(_LyricText, true);
            }
            break;
          }
        case MediaPortal.GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
          {
            if (!isFocusedBlacklisted())
            {
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK && !action.m_key.KeyChar.Equals(42))
                ShowLrcPick();
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
                TagLine();
            }
            break;
          }
        case MediaPortal.GUI.Library.Action.ActionType.ACTION_KEY_PRESSED:
          {
            if (/*action.m_key.KeyChar.Equals(13) || */action.m_key.KeyChar.Equals(35) || action.m_key.KeyChar.Equals(41)) // 'Enter' or '#' or ')'
            {
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK && !action.m_key.KeyChar.Equals(35))
                ShowLrcPick();
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
                TagLine();
            }
            else if (action.m_key.KeyChar.Equals(48) || action.m_key.KeyChar.Equals(101)) // '0' or 'E'
            {
              // Don't use a stream to create a LRC
              if (g_Player.IsRadio || String.IsNullOrEmpty(_artist))
              {
                break;
              }

              _lyricsFound = false;
              if (_selectedScreen != (int)MyLyricsSettings.Screen.LRC_EDITOR)
              {
                if (CheckReallyEditLRCBeforeEdit())
                {
                  resetGUI((int)MyLyricsSettings.Screen.LRC_EDITOR);
                  ShowLRCtoEdit();
                  Process();
                }
              }
              else
              {
                // parameter could be anything but LRC_EDITOR. Will find correct type when running findLyric().
                if (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS ||
                    _SearchType == (int)SEARCH_TYPES.ONLY_LRCS)
                {
                  resetGUI((int)MyLyricsSettings.Screen.LRC);
                }
                else
                {
                  resetGUI((int)MyLyricsSettings.Screen.LYRICS);
                }
                //resetGUI((int) MyLyricsSettings.Screen.LYRICS);
                _newTrack = true;
                Process();
              }
            }
            else if (action.m_key.KeyChar.Equals(115)) // 'S' 
            {
              if (GUIGraphicsContext.ScrollSpeedVertical >= 10)
                GUIGraphicsContext.ScrollSpeedVertical = 0;
              else if (GUIGraphicsContext.ScrollSpeedVertical >= 8)
                GUIGraphicsContext.ScrollSpeedVertical = 10;
              else if (GUIGraphicsContext.ScrollSpeedVertical >= 6)
                GUIGraphicsContext.ScrollSpeedVertical = 8;
              else if (GUIGraphicsContext.ScrollSpeedVertical >= 4)
                GUIGraphicsContext.ScrollSpeedVertical = 7; // MediaPortal BUG here, scroll 6 gets many errors in log:  
              //GUITextScrollUpControl.cs: if (_frameLimiter % (6 - GUIGraphicsContext.ScrollSpeedVertical) == 0)
              else if (GUIGraphicsContext.ScrollSpeedVertical >= 2)
                GUIGraphicsContext.ScrollSpeedVertical = 4;
              else if (GUIGraphicsContext.ScrollSpeedVertical >= 0)
                GUIGraphicsContext.ScrollSpeedVertical = 2;
              /*
              if (GUIGraphicsContext.ScrollSpeedVertical >= 10
                  || GUIGraphicsContext.ScrollSpeedVertical == 1
                  || GUIGraphicsContext.ScrollSpeedVertical == 3
                  || GUIGraphicsContext.ScrollSpeedVertical == 5)
              {
                  GUIGraphicsContext.ScrollSpeedVertical = 0;
              }
              else
              {
                  ++GUIGraphicsContext.ScrollSpeedVertical;
                  ++GUIGraphicsContext.ScrollSpeedVertical;
              }
              */
            }
            else if (action.m_key.KeyChar.Equals(102)) // 'F'
            {
              //if (_selectedScreen != (int) MyLyricsSettings.Screen.LRC_PICK
              //    && _selectedScreen != (int) MyLyricsSettings.Screen.LRC_EDITOR)
              //{

              string lyricText = String.Empty;
              bool hasValidLRC = false;
              if (LyricsDb.ContainsKey(DatabaseUtil.CorrectKeyFormat(_artist, _title)))
              {
                lyricText = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)].Lyrics;
              }
              if (lyricText != null && (new SimpleLRC(_artist, _title, lyricText)).IsValid)
                hasValidLRC = true;

              bool shouldNewTrack = true;
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR || _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK || _isInTranslation)
              {
                //shouldNewTrack = true;
              }
              else
              {
                if (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS)
                {
                  if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC)
                  {
                    _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;
                  }
                  else
                  {
                    _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;
                  }
                }
                else if (_SearchType == (int)SEARCH_TYPES.ONLY_LRCS)
                {
                  _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;
                }
                else
                {
                  _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;
                }

                //if (hasValidLRC && _SearchType == (int)SEARCH_TYPES.ONLY_LYRICS)
                //    _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;

                if (_SearchType != (int)SEARCH_TYPES.ONLY_LYRICS && !_useLrcFinder)
                {
                  _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;

                  GUIDialogOK dlg2 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
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

              if (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS ||
                  _SearchType == (int)SEARCH_TYPES.ONLY_LRCS)
              {
                resetGUI((int)MyLyricsSettings.Screen.LRC);
              }
              else
              {
                resetGUI((int)MyLyricsSettings.Screen.LYRICS);
              }

              _newTrack = shouldNewTrack;

            }
            else if (action.m_key.KeyChar.Equals(112)) // 'P'
            {
              ShowLrcPick();
            }
            else if (49 <= action.m_key.KeyChar && action.m_key.KeyChar <= 57) // '1'-'9'
            {
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
              {

                int index = action.m_key.KeyChar - 49;
                int noOfRows = index + 1;

                if (noOfRows <= _LrcTable.Rows.Count)
                {
                  string status = string.Format("LRC {0} of {1} shown", noOfRows, _LrcTable.Rows.Count);
                  GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);

                  _LyricText = _LrcTable.Rows[index]["Lyrics"] as string;
                  _SimpleLrc = new SimpleLRC(null, null, _LyricText);
                  StartShowingLrc(_LyricText, true);
                }
              }
            }
            else if (action.m_key.KeyChar.Equals(8) || action.m_key.KeyChar.Equals(42) || action.m_key.KeyChar.Equals(40)) // 'Backslash' or '*'
            {
              if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
                RemoveLatestTagLine();
            }
            else if (action.m_key.KeyChar.Equals(98)) // 'B' (stop playing media)
            {
              resetLrcFields();
              resetGUI(_selectedScreen);
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

      if (_selectedScreen != (int)MyLyricsSettings.Screen.LRC_PICK)
      {
        LrcFinder lrcFinder = new LrcFinder();
        _LrcTable = lrcFinder.FindLRCs(_artist, _title);
        //_LrcTable = lrcFinder.FindLRCs("madonna", "like a virgin");

        if (_LrcTable != null && _LrcTable.Rows.Count > 0)
        {
          _selectedinLRCPicker = 0;
          if (_SimpleLrc != null)
          {
            for (int i = 0; i < _LrcTable.Rows.Count; i++)
            {
              SimpleLRC _SimpleLrcTemp = new SimpleLRC(null, null, _LrcTable.Rows[i]["Lyrics"] as string);
              if (_SimpleLrc.LyricAsLRC == _SimpleLrcTemp.LyricAsLRC)
              {
                _selectedinLRCPicker = i;
                break;
              }
            }
          }
          _LyricText = _LrcTable.Rows[_selectedinLRCPicker]["Lyrics"] as string;
          _SimpleLrc = new SimpleLRC(null, null, _LyricText);
          StartShowingLrc(_LyricText, true);

          string status = null;

          if (_LrcTable.Rows.Count == 1)
          {
            status = "One LRC file found";
          }
          else
          {
            status = string.Format("LRC {0} of {1} shown", _selectedinLRCPicker + 1, _LrcTable.Rows.Count);
          }

          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);
        }
        else if (_LrcTable == null)
        {
          //resetGUI((int) MyLyricsSettings.Screen.LRC_PICK);

          string status = "LrcFinder could not be reached";
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);
          _lastLRCPickLabel = status;
          _LRCPickTimer.Stop(); _LRCPickTimer.Start();
        }
        else
        {
          //resetGUI((int)MyLyricsSettings.Screen.LRC_PICK);

          string status = "No LRC found";
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);
          _lastLRCPickLabel = status;
          _LRCPickTimer.Stop(); _LRCPickTimer.Start();

          //string lyricInfo = "Press the 'P' key to return";
          //GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, lyricInfo);
        }
      }
      else
      {
        SaveLyricToDatabase(_artist, _title, _LyricText, "MyLyrics LRC Pick", true);

        if (_CurrentTrackTag != null)
        {
          if (worker.IsBusy)
            worker.CancelAsync();
          SaveLyricToTagListsData data = new SaveLyricToTagListsData(_LyricText, _artist, _title, _CurrentTrackTag.FileName);
          //worker.RunWorkerAsync(data);
          //SaveLyricToTagLists(_CurrentTrackTag.FileName, _LyricText);
          SaveLyricToTagLists(this, new DoWorkEventArgs(data));

        }

        _LrcTable = null;
        _newTrack = true;
        _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;

        Process();
      }
    }


    protected override void OnShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
      {
        return;
      }

      dlg.Reset();
      dlg.SetHeading(498); // menu

      dlg.Add("Find LRC");
      dlg.Add("Find lyric");
      dlg.Add("Make LRC");

      string translateLabelString = "Translate to " + _translationLanguage.ToLower();
      dlg.Add(translateLabelString);

      if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
      {
        dlg.Add("Use picked LRC");
      }
      else
      {
        dlg.Add("Pick LRC");
      }

      if (CONTROL_LYRIC_SELECTED == (int)GUI_Lyrics_Controls.CONTROL_Lyric)
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
        if (CONTROL_LYRIC_SELECTED == (int)GUI_Lyrics_Controls.CONTROL_Lyric)
        {
          GUIControl.FocusControl(GetID, CONTROL_LYRIC_SELECTED);
        }
        else if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
        {
          GUIControl.FocusControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        }
        else
        {
          return;
        }
      }
      switch (dlg.SelectedLabelText)
      {
        case "Find LRC":
          if (_useLrcFinder)
          {
            _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;

            //_selectedScreen = (int) MyLyricsSettings.Screen.LRC;
            resetGUI((int)MyLyricsSettings.Screen.LRC);

            _newTrack = true;
          }
          else
          {
            GUIDialogOK dlg2 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
            dlg2.SetHeading("LRC screen disabled");
            dlg2.SetLine(1, "You haven't enabled LRCFinder");
            dlg2.SetLine(2, "in configuration.");
            dlg2.SetLine(3, "LRC screen is disabled!");
            dlg2.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;

        case "Find lyric":
          _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;

          //_selectedScreen = (int) MyLyricsSettings.Screen.LYRICS;
          resetGUI((int)MyLyricsSettings.Screen.LYRICS);

          _newTrack = true;
          break;

        case "Make LRC":
          // Don't use a stream to create a LRC
          if (g_Player.IsRadio)
          {
            GUIDialogOK dlg2 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
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
          if (_selectedScreen != (int)MyLyricsSettings.Screen.LRC_EDITOR)
          {
            if (CheckReallyEditLRCBeforeEdit())
            {
              resetGUI((int)MyLyricsSettings.Screen.LRC_EDITOR);
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

          _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;

          //_selectedScreen = (int)MyLyricsSettings.Screen.LYRICS;
          resetGUI((int)MyLyricsSettings.Screen.LYRICS);

          _newTrack = true;

          if (CONTROL_LYRIC_SELECTED == (int)GUI_Lyrics_Controls.CONTROL_Lyric)
          {
            CONTROL_LYRIC_SELECTED = (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll;
            GUIControl.ShowControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll);
            GUIControl.HideControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric);
          }
          else
          {
            CONTROL_LYRIC_SELECTED = (int)GUI_Lyrics_Controls.CONTROL_Lyric;
            GUIControl.HideControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric_Scroll);
            GUIControl.ShowControl(GetID, (int)GUI_Lyrics_Controls.CONTROL_Lyric);
          }

          break;
      }

      if (dlg.SelectedLabelText.Equals(translateLabelString))
      {
        TranslateProvider.TranslateProvider translate = null;

        string lyricToTranslate = string.Empty;

        SimpleLRC lrc = new SimpleLRC(_artist, _title, _LyricText);
        if (lrc.IsValid)
        {
          lyricToTranslate = lrc.LyricAsPlainLyric;
        }
        else
        {
          lyricToTranslate = _LyricText;
        }

        if (String.IsNullOrEmpty(lyricToTranslate)) return;

        _StatusText = string.Empty;
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);

        try
        {
          translate = new TranslateProvider.TranslateProvider("www.team-mediaportal.com/MyLyrics");
        }
        catch (FileNotFoundException)
        {
          GUIDialogOK dlg3 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlg3.SetHeading("File not found");
          dlg3.SetLine(1, "The TranslateProvider.dll assembly");
          dlg3.SetLine(2, "could not be located!");
          dlg3.SetLine(3, String.Empty);
          dlg3.DoModal(GUIWindowManager.ActiveWindow);
        }

        string detectedLanguage;
        bool reliable;
        double confidence;

        lyricToTranslate = lyricToTranslate.Replace("\n", "\n ");

        string translation = string.Empty;
        try
        {
          translation = translate.Translate(lyricToTranslate, _translationLanguageCode,
                                                   out detectedLanguage, out reliable, out confidence, "\n");
        }
        catch
        {
          GUIDialogOK dlg3 = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
          dlg3.SetHeading("Error");
          dlg3.SetLine(1, "Error occured while trying to translate!");
          dlg3.SetLine(2, String.Empty);
          dlg3.SetLine(3, String.Empty);
          dlg3.DoModal(GUIWindowManager.ActiveWindow);
          return;
        }

        translation = translation.Replace("\n ", "\n");

        translation = MediaPortalUtil.MakePlainLyricPerfectToShow(translation);

        resetGUI((int)MyLyricsSettings.Screen.LYRICS);
        _isInTranslation = true;
        GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, translation);
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_TITLE,
                                   string.Format("{0} translation", _translationLanguage));

        GUIControl.FocusControl(GetID, CONTROL_LYRIC_SELECTED);
      }
    }


    //event driven handler to detect track change
    private void trackChangeHandler(string tag2, string value)
    {
      if (tag2.Contains("#percentage") || tag2.Contains("#currentplaytime") || tag2.Contains("#currentremaining"))
        return;

      if (tag2.Equals("#Play.Current.Track") || tag2.Equals("#Play.Current.Title") || tag2.Equals("#Play.Current.File") || tag2.Equals("#Play.Current.Artist")) // track has changed
      {
        if (value.Length != 0) // additional check        
        {
          resetLrcFields();
          resetGUI(_selectedScreen);
          StopThread();
          resetAll();
          _newTrack = true;
          _SearchType = (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS;
          _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
          Process();
        }
        else
        {
          _artist = "";
          _title = "";
          _TrackText = "";
          _LyricText = "";
          _ImagePathContainer.Clear();
          GUIelement_ImgCoverArt.Dispose();

          resetGUI(_selectedScreen);

          _StatusText = "No music file is playing";
          GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
        }
      }
      else if (g_Player.IsRadio)
      {
        string newArtist = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Artist"));
        string newTitle = LyricUtil.CapatalizeString(GUIPropertyManager.GetProperty("#Play.Current.Title"));

        if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Artist"))
            || string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Title")))
        {
          _StatusText = "Stream info not complete";
          GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
        }
        else if (!g_Player.CurrentFile.Equals(_LastStreamFile)
                 || !newArtist.Equals(_artist) || !newTitle.Equals(_title))
        {
          resetGUI(_selectedScreen);
          StopThread();
          resetAll();

          _SearchType = (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS;
          _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;

          _LastStreamFile = g_Player.CurrentFile;

          _LastFileName = g_Player.CurrentFile;
          _artist = newArtist;
          _title = newTitle;

          _newTrack = true;

          Process();
        }
      }
    }


    //event driven handler to detect track change. Only used in LRC_Editor mode.
    private void TimeHandler(string tag2, string value)
    {
      if (tag2.Equals("#currentplaytime"))
      {
        if (_Stopwatch != null && _Stopwatch.IsRunning)
        {
          _Stopwatch.Stop();
        }
        _Stopwatch = new Stopwatch();
        _Stopwatch.Start();

        string[] timeStrings = value.Split(':');
        _min = int.Parse(timeStrings[0]);
        _sec = int.Parse(timeStrings[1]);
      }
    }

    /// <summary>
    /// FindLrc searches for a lyric related to the given tag.
    /// </summary>
    private bool FindLrc()
    {
      _EventStopThread.Reset();

      GUIControl.ClearControl(GetID, CONTROL_LYRIC_SELECTED);
      GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, "");

      resetGUI((int)MyLyricsSettings.Screen.LRC);

      if ((_CurrentTrackTag != null && _CurrentTrackTag.Artist != "") || g_Player.IsRadio)
      {
        _TrackText = _artist + " - " + _title;

        if (_enableLogging)
        {
          LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                  LyricDiagnostics.ElapsedTimeString() + "FindLrc(" + _artist +
                                                  ", " + _title + ")");
        }

        /* The prioritized search order is:
           1) LRC in music tag
           2) LRC in database
           3) Search for LRC
           4) Lyric in music tag
           5) Lyric in database
           6) Search for lyric */

        // (1 of 2) Search LRCS
        if (_SearchType != (int)SEARCH_TYPES.ONLY_LYRICS)
        {
          _StatusText = "Searching for a matching LRC...";
          GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);

          #region 1) LRC in music tag

          if (g_Player.IsRadio == false
              && _CurrentTrackTag != null && ((_CurrentTrackTag.Lyrics.Length != 0
                                                &&
                                                (_SimpleLrc =
                                                 new SimpleLRC(_artist, _title, _CurrentTrackTag.Lyrics)).
                                                    IsValid)))
          {
            if (_SimpleLrc.IsValid)
            {
              StartShowingLrc(_CurrentTrackTag.Lyrics, false);
              SaveLyricToDatabase(_artist, _title, _CurrentTrackTag.Lyrics, "music tag", true);
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

          if (lyricText != null && (_SimpleLrc = new SimpleLRC(_artist, _title, lyricText)).IsValid)
          {
            if (_SimpleLrc.IsValid)
            {
              StartShowingLrc(lyricText, false);
              return true;
            }
          }

          #endregion

          #region 3) Search the Internet for a LRC

          if (_useLrcFinder && Setup.IsMember("LrcFinder"))
          {
            _lyricsFound = false;

            _lc = new LyricsController(this, _EventStopThread, new string[1] { "LrcFinder" }, false, false,
                                        _Find, _Replace);

            // create worker thread instance
            ThreadStart job = delegate { _lc.Run(); };

            _LyricControllerThread = new Thread(job);
            _LyricControllerThread.Name = "LrcFinder search";
            _LyricControllerThread.Start();

            _lc.AddNewLyricSearch(_artist, _title,
                                   MediaPortalUtil.GetStrippedPrefixArtist(_artist, _strippedPrefixStrings));

            _LyriccontrollerIsWorking = true;
          }
          else
          {
            _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;
            _StatusText = "No matching LRC found";
            GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
          }

          #endregion

          return false;
        }
        else
        {
          return false;
        }
      }
      else
      {
        return false;
      }
    }

    private bool FindLyric()
    {
      _EventStopThread.Reset();

      if ((_CurrentTrackTag != null && _CurrentTrackTag.Artist != "") || g_Player.IsRadio)
      {
        if (_enableLogging)
        {
          LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                  LyricDiagnostics.ElapsedTimeString() + "FindLyric(" +
                                                  _artist + ", " + _title + ")");
        }

        string lyricText = string.Empty;

        if (LyricsDb.ContainsKey(DatabaseUtil.CorrectKeyFormat(_artist, _title)))
        {
          lyricText = LyricsDb[DatabaseUtil.CorrectKeyFormat(_artist, _title)].Lyrics;
        }

        if (_SearchType != (int)SEARCH_TYPES.ONLY_LRCS)
        {
          #region 4) Lyric in music tag

          if (_AutomaticReadFromMusicTag && g_Player.IsRadio == false
              && _CurrentTrackTag != null && _CurrentTrackTag.Lyrics.Length != 0
              &&
              !((/*!_useLrcFinder &&*/
                new SimpleLRC(_artist, _title, _CurrentTrackTag.Lyrics).IsValid)))
          {
            string lyric = LyricUtil.FixLyrics(_CurrentTrackTag.Lyrics);
            _CurrentTrackTag.Lyrics = lyric;
            ShowLyricOnScreen(lyric, "music tag");
            SaveLyricToDatabase(_CurrentTrackTag.Artist, _CurrentTrackTag.Title, lyric, "music tag", false);
            return true;
          }

          #endregion

          #region 5) Lyric in music database

          if (lyricText.Length != 0 &&
              !((/*!_useLrcFinder && */ new SimpleLRC(_artist, _title, lyricText).IsValid)))
          {
            LyricFound = new Object[] { lyricText, _artist, _title, "lyrics database" };
            return true;
          }
          #endregion

          #region 6) Search the Internet for a lyric

          else if (_LyricSitesTosearch.Length > 0)
          {
            _StatusText = "Searching for a matching lyric...";
            GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);

            resetGUI((int)MyLyricsSettings.Screen.LYRICS);

            _lyricsFound = false;

            _lc = new LyricsController(this, _EventStopThread, _LyricSitesTosearch, false, false, _Find,
                                        _Replace);
            // create worker thread instance
            ThreadStart job = delegate { _lc.Run(); };

            _LyricControllerThread = new Thread(job);
            _LyricControllerThread.Name = "lyricSearch Thread"; // looks nice in Output window
            _LyricControllerThread.Start();

            _lc.AddNewLyricSearch(_artist, _title,
                                   MediaPortalUtil.GetStrippedPrefixArtist(_artist, _strippedPrefixStrings));

            _LyriccontrollerIsWorking = true;

            return false;
          }
          else
          {
            return false;
          }

          #endregion
        }
        else
        {
          return false;
        }
      }
      else
      {
        _TrackText = "";
        _ImagePathContainer.Clear();
        GUIelement_ImgCoverArt.Dispose();

        resetGUI(_selectedScreen);
        resetLrcFields();

        _StatusText = "No music file is playing";
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
        return false;
      }
    }

    private void StartShowingLrc(string lyricText, bool showLrcPickScreen)
    {
      _lyricsFound = true;
      _ValidLrcLyric = true;

      _LrcTimeCollection = _SimpleLrc.SimpleLRCTimeAndLineCollectionWithOffset;
      _lines = _LrcTimeCollection.Copy();

      if (_enableLogging)
      {
        LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                LyricDiagnostics.ElapsedTimeString() + "LRC found: " + _artist +
                                                " - " + _title + ".");
      }

      if (showLrcPickScreen)
      {
        resetGUI((int)MyLyricsSettings.Screen.LRC_PICK);
      }
      else
      {
        resetGUI((int)MyLyricsSettings.Screen.LRC);
      }

      _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;
      _StatusText = "";
      GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);

      _LyricText = lyricText;

      try
      {
        for (int i = _tagRoundFinished * _TAG_IN_ROUND; i < (_tagRoundFinished + 1) * _TAG_IN_ROUND; i++)
        {
          if (useEditControlsOnLRCPick() && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
          {
            ShowLrcLine((int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, _lines[i]);
            ShowLrcLine((int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, _lines[i]);
          }
          else
          {
            ShowLrcLine((int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, _lines[i]);
            ShowLrcLine((int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i, _lines[i]);
          }
        }

        if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
        {
          for (int i = _tagRoundFinished * _TAG_IN_ROUND; i < (_tagRoundFinished + 1) * _TAG_IN_ROUND; i++)
          {
            SimpleLRCTimeAndLine currentLine = _LrcTimeCollection[i];
            GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i,
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
      if (_alreadyValidLRC == false && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
      {
        if (_Stopwatch == null)
          return;

        if (_LRCLinesTotal < _lines.Length)
        {
          string time = "[" + _min + ":" + (_sec.ToString().Length == 2 ? _sec.ToString() : "0" + _sec) +
                        "." +
                        (_Stopwatch.ElapsedMilliseconds.ToString().Length >= 2
                             ? _Stopwatch.ElapsedMilliseconds.ToString().Substring(0, 2)
                             : _Stopwatch.ElapsedMilliseconds + "0") + "]";
          _lines[_LRCLinesTotal] = time + _lines[_LRCLinesTotal];
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + nextLRCLineIndex,
                                     time);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + nextLRCLineIndex);
          GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + nextLRCLineIndex);
        }

        if (++_LRCLinesTotal < _lines.Length)
        {
          // If a new round has to start
          if (++nextLRCLineIndex == _TAG_IN_ROUND)
          {
            nextLRCLineIndex = 0;
            ++_tagRoundFinished;


            for (int i = 0; i < _TAG_IN_ROUND; i++)
            {
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i, "");
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, "");
            }

            try
            {
              for (int i = 0; i < _TAG_IN_ROUND && _LRCLinesTotal + i < _lines.Length; i++)
              {
                GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i,
                                           _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
                GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i,
                                           _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
                GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
                GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
                GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i,
                                           "[xx:xx.xx]");
              }
            }
            catch
            {
              ;
            }
          }
        }
        else
        {
          StringBuilder lyric = new StringBuilder();

          string artist = LyricUtil.CapatalizeString(_artist);
          string title = LyricUtil.CapatalizeString(_title);

          lyric.AppendLine(string.Format("[ar:{0}]", artist));
          lyric.AppendLine(string.Format("[ti:{0}]", title));
          if (!string.IsNullOrEmpty(_LrcTaggingName))
          {
            lyric.AppendLine(string.Format("[by:{0}]", _LrcTaggingName));
          }
          if (!string.IsNullOrEmpty(_LrcTaggingOffset))
          {
            lyric.AppendLine(string.Format("[offset:{0}]", _LrcTaggingOffset));
          }
          lyric.AppendLine("[ap:MediaPortal]");

          for (int i = 0; i < _lines.Length; i++)
          {
            lyric.AppendLine(_lines[i] + "");
          }

          lyric.Replace("\r", "");

          string lyricAsString = lyric.ToString();

          //lyricAsString = lyricAsString.Substring(0, lastLineShift);
          _LyricText = lyricAsString;

          SaveLyricToDatabase(artist, title, _LyricText, "MyLyrics LRC Editor", true);

          _ValidLrcLyric = true;

          if (_CurrentTrackTag != null)
          {
            if (worker.IsBusy)
              worker.CancelAsync();
            SaveLyricToTagListsData data = new SaveLyricToTagListsData(_LyricText, artist, title, _CurrentTrackTag.FileName);
            //worker.RunWorkerAsync(data);
            //SaveLyricToTagLists(_CurrentTrackTag.FileName, _LyricText);
            SaveLyricToTagLists(this, new DoWorkEventArgs(data));
          }

          _selectedScreen = (int)MyLyricsSettings.Screen.LRC;
          ShowLyricOnScreen(_LyricText, "MediaPortal");

          // Upload LRC to LrcFinder if user has accepted in configuration
          if (_uploadLrcToLrcFinder && !_alwaysAskUploadLrcToLrcFinder)
          {
            UploadLrcFile(_LyricText);
          }
          else if (_alwaysAskUploadLrcToLrcFinder || !_confirmedNoUploadLrcToLrcFinder)
          {
            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_YES_NO);
            if (dlgYesNo != null)
            {
              dlgYesNo.Reset();

              dlgYesNo.SetHeading("Upload to LRCFinder?");

              dlgYesNo.SetLine(1, "Upload this and future created");
              dlgYesNo.SetLine(2, "LRC files to LrcFinder?");

              dlgYesNo.DoModal(GetID);

              if (dlgYesNo.IsConfirmed)
              {
                UploadLrcFile(_LyricText);

                _uploadLrcToLrcFinder = true;

                using (Settings xmlwriter = new Settings("MediaPortal.xml"))
                {
                  xmlwriter.SetValue("myLyrics", "uploadLrcToLrcFinder", "yes");
                }
              }
              else
              {
                _confirmedNoUploadLrcToLrcFinder = true;

                using (Settings xmlwriter = new Settings("MediaPortal.xml"))
                {
                  xmlwriter.SetValue("myLyrics", "confirmedNoUploadLrcToLrcFinder", "yes");
                }
              }
            }
          }
        }
      }
    }


    private void UploadLrcFile(string lrcFile)
    {
      LrcFinder lrcFinder = new LrcFinder();

      bool lrcUploaded = lrcFinder.SaveLrcWithGuid(lrcFile, _guid);

      if (lrcUploaded)
      {
        string status = "Your LRC was successfully uploaded";
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);
      }
      else
      {
        string status = "LrcFinder could not be reached";
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, status);
      }
    }

    private void RemoveLatestTagLine()
    {
      --_LRCLinesTotal;

      if (_LRCLinesTotal < 0)
      {
        _LRCLinesTotal = 0;
        return;
      }

      string lastTimeStampTemp = _lines[_LRCLinesTotal].Substring(0, 9);
      _lines[_LRCLinesTotal] = _lines[_LRCLinesTotal].Substring(9);

      if (--nextLRCLineIndex < 0)
      {
        nextLRCLineIndex = _TAG_IN_ROUND - 1;
        --_tagRoundFinished;

        try
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i,
                                       _lines[_tagRoundFinished * _TAG_IN_ROUND + i].Substring(0, 9));
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i,
                                       _lines[_tagRoundFinished * _TAG_IN_ROUND + i].Substring(9));
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i,
                                       _lines[_tagRoundFinished * _TAG_IN_ROUND + i].Substring(9));
            GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
          }
        }
        catch
        {
          ;
        }

        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + nextLRCLineIndex);
        GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + nextLRCLineIndex);
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + nextLRCLineIndex,
                                   "[xx:xx.xx]");

        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + nextLRCLineIndex,
                                   _lines[_tagRoundFinished * _TAG_IN_ROUND + nextLRCLineIndex]);
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + nextLRCLineIndex,
                                   _lines[_tagRoundFinished * _TAG_IN_ROUND + nextLRCLineIndex]);
      }
      else
      {
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + nextLRCLineIndex);
        GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + nextLRCLineIndex);
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + nextLRCLineIndex,
                                   "[xx:xx.xx]");
      }
    }


    private void CalculateNextInterval()
    {
      if (_LrcTimeCollection != null)
      {
        int trackTime = (int)(g_Player.CurrentPosition * 1000);
        nextLRCLineIndex = _LrcTimeCollection.GetSimpleLRCTimeAndLineIndex(trackTime);

        SimpleLRCTimeAndLine currentLine = _LrcTimeCollection[nextLRCLineIndex];

        _tagRoundFinished = nextLRCLineIndex / _TAG_IN_ROUND;
        int localIndex = (nextLRCLineIndex % _TAG_IN_ROUND);

        if (nextLRCLineIndex == _LrcTimeCollection.Count - 1)
        {
          if (currentLine.Time - trackTime < 500)
          {
            ++localIndex;
          }
        }

        #region Show LRC lines in LRC mini labels

        int currentLRCLineIndex = nextLRCLineIndex > 0 ? nextLRCLineIndex - 1 : 0;

        if (_LrcTimeCollection[nextLRCLineIndex].Time < trackTime)
        {
          ++currentLRCLineIndex;
        }


        // 1. The two previous lines
        if (currentLRCLineIndex >= 2)
        {
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 0, _lines[currentLRCLineIndex - 2]);
        }
        if (currentLRCLineIndex >= 1)
        {
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 1, _lines[currentLRCLineIndex - 1]);
        }

        // 2. The current line
        ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 2, _lines[currentLRCLineIndex + 0]);

        // 3. The two future lines
        // If last, then show empty lines for fourth and fifth label (showing the future LRC lines)
        if (currentLRCLineIndex + 1 == _LrcTimeCollection.Count)
        {
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 3, string.Empty);
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 4, string.Empty);
        }
        // If second last then clear the last label (only one future LRC lines left)
        else if (currentLRCLineIndex + 2 == _LrcTimeCollection.Count)
        {
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 3, _lines[currentLRCLineIndex + 1]);
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 4, string.Empty);
        }
        else
        {
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 3, _lines[currentLRCLineIndex + 1]);
          ShowLrcLine((int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + 4, _lines[currentLRCLineIndex + 2]);
        }

        #endregion

        if (_tagRoundFinished > 0 && localIndex == 0)
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            if (useEditControlsOnLRCPick() && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
            {
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
            }
            else
            {
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
            }
          }
        }
        else
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            if (useEditControlsOnLRCPick() && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
            {
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, "");
            }
            else
            {
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i, "");
            }
          }

          if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
          {
            for (int i = 0; i < _TAG_IN_ROUND; i++)
            {
              SimpleLRCTimeAndLine currentLineTime =
                  _LrcTimeCollection[_tagRoundFinished * _TAG_IN_ROUND + i];
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i,
                                         (currentLineTime != null
                                              ? currentLineTime.TimeString
                                              : string.Empty));
            }
          }

          try
          {
            for (int i = 0; i < _TAG_IN_ROUND; i++)
            {
              if (useEditControlsOnLRCPick() && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
              {
                ShowLrcLine((int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i,
                            _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
                GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
                ShowLrcLine((int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i,
                            _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
              }
              else
              {
                ShowLrcLine((int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i,
                            _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
                GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
                ShowLrcLine((int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i,
                            _lines[_tagRoundFinished * _TAG_IN_ROUND + i]);
              }
            }
          }
          catch
          {
            ;
          }
          ;

          // Highlight the lines that have been passed in the current interval
          for (int i = 0; i < localIndex; i++)
          {
            if (useEditControlsOnLRCPick() && _selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
            {
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
            }
            else
            {
              GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
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
      _LyricText = lyricText;

      if (_enableLogging)
      {
        LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                LyricDiagnostics.ElapsedTimeString() + "Lyric found: " +
                                                _artist + " - " + _title + ". Place: " + source);
      }

      _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;

      _StatusText = "";
      GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);

      if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
      {
        ShowLRCtoEdit();
      }
      else
      {
        // If LRC that has been found, then show lyric in LRC-mode
        _SimpleLrc = new SimpleLRC(null, null, _LyricText);

        if (_SimpleLrc.IsValid)
        {
          _SearchType = (int)SEARCH_TYPES.ONLY_LRCS;
          StartShowingLrc(_LyricText, false);
        }
        // else show plain lyric
        else
        {
          _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;

          _LyricText = MediaPortalUtil.MakePlainLyricPerfectToShow(_LyricText);

          resetGUI((int)MyLyricsSettings.Screen.LYRICS);
          GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, _LyricText);
          GUIControl.FocusControl(GetID, CONTROL_LYRIC_SELECTED);
        }
      }
    }

    private void SaveLyricToDatabase(string artist, string title, string lyric, string site, bool lrc)
    {
      string capArtist = LyricUtil.CapatalizeString(artist);
      string capTitle = LyricUtil.CapatalizeString(title);

      if (DatabaseUtil.IsSongInLyricsDatabase(LyricsDb, capArtist, capTitle).Equals(DatabaseUtil.LYRIC_NOT_FOUND))
      {
        LyricsDb.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                       new LyricsItem(capArtist, capTitle, lyric, site));
        SaveDatabase(MLyricsDbName, LyricsDb);
      }
      else if (
          !DatabaseUtil.IsSongInLyricsDatabaseAsLRC(LyricsDb, capArtist, capTitle).Equals(
               DatabaseUtil.LRC_FOUND))
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
              DatabaseUtil.LYRIC_MARKED))
      {
        LyricsMarkedDb.Remove(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle));
        SaveDatabase(MLyricsMarkedDbName, LyricsMarkedDb);
      }
    }

    private void SaveDatabase(string dbName, LyricsDatabase lyricsDatabase)
    {
      string path = Config.GetFile(Config.Dir.Database, dbName);
      using (FileStream fs = new FileStream(path, FileMode.Open))
      {
        BinaryFormatter bf = new BinaryFormatter();
        lyricsDatabase.SetLastModified();
        bf.Serialize(fs, lyricsDatabase);
        fs.Close();
      }
    }

    // Stop worker thread if it is running.
    // Called when user presses Stop button of form is closed.
    private void StopThread()
    {
      if (_LyricControllerThread != null && _LyricControllerThread.IsAlive) // thread is active
      {
        // set event "Stop"
        _EventStopThread.Set();

        while (_LyricControllerThread.IsAlive)
          Thread.Sleep(100);

        _LyricControllerThread = null;
        _lc = null;
      }

      _LyriccontrollerIsWorking = false;
    }


    private void resetAll()
    {
      _newTrack = false;
      _artist = "";
      _title = "";
      _TrackText = "";
      _StatusText = "";
      _LyricText = "";
      _lyricsFound = false;
      _LyricControllerThread = null;
      _lc = null;

      resetLrcFields();
    }

    private void resetLrcFields()
    {
      if (_Stopwatch != null)
      {
        _Stopwatch.Reset();
      }

      _SimpleLrc = null;
      ;
      _LrcTimeCollection = null;
      ;
      nextLRCLineIndex = 0;
      _LRCLinesTotal = 0;
      _tagRoundFinished = 0;
      _min = 0;
      _sec = 0;
      _msec = 0;
      _lines = null;
    }

    private void resetGUI(int screenID)
    {
      int prevSelectedScreen = _selectedScreen;
      _selectedScreen = screenID;

      _isInTranslation = false;

      GUIPropertyManager.OnPropertyChanged -= TimeHandler;

      if (_selectedScreen == (int)MyLyricsSettings.Screen.LYRICS)
      {
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_TITLE, "Lyrics screen");

        GUIControl.ClearControl(GetID, CONTROL_LYRIC_SELECTED);
        GUIControl.FocusControl(GetID, CONTROL_LYRIC_SELECTED);

        // Reset general and lyrics controls
        GUIControl.ShowControl(GetID, (int)GUI_General_Controls.CONTROL_LBStatus);
        GUIControl.HideControl(GetID, CONTROL_LYRIC_SELECTED);
        GUIControl.ShowControl(GetID, CONTROL_LYRIC_SELECTED);
        GUIControl.SetControlLabel(GetID, CONTROL_LYRIC_SELECTED, "");

        // album art only visible for basic screen
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_CURRENTLY);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_DURATION);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUM);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_YEAR);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_NOWPLAYINGBACK);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUMART);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESS);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESSIMAGE);

        // Hide LRC controls
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "");
        GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

        for (int i = 0; i < _TAG_IN_ROUND; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);

          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
        }

        for (int i = 0; i < 5; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i, "");
        }
      }

      else if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC)
      {
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_TITLE, "LRC screen");

        GUIControl.FocusControl(GetID, CONTROL_LYRIC_SELECTED);

        // Lyrics controls
        GUIControl.ShowControl(GetID, (int)GUI_General_Controls.CONTROL_LBStatus);
        //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
        GUIControl.HideControl(GetID, CONTROL_LYRIC_SELECTED);

        // album art only visible for basic screen
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_CURRENTLY);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_DURATION);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUM);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_YEAR);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_NOWPLAYINGBACK);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUMART);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESS);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESSIMAGE);

        // LRC controls
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "");
        GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

        for (int i = 0; i < _TAG_IN_ROUND; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);

          GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i, "");
        }

        if (string.IsNullOrEmpty(_artist))
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
          }
        }

        for (int i = 0; i < 5; i++)
        {
          GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i, "");
        }
      }


      else if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_EDITOR)
      {
        GUIPropertyManager.OnPropertyChanged += TimeHandler;

        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_TITLE, "LRC editor");

        // Lyrics controls
        GUIControl.ShowControl(GetID, (int)GUI_General_Controls.CONTROL_LBStatus);
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, "");
        //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
        GUIControl.HideControl(GetID, CONTROL_LYRIC_SELECTED);

        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_CURRENTLY);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_DURATION);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUM);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_YEAR);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_NOWPLAYINGBACK);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUMART);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESS);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESSIMAGE);

        // LRC controls
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "Tag");
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        GUIControl.FocusControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

        for (int i = 0; i < _TAG_IN_ROUND; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i, "");
          //GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, "");
          GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");

          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
        }

        if (string.IsNullOrEmpty(_artist))
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i, "");
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, "");
          }
          resetGUI((int)MyLyricsSettings.Screen.LYRICS);
        }

        for (int i = 0; i < 5; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i, "");
        }
      }

      else if (_selectedScreen == (int)MyLyricsSettings.Screen.LRC_PICK)
      {
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_TITLE, "LRC pick");

        // Lyrics controls
        GUIControl.ShowControl(GetID, (int)GUI_General_Controls.CONTROL_LBStatus);
        //GUIControl.SetControlLabel(GetID, (int) GUI_General_Controls.CONTROL_LBStatus, "");
        GUIControl.HideControl(GetID, CONTROL_LYRIC_SELECTED);

        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_CURRENTLY);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_DURATION);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUM);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_YEAR);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_NOWPLAYINGBACK);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_ALBUMART);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESS);
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_ART_PROGRESSIMAGE);

        // LRC controls
        GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "Pick");
        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        GUIControl.FocusControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);
        //GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON, "");
        //GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_TAGPICKBUTTON);

        GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS);

        //if (prevSelectedScreen != (int) MyLyricsSettings.Screen.LRC)
        //{
        for (int i = 0; i < _TAG_IN_ROUND; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_TIME + i, " ");
          if (useEditControlsOnLRCPick())
          {
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);

            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");
            GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i, "");
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);
          }
          else
          {
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE_DONE + i);

            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
            GUIControl.ShowControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i, "");
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE_DONE + i);
          }
        }
        //}

        if (string.IsNullOrEmpty(_artist))
        {
          for (int i = 0; i < _TAG_IN_ROUND; i++)
          {
            GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i);
            GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_VIEW_LINE + i, "");
            if (useEditControlsOnLRCPick())
            {
              GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i);
              GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_EDIT_LINE + i, "");
            }
          }
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_LRCPICK_STATUS, "");
          resetGUI((int)MyLyricsSettings.Screen.LRC);
        }

        for (int i = 0; i < 5; i++)
        {
          GUIControl.HideControl(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i);
          GUIControl.SetControlLabel(GetID, (int)GUI_LRC_Controls.CONTROL_MINI_VIEW_LINE + i, "");
        }
      }
    }

    #region ILyricForm properties

    public Object[] UpdateString
    {
      set
      {
        string line1 = (string)value[0];
        string line2 = (string)value[1];
        _StatusText = line1 + "\r\n" + line2;
        GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
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
          string lyricText = (String)value[0];
          string artist = (String)value[1];
          string title = (String)value[2];
          string site = (String)value[3];

          // If the lrc or lyric found matches the currently playing show the lyric else don't!
          if (_artist.Equals(artist) && _title.Equals(title))
          {
            ShowLyricOnScreen(lyricText, site);

            SaveLyricToDatabase(artist, title, lyricText, site, site.Equals("LrcFinder"));

            if (_CurrentTrackTag != null)
            {
              if (worker.IsBusy)
                worker.CancelAsync();
              SaveLyricToTagListsData data = new SaveLyricToTagListsData(lyricText, _artist, _title, _CurrentTrackTag.FileName);
              worker.RunWorkerAsync(data);
              //SaveLyricToTagLists(_CurrentTrackTag.FileName, lyricText);
            }
          }

          _SearchingState = (int)SEARCH_STATE.NOT_SEARCHING;
          _LyriccontrollerIsWorking = false;
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
        string artist = (String)value[0];
        string title = (String)value[1];
        string message = (String)value[2];
        string site = (String)value[3];

        if (!_lyricsFound && _artist.Equals(artist) && _title.Equals(title))
        {
          if (site.Equals("LrcFinder"))
          {
            if (_SearchType == (int)SEARCH_TYPES.BOTH_LRCS_AND_LYRICS)
              _SearchType = (int)SEARCH_TYPES.ONLY_LYRICS;
            _StatusText = "No matching LRC found";

            if (_enableLogging)
            {
              LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                      LyricDiagnostics.ElapsedTimeString() +
                                                      "No matching LRC found for " + artist + " - " +
                                                      title + ". Place: " + site);
            }
          }
          else
          {
            _StatusText = "No matching lyric found";

            if (_enableLogging)
            {
              LyricDiagnostics.TraceSource.TraceEvent(TraceEventType.Information, 0,
                                                      LyricDiagnostics.ElapsedTimeString() +
                                                      "No matching lyric found for " + artist + " - " +
                                                      title + ". Place: " + site);
            }
          }
          GUIControl.SetControlLabel(GetID, (int)GUI_General_Controls.CONTROL_LBStatus, _StatusText);
        }

        GUIControl.ShowControl(GetID, CONTROL_LYRIC_SELECTED);

        _LyriccontrollerIsWorking = false;
      }
    }

    public string ThreadException
    {
      set { string message = value; }
    }

    //private void SaveLyricToTagLists(string fileName, string lyric)
    private void SaveLyricToTagLists(object sender, DoWorkEventArgs e)
    {
      SaveLyricToTagListsData data = (SaveLyricToTagListsData)e.Argument;
      if (!string.IsNullOrEmpty(data.FileName) && !string.IsNullOrEmpty(data.Lyrics))
      {
        lock (_LyricsToWriteToTag)
        {
          MusicTag currentTrackTag = _CurrentTrackTag;
          // if lyric is a LRC, then always add
          if (new SimpleLRC(data.Artist, data.Title, data.Lyrics).IsValid)
          {
            if (currentTrackTag == _CurrentTrackTag) //only if track didnt change - i think it's unsafe to lock it
            {
              _CurrentTrackTag.Lyrics = data.Lyrics;
            }
            _LyricsToWriteToTag.Add(new string[2] { data.FileName, data.Lyrics });
          }
          // if 'lyric' is plain lyric and lyric in musictag is LRC, then DON'T add
          else if (new SimpleLRC(data.FileName).IsValid)
          {
            return;
          }
          // if lyric in musictag is not LRC, then add plain lyric
          else
          {
            if (currentTrackTag == _CurrentTrackTag) //only if track didnt change - i think it's unsafe to lock it
            {
              _CurrentTrackTag.Lyrics = data.Lyrics;
            }
            _LyricsToWriteToTag.Add(new string[2] { data.FileName, data.Lyrics });
          }
        }
      }
    }

    #endregion

    #region ISetup Interface methods

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        strButtonText = (xmlreader.GetValueAsString("myLyrics", "pluginsName", "My Lyrics"));
      }
      //strButtonText = "Myrics";//GUILocalizeStrings.Get(9); // My News
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
      return WINDOW_MYLYRICS;
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
      return "mackey & yoavain (with the help of BennieBoy, SilentException, Søren)";
    }

    /// <summary>
    /// See ISetupForm interface
    /// </summary>
    public void ShowPlugin()
    {
      MyLyricsSetup dlg = new MyLyricsSetup();
      dlg.ShowDialog();
    }

    #endregion

    #region Nested type: GUI_General_Controls

    private enum GUI_General_Controls
    {
      CONTROL_BACKGROUND = 1,
      CONTROL_TITLE = 2,
      CONTROL_LBStatus = 11,
      CONTROL_UPNEXT = 22,
      CONTROL_ALBUM = 26,
      CONTROL_YEAR = 27,
      CONTROL_TRACKTITLE = 30,
      CONTROL_TRACKARTIST = 32,
      CONTROL_NUMBERDURATION = 33,
      CONTROL_NEXTTRACK = 121,
      CONTROL_NEXTARTIST = 123,
    }

    #endregion

    #region Nested type: GUI_LRC_Controls

    private enum GUI_LRC_Controls
    {
      CONTROL_ART_CURRENTLY = 24,
      CONTROL_ART_DURATION = 25,
      CONTROL_ART_ALBUM = 26,
      CONTROL_ART_YEAR = 27,
      CONTROL_ART_NOWPLAYINGBACK = 31,
      CONTROL_ART_ALBUMART = 112,
      CONTROL_ART_PROGRESS = 118,
      CONTROL_ART_PROGRESSIMAGE = 117,

      CONTROL_TAGPICKBUTTON = 50,

      CONTROL_EDIT_TIME = 600,
      CONTROL_EDIT_LINE = 200,
      CONTROL_EDIT_LINE_DONE = 300,

      CONTROL_VIEW_LINE = 400,
      CONTROL_VIEW_LINE_DONE = 500,

      CONTROL_LRCPICK_STATUS = 1011,

      CONTROL_MINI_VIEW_LINE = 1400
    }

    #endregion

    #region Nested type: GUI_Lyrics_Controls

    private enum GUI_Lyrics_Controls
    {
      CONTROL_Lyric = 20,
      CONTROL_Lyric_Scroll = 1020,
    }

    #endregion

    #region Nested type: SEARCH_STATE

    private enum SEARCH_STATE
    {
      NOT_SEARCHING = 0,
      SEARCHING_FOR_LRC = 1,
      SEARCHING_FOR_LYRIC = 2
    }

    #endregion

    #region Nested type: SEARCH_TYPES

    private enum SEARCH_TYPES
    {
      BOTH_LRCS_AND_LYRICS = 0,
      ONLY_LRCS = 1,
      ONLY_LYRICS = 2
    }

    #endregion
  }

  public class SaveLyricToTagListsData
  {
    public string Lyrics { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string FileName { get; set; }

    public SaveLyricToTagListsData(string _lyrics, string _artist, string _title, string _filename)
    {
      Lyrics = _lyrics;
      Artist = _artist;
      Title = _title;
      FileName = _filename;
    }
  }
}