using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading;

using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using MediaPortal.Music.Database;
using MediaPortal.Playlists;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.Configuration;
using MediaPortal.Profile;

using LyricsEngine;

namespace MyLyrics
{
    public partial class MyLyricsSetup : Form, ILyricForm, IDisposable
    {

        Thread m_LyricControllerThread;
        LyricsController lc = null;

        MyLyricsSetup_LyricsLibrary lyricsLibraryUC;

        // events used to stop worker thread
        ManualResetEvent m_EventStopThread;

        // Delegates
        public delegate void DelegateStringUpdate(String message, String site);
        public DelegateStringUpdate m_DelegateStringUpdate;
        public delegate void DelegateStatusUpdate(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);
        public DelegateStatusUpdate m_DelegateStatusUpdate;
        public delegate void DelegateLyricFound(String s, String artist, String track, String site);
        public DelegateLyricFound m_DelegateLyricFound;
        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site);
        public DelegateLyricNotFound m_DelegateLyricNotFound;
        public delegate void DelegateThreadFinished(String artist, String title, String message, String site);
        public DelegateThreadFinished m_DelegateThreadFinished;
        public delegate void DelegateThreadException(String s);
        public DelegateThreadException m_DelegateThreadException;

        // Track info
        string m_artist = "";
        string m_track = "";
        string m_StatusText = "";
        string m_LyricText = "";

        // Search stats
        int m_noOfMessages = 0;
        int m_noOfArtistsToSearch = 0;
        int m_TotalTitles = 0;
        int m_SongsNotKnown = 0;
        int m_SongsWithLyric = 0;
        int m_SongsWithMark = 0;
        int m_DisregardedSongs = 0;
        int m_SongsToSearch = 0;
        int m_LyricsFound = 0;
        int m_LyricsNotFound = 0;
        int m_noOfCurrentlySearches = 0;

        // Search criteries
        bool m_DisregardKnownLyric = true;
        bool m_MarkSongsWhenNoLyricFound = true;
        bool m_DisregardMarkedLyric = true;
        bool m_DisregardSongWithLyricInTag = true;
        bool m_DisregardVariousArtist = true;
        bool m_SearchOnlyMarkedSongs = false;
        const int m_NoOfCurrentSearchesAllowed = 6;
        int m_Limit = 100;

        // Collections and arrays
        ArrayList songs = new ArrayList();
        ArrayList artists = new ArrayList();
        private Queue lyricConfigInfosQueue;
        string[] sitesToSearchArray = null;

        // Timer variables
        int hour = 0;
        int min = 0;
        int sec = 0;
        System.Windows.Forms.Timer timer;
        StopWatch stopwatch = new StopWatch();

        // log information
        private string log;
        private string logFullFileName = "";

        private int lastShownLyricsTitles = 0;
        private int lastShownMarkedLyricsTitles = 0;

        bool stopCollectingOfTitles = false;


        public MyLyricsSetup()
        {
            LyricDiagnostics.OpenLog(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log, MyLyricsSettings.LogName));
            //LyricDiagnostics.OpenLog(MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Log));

            #region Initialize GUI and class
            InitializeComponent();
            lyricsLibraryUC = new MyLyricsSetup_LyricsLibrary(this);
            this.tabPageLyricsDatabase.Controls.Add(lyricsLibraryUC);

            // initialize delegates
            m_DelegateLyricFound = new DelegateLyricFound(this.lyricFoundMethod);
            m_DelegateLyricNotFound = new DelegateLyricNotFound(this.lyricNotFoundMethod);
            m_DelegateThreadFinished = new DelegateThreadFinished(this.ThreadFinishedMethod);
            m_DelegateThreadException = new DelegateThreadException(this.ThreadExceptionMethod);

            // Grab music database
            MusicDatabase dbs = new MusicDatabase();
            m_TotalTitles = dbs.GetNumOfSongs();
            #endregion

