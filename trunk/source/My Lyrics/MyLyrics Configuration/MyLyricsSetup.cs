using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Google.API.Translate;
using LyricsEngine;
using LyricsEngine.LyricsSites;
using MediaPortal.Configuration;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MyLyrics.XmlSettings;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace MyLyrics
{
    public partial class MyLyricsSetup : Form, ILyricForm
    {
        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Delegates

        public delegate void DelegateLyricFound(String s, String artist, String track, String site, int row);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site, int row);

        public delegate void DelegateStatusUpdate(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(String s);

        public delegate void DelegateThreadFinished(String artist, String title, String message, String site);

        #endregion

        private const int NumberOfCurrentSearchesAllowed = 6;

        public static bool UpdateLibraryUI = false;
        private ArrayList _artists = new ArrayList();
        private int _hour;

        private readonly Information _informationUserControl;
        private readonly Help _helpUserControl;
        private LyricsController _lyricsController;

        private Queue _lyricConfigInfosQueue;
        private readonly LyricsLibrary _lyricsLibraryUserControl;
        private string _mArtist = "";
        private bool _mAutomaticReadFromToMusicTag = true;
        private bool _mAutomaticWriteToMusicTag = true;

        public DelegateLyricFound MDelegateLyricFound;

        public DelegateLyricNotFound MDelegateLyricNotFound;
        public DelegateStatusUpdate MDelegateStatusUpdate;
        public DelegateStringUpdate MDelegateStringUpdate;

        public DelegateThreadException MDelegateThreadException;
        public DelegateThreadFinished MDelegateThreadFinished;

        // Search criteries
        private bool _mDisregardKnownLyric = true;
        private bool _mDisregardMarkedLyric = true;
        private bool _mDisregardSongWithLyricInTag = true;
        private bool _mDisregardVariousArtist = true;

        private ManualResetEvent _mEventStopThread;
        private string _mFind = string.Empty;
        public Guid MGuid;
        private string _mGuidString = string.Empty;

        private int _mLimit = 100;
        private Thread _mLyricControllerThread;
        private int _mLyricsFound;
        private int _mLyricsNotFound;
        private bool _mMarkSongsWhenNoLyricFound = true;
        

        // Statistics
        private readonly Dictionary<string, int> _lyricsFoundBySite = new Dictionary<string, int>();

        // Collections and arrays
        private string _mReplace = string.Empty;
        private bool _mSearchOnlyMarkedSongs;
        private int _mSongsNotKnown;
        private int _mSongsToSearch;
        private int _mSongsWithLyric;
        private int _mSongsWithMark;
        private string[] _mStrippedPrefixStrings;
        private int _mTotalTitles;
        private string _mTrack = "";
        private MusicDatabase _mMusicDatabase = null;

        // Timer variables
        private int _min;
        private readonly MusicDatabaseBrowse _musicDatabaseBrowseUserControl;
        private int _sec;
        private string[] _sitesToSearchArray;
        private List<Song> _songs = new List<Song>();

        private bool _stopCollectingOfTitles;
        private readonly StopWatch _stopwatch = new StopWatch();
        private Timer _timer;


        public MyLyricsSetup()
        {
            #region Initialize GUI and class

            InitializeComponent();
            _lyricsLibraryUserControl = new LyricsLibrary(this);
            _musicDatabaseBrowseUserControl = new MusicDatabaseBrowse(this);
            _informationUserControl = new Information(this);
            _helpUserControl = new Help(this);

            tabPageLyricsDatabase.Controls.Add(_lyricsLibraryUserControl);
            tabPageMusicDatabaseBrowse.Controls.Add(_musicDatabaseBrowseUserControl);
            tabPageAbout.Controls.Add(_informationUserControl);
            tabPageHelp.Controls.Add(_helpUserControl);

            // initialize delegates
            MDelegateLyricFound = LyricFoundMethod;
            MDelegateLyricNotFound = LyricNotFoundMethod;
            MDelegateThreadFinished = ThreadFinishedMethod;
            MDelegateThreadException = ThreadExceptionMethod;

            // Grab music database
            MusicDatabase mDB = MusicDatabase.Instance;
            //dbs.
            List<AlbumInfo> albums = new List<AlbumInfo>();
            //dbs.GetAllAlbums(ref albums);

            //foreach (AlbumInfo albumInfo in albums)
            //{
            //    ArrayList songs = new ArrayList();
            //    dbs.GetSongsByAlbum(albumInfo.Album, ref songs);
            //    m_TotalTitles += songs.Count;
            //}

            _mTotalTitles = mDB.GetTotalSongs();


            foreach (Language lang in Language.TranslatableCollection)
            {
                comboBoxLanguages.Items.Add(string.Format("{0} ({1})", lang.Name, lang.Value));
            }

            #endregion

            InitSitesList();

            GetSettingsFromConfigurationXml();

            #region Serialzie/deserialize lyricsdatabases

            string path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsDBName);
            FileInfo fileInfo = new FileInfo(path);

            // .. but only if the databases hasn't been created
            if (fileInfo.Exists == false)
            {
                path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsDBName);

                // Serialize empty LyricsDatabase if no lyrics.xml present
                FileStream fs = new FileStream(path, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                MyLyricsUtils.LyricsDB = new LyricsDatabase();
                bf.Serialize(fs, MyLyricsUtils.LyricsDB);
                fs.Close();

                // Serialize empty LyricsMarkedDatabase
                path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsMarkedDBName);
                fs = new FileStream(path, FileMode.Create);
                MyLyricsUtils.LyricsMarkedDB = new LyricsDatabase();
                bf.Serialize(fs, MyLyricsUtils.LyricsMarkedDB);
                fs.Close();
            }
            else
            {
                DeserializeBothDB();
            }

            LyricsLibrary.CurrentDB = MyLyricsUtils.LyricsDB;

            #endregion

            _lyricsLibraryUserControl.updateLyricsTree(false);
        }

        public void CopyCheckedListBox(CheckedListBox copy)
        {
            copy.Items.Clear();

        }

        private void InitSitesList()
        {
            var lyricsSitesNames = LyricsSiteFactory.LyricsSitesNames();
            sitesList.Items.Clear();
            foreach (var site in lyricsSitesNames)
            {
                sitesList.Items.Add(site);
            }
        }

        private void GetSettingsFromConfigurationXml()
        {
            #region Get settings from in configuration file

            try
            {
                var sitesMode = SettingManager.GetParamAsString(SettingManager.SitesMode, rdLyricsMode.Tag as string);

                if (sitesMode.Equals(rdLyricsMode.Tag))
                {
                    rdLyricsMode.Checked = true;
                }
                else if (sitesMode.Equals(rbUserSelectMode.Tag))
                {
                    rbUserSelectMode.Checked = true;

                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        var active = SettingManager.GetParamAsBool(SettingManager.SitePrefix + (sitesList.Items[index]), true);
                        sitesList.SetItemChecked(index, active);
                    }
                }
                else
                {
                    rbLrcMode.Checked = true;
                }

                rdTrackBar_CheckedChanged(null, null);
                Setup.GetInstance().UpdateActiveSitesInSetup(sitesList);                

                tbLimit.Text = SettingManager.GetParamAsString(SettingManager.Limit, _mTotalTitles.ToString());
                tbPluginName.Text = SettingManager.GetParamAsString(SettingManager.PluginsName, SettingManager.DefaultPluginName);

                cbAutoFetch.Checked = SettingManager.GetParamAsBool(SettingManager.AutomaticFetch, false);
                cbAutomaticUpdate.Checked = SettingManager.GetParamAsBool(SettingManager.AutomaticUpdateWhenFirstFound, false);
                cbMoveSongFrom.Checked = SettingManager.GetParamAsBool(SettingManager.MoveLyricFromMarkedDatabase, false);

                _mAutomaticWriteToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, false);
                cbMusicTagWrite.Checked = _mAutomaticWriteToMusicTag;

                _mAutomaticReadFromToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticReadFromMusicTag, false);
                cbMusicTagAlwaysCheck.Checked = _mAutomaticReadFromToMusicTag;

                cbUseAutoScrollAsDefault.Checked = SettingManager.GetParamAsBool(SettingManager.UseAutoscroll, false);
                tbLrcTaggingOffset.Text = SettingManager.GetParamAsString(SettingManager.LrcTaggingOffset, SettingManager.DefaultLrcTaggingOffset);
                tbLrcTaggingName.Text = SettingManager.GetParamAsString(SettingManager.LrcTaggingName, "");

                _mGuidString = SettingManager.GetParamAsString(SettingManager.Guid, "");

                cbUploadLrcAutomatically.Checked = SettingManager.GetParamAsBool(SettingManager.UploadLrcToLrcFinder, false);

                cbAlwaysAskForUploadToLrcFinder.Checked =
                    SettingManager.GetParamAsBool(SettingManager.AlwaysAskUploadLrcToLrcFinder, false);

                lbSongsLimitNote.Text = ("(You have currently " + _mTotalTitles.ToString() + " titles in your music database)");

                trackBar.Value = SettingManager.GetParamAsInt(SettingManager.DefaultSitesModeValue, 2);

                // Update the search sites according to trackbar
                if (rdLyricsMode.Checked)
                {
                    trackBar_Scroll(null, null);
                }

                comboBoxLanguages.SelectedItem = SettingManager.GetParamAsString(SettingManager.TranslationLanguage, SettingManager.DefaultTranslationLanguage);

                _mFind = SettingManager.GetParamAsString(SettingManager.Find, "");
                _mReplace = SettingManager.GetParamAsString(SettingManager.Replace, "");


                if (_mFind != "")
                {
                    string[] findArray = _mFind.Split(',');
                    string[] replaceArray = _mReplace.Split(',');
                    int valueIndex = 0;

                    dbGridView.Rows.Clear();

                    foreach (string findValue in findArray)
                    {
                        if (findValue != "")
                        {
                            dbGridView.Rows.Insert(valueIndex, 1);
                            DataGridViewRow row = dbGridView.Rows[valueIndex];
                            row.Cells[0].Value = findValue;
                            row.Cells[1].Value = replaceArray[valueIndex];
                            valueIndex++;
                        }
                    }
                }

                if (string.IsNullOrEmpty(_mGuidString))
                {
                    MGuid = Guid.NewGuid();
                    _mGuidString = MGuid.ToString("P");

                    SettingManager.SetParam(SettingManager.Guid, _mGuidString);
                }
                else
                {
                    MGuid = new Guid(_mGuidString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something has gone wrong when reading Mediaportal.xml (" + ex.Message + ")");
            }

            _mStrippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            #endregion
        }

        // Stop worker thread if it is running.
        // Called when user presses Stop button of form is closed.
        private void StopThread()
        {
            if (_mLyricControllerThread != null && _mLyricControllerThread.IsAlive) // thread is active
            {
                //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LBStatus, m_StatusText);
                // set event "Stop"
                _mEventStopThread.Set();
                //m_LyricControllerThread = null;

                // wait when thread  will stop or finish
                while (_mLyricControllerThread.IsAlive)
                {
                    // We cannot use here infinite wait because our thread
                    // makes syncronous calls to main form, this will cause deadlock.
                    // Instead of this we wait for event some appropriate time
                    // (and by the way give time to worker thread) and
                    // process events. These events may contain Invoke calls.
                    //if (WaitHandle.WaitAll(
                    //    (new ManualResetEvent[] { m_EventThreadStopped }),
                    //    100,
                    //    true))
                    //{
                    //break;
                    //}

                    Application.DoEvents();
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void isSearching(bool searching)
        {
            if (searching)
            {
                _lyricsLibraryUserControl.btAdd.Enabled = false;
                _lyricsLibraryUserControl.btDelete.Enabled = false;
                _lyricsLibraryUserControl.btImportFiles.Enabled = false;
                _lyricsLibraryUserControl.btImportDirs.Enabled = false;
                _lyricsLibraryUserControl.btResetLyricsDatabase.Enabled = false;
                _lyricsLibraryUserControl.btResetMarkedLyricsDatabase.Enabled = false;
                _lyricsLibraryUserControl.btSave.Enabled = false;
                btClose.Enabled = false;
            }
            else
            {
                _lyricsLibraryUserControl.btAdd.Enabled = true;
                _lyricsLibraryUserControl.btDelete.Enabled = true;
                _lyricsLibraryUserControl.btImportFiles.Enabled = true;
                _lyricsLibraryUserControl.btImportDirs.Enabled = true;
                _lyricsLibraryUserControl.btResetLyricsDatabase.Enabled = true;
                _lyricsLibraryUserControl.btResetMarkedLyricsDatabase.Enabled = true;
                _lyricsLibraryUserControl.btSave.Enabled = true;
                btClose.Enabled = true;
            }
        }

        private void btStartBatchSearch_Click(object sender, EventArgs e)
        {
            if (bgWorkerSearch.IsBusy)
            {
                Thread.Sleep(2000);
            }

            GetSettingsFromConfigurationXml();

            UpdateLibraryUI = true;

            _mLimit = int.Parse(tbLimit.Text);

            if (_mLimit == 0)
            {
                ThreadFinished = new string[] {"", "", "You must select a number of songs to search", "error"};
                return;
            }
            else if (_mLimit > _mTotalTitles)
            {
                tbLimit.Text = _mTotalTitles.ToString();
                _mLimit = _mTotalTitles;
            }

            logger.Info("New batch search for {0} lyrics started.", _mLimit);

            _stopwatch.StartZero();
            lbTimer.Text = "00:00.00";
            _timer = new Timer();
            _timer.Enabled = true;
            _timer.Interval = 1000;
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();

            _stopCollectingOfTitles = false;
            isSearching(true);
            lbStep1a.Text = "The collecting of songs has started.";
            lbStep2a.Text = "";

            lbStep2a.Visible = false;
            lbStep2.Visible = false;

            // Reset text in textboxes
            lbLastActivity2.Text = "";
            lbMessage.Text = "";
            lbTotalSongs2.Text = _mTotalTitles.ToString();

            _mEventStopThread = new ManualResetEvent(false);

            btStartBatchSearch.Enabled = false;

            _mDisregardKnownLyric = cbDontSearchSongsInLyricDB.Enabled && cbDontSearchSongsInLyricDB.Checked;
            _mMarkSongsWhenNoLyricFound = cbMarkSongsWithNoLyrics.Enabled && cbMarkSongsWithNoLyrics.Checked;
            _mDisregardMarkedLyric = cbDisregardSongsWithNoLyric.Enabled && cbDisregardSongsWithNoLyric.Checked;
            _mDisregardSongWithLyricInTag = cbDontSearchSongsWithLyricInTag.Enabled &&
                                            cbDontSearchSongsWithLyricInTag.Checked;
            _mDisregardVariousArtist = cbDontSearchVariousArtist.Enabled && cbDontSearchVariousArtist.Checked;
            _mSearchOnlyMarkedSongs = cbSearchOnlyForMarkedSongs.Enabled && cbSearchOnlyForMarkedSongs.Checked;

            if (_mSearchOnlyMarkedSongs)
            {
                progressBar.Maximum = MyLyricsUtils.LyricsMarkedDB.Count;
            }
            else
            {
                progressBar.Maximum = _mTotalTitles;
            }

            progressBar.Enabled = true;
            progressBar.Value = 0;

            _mSongsNotKnown = 0;
            _mSongsWithLyric = 0;
            _mSongsWithMark = 0;
            _mSongsToSearch = 0;

            _mLyricsFound = 0;
            _mLyricsNotFound = 0;

            lbSongsWithLyric2.Text = "-";
            lbSongsWithMark2.Text = "-";
            lbLyricsFound2.Text = "-";
            lbLyricsNotFound2.Text = "-";
            lbSongsToSearch2.Text = "-";
            lbDisregardedSongs2.Text = "-";

            var sitesToSearch = new ArrayList();
            foreach (var site in Setup.GetInstance().ActiveSites)
            {
                sitesToSearch.Add(site);
            }

            if (sitesToSearch.Count == 0)
            {
                ThreadFinished = new[] {"", "", "You haven't selected any sites to search", "error"};
                return;
            }

            _sitesToSearchArray = (string[]) sitesToSearch.ToArray(typeof (string));


            _mEventStopThread.Reset();

            bgWorkerSearch.RunWorkerAsync();

            btCancel.Enabled = true;
            btCancel.Focus();
        }


        private void LyricFoundMethod(String lyricStrings, String artist, String track, String site, int row)
        {
            _mArtist = artist;
            _mTrack = track;

            --_mSongsToSearch;
            lbSongsToSearch2.Text = _mSongsToSearch.ToString();

            _mSongsWithLyric += 1;
            _mLyricsFound += 1;

            IncrementLyricsFoundBySite(site);

            lbSongsWithLyric2.Text = _mSongsWithLyric.ToString();
            lbLyricsFound2.Text = _mLyricsFound.ToString();

            var capArtist = LyricUtil.CapatalizeString(_mArtist);
            var capTitle = LyricUtil.CapatalizeString(_mTrack);

            DatabaseUtil.WriteToLyricsDatabase(MyLyricsUtils.LyricsDB, MyLyricsUtils.LyricsMarkedDB, capArtist, capTitle, lyricStrings, site);
            lbMessage.Text = GetLyricsFoundBySite();

            if (!site.Equals("music tag") && _mAutomaticWriteToMusicTag)
            {
                TagReaderUtil.WriteLyrics(capArtist, capTitle, lyricStrings);
            }

            var logText = capArtist + " - " + capTitle + " has a match at " + site;
            lbLastActivity2.Text = logText;

            logger.Info("HIT!: {0}", logText);

            progressBar.PerformStep();
            Update();
        }

        private void IncrementLyricsFoundBySite(string site)
        {
            if (_lyricsFoundBySite.ContainsKey(site))
            {
                _lyricsFoundBySite[site] += 1;

            }
            else
            {
                _lyricsFoundBySite.Add(site, 1);
            }
        }

        private string GetLyricsFoundBySite()
        {
            var output = new StringBuilder();
            output.Append("Lyrics found during this search (by site):").Append(Environment.NewLine);

            // Sort by value
            var sortedDictByValue = (from entry in _lyricsFoundBySite orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
            
            foreach (var siteCount in sortedDictByValue)
            {
                output.AppendFormat("{0}: {1}{2}", siteCount.Key, siteCount.Value, Environment.NewLine);
            }
            output.Append(Environment.NewLine);

            return output.ToString();
        }

        private void LyricNotFoundMethod(String artist, String title, String message, String site, int row)
        {
            _mLyricsNotFound += 1;

            --_mSongsToSearch;
            lbSongsToSearch2.Text = _mSongsToSearch.ToString();

            lbLyricsNotFound2.Text = _mLyricsNotFound.ToString();

            string capArtist = LyricUtil.CapatalizeString(artist);
            string capTitle = LyricUtil.CapatalizeString(title);
            if (_mMarkSongsWhenNoLyricFound &&
                DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, capArtist, capTitle).Equals(
                    DatabaseUtil.LYRIC_NOT_FOUND))
            {
                MyLyricsUtils.LyricsMarkedDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                                                 new LyricsItem(capArtist, capTitle, "", ""));
            }

            _mSongsWithMark += 1;
            lbSongsWithMark2.Text = _mSongsWithMark.ToString();

            string logText = "No match found to " + capArtist + " - " + capTitle;
            lbLastActivity2.Text = logText;
            logger.Info("Miss: {0}", logText);

            progressBar.PerformStep();
            Update();
        }

        // Set initial state of controls.
        // Called from worker thread using delegate and Control.Invoke
        private void ThreadFinishedMethod(String artist, String title, String message, String site)
        {
            if (_timer != null)
            {
                _timer.Enabled = false;
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
                _hour = 0;
                _min = 0;
                _sec = 0;
            }

            if (_lyricsController != null)
            {
                _lyricsController.StopSearches = true;
                DatabaseUtil.SerializeDBs();
                _lyricsLibraryUserControl.updateLyricsTree(false);
            }
            bgWorkerSearch.CancelAsync();
            StopThread();
            progressBar.ResetText();
            progressBar.Enabled = false;

            if (site.Equals("error"))
            {
                lbStep1a.Text = "-";
                lbStep2a.Text = "-";
            }
            else
            {
                lbStep1a.Text = "Completed";
                lbStep2a.Text = "Completed";
            }

            lbMessage.Text += message + Environment.NewLine;

            btStartBatchSearch.Enabled = true;
            btCancel.Enabled = false;
            isSearching(false);

            string logText = string.Format("The search has ended with {0} found and {1} missed.", _mLyricsFound,
                                           _mLyricsNotFound);
            lbLastActivity2.Text = logText;

            logger.Info("{0}", logText);
            logger.Info("Batch search ended.");
        }

        // Called from worker thread using delegate and Control.Invoke
        private void ThreadExceptionMethod(String o)
        {
        }

        private void bgWorkerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 1. Sorting song

            _lyricConfigInfosQueue = new Queue();

            _mMusicDatabase = MusicDatabase.Instance;

            if (_mSearchOnlyMarkedSongs == false)
            {
                //System.IO.Directory.SetCurrentDirectory(@"C:\Program Files\Team MediaPortal\MediaPortal");
                //string test = System.IO.Directory.GetCurrentDirectory();

                _mMusicDatabase.GetAllArtists(ref _artists);

                var canStartSearch = false;

                foreach (var artist in _artists)
                {
                    // If the user has cancelled the search => end this
                    if (_stopCollectingOfTitles)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }

                    // Reached the limit
                    if (canStartSearch)
                    {
                        break;
                    }

                    var currentArtist = (string) artist;
                    _mMusicDatabase.GetSongsByArtist(currentArtist, ref _songs);

                    foreach (var song in _songs)
                    {
                        if (canStartSearch)
                        {
                            break;
                        }

                        /* Don't include song if one of the following is true
                         * 1. The artist is unknown or empty
                         * 2. The title is empty
                         * 3. Various artister should not be considered and the artist is "various artist"
                         * 4. Song with a lyric in the tag should not be considered, but instead include the file to the database right away */

                        MusicTag tag;

                        if (song.Artist.Equals("unknown") || string.IsNullOrEmpty(song.Artist) ||
                            string.IsNullOrEmpty(song.Title)
                            || (_mDisregardVariousArtist && (song.Artist.ToLower().Equals("various artists"))))
                        {
                        }
                        else if ((_mDisregardSongWithLyricInTag && ((tag = TagReader.ReadTag(song.FileName)) != null) &&
                                  tag.Lyrics.Length > 0))
                        {
                            _mSongsWithLyric += 1;

                            var capArtist = LyricUtil.CapatalizeString(tag.Artist);
                            var capTitle = LyricUtil.CapatalizeString(tag.Title);

                            if (
                                DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, capArtist, capTitle).
                                             Equals(DatabaseUtil.LYRIC_NOT_FOUND))
                            {
                                MyLyricsUtils.LyricsDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                                                           new LyricsItem(capArtist, capTitle, tag.Lyrics,
                                                                          "music tag"));
                            }

                            if (
                                DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, capArtist,
                                                                          capTitle)
                                            .Equals(DatabaseUtil.LYRIC_MARKED))
                            {
                                MyLyricsUtils.LyricsMarkedDB.Remove(DatabaseUtil.CorrectKeyFormat(capArtist,
                                                                                                  capTitle));
                            }
                        }
                        else
                        {
                            var status = DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, song.Artist,
                                                                             song.Title);

                            if (!_mDisregardKnownLyric && status.Equals(DatabaseUtil.LYRIC_FOUND)
                                ||
                                (!_mDisregardMarkedLyric &&
                                 ((DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB,
                                                                             song.Artist, song.Title).Equals(
                                                                                 DatabaseUtil.LYRIC_MARKED)) ||
                                  status.Equals(DatabaseUtil.LYRIC_MARKED)))
                                ||
                                (status.Equals(DatabaseUtil.LYRIC_NOT_FOUND) &&
                                 !DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB,
                                                                            song.Artist, song.Title).Equals(
                                                                                DatabaseUtil.LYRIC_MARKED)))
                            {
                                if (++_mSongsNotKnown > _mLimit)
                                {
                                    bgWorkerSearch.ReportProgress(0);
                                    canStartSearch = true;
                                    continue;
                                }

                                var lyricId = new[] {song.Artist, song.Title};
                                _lyricConfigInfosQueue.Enqueue(lyricId);

                                _mSongsToSearch = _lyricConfigInfosQueue.Count;
                            }
                            else if (status.Equals(DatabaseUtil.LYRIC_FOUND))
                            {
                                _mSongsWithLyric += 1;
                            }
                            else //if (status.Equals(MyLyricsUtil.LYRIC_MARKED))
                            {
                                _mSongsWithMark += 1;
                            }
                        }
                        bgWorkerSearch.ReportProgress(-1);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, LyricsItem> kvp in MyLyricsUtils.LyricsMarkedDB)
                {
                    if (++_mSongsNotKnown > _mLimit)
                    {
                        break;
                    }
                    var lyricId = new[] {kvp.Value.Artist, kvp.Value.Title};
                    _lyricConfigInfosQueue.Enqueue(lyricId);
                    _mSongsToSearch = _lyricConfigInfosQueue.Count;

                    bgWorkerSearch.ReportProgress(-1);
                }
            }
            bgWorkerSearch.ReportProgress(0);

            #endregion

            #region 2. Search music tags for lyrics

            // only if user wants to read from music tag and the music tags already aren't disregarded in the search
            if (_mAutomaticReadFromToMusicTag && !_mDisregardSongWithLyricInTag)
            {
                Queue m_SongsToSearchOnline = new Queue();

                foreach (string[] song in _lyricConfigInfosQueue)
                {
                    if (!LyricFoundInMusicTag(song[0], song[1]))
                    {
                        m_SongsToSearchOnline.Enqueue(new string[2] {song[0], song[1]});
                    }

                    if (_stopCollectingOfTitles)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }
                }

                _lyricConfigInfosQueue = m_SongsToSearchOnline;
            }

            #endregion

            #region 3. Searching for lyrics

            // create worker thread instance
            if (_lyricConfigInfosQueue.Count > 0)
            {
                _mFind = SettingManager.GetParamAsString(SettingManager.Find, "");
                _mReplace = SettingManager.GetParamAsString(SettingManager.Replace, "");

                _mEventStopThread = new ManualResetEvent(false);
                _lyricsController = new LyricsController(this, _mEventStopThread, _sitesToSearchArray, false, false, _mFind, _mReplace);

                _lyricsController.NoOfLyricsToSearch = _lyricConfigInfosQueue.Count;
                ThreadStart runLyricController = delegate { _lyricsController.Run(); };
                _mLyricControllerThread = new Thread(runLyricController);
                _mLyricControllerThread.Start();

                _lyricsController.StopSearches = false;


                while (_lyricConfigInfosQueue.Count != 0)
                {
                    // If the user has cancelled the search => end this
                    if (_stopCollectingOfTitles && _lyricsController != null)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }
                    else if (_lyricsController == null)
                        return;

                    if (_lyricsController.NoOfCurrentSearches < NumberOfCurrentSearchesAllowed && _lyricsController.StopSearches == false)
                    {
                        string[] lyricID = (string[]) _lyricConfigInfosQueue.Dequeue();
                        //TODO: if there is a lyric in the music tag of the file, then include this in the db and don't search online

                        string artist = lyricID[0];
                        string title = lyricID[1];

                        logger.Info("New!: Looking for {0} - {1}.", artist, title);

                        _lyricsController.AddNewLyricSearch(artist, title,
                                             MediaPortalUtil.GetStrippedPrefixArtist(artist, _mStrippedPrefixStrings));
                    }

                    Thread.Sleep(100);
                }
            }
            else
            {
                ThreadFinished = new string[] {"", "", "No titles left for online search", ""};
            }

            #endregion

        }

        private void bgWorkerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbSongsWithMark2.Text = _mSongsWithMark.ToString();
            lbSongsWithLyric2.Text = _mSongsWithLyric.ToString();
            //lbDisregardedSongs2.Text = m_DisregardedSongs.ToString();
            lbSongsToSearch2.Text = _mSongsToSearch.ToString();

            if (e.ProgressPercentage == -1)
            {
                progressBar.PerformStep();
            }
            else if (e.ProgressPercentage == 0)
            {
                lbStep1a.Text = "Completed";
                lbStep2a.Text = "The search for lyrics has started.";

                int disregardedSongs = _mTotalTitles - _mSongsWithLyric - _mSongsWithMark - _mSongsToSearch;
                lbDisregardedSongs2.Text = disregardedSongs.ToString();

                if (_mSongsToSearch > 0)
                {
                    lbStep2a.Visible = true;
                    lbStep2.Visible = true;

                    progressBar.Value = 0;
                    progressBar.Maximum = _mSongsToSearch;
                }
                else
                {
                    progressBar.Maximum = progressBar.Maximum - disregardedSongs;
                }

                Update();
            }
        }

        private void bgWorkerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.PerformStep();
        }

        private bool LyricFoundInMusicTag(string artist, string title)
        {
            Song song = new Song();

            _mMusicDatabase.GetSongByMusicTagInfo(artist, string.Empty, title, true, ref song);

            MusicTag tag = TagReader.ReadTag(song.FileName);
            if (tag != null && tag.Lyrics != string.Empty)
            {
                LyricFound = new Object[] {tag.Lyrics, artist, title, "music tag"};
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            _stopCollectingOfTitles = true;

            if (_lyricsController != null)
            {
                _lyricsController.FinishThread(_mArtist, _mTrack, "The search has been stopped by the user.", "none");
                _lyricsController.Dispose();
                _lyricsController = null;
            }
            else
            {
                _mEventStopThread.Set();
                ThreadFinishedMethod(_mArtist, _mTrack, "The search has been stopped by the user.", "none");
            }

            bgWorkerSearch.CancelAsync();
            progressBar.ResetText();
            progressBar.Value = 0;
            _mLyricControllerThread = null;

            Update();
        }

        private void DeserializeBothDB()
        {
            string path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsDBName);

            // Open database to read data from
            var fs = new FileStream(path, FileMode.Open);

            // Create a BinaryFormatter object to perform the deserialization
            BinaryFormatter bf = new BinaryFormatter();

            // Use the BinaryFormatter object to deserialize the database
            MyLyricsUtils.LyricsDB = (LyricsDatabase) bf.Deserialize(fs);
            fs.Close();

            // Deserialize LyricsRemainingDatabase
            path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsMarkedDBName);

            fs = new FileStream(path, FileMode.Open);
            MyLyricsUtils.LyricsMarkedDB = (LyricsDatabase) bf.Deserialize(fs);
            fs.Close();
        }

        private void MyLyricsSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            //LyricDiagnostics.Dispose();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int t = (int) _stopwatch.ElapsedMilliseconds;
            _sec = (t /= 1000);

            int secOverflow = _sec/60;
            _sec -= (secOverflow*60);
            _min = secOverflow;

            int minOverflow = _min/60;
            _min -= (minOverflow*60);
            _hour = minOverflow;

            lbTimer.Text = (_hour < 10 ? "0" + _hour.ToString() : _hour.ToString()) + ":" +
                           (_min < 10 ? "0" + _min.ToString() : _min.ToString()) + "." +
                           (_sec < 10 ? "0" + _sec.ToString() : _sec.ToString());
        }

        private void cbSearchOnlyForMarkedSongs_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSearchOnlyForMarkedSongs.Checked)
            {
                cbDontSearchSongsInLyricDB.Visible = false;
                cbDisregardSongsWithNoLyric.Visible = false;
                cbDontSearchSongsWithLyricInTag.Visible = false;
                cbDontSearchVariousArtist.Visible = false;
                cbMarkSongsWithNoLyrics.Visible = false;
            }
            else
            {
                cbDontSearchSongsInLyricDB.Visible = true;
                cbDisregardSongsWithNoLyric.Visible = true;
                cbDontSearchSongsWithLyricInTag.Visible = true;
                cbDontSearchVariousArtist.Visible = true;
                cbMarkSongsWithNoLyrics.Visible = true;
            }
        }

        public void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                if (UpdateLibraryUI)
                {
                    _lyricsLibraryUserControl.updateLyricsTree(false);
                    UpdateLibraryUI = false;
                }
                _lyricsLibraryUserControl.treeView.Select();
            }
            else if (tabControl.SelectedIndex == 1)
            {
                _musicDatabaseBrowseUserControl.RefreshSongsListView();
                _musicDatabaseBrowseUserControl.RefreshArtistStats();
                _musicDatabaseBrowseUserControl.ClearSelectedSongs();
            }
            else
            {
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            var lyricsSites = new List<string>();
            switch (trackBar.Value)
            {
                case 0:
                    lyricsSites = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Fast);
                    break;
                case 1:
                    lyricsSites = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Medium);
                    break;
                case 2:
                    lyricsSites = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Slow);
                    break;
                case 3:
                    lyricsSites = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.VerySlow);
                    break;
            }
            
            // Update sitesList & Setup
            for (var index = 0; index < sitesList.Items.Count; index++)
            {
                sitesList.SetItemChecked(index, lyricsSites.Contains(sitesList.Items[index].ToString()));
            }
            Setup.GetInstance().UpdateActiveSitesInSetup(sitesList);

            if (sender != null)
            {
                WriteMediaPortalXml(null, null);
            }
        }

        private void rdTrackBar_CheckedChanged(object sender, EventArgs e)
        {
            if (rdLyricsMode.Checked)
            {
                trackBar.Enabled = true;
                sitesList.Enabled = false;

                trackBar_Scroll(null, null);

                cbMusicTagAlwaysCheck.Checked = true;
                cbMusicTagWrite.Checked = true;

                cbDontSearchSongsInLyricDB.Checked = true;
                cbDontSearchSongsWithLyricInTag.Checked = true;
            }
            else if (rbUserSelectMode.Checked)
            {
                trackBar.Enabled = false;
                sitesList.Enabled = true;
            }
            else if (rbLrcMode.Checked)
            {
                trackBar.Enabled = false;

                var lyricsSitesLrc = LyricsSiteFactory.LrcLyricsSiteNames();
                for (var index = 0; index < sitesList.Items.Count; index++)
                {
                    sitesList.SetItemChecked(index, lyricsSitesLrc.Contains(sitesList.Items[index].ToString()));
                }
                sitesList.Enabled = false;
                Setup.GetInstance().ActiveSites = lyricsSitesLrc;

                cbMusicTagAlwaysCheck.Checked = false;
                cbMusicTagWrite.Checked = true;

                cbDontSearchSongsInLyricDB.Checked = false;
                cbDontSearchSongsWithLyricInTag.Checked = false;
            }
        }

        private void WriteMediaPortalXml(object sender, EventArgs e)
        {
            string sitesMode;
            if (rdLyricsMode.Checked)
            {
                sitesMode = rdLyricsMode.Tag as string;
            }
            else if (rbUserSelectMode.Checked)
            {
                sitesMode = rbUserSelectMode.Tag as string;
            }
            else
            {
                sitesMode = rbLrcMode.Tag as string;
            }

            SettingManager.SetParam(SettingManager.PluginsName, tbPluginName.Text);
            SettingManager.SetParam(SettingManager.SitesMode, sitesMode);
            SettingManager.SetParam(SettingManager.DefaultSitesModeValue, trackBar.Value.ToString());
            SettingManager.SetParam(SettingManager.Limit, tbLimit.Text);

            Setup.GetInstance().ActiveSites.Clear();
            foreach (var site in sitesList.Items)
            {
                var active = sitesList.CheckedItems.Contains(site);
                if (active)
                {
                    Setup.GetInstance().ActiveSites.Add(site as string);
                }
                SettingManager.SetParamAsBool(SettingManager.SitePrefix + (string)site, active);
            }

            SettingManager.SetParamAsBool(SettingManager.UseAutoscroll, cbUseAutoScrollAsDefault.Checked);
            SettingManager.SetParamAsBool(SettingManager.UploadLrcToLrcFinder, cbUploadLrcAutomatically.Checked);
            SettingManager.SetParamAsBool(SettingManager.AlwaysAskUploadLrcToLrcFinder, cbAlwaysAskForUploadToLrcFinder.Checked);
            SettingManager.SetParam(SettingManager.LrcTaggingName, tbLrcTaggingName.Text);
            SettingManager.SetParamAsBool(SettingManager.AutomaticFetch, cbAutoFetch.Checked);
            SettingManager.SetParamAsBool(SettingManager.AutomaticUpdateWhenFirstFound, cbAutomaticUpdate.Checked);
            SettingManager.SetParamAsBool(SettingManager.MoveLyricFromMarkedDatabase, cbMoveSongFrom.Checked);
            SettingManager.SetParamAsBool(SettingManager.AutomaticWriteToMusicTag, cbMusicTagWrite.Checked);
            SettingManager.SetParamAsBool(SettingManager.AutomaticReadFromMusicTag, cbMusicTagAlwaysCheck.Checked);
            SettingManager.SetParam(SettingManager.LrcTaggingOffset, tbLrcTaggingOffset.Text);


            var text = comboBoxLanguages.SelectedItem as string;
            if (!string.IsNullOrEmpty(text))
            {
                SettingManager.SetParam(SettingManager.TranslationLanguage, text);
            }

            _mAutomaticWriteToMusicTag = cbMusicTagWrite.Checked;
            _mAutomaticReadFromToMusicTag = cbMusicTagAlwaysCheck.Checked;

            try
            {
                var find = new StringBuilder();
                var replace = new StringBuilder();

                dbGridView.EndEdit();

                foreach (DataGridViewRow row in dbGridView.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                    {
                        find.Append(row.Cells[0].Value.ToString().Replace(",", "") + ",");
                        replace.Append(row.Cells[1].Value.ToString().Replace(",", "") + ",");
                    }
                }

                SettingManager.SetParam(SettingManager.Find, find.ToString());
                SettingManager.SetParam(SettingManager.Replace, replace.ToString());

                _mFind = find.ToString();
                _mReplace = replace.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region ILyricForm properties

        public Object[] UpdateString
        {
            set { }
        }

        public Object[] UpdateStatus
        {
            set { }
        }

        public Object[] LyricFound
        {
            set { Invoke(MDelegateLyricFound, value); }
        }

        public Object[] LyricNotFound
        {
            set { Invoke(MDelegateLyricNotFound, value); }
        }

        public Object[] ThreadFinished
        {
            set { Invoke(MDelegateThreadFinished, value); }
        }

        public string ThreadException
        {
            set
            {
                try
                {
                    Invoke(MDelegateThreadException, value);
                }
                catch
                {
                }
                ;
            }
        }

        #endregion
    }
}