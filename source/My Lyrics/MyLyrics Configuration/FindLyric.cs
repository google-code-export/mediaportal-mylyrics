using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using LyricsEngine;
using MediaPortal.Profile;

namespace MyLyrics
{
    public partial class FindLyric : Form, ILyricForm
    {
        #region Delegates

        public delegate void DelegateLyricFound(String s, String artist, String track, String site);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site);

        public delegate void DelegateStatusUpdate(
            Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(Object o);

        public delegate void DelegateThreadFinished(String arist, String title, String message, String site);

        #endregion

        private int counter;
        private LyricsController lc;
        private bool m_automaticFetch = true;
        private bool m_automaticUpdate = true;
        private bool m_automaticWriteToMusicTag = true;
        public DelegateLyricFound m_DelegateLyricFound;
        public DelegateLyricNotFound m_DelegateLyricNotFound;
        public DelegateStatusUpdate m_DelegateStatusUpdate;
        public DelegateStringUpdate m_DelegateStringUpdate;
        public DelegateThreadException m_DelegateThreadException;
        public DelegateThreadFinished m_DelegateThreadFinished;
        private ManualResetEvent m_EventStopThread;

        private string m_find = string.Empty;
        private Thread m_LyricControllerThread;
        private bool m_moveLyricFromMarkedDatabase = true;
        private string m_replace = string.Empty;

        private string[] m_strippedPrefixStrings = null;
        private bool markedDatabase;
        private string originalArtist;
        private string originalTitle;

        private LyricsLibrary parent = null;
        private List<string> sitesToSearch;

        private int treeArtistIndex, treeTitleIndex;

        public FindLyric(LyricsLibrary parent, string artist, string title, bool markedDatabase, int treeArtistIndex,
                         int treeTitleIndex)
        {
            InitializeComponent();

            Text = String.Format("Find a lyric for {0} - {1}", artist, title);

            this.parent = parent;
            this.markedDatabase = markedDatabase;
            this.treeArtistIndex = treeArtistIndex;
            this.treeTitleIndex = treeTitleIndex;

            // initialize delegates
            m_DelegateStringUpdate = new DelegateStringUpdate(updateStringMethod);
            m_DelegateStatusUpdate = new DelegateStatusUpdate(updateStatusMethod);
            m_DelegateLyricFound = new DelegateLyricFound(lyricFoundMethod);
            m_DelegateLyricNotFound = new DelegateLyricNotFound(lyricNotFoundMethod);
            m_DelegateThreadFinished = new DelegateThreadFinished(ThreadFinishedMethod);
            m_DelegateThreadException = new DelegateThreadException(ThreadExceptionMethod);

            // initialize events
            m_EventStopThread = new ManualResetEvent(false);

            tbArtist.Text = artist;
            tbTitle.Text = title;

            originalArtist = artist;
            originalTitle = title;

            using (Settings xmlreader = new Settings("MediaPortal.xml"))
            {
                cbLrcFinder.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useLrcFinder", "True")).ToString().Equals("True")
                        ? true
                        : false;
                cbActionext.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useActionext", "True")).ToString().Equals("True")
                        ? true
                        : false;
                cbLyrDB.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useLyrDB", "True")).ToString().Equals("True")
                        ? true
                        : false;
                cbLyrics007.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).ToString().Equals("True")
                        ? true
                        : false;
                cbLyricsOnDemand.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).ToString().Equals(
                        "True")
                        ? true
                        : false;
                cbShironet.Checked =
                    ((string)xmlreader.GetValueAsString("myLyrics", "useShironet", "True")).ToString().Equals(
                        "True")
                        ? true
                        : false;
                cbHotLyrics.Checked =
                    ((string) xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).ToString().Equals("True")
                        ? true
                        : false;
                m_automaticFetch = xmlreader.GetValueAsBool("myLyrics", "automaticFetch", true);
                m_automaticUpdate = xmlreader.GetValueAsBool("myLyrics", "automaticUpdateWhenFirstFound", false);
                m_moveLyricFromMarkedDatabase = xmlreader.GetValueAsBool("myLyrics", "moveLyricFromMarkedDatabase", true);
                m_automaticWriteToMusicTag = xmlreader.GetValueAsBool("myLyrics", "automaticWriteToMusicTag", true);

                m_find = xmlreader.GetValueAsString("myLyrics", "find", "");
                m_replace = xmlreader.GetValueAsString("myLyrics", "replace", "");
            }