            #region Get settings from in MediaPortal.xml
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                try
                {
                    tbLimit.Text = xmlreader.GetValueAsString("myLyrics", "limit", m_TotalTitles.ToString());
                    tbPluginName.Text = xmlreader.GetValueAsString("myLyrics", "pluginsName", "My Lyrics");

                    cbAutoFetch.Checked = xmlreader.GetValueAsBool("myLyrics", "automaticFetch", true);
                    cbAutomaticUpdate.Checked = xmlreader.GetValueAsBool("myLyrics", "automaticUpdateWhenFirstFound", false);
                    cbMoveSongFrom.Checked = xmlreader.GetValueAsBool("myLyrics", "moveLyricFromMarkedDatabase", true);

                    lbSongsLimitNote.Text = ("(You have currently " + m_TotalTitles.ToString() + " titles in your music database)");

                    rdDefault.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useDefaultSitesMode", "True")).ToString().Equals("True") ? true : false;
                    trackBar.Value = ((int)xmlreader.GetValueAsInt("myLyrics", "defaultSitesModeValue", 2));
                    trackBar_Scroll(null, null);

                    if (!rdDefault.Checked)
                    {
                        trackBar.Enabled = false;
                        rbUserDefined.Checked = true;

                        cbLyricWiki.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyricWiki", "True")).ToString().Equals("True") ? true : false;
                        cbEvilLabs.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useEvilLabs", "True")).ToString().Equals("True") ? true : false;
                        cbLyrics007.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).ToString().Equals("True") ? true : false;
                        cbLyricsOnDemand.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).ToString().Equals("True") ? true : false;
                        cbSeekLyrics.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useSeekLyrics", "True")).ToString().Equals("True") ? true : false;
                        cbHotLyrics.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).ToString().Equals("True") ? true : false;
                    }
                }
                catch {
                    MessageBox.Show("Something has gone wrong when reading Mediaportal.xml");
                }
            }
            #endregion

            #region Serialzie/deserialize lyricsdatabases
            string lyricsXMLpath = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Base, MyLyricsSettings.LyricsXMLName);
            FileInfo lyricsXMLfileInfo = new FileInfo(lyricsXMLpath);


            // If lyrics.xml present, then convert database to new format...
            if (lyricsXMLfileInfo.Exists)
            {

                string path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsDBName);
                FileInfo fileInfo = new FileInfo(path);

                // .. but only if it hasn't already been converted
                if (fileInfo.Exists == false)
                {
                    if (MessageBox.Show(this, "Your database will have to be upgraded to work with this version\r\nUpgrade now?", "Upgrade lyricsdatabase", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        ConvertFromXMLtoLyricsDatabase convertFromXMLtoLyricsDatabase = new ConvertFromXMLtoLyricsDatabase();
                        MyLyricsSettings.LyricsDB = convertFromXMLtoLyricsDatabase.Convert(lyricsXMLpath);

                        // Create file to save the database to
                        FileStream fs = new FileStream(path, FileMode.Create);

                        // Create a BinaryFormatter object to perform the serialization
                        BinaryFormatter bf = new BinaryFormatter();

                        // Use the BinaryFormatter object to serialize the database to the file
                        bf.Serialize(fs, MyLyricsSettings.LyricsDB);
                        fs.Close();

                        // Create likewise a database for the remainingLyrics
                        path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsMarkedDBName);
                        fs = new FileStream(path, FileMode.Create);
                        MyLyricsSettings.LyricsMarkedDB = new LyricsDatabase();
                        bf.Serialize(fs, MyLyricsSettings.LyricsMarkedDB);

                        // Close the file
                        fs.Close();
                    }
                }

                // ... else deserialize the databases and save reference in LyricsDB and LyricsMarkedDB
                else
                {
                    DeserializeBothDB();
                }
            }

            // If no Lyrics.xml present in base, then create new serialized databases
            else
            {
                string path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsDBName);
                FileInfo fileInfo = new FileInfo(path);

                // .. but only if the databases hasn't been created
                if (fileInfo.Exists == false)
                {
                    path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsDBName);

                    // Serialize empty LyricsDatabase if no lyrics.xml present
                    FileStream fs = new FileStream(path, FileMode.Create);
                    BinaryFormatter bf = new BinaryFormatter();
                    MyLyricsSettings.LyricsDB = new LyricsDatabase();
                    bf.Serialize(fs, MyLyricsSettings.LyricsDB);
                    fs.Close();

                    // Serialize empty LyricsMarkedDatabase
                    path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsMarkedDBName);
                    fs = new FileStream(path, FileMode.Create);
                    MyLyricsSettings.LyricsMarkedDB = new LyricsDatabase();
                    bf.Serialize(fs, MyLyricsSettings.LyricsMarkedDB);
                    fs.Close();
                }
                else
                {
                    DeserializeBothDB();
                }
            }

            MyLyricsSetup_LyricsLibrary.CurrentDB = MyLyricsSettings.LyricsDB;
            #endregion

            lyricsLibraryUC.updateLyricsTree();
        }


        // Stop worker thread if it is running.
        // Called when user presses Stop button of form is closed.
        private void StopThread()
        {
            if (m_LyricControllerThread != null && m_LyricControllerThread.IsAlive)  // thread is active
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

        private void btSave_Click(object sender, System.EventArgs e)
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                xmlwriter.SetValue("myLyrics", "useDefaultSitesMode", rdDefault.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "defaultSitesModeValue", trackBar.Value);
                xmlwriter.SetValue("myLyrics", "limit", tbLimit.Text);
                xmlwriter.SetValue("myLyrics", "pluginsName", tbPluginName.Text);
                xmlwriter.SetValue("myLyrics", "useLyricWiki", cbLyricWiki.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "useEvilLabs", cbEvilLabs.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "useLyrics007", cbLyrics007.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "useLyricsOnDemand", cbLyricsOnDemand.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "useSeekLyrics", cbSeekLyrics.Checked.ToString());
                xmlwriter.SetValue("myLyrics", "useHotLyrics", cbHotLyrics.Checked.ToString());
                xmlwriter.SetValueAsBool("myLyrics", "automaticFetch", cbAutoFetch.Checked);
                xmlwriter.SetValueAsBool("myLyrics", "automaticUpdateWhenFirstFound", cbAutomaticUpdate.Checked);
                xmlwriter.SetValueAsBool("myLyrics", "moveLyricFromMarkedDatabase", cbMoveSongFrom.Checked);
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.Close();
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

        private void btImportAll_Click(object sender, System.EventArgs e)
        {

            if (bgWorkerSearch.IsBusy)
            {
                Thread.Sleep(2000);
            }

            stopwatch.StartZero();
            lbTimer.Text = "00:00.00";
            timer = new System.Windows.Forms.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            stopCollectingOfTitles = false;
            isSearching(true);
            lbStep1a.Text = "The collecting of songs has started.";
            lbStep2a.Text = "";

            // Reset text in textboxes
            lbLastActivity2.Text = "";
            lbMessage.Text = "";
            lbTotalSongs2.Text = m_TotalTitles.ToString();

            m_EventStopThread = new ManualResetEvent(false);

            m_noOfMessages = 0;
            m_noOfCurrentlySearches = 0;

            btImportAll.Enabled = false;

            logFullFileName = Config.GetFile(Config.Dir.Log, MyLyricsSettings.LogBatchFileName);

            //if file is not found, create a new xml file
            if (!System.IO.File.Exists(logFullFileName))
            {
                FileStream file = new FileStream(logFullFileName, FileMode.OpenOrCreate, FileAccess.Write);
                file.Close();
                StreamReader sr = File.OpenText(logFullFileName);
                log = sr.ReadToEnd();
                sr.Close();
                log += DateTime.Now.ToString() + " The log has been created.\r\n";
                System.IO.StreamWriter writerLog = new System.IO.StreamWriter(logFullFileName);
                writerLog.Write(log);
                writerLog.Close();
            }

            m_Limit = int.Parse(tbLimit.Text);

            m_DisregardKnownLyric = cbDisconsiderTitlesWithLyrics.Enabled && cbDisconsiderTitlesWithLyrics.Checked;
            m_MarkSongsWhenNoLyricFound = cbMarkSongsWithNoLyrics.Enabled && cbMarkSongsWithNoLyrics.Checked;
            m_DisregardMarkedLyric = cbDisregardSongsWithNoLyric.Enabled && cbDisregardSongsWithNoLyric.Checked;
            m_DisregardSongWithLyricInTag = cbDisregardSongWithLyricInTag.Enabled && cbDisregardSongWithLyricInTag.Checked;
            m_DisregardVariousArtist = cbDisregardVariousArtist.Enabled && cbDisregardVariousArtist.Checked;
            m_SearchOnlyMarkedSongs = cbSearchOnlyForMarkedSongs.Enabled && cbSearchOnlyForMarkedSongs.Checked;

            if (m_SearchOnlyMarkedSongs)
            {
                progressBar.Maximum = MyLyricsSettings.LyricsMarkedDB.Count;
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

            ArrayList sitesToSearch = new ArrayList();

            if (cbLyricWiki.Checked)
                sitesToSearch.Add("LyricWiki");
            if (cbEvilLabs.Checked)
                sitesToSearch.Add("EvilLabs");
            if (cbLyrics007.Checked)
                sitesToSearch.Add("Lyrics007");
            if (cbLyricsOnDemand.Checked)
                sitesToSearch.Add("LyricsOnDemand");
            if (cbSeekLyrics.Checked)
                sitesToSearch.Add("SeekLyrics");
            if (cbHotLyrics.Checked)
                sitesToSearch.Add("HotLyrics");

            sitesToSearchArray = (string[])sitesToSearch.ToArray(typeof(string));

            m_EventStopThread.Reset();

            bgWorkerSearch.RunWorkerAsync();

            btCancel.Enabled = true;
            btCancel.Focus();
        }




        private void lyricFoundMethod(String lyricStrings, String artist, String track, String site)
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

            string capArtist = LyricUtil.CapatalizeString(m_artist);
            string capTitle = LyricUtil.CapatalizeString(m_track);

            DatabaseUtil.WriteToLyricsDatabase(MyLyricsSettings.LyricsDB, MyLyricsSettings.LyricsMarkedDB, capArtist, capTitle, lyricStrings, site);

            StreamReader sr = File.OpenText(logFullFileName);
            log = sr.ReadToEnd();
            sr.Close();
            string logText = capArtist + " - " + capTitle + " has a match at " + site + ".\r\n";
            log += DateTime.Now.ToString() + " " + logText;
            lbLastActivity2.Text = logText;
            System.IO.StreamWriter writerLog = new System.IO.StreamWriter(logFullFileName);
            writerLog.Write(log);
            writerLog.Close();

            m_noOfCurrentlySearches -= 1;
            progressBar.PerformStep();
            this.Update();
        }

        private void lyricNotFoundMethod(String artist, String title, String message, String site)
        {
            m_LyricsNotFound += 1;

            --m_SongsToSearch;
            lbSongsToSearch2.Text = m_SongsToSearch.ToString();

            lbLyricsNotFound2.Text = m_LyricsNotFound.ToString();

            string capArtist = LyricUtil.CapatalizeString(artist);
            string capTitle = LyricUtil.CapatalizeString(title);
            if (m_MarkSongsWhenNoLyricFound && DatabaseUtil.IsTrackInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, capArtist, capTitle).Equals(DatabaseUtil.LYRIC_NOT_FOUND))
            {
                MyLyricsSettings.LyricsMarkedDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle), new LyricsItem(capArtist, capTitle, "", ""));
            }

            m_SongsWithMark += 1;
            lbSongsWithMark2.Text = m_SongsWithMark.ToString();

            StreamReader sr = File.OpenText(logFullFileName);
            log = sr.ReadToEnd();
            sr.Close();
            string logText = "No match found to " + capArtist + " - " + capTitle + ".\r\n";
            log += DateTime.Now.ToString() + " " + logText;
            lbLastActivity2.Text = logText;
            System.IO.StreamWriter writerLog = new System.IO.StreamWriter(logFullFileName);
            writerLog.Write(log);
            writerLog.Close();

            m_noOfCurrentlySearches -= 1;
            progressBar.PerformStep();
            this.Update();
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
                hour = 0; min = 0; sec = 0;
            }

            if (lc != null)
            {
                lc.StopSearches = true;
                DatabaseUtil.SerializeDBs();
                lyricsLibraryUC.updateLyricsTree();
            }
            bgWorkerSearch.CancelAsync();
            StopThread();
            progressBar.ResetText();
            progressBar.Enabled = false;
            lbStep1a.Text = "Completed";
            lbStep2a.Text = "Completed";
            lbMessage.Text = "#" + ((int)(++m_noOfMessages)).ToString() + " - " + message + "\r\n" + lbMessage.Text + "\r\n";
            btImportAll.Enabled = true;
            btCancel.Enabled = false;
            isSearching(false);
        }

        // Called from worker thread using delegate and Control.Invoke
        private void ThreadExceptionMethod(String o)
        {

        }


        #region ILyricForm properties
        public Object[] UpdateString
        {
            set
            {
                m_StatusText = ((String)value[0]).ToString() + " - " + ((String)value[1]).ToString();
            }
        }
        public Object[] UpdateStatus
        {
            set
            {
            }
        }
        public Object[] LyricFound
        {
            set
            {
                this.Invoke(m_DelegateLyricFound, value);
            }
        }
        public Object[] LyricNotFound
        {
            set
            {
                this.Invoke(m_DelegateLyricNotFound, value);
            }
        }
        public Object[] ThreadFinished
        {
            set
            {
                this.Invoke(m_DelegateThreadFinished, value);
            }
        }

        public string ThreadException
        {
            set
            {
                try
                {
                    this.Invoke(m_DelegateThreadException, value);
                }
                catch { };
            }
        }
        #endregion

        private void bgWorkerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
