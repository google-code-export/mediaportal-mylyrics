using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Google.API.Translate;
using LyricsEngine;
using LyricsEngine.LyricsSites;
using MediaPortal.Configuration;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MyLyrics.XmlSettings;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace MyLyrics
{
    public partial class MyLyricsSetup : Form, ILyricForm, IDisposable
    {
        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Delegates

        public delegate void DelegateLyricFound(String s, String artist, String track, String site, int row);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site, int row);

        public delegate void DelegateStatusUpdate(
            Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(String s);

        public delegate void DelegateThreadFinished(String artist, String title, String message, String site);

        #endregion

        public static bool UpdateLibraryUI = false;
        private ArrayList artists = new ArrayList();
        private int hour = 0;

        private Information informationUC;
        private int lastShownLyricsTitles = 0;
        private int lastShownMarkedLyricsTitles = 0;
        private string latestArtistBeforeCrash = null;
        private string latestTitleBeforeCrash = null;
        private LyricsController lc = null;

        private Queue lyricConfigInfosQueue;
        private LyricsLibrary lyricsLibraryUC;
        private string m_artist = "";
        private bool m_automaticReadFromToMusicTag = true;
        private bool m_automaticWriteToMusicTag = true;

        public DelegateLyricFound m_DelegateLyricFound;

        public DelegateLyricNotFound m_DelegateLyricNotFound;
        public DelegateStatusUpdate m_DelegateStatusUpdate;
        public DelegateStringUpdate m_DelegateStringUpdate;

        public DelegateThreadException m_DelegateThreadException;
        public DelegateThreadFinished m_DelegateThreadFinished;

        // Track info
        private int m_DisregardedSongs = 0;

        // Search criteries
        private bool m_DisregardKnownLyric = true;
        private bool m_DisregardMarkedLyric = true;
        private bool m_DisregardSongWithLyricInTag = true;
        private bool m_DisregardVariousArtist = true;

        private ManualResetEvent m_EventStopThread;
        private string m_find = string.Empty;
        public Guid m_guid;
        private string m_guidString = string.Empty;

        private int m_Limit = 100;
        private Thread m_LyricControllerThread;
        private int m_LyricsFound = 0;
        private int m_LyricsNotFound = 0;
        private string m_LyricText = "";
        private bool m_MarkSongsWhenNoLyricFound = true;
        private int m_noOfArtistsToSearch = 0;
        private int m_NoOfCurrentSearchesAllowed = 6;
        private int m_noOfMessages = 0;

        // Collections and arrays
        private string m_replace = string.Empty;
        private bool m_SearchOnlyMarkedSongs = false;
        private int m_SongsNotKnown = 0;
        private int m_SongsToSearch = 0;
        private int m_SongsWithLyric = 0;
        private int m_SongsWithMark = 0;
        private string m_StatusText = "";
        private string[] m_strippedPrefixStrings;
        private int m_TotalTitles = 0;
        private string m_track = "";
        private MusicDatabase mDB = null;

        // Timer variables
        private int min = 0;
        private MusicDatabaseBrowse musicDatabaseBrowseUC;
        private int sec = 0;
        private string[] sitesToSearchArray = null;
        private List<Song> songs = new List<Song>();

        private bool stopCollectingOfTitles = false;
        private StopWatch stopwatch = new StopWatch();
        private Timer timer;


        public MyLyricsSetup()
        {
            #region Initialize GUI and class

            InitializeComponent();
            lyricsLibraryUC = new LyricsLibrary(this);
            musicDatabaseBrowseUC = new MusicDatabaseBrowse(this);
            informationUC = new Information(this);

            tabPageLyricsDatabase.Controls.Add(lyricsLibraryUC);
            tabPageMusicDatabaseBrowse.Controls.Add(musicDatabaseBrowseUC);
            tabPageAbout.Controls.Add(informationUC);

            // initialize delegates
            m_DelegateLyricFound = LyricFoundMethod;
            m_DelegateLyricNotFound = LyricNotFoundMethod;
            m_DelegateThreadFinished = ThreadFinishedMethod;
            m_DelegateThreadException = ThreadExceptionMethod;

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

            m_TotalTitles = mDB.GetTotalSongs();


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

            lyricsLibraryUC.updateLyricsTree(false);
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

                    Setup.ActiveSites.Clear();
                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        var active = SettingManager.GetParamAsBool(SettingManager.SitePrefix + (sitesList.Items[index]), true);
                        sitesList.SetItemChecked(index, active);
                        if (active)
                        {
                            Setup.ActiveSites.Add((string) (sitesList.Items[index]));
                        }
                    }
                }
                else
                {
                    rbLrcMode.Checked = true;
                }

                rdTrackBar_CheckedChanged(null, null);

                tbLimit.Text = SettingManager.GetParamAsString(SettingManager.Limit, m_TotalTitles.ToString());
                tbPluginName.Text = SettingManager.GetParamAsString(SettingManager.PluginsName, SettingManager.DefaultPluginName);

                cbAutoFetch.Checked = SettingManager.GetParamAsBool(SettingManager.AutomaticFetch, false);
                cbAutomaticUpdate.Checked = SettingManager.GetParamAsBool(SettingManager.AutomaticUpdateWhenFirstFound, false);
                cbMoveSongFrom.Checked = SettingManager.GetParamAsBool(SettingManager.MoveLyricFromMarkedDatabase, false);

                m_automaticWriteToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, false);
                cbMusicTagWrite.Checked = m_automaticWriteToMusicTag;

                m_automaticReadFromToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticReadFromMusicTag, false);
                cbMusicTagAlwaysCheck.Checked = m_automaticReadFromToMusicTag;

                cbUseAutoScrollAsDefault.Checked = SettingManager.GetParamAsBool(SettingManager.UseAutoscroll, false);
                tbLrcTaggingOffset.Text = SettingManager.GetParamAsString(SettingManager.LrcTaggingOffset, SettingManager.DefaultLrcTaggingOffset);
                tbLrcTaggingName.Text = SettingManager.GetParamAsString(SettingManager.LrcTaggingName, "");

                m_guidString = SettingManager.GetParamAsString(SettingManager.Guid, "");

                cbUploadLrcAutomatically.Checked = SettingManager.GetParamAsBool(SettingManager.UploadLrcToLrcFinder, false);

                cbAlwaysAskForUploadToLrcFinder.Checked = SettingManager.GetParamAsBool(SettingManager.AlwaysAskUploadLrcToLrcFinder, false);

                lbSongsLimitNote.Text = ("(You have currently " + m_TotalTitles.ToString() + " titles in your music database)");

                trackBar.Value = SettingManager.GetParamAsInt(SettingManager.DefaultSitesModeValue, 2);

                // Update the search sites according to trackbar
                if (rdLyricsMode.Checked)
                {
                    trackBar_Scroll(null, null);
                }

                comboBoxLanguages.SelectedItem = SettingManager.GetParamAsString(SettingManager.TranslationLanguage, SettingManager.DefaultTranslationLanguage);

                m_find = SettingManager.GetParamAsString(SettingManager.Find, "");
                m_replace = SettingManager.GetParamAsString(SettingManager.Replace, "");


                if (m_find != "")
                {
                    string[] findArray = m_find.Split(',');
                    string[] replaceArray = m_replace.Split(',');
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

                if (string.IsNullOrEmpty(m_guidString))
                {
                    m_guid = Guid.NewGuid();
                    m_guidString = m_guid.ToString("P");
                    
                    SettingManager.SetParam(SettingManager.Guid, m_guidString);
                }
                else
                {
                    m_guid = new Guid(m_guidString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something has gone wrong when reading Mediaportal.xml (" + ex.Message + ")");
            }

            m_strippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            #endregion
        }


        // Stop worker thread if it is running.
        // Called when user presses Stop button of form is closed.
        private void StopThread()
        {
            if (m_LyricControllerThread != null && m_LyricControllerThread.IsAlive) // thread is active
            {
                //GUIControl.SetControlLabel(GetID, (int)Controls.CONTROL_LBStatus, m_StatusText);
                // set event "Stop"
                m_EventStopThread.Set();
                //m_LyricControllerThread = null;

                // wait when thread  will stop or finish
                while (m_LyricControllerThread.IsAlive)
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
                lyricsLibraryUC.btAdd.Enabled = false;
                lyricsLibraryUC.btDelete.Enabled = false;
                lyricsLibraryUC.btImportFiles.Enabled = false;
                lyricsLibraryUC.btImportDirs.Enabled = false;
                lyricsLibraryUC.btResetLyricsDatabase.Enabled = false;
                lyricsLibraryUC.btResetMarkedLyricsDatabase.Enabled = false;
                lyricsLibraryUC.btSave.Enabled = false;
                btClose.Enabled = false;
            }
            else
            {
                lyricsLibraryUC.btAdd.Enabled = true;
                lyricsLibraryUC.btDelete.Enabled = true;
                lyricsLibraryUC.btImportFiles.Enabled = true;
                lyricsLibraryUC.btImportDirs.Enabled = true;
                lyricsLibraryUC.btResetLyricsDatabase.Enabled = true;
                lyricsLibraryUC.btResetMarkedLyricsDatabase.Enabled = true;
                lyricsLibraryUC.btSave.Enabled = true;
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

            m_Limit = int.Parse(tbLimit.Text);

            if (m_Limit == 0)
            {
                ThreadFinished = new string[] {"", "", "You must select a number of songs to search", "error"};
                return;
            }
            else if (m_Limit > m_TotalTitles)
            {
                tbLimit.Text = m_TotalTitles.ToString();
                m_Limit = m_TotalTitles;
            }

            logger.Info("New batch search for {0} lyrics started.", m_Limit);

            stopwatch.StartZero();
            lbTimer.Text = "00:00.00";
            timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            stopCollectingOfTitles = false;
            isSearching(true);
            lbStep1a.Text = "The collecting of songs has started.";
            lbStep2a.Text = "";

            lbStep2a.Visible = false;
            lbStep2.Visible = false;

            // Reset text in textboxes
            lbLastActivity2.Text = "";
            lbMessage.Text = "";
            lbTotalSongs2.Text = m_TotalTitles.ToString();

            m_EventStopThread = new ManualResetEvent(false);

            m_noOfMessages = 0;

            btStartBatchSearch.Enabled = false;

            m_DisregardKnownLyric = cbDontSearchSongsInLyricDB.Enabled && cbDontSearchSongsInLyricDB.Checked;
            m_MarkSongsWhenNoLyricFound = cbMarkSongsWithNoLyrics.Enabled && cbMarkSongsWithNoLyrics.Checked;
            m_DisregardMarkedLyric = cbDisregardSongsWithNoLyric.Enabled && cbDisregardSongsWithNoLyric.Checked;
            m_DisregardSongWithLyricInTag = cbDontSearchSongsWithLyricInTag.Enabled &&
                                            cbDontSearchSongsWithLyricInTag.Checked;
            m_DisregardVariousArtist = cbDontSearchVariousArtist.Enabled && cbDontSearchVariousArtist.Checked;
            m_SearchOnlyMarkedSongs = cbSearchOnlyForMarkedSongs.Enabled && cbSearchOnlyForMarkedSongs.Checked;

            if (m_SearchOnlyMarkedSongs)
            {
                progressBar.Maximum = MyLyricsUtils.LyricsMarkedDB.Count;
            }
            else
            {
                progressBar.Maximum = m_TotalTitles;
            }

            progressBar.Enabled = true;
            progressBar.Value = 0;

            m_SongsNotKnown = 0;
            m_SongsWithLyric = 0;
            m_SongsWithMark = 0;
            m_DisregardedSongs = 0;
            m_SongsToSearch = 0;

            m_LyricsFound = 0;
            m_LyricsNotFound = 0;

            lbSongsWithLyric2.Text = "-";
            lbSongsWithMark2.Text = "-";
            lbLyricsFound2.Text = "-";
            lbLyricsNotFound2.Text = "-";
            lbSongsToSearch2.Text = "-";
            lbDisregardedSongs2.Text = "-";

            var sitesToSearch = new ArrayList();
            foreach (var site in Setup.ActiveSites)
            {
                sitesToSearch.Add(site);
            }

            if (sitesToSearch.Count == 0)
            {
                ThreadFinished = new[] {"", "", "You haven't selected any sites to search", "error"};
                return;
            }

            sitesToSearchArray = (string[]) sitesToSearch.ToArray(typeof (string));


            m_EventStopThread.Reset();

            bgWorkerSearch.RunWorkerAsync();

            btCancel.Enabled = true;
            btCancel.Focus();
        }


        private void LyricFoundMethod(String lyricStrings, String artist, String track, String site, int row)
        {
            m_LyricText = lyricStrings;
            m_artist = artist;
            m_track = track;

            --m_SongsToSearch;
            lbSongsToSearch2.Text = m_SongsToSearch.ToString();

            m_SongsWithLyric += 1;
            m_LyricsFound += 1;

            lbSongsWithLyric2.Text = m_SongsWithLyric.ToString();
            lbLyricsFound2.Text = m_LyricsFound.ToString();

            var capArtist = LyricUtil.CapatalizeString(m_artist);
            var capTitle = LyricUtil.CapatalizeString(m_track);

            DatabaseUtil.WriteToLyricsDatabase(MyLyricsUtils.LyricsDB, MyLyricsUtils.LyricsMarkedDB, capArtist,
                                               capTitle, lyricStrings, site);

            if (!site.Equals("music tag") && m_automaticWriteToMusicTag)
            {
                TagReaderUtil.WriteLyrics(capArtist, capTitle, lyricStrings);
            }

            var logText = capArtist + " - " + capTitle + " has a match at " + site;
            lbLastActivity2.Text = logText;

            logger.Info("HIT!: {0}", logText);

            progressBar.PerformStep();
            Update();
        }

        private void LyricNotFoundMethod(String artist, String title, String message, String site, int row)
        {
            m_LyricsNotFound += 1;

            --m_SongsToSearch;
            lbSongsToSearch2.Text = m_SongsToSearch.ToString();

            lbLyricsNotFound2.Text = m_LyricsNotFound.ToString();

            string capArtist = LyricUtil.CapatalizeString(artist);
            string capTitle = LyricUtil.CapatalizeString(title);
            if (m_MarkSongsWhenNoLyricFound &&
                DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, capArtist, capTitle).Equals(
                    DatabaseUtil.LYRIC_NOT_FOUND))
            {
                MyLyricsUtils.LyricsMarkedDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                                                    new LyricsItem(capArtist, capTitle, "", ""));
            }

            m_SongsWithMark += 1;
            lbSongsWithMark2.Text = m_SongsWithMark.ToString();
            
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
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Dispose();
                timer = null;
                hour = 0;
                min = 0;
                sec = 0;
            }

            if (lc != null)
            {
                lc.StopSearches = true;
                DatabaseUtil.SerializeDBs();
                lyricsLibraryUC.updateLyricsTree(false);
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

            lbMessage.Text = "#" + ((int) (++m_noOfMessages)).ToString() + " - " + message + "\r\n" + lbMessage.Text + "\r\n";
            btStartBatchSearch.Enabled = true;
            btCancel.Enabled = false;
            isSearching(false);
            
            string logText = string.Format("The search has ended with {0} found and {1} missed.", m_LyricsFound, m_LyricsNotFound);
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
            //            Thread.CurrentThread.Name = "bgWorker - Search";

            //try
            //{

            #region 1. Sorting song

            lyricConfigInfosQueue = new Queue();

            mDB = MusicDatabase.Instance;

            if (m_SearchOnlyMarkedSongs == false)
            {
                //System.IO.Directory.SetCurrentDirectory(@"C:\Program Files\Team MediaPortal\MediaPortal");
                //string test = System.IO.Directory.GetCurrentDirectory();

                mDB.GetAllArtists(ref artists);

                m_noOfArtistsToSearch = artists.Count;

                var canStartSearch = false;

                foreach (var artist in artists)
                {
                    // If the user has cancelled the search => end this
                    if (stopCollectingOfTitles)
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
                    mDB.GetSongsByArtist(currentArtist, ref songs);

                    foreach (var song in songs)
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
                            || (m_DisregardVariousArtist && (song.Artist.ToLower().Equals("various artists"))))
                        {
                            m_DisregardedSongs += 1;
                        }
                        else if ((m_DisregardSongWithLyricInTag && ((tag = TagReader.ReadTag(song.FileName)) != null) && tag.Lyrics.Length > 0))
                        {
                            m_SongsWithLyric += 1;

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
                                                                          capTitle).Equals(DatabaseUtil.LYRIC_MARKED))
                            {
                                MyLyricsUtils.LyricsMarkedDB.Remove(DatabaseUtil.CorrectKeyFormat(capArtist,
                                                                                                     capTitle));
                            }
                        }
                        else
                        {
                            var status = DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, song.Artist, song.Title);

                            if (!m_DisregardKnownLyric && status.Equals(DatabaseUtil.LYRIC_FOUND)
                                ||
                                (!m_DisregardMarkedLyric &&
                                 ((DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB,
                                                                             song.Artist, song.Title).Equals(
                                                                                 DatabaseUtil.LYRIC_MARKED)) || status.Equals(DatabaseUtil.LYRIC_MARKED)))
                                ||
                                (status.Equals(DatabaseUtil.LYRIC_NOT_FOUND) &&
                                 !DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB,
                                                                            song.Artist, song.Title).Equals(
                                                                                DatabaseUtil.LYRIC_MARKED)))
                            {
                                if (++m_SongsNotKnown > m_Limit)
                                {
                                    bgWorkerSearch.ReportProgress(0);
                                    canStartSearch = true;
                                    continue;
                                }

                                var lyricId = new[] {song.Artist, song.Title};
                                lyricConfigInfosQueue.Enqueue(lyricId);

                                m_SongsToSearch = lyricConfigInfosQueue.Count;
                            }
                            else if (status.Equals(DatabaseUtil.LYRIC_FOUND))
                            {
                                m_SongsWithLyric += 1;
                            }
                            else //if (status.Equals(MyLyricsUtil.LYRIC_MARKED))
                            {
                                m_SongsWithMark += 1;
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
                    if (++m_SongsNotKnown > m_Limit)
                    {
                        break;
                    }
                    var lyricId = new[] {kvp.Value.Artist, kvp.Value.Title};
                    lyricConfigInfosQueue.Enqueue(lyricId);
                    m_SongsToSearch = lyricConfigInfosQueue.Count;

                    bgWorkerSearch.ReportProgress(-1);
                }
            }
            bgWorkerSearch.ReportProgress(0);

            #endregion

            #region 2. Search music tags for lyrics

            // only if user wants to read from music tag and the music tags already aren't disregarded in the search
            if (m_automaticReadFromToMusicTag && !m_DisregardSongWithLyricInTag)
            {
                Queue m_SongsToSearchOnline = new Queue();

                foreach (string[] song in lyricConfigInfosQueue)
                {
                    if (!LyricFoundInMusicTag(song[0], song[1]))
                    {
                        m_SongsToSearchOnline.Enqueue(new string[2] {song[0], song[1]});
                    }

                    if (stopCollectingOfTitles)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }
                }

                lyricConfigInfosQueue = m_SongsToSearchOnline;
            }

            #endregion

            #region 3. Searching for lyrics

            // create worker thread instance
            if (lyricConfigInfosQueue.Count > 0)
            {
                m_find = SettingManager.GetParamAsString(SettingManager.Find, "");
                m_replace = SettingManager.GetParamAsString(SettingManager.Replace, "");

                m_EventStopThread = new ManualResetEvent(false);
                lc = new LyricsController(this, m_EventStopThread, sitesToSearchArray, false, false, m_find, m_replace);

                lc.NoOfLyricsToSearch = lyricConfigInfosQueue.Count;
                ThreadStart runLyricController = delegate { lc.Run(); };
                m_LyricControllerThread = new Thread(runLyricController);
                m_LyricControllerThread.Start();

                lc.StopSearches = false;


                while (lyricConfigInfosQueue.Count != 0)
                {
                    // If the user has cancelled the search => end this
                    if (stopCollectingOfTitles && lc != null)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }
                    else if (lc == null)
                        return;

                    if (lc.NoOfCurrentSearches < m_NoOfCurrentSearchesAllowed && lc.StopSearches == false)
                    {
                        string[] lyricID = (string[]) lyricConfigInfosQueue.Dequeue();
                        //TODO: if there is a lyric in the music tag of the file, then include this in the db and don't search online

                        string artist = lyricID[0];
                        string title = lyricID[1];

                        latestArtistBeforeCrash = artist;
                        latestTitleBeforeCrash = title;

                        logger.Info("New!: Looking for {0} - {1}.", artist, title);

                        lc.AddNewLyricSearch(artist, title,
                                             MediaPortalUtil.GetStrippedPrefixArtist(artist, m_strippedPrefixStrings));
                    }

                    Thread.Sleep(100);
                }
            }
            else
            {
                ThreadFinished = new string[] {"", "", "No titles left for online search", ""};
            }

            #endregion

            //}
            //catch (Exception ex)
            //{
            //    StreamReader sr = File.OpenText(logFullFileName);
            //    log = sr.ReadToEnd();
            //    sr.Close();
            //    string logText = "Message:" + ex.Message + "\r\n";
            //    logText += "StackTrace:" + ex.StackTrace + "\r\n";
            //    logText += "Source:" + ex.Source + "\r\n";
            //    logText += "latestArtistBeforeCrash:" + latestArtistBeforeCrash + "\r\n";
            //    logText += "latestTitleBeforeCrash:" + latestTitleBeforeCrash + "\r\n";

            //    if (ex.InnerException != null)
            //    {
            //        logText = "InnerException.Message:" + ex.InnerException.Message + "\r\n";
            //        logText += "InnerException.StackTrace:" + ex.InnerException.StackTrace + "\r\n";
            //        logText += "InnerException.Source:" + ex.InnerException.Source + "\r\n";
            //    }

            //    log += DateTime.Now.ToString() + " " + logText;
            //    lbLastActivity2.Text = logText;
            //    System.IO.StreamWriter writerLog = new System.IO.StreamWriter(logFullFileName);
            //    writerLog.Write(log);
            //    writerLog.Close();
            //}
        }

        private void bgWorkerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbSongsWithMark2.Text = m_SongsWithMark.ToString();
            lbSongsWithLyric2.Text = m_SongsWithLyric.ToString();
            //lbDisregardedSongs2.Text = m_DisregardedSongs.ToString();
            lbSongsToSearch2.Text = m_SongsToSearch.ToString();

            if (e.ProgressPercentage == -1)
            {
                progressBar.PerformStep();
            }
            else if (e.ProgressPercentage == 0)
            {
                lbStep1a.Text = "Completed";
                lbStep2a.Text = "The search for lyrics has started.";

                int disregardedSongs = m_TotalTitles - m_SongsWithLyric - m_SongsWithMark - m_SongsToSearch;
                lbDisregardedSongs2.Text = disregardedSongs.ToString();

                if (m_SongsToSearch > 0)
                {
                    lbStep2a.Visible = true;
                    lbStep2.Visible = true;

                    progressBar.Value = 0;
                    progressBar.Maximum = m_SongsToSearch;
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

            mDB.GetSongByMusicTagInfo(artist, string.Empty, title, true, ref song);

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
            stopCollectingOfTitles = true;

            if (lc != null)
            {
                lc.FinishThread(m_artist, m_track, "The search has been stopped by the user.", "none");
                lc.Dispose();
                lc = null;
            }
            else
            {
                m_EventStopThread.Set();
                ThreadFinishedMethod(m_artist, m_track, "The search has been stopped by the user.", "none");
            }

            bgWorkerSearch.CancelAsync();
            progressBar.ResetText();
            progressBar.Value = 0;
            m_LyricControllerThread = null;

            Update();
        }

        private void DeserializeBothDB()
        {
            string path = Config.GetFile(Config.Dir.Database, MyLyricsUtils.LyricsDBName);

            // Open database to read data from
            FileStream fs = new FileStream(path, FileMode.Open);

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
            int t = (int) stopwatch.ElapsedMilliseconds;
            sec = (t /= 1000);

            int secOverflow = sec/60;
            sec -= (secOverflow*60);
            min = secOverflow;

            int minOverflow = min/60;
            min -= (minOverflow*60);
            hour = minOverflow;

            lbTimer.Text = (hour < 10 ? "0" + hour.ToString() : hour.ToString()) + ":" +
                           (min < 10 ? "0" + min.ToString() : min.ToString()) + "." +
                           (sec < 10 ? "0" + sec.ToString() : sec.ToString());
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
                    lyricsLibraryUC.updateLyricsTree(false);
                    UpdateLibraryUI = false;
                }
                lyricsLibraryUC.treeView.Select();
            }
            else if (tabControl.SelectedIndex == 1)
            {
                musicDatabaseBrowseUC.RefreshSongsListView();
                musicDatabaseBrowseUC.RefreshArtistStats();
                musicDatabaseBrowseUC.ClearSelectedSongs();
            }
            else
            {
                lastShownLyricsTitles = MyLyricsUtils.LyricsDB.Count;
                lastShownMarkedLyricsTitles = MyLyricsUtils.LyricsMarkedDB.Count;
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            switch (trackBar.Value)
            {
                case 0:
                    var lyricsSitesFast = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Fast);
                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        sitesList.SetItemChecked(index, lyricsSitesFast.Contains(sitesList.Items[index].ToString()));
                    }
                    Setup.ActiveSites = lyricsSitesFast;
                    break;
                case 1:
                    var lyricsSitesMedium = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Medium);
                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        sitesList.SetItemChecked(index, lyricsSitesMedium.Contains(sitesList.Items[index].ToString()));
                    }
                    Setup.ActiveSites = lyricsSitesMedium;
                    break;
                case 2:
                    var lyricsSitesSlow = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.Slow);
                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        sitesList.SetItemChecked(index, lyricsSitesSlow.Contains(sitesList.Items[index].ToString()));
                    }
                    Setup.ActiveSites = lyricsSitesSlow;
                    break;
                case 3:
                    var lyricsSitesVerySlow = LyricsSiteFactory.LyricsSitesBySpeed(SiteSpeed.VerySlow);
                    for (var index = 0; index < sitesList.Items.Count; index++)
                    {
                        sitesList.SetItemChecked(index, lyricsSitesVerySlow.Contains(sitesList.Items[index].ToString()));
                    }
                    Setup.ActiveSites = lyricsSitesVerySlow;
                    break;
            }

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
                Setup.ActiveSites = lyricsSitesLrc;
                
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

            foreach (var site in sitesList.Items)
            {
                SettingManager.SetParamAsBool(SettingManager.SitePrefix + (string) site, sitesList.CheckedItems.Contains(site));
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

            m_automaticWriteToMusicTag = cbMusicTagWrite.Checked;
            m_automaticReadFromToMusicTag = cbMusicTagAlwaysCheck.Checked;

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

                m_find = find.ToString();
                m_replace = replace.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region ILyricForm properties

        public Object[] UpdateString
        {
            set { m_StatusText = ((String) value[0]).ToString() + " - " + ((String) value[1]).ToString(); }
        }

        public Object[] UpdateStatus
        {
            set { }
        }

        public Object[] LyricFound
        {
            set { Invoke(m_DelegateLyricFound, value); }
        }

        public Object[] LyricNotFound
        {
            set { Invoke(m_DelegateLyricNotFound, value); }
        }

        public Object[] ThreadFinished
        {
            set { Invoke(m_DelegateThreadFinished, value); }
        }

        public string ThreadException
        {
            set
            {
                try
                {
                    Invoke(m_DelegateThreadException, value);
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