            m_strippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            BeginSearchIfPossible(artist, title);
            ShowDialog();
        }

        internal void BeginSearchIfPossible(string artist, string title)
        {
            if (artist.Length != 0 && title.Length != 0)
            {
                if (m_automaticFetch)
                {
                    lvSearchResults.Focus();
                    fetchLyric(originalArtist, originalTitle, m_automaticUpdate);
                }
                else
                {
                    btFind.Focus();
                }
            }
            else if (artist.Length != 0)
            {
                tbTitle.Focus();
            }
            else
            {
                tbArtist.Focus();
            }
        }

        private void lockGUI()
        {
            btFind.Enabled = false;
            btCancel.Enabled = true;
            btClose.Enabled = false;
        }

        private void openGUI()
        {
            btFind.Enabled = true;
            btCancel.Enabled = false;
            btClose.Enabled = true;
        }

        private void fetchLyric(string artist, string title, bool automaticUpdate)
        {
            lockGUI();
            tbLyrics.Text = "";
            lvSearchResults.Items.Clear();

            counter = 0;

            sitesToSearch = new List<string>();

            if (cbLrcFinder.Checked)
            {
                sitesToSearch.Add("LrcFinder");
            }
            if (cbActionext.Checked)
            {
                sitesToSearch.Add("Actionext");
            }
            if (cbLyrDB.Checked)
            {
                sitesToSearch.Add("LyrDB");
            }
            if (cbHotLyrics.Checked)
            {
                sitesToSearch.Add("HotLyrics");
            }
            if (cbLyrics007.Checked)
            {
                sitesToSearch.Add("Lyrics007");
            }
            if (cbLyricsOnDemand.Checked)
            {
                sitesToSearch.Add("LyricsOnDemand");
            }
            if (cbShironet.Checked)
            {
                sitesToSearch.Add("Shironet");
            }

            // If automaticUpdate is set then return after the first positive search
            m_EventStopThread = new ManualResetEvent(false);
            lc = new LyricsController(this, m_EventStopThread, (string[])sitesToSearch.ToArray(), true, automaticUpdate,
                                      m_find, m_replace);

            ThreadStart job = delegate { lc.Run(); };

            m_LyricControllerThread = new Thread(job);
            m_LyricControllerThread.Name = "lyricSearch Thread"; // looks nice in Output window
            m_LyricControllerThread.Start();

            lc.AddNewLyricSearch(artist, title, MediaPortalUtil.GetStrippedPrefixArtist(artist, m_strippedPrefixStrings));
        }

        private void stopSearch()
        {
            Monitor.Enter(this);
            try
            {
                if (lc != null)
                {
                    lc.FinishThread(originalArtist, originalTitle, "", "");
                    lc.Dispose();
                    lc = null;
                }
                else
                {
                    m_EventStopThread.Set();
                    ThreadFinishedMethod(originalArtist, originalTitle, "", "");
                }

                m_LyricControllerThread = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            string artist = tbArtist.Text.Trim();
            string title = tbTitle.Text.Trim();

            if (artist.Length != 0 && title.Length != 0)
            {
                fetchLyric(artist, title, m_automaticUpdate);
            }
            else if (artist.Length == 0)
            {
                tbArtist.Focus();
            }
            else
            {
                tbTitle.Focus();
            }
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            stopSearch();
            Close();

            parent.highlightNextSong(treeArtistIndex, treeTitleIndex);
        }

        private void lvSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                tbLyrics.Text = LyricUtil.ReturnEnvironmentNewLine(lvSearchResults.SelectedItems[0].SubItems[2].Text);
                if (tbLyrics.Text.Length != 0)
                {
                    btUpdate.Enabled = true;
                }
                else
                {
                    btUpdate.Enabled = false;
                }
            }
            else
            {
                btUpdate.Enabled = false;
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            UpdateSong();
            Close();
        }

        private void lvSearchResults_DoubleClick(object sender, EventArgs e)
        {
            btUpdate.PerformClick();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            stopSearch();
            openGUI();
        }

        #region delegate called methods

        // Called from worker thread using delegate and Control.Invoke
        private void updateStringMethod(String message, String site)
        {
            //string m_message = message.ToString();
            //int siteIndex = System.Array.IndexOf<string>(Setup.AllSites(), site);
            //lyricSiteArray[siteIndex].Lyric = m_message;
            //lyricSiteArray[siteIndex].stop();
        }

        // Called from worker thread using delegate and Control.Invoke
        private void updateStatusMethod(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound,
                                        Int32 noOfLyricsNotFound)
        {
        }

        private void lyricFoundMethod(String lyricStrings, String artist, String title, String site)
        {
            ListViewItem item = new ListViewItem(site);
            item.SubItems.Add("yes");
            item.SubItems.Add(lyricStrings);
            lvSearchResults.Items.Add(item);
            lvSearchResults.Items[lvSearchResults.Items.Count - 1].Selected = true;

            if (m_automaticUpdate)
            {
                UpdateSong();
                Close();
            }
            else if (++counter == sitesToSearch.Count)
            {
                stopSearch();
                openGUI();
            }
        }

        private void lyricNotFoundMethod(String artist, String title, String message, String site)
        {
            ListViewItem item = new ListViewItem(site);
            item.SubItems.Add("no");
            item.SubItems.Add("");
            lvSearchResults.Items.Add(item);

            if (++counter == sitesToSearch.Count)
            {
                stopSearch();
                openGUI();
                btClose.Focus();
            }
        }

        private void UpdateSong()
        {
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                stopSearch();
                ListViewItem item = lvSearchResults.Items[0];
                string site = lvSearchResults.SelectedItems[0].Text;
                string lyric = tbLyrics.Text;

                if (markedDatabase && m_moveLyricFromMarkedDatabase)
                {
                    parent.RemoveSong(originalArtist, originalTitle, true);
                    string key = DatabaseUtil.CorrectKeyFormat(originalArtist, originalTitle);
                    MyLyricsSettings.LyricsDB[key] = new LyricsItem(originalArtist, originalTitle, lyric, site);
                    DatabaseUtil.SerializeLyricDB();
                    parent.updateInfo();
                }
                else if (markedDatabase)
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsSettings.LyricsMarkedDB, originalArtist, originalTitle,
                                                         lyric, site);
                    DatabaseUtil.SerializeDBs();
                    parent.updateInfo();
                    parent.highlightSong(originalArtist, originalTitle, false);
                }
                else
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsSettings.LyricsDB, originalArtist, originalTitle, lyric,
                                                         site);
                    DatabaseUtil.SerializeDBs();
                    parent.updateInfo();
                    parent.highlightNextSong(treeArtistIndex, treeTitleIndex);
                }

                if (m_automaticWriteToMusicTag)
                {
                    TagReaderUtil.WriteLyrics(originalArtist, originalTitle, lyric);
                }

                parent.updateLyricDatabaseStats();
            }
        }


        // Set initial state of controls.
        // Called from worker thread using delegate and Control.Invoke
        private void ThreadFinishedMethod(string artist, string title, string message, string site)
        {
        }

        private void ThreadExceptionMethod(Object o)
        {
        }

        #endregion

        #region DelegateCalls

        public Object[] UpdateString
        {
            set
            {
                if (IsDisposed == false)
                {
                    Invoke(m_DelegateStringUpdate, value);
                }
            }
        }

        public Object[] UpdateStatus
        {
            set
            {
                //if (this.IsDisposed == false)
                //{
                //    this.Invoke(m_DelegateStatusUpdate, value);
                //}
            }
        }

        public Object[] LyricFound
        {
            set
            {
                if (IsDisposed == false)
                {
                    try
                    {
                        Invoke(m_DelegateLyricFound, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    ;
                }
            }
        }

        public Object[] LyricNotFound
        {
            set
            {
                if (IsDisposed == false)
                {
                    try
                    {
                        Invoke(m_DelegateLyricNotFound, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    ;
                }
            }
        }

        public Object[] ThreadFinished
        {
            set
            {
                if (IsDisposed == false)
                {
                    try
                    {
                        Invoke(m_DelegateThreadFinished, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    ;
                }
            }
        }

        public string ThreadException
        {
            set
            {
                if (IsDisposed == false)
                {
                    try
                    {
                        Invoke(m_DelegateThreadException);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    ;
                }
            }
        }

        #endregion
    }
}