//            Thread.CurrentThread.Name = "bgWorker - Search";

            #region 1. Sorting song
            lyricConfigInfosQueue = new Queue();

            if (m_SearchOnlyMarkedSongs == false)
            {

                //System.IO.Directory.SetCurrentDirectory(@"C:\Program Files\Team MediaPortal\MediaPortal");
                //string test = System.IO.Directory.GetCurrentDirectory();

                MusicDatabase dbs = new MusicDatabase();

                dbs.GetArtists(ref artists);

                m_noOfArtistsToSearch = artists.Count;

                for (int albumIndex = 0; albumIndex < artists.Count; albumIndex++)
                {

                    // If the user has cancelled the search => end this
                    if (stopCollectingOfTitles)
                    {
                        bgWorkerSearch.CancelAsync();
                        return;
                    }

                    string currentArtist = (string)artists[albumIndex];
                    dbs.GetSongsByArtist(currentArtist, ref songs);

                    for (int i = 0; i < songs.Count; i++)
                    {
                        // if song isn't known in lyric database the progressbar should not be incremented
                        int songNotKnown = -1;
                        Song song = (Song)songs[i];

                        /* Don't include song if one of the following is true
                         * 1. The artist isn't known (MP issue - should be deleted?) 
                         * 2. Various artister should not be considered and the artist is "various artist"
                         * 3. Song with a lyric in the tag should not be considered, but instead include the file to the database right away */

                        MusicTag tag = null;

                        if (song.Artist.Equals("unknown")
                            || (m_DisregardVariousArtist && (song.Artist.ToLower().Equals("various artists") || song.Artist.ToLower().Equals("diverse kunstnere"))))
                        {
                            m_DisregardedSongs += 1;
                        }
                        else if ((m_DisregardSongWithLyricInTag == false && ((tag = MediaPortal.TagReader.TagReader.ReadTag(song.FileName)) != null) && tag.Lyrics.Length > 0))
                        {
                            m_SongsWithLyric += 1;

                            string capArtist = LyricUtil.CapatalizeString(tag.Artist);
                            string capTitle = LyricUtil.CapatalizeString(tag.Title);

                            if (DatabaseUtil.IsTrackInLyricsDatabase(MyLyricsSettings.LyricsDB, capArtist, capTitle).Equals(DatabaseUtil.LYRIC_NOT_FOUND))
                            {
                                MyLyricsSettings.LyricsDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle), new LyricsItem(capArtist, capTitle, tag.Lyrics, "Tag"));
                            }

                            if (DatabaseUtil.IsTrackInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, capArtist, capTitle).Equals(DatabaseUtil.LYRIC_MARKED))
                            {
                                MyLyricsSettings.LyricsMarkedDB.Remove(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle));
                            }
                        }
                        else
                        {
                            int status = DatabaseUtil.IsTrackInLyricsDatabase(MyLyricsSettings.LyricsDB, song.Artist, song.Title);
                            bool isTrackInLyricsMarkedDatabase = true;

                            if (!m_DisregardKnownLyric && status.Equals(DatabaseUtil.LYRIC_FOUND)
                                || (!m_DisregardMarkedLyric && ((isTrackInLyricsMarkedDatabase = DatabaseUtil.IsTrackInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, song.Artist, song.Title).Equals(DatabaseUtil.LYRIC_MARKED)) || status.Equals(DatabaseUtil.LYRIC_MARKED)))
                                || (status.Equals(DatabaseUtil.LYRIC_NOT_FOUND) && !DatabaseUtil.IsTrackInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, song.Artist, song.Title).Equals(DatabaseUtil.LYRIC_MARKED)))
                            {

                                songNotKnown = 1;
                                if (++m_SongsNotKnown > m_Limit)
                                {
                                    songNotKnown = 0;
                                    bgWorkerSearch.ReportProgress(songNotKnown);
                                    goto startSearch;
                                }

                                string[] lyricId = new string[2] { song.Artist, song.Title };
                                lyricConfigInfosQueue.Enqueue(lyricId);

                                m_SongsToSearch = lyricConfigInfosQueue.Count;
                                bgWorkerSearch.ReportProgress(songNotKnown);
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
                        bgWorkerSearch.ReportProgress(songNotKnown);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, LyricsItem> kvp in MyLyricsSettings.LyricsMarkedDB)
                {
                    int songNotKnown = 1;
                    if (++m_SongsNotKnown > m_Limit)
                    {
                        songNotKnown = 0;
                        bgWorkerSearch.ReportProgress(-1);
                        goto startSearch;
                    }
                    string[] lyricId = new string[2] { kvp.Value.Artist, kvp.Value.Title };
                    lyricConfigInfosQueue.Enqueue(lyricId);
                    m_SongsToSearch = lyricConfigInfosQueue.Count;
                    bgWorkerSearch.ReportProgress(songNotKnown);
                }
            }

        startSearch:

            

            # endregion

            #region 2. Searching for lyrics
            // create worker thread instance
            if (lyricConfigInfosQueue.Count > 0)
            {
                // start running the lyricController
                lc = new LyricsController(this, m_EventStopThread, sitesToSearchArray, false);

                lc.NoOfLyricsToSearch = lyricConfigInfosQueue.Count;
                ThreadStart runLyricController = delegate
                {
                    lc.Run();
                };
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

                    if (m_noOfCurrentlySearches < m_NoOfCurrentSearchesAllowed && lc.StopSearches == false)
                    {
                        m_noOfCurrentlySearches += 1;
                        string[] lyricID = (string[])lyricConfigInfosQueue.Dequeue();
                        lc.AddNewLyricSearch(lyricID[0], lyricID[1]);
                    }

                    Thread.Sleep(100);
                }
            }
            else
            {
                ThreadFinished = new string[] { "", "", "There is no titles to search", "" };
            }
            #endregion
        }

        private void bgWorkerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbSongsWithMark2.Text = m_SongsWithMark.ToString();
            lbSongsWithLyric2.Text = m_SongsWithLyric.ToString();
            lbDisregardedSongs2.Text = m_DisregardedSongs.ToString();
            lbSongsToSearch2.Text = m_SongsToSearch.ToString();

            if (e.ProgressPercentage == -1)
            {
                progressBar.PerformStep();
            }
            else if (e.ProgressPercentage == 0)
            {
                lbStep1a.Text = "Completed";
                lbStep2a.Text = "The search for lyrics has started.";
            }
        }

        private void bgWorkerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.PerformStep();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            stopCollectingOfTitles = true;

            if (lc != null)
            {
                lc.FinishThread(m_artist, m_track, "The search has been cancelled by the user.", "none");
                lc.Dispose();
                lc = null;
            }
            else
            {
                m_EventStopThread.Set();
                ThreadFinishedMethod(m_artist, m_track, "The search has been cancelled by the user.", "none");
            }

            bgWorkerSearch.CancelAsync();
            progressBar.ResetText();
            progressBar.Value = 0;
            m_LyricControllerThread = null;
        }

        private void DeserializeBothDB()
        {
            string path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsDBName);

            // Open database to read data from
            FileStream fs = new FileStream(path, FileMode.Open);

            // Create a BinaryFormatter object to perform the deserialization
            BinaryFormatter bf = new BinaryFormatter();

            // Use the BinaryFormatter object to deserialize the database
            MyLyricsSettings.LyricsDB = (LyricsDatabase)bf.Deserialize(fs);
            fs.Close();

            // Deserialize LyricsRemainingDatabase
            path = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Database, MyLyricsSettings.LyricsMarkedDBName);

            fs = new FileStream(path, FileMode.Open);
            MyLyricsSettings.LyricsMarkedDB = (LyricsDatabase)bf.Deserialize(fs);
            fs.Close();
        }

        private void MyLyricsSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            LyricDiagnostics.Dispose();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            int t = (int)stopwatch.ElapsedMilliseconds;
            sec = (t /= 1000);

            int secOverflow = sec / 60;
            sec -= (secOverflow * 60);
            min = secOverflow;

            int minOverflow = min / 60;
            min -= (minOverflow * 60);
            hour = minOverflow;

            lbTimer.Text = (hour < 10 ? "0" + hour.ToString() : hour.ToString()) + ":" + (min < 10 ? "0" + min.ToString() : min.ToString()) + "." + (sec < 10 ? "0" + sec.ToString() : sec.ToString());
        }

        private void cbSearchOnlyForMarkedSongs_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSearchOnlyForMarkedSongs.Checked)
            {
                cbDisconsiderTitlesWithLyrics.Enabled = false;
                cbDisregardSongsWithNoLyric.Enabled = false;
                cbDisregardSongWithLyricInTag.Enabled = false;
                cbDisregardVariousArtist.Enabled = false;
                cbMarkSongsWithNoLyrics.Enabled = false;
            }
            else
            {
                cbDisconsiderTitlesWithLyrics.Enabled = true;
                cbDisregardSongsWithNoLyric.Enabled = true;
                cbDisregardSongWithLyricInTag.Enabled = true;
                cbDisregardVariousArtist.Enabled = true;
                cbMarkSongsWithNoLyrics.Enabled = true;
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                if ((MyLyricsSetup_LyricsLibrary.CurrentDB.Equals(MyLyricsSettings.LyricsDB) && MyLyricsSettings.LyricsDB.Count != lastShownLyricsTitles)
                    || (MyLyricsSetup_LyricsLibrary.CurrentDB.Equals(MyLyricsSettings.LyricsMarkedDB) && MyLyricsSettings.LyricsMarkedDB.Count != lastShownMarkedLyricsTitles))
                {
                    lyricsLibraryUC.updateLyricsTree();
                }
                lyricsLibraryUC.treeView.Select();
            }
            else
            {
                lastShownLyricsTitles = MyLyricsSettings.LyricsDB.Count;
                lastShownMarkedLyricsTitles = MyLyricsSettings.LyricsMarkedDB.Count;
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            if (trackBar.Value == 0)
            {
                cbLyricsOnDemand.Checked = true;
                cbLyrics007.Checked = false;
                cbLyricWiki.Checked = false;
                cbHotLyrics.Checked = false;
                cbSeekLyrics.Checked = false;
                cbEvilLabs.Checked = false;
            }
            else if (trackBar.Value == 1)
            {
                cbLyricsOnDemand.Checked = true;
                cbLyrics007.Checked = true;
                cbLyricWiki.Checked = false;
                cbHotLyrics.Checked = false;
                cbSeekLyrics.Checked = false;
                cbEvilLabs.Checked = false;
            }
            else if (trackBar.Value == 2)
            {
                cbLyricsOnDemand.Checked = true;
                cbLyrics007.Checked = true;
                cbLyricWiki.Checked = true;
                cbHotLyrics.Checked = true;
                cbSeekLyrics.Checked = false;
                cbEvilLabs.Checked = false;
            }
            else if (trackBar.Value == 3)
            {
                cbLyricsOnDemand.Checked = true;
                cbLyrics007.Checked = true;
                cbLyricWiki.Checked = true;
                cbHotLyrics.Checked = true;
                cbSeekLyrics.Checked = true;
                cbEvilLabs.Checked = true;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (trackBar.Value == 3)
            {
                cbLyricsOnDemand.Checked = true;
                cbLyrics007.Checked = true;
                cbLyricWiki.Checked = true;
                cbHotLyrics.Checked = true;
                cbSeekLyrics.Checked = false;
                cbEvilLabs.Checked = false;

                trackBar.Value = 2;
            }
        }

        private void rdTrackBar_CheckedChanged(object sender, EventArgs e)
        {
            if (rdDefault.Checked)
            {
                trackBar.Enabled = true;
                cbLyricsOnDemand.Enabled = false;
                cbLyrics007.Enabled = false;
                cbLyricWiki.Enabled = false;
                cbHotLyrics.Enabled = false;
                cbSeekLyrics.Enabled = false;
                cbEvilLabs.Enabled = false;
                trackBar_Scroll(null, null);
            }
            else
            {
                trackBar.Enabled = false;
                cbLyricsOnDemand.Enabled = true;
                cbLyrics007.Enabled = true;
                cbLyricWiki.Enabled = true;
                cbHotLyrics.Enabled = true;
                cbSeekLyrics.Enabled = true;
                cbEvilLabs.Enabled = true;
            }
        }
    }
}