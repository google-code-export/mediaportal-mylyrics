using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using LyricsEngine;

namespace MyLyrics
{
    public partial class MyLyricsSetup_SearchTitleDialog : Form, ILyricForm
    {

        public delegate void DelegateStringUpdate(String message, String site);
        public DelegateStringUpdate m_DelegateStringUpdate;
        public delegate void DelegateStatusUpdate(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);
        public DelegateStatusUpdate m_DelegateStatusUpdate;
        public delegate void DelegateLyricFound(String s, String artist, String track, String site);
        public DelegateLyricFound m_DelegateLyricFound;
        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site);
        public DelegateLyricNotFound m_DelegateLyricNotFound;
        public delegate void DelegateThreadFinished(String arist, String title, String message, String site);
        public DelegateThreadFinished m_DelegateThreadFinished;
        public delegate void DelegateThreadException(Object o);
        public DelegateThreadException m_DelegateThreadException;
        LyricsController lc;

        // worker thread
        Thread m_LyricControllerThread;

        ManualResetEvent m_EventStopThread;

        string originalArtist;
        string originalTitle;
        int counter;
        bool automaticFetch = true;
        bool automaticUpdate = true;
        bool moveLyricFromMarkedDatabase = true;
        bool markedDatabase;

        List<string> sitesToSearch;
        MyLyricsSetup_LyricsLibrary parent = null;


        public MyLyricsSetup_SearchTitleDialog(MyLyricsSetup_LyricsLibrary form, string artist, string title, bool markedDatabase)
        {
            InitializeComponent();

            this.parent = form;
            this.markedDatabase = markedDatabase;

            // initialize delegates
            m_DelegateStringUpdate = new DelegateStringUpdate(this.updateStringMethod);
            m_DelegateStatusUpdate = new DelegateStatusUpdate(this.updateStatusMethod);
            m_DelegateLyricFound = new DelegateLyricFound(this.lyricFoundMethod);
            m_DelegateLyricNotFound = new DelegateLyricNotFound(this.lyricNotFoundMethod);
            m_DelegateThreadFinished = new DelegateThreadFinished(this.ThreadFinishedMethod);
            m_DelegateThreadException = new DelegateThreadException(this.ThreadExceptionMethod);

            // initialize events
            m_EventStopThread = new ManualResetEvent(false);

            tbArtist.Text = artist;
            tbTitle.Text = title;            

            originalArtist = artist;
            originalTitle = title;

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                cbLyricWiki.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyricWiki", "True")).ToString().Equals("True") ? true : false;
                cbEvilLabs.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useEvilLabs", "True")).ToString().Equals("True") ? true : false;
                cbLyrics007.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).ToString().Equals("True") ? true : false;
                cbLyricsOnDemand.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).ToString().Equals("True") ? true : false;
                cbSeekLyrics.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useSeekLyrics", "True")).ToString().Equals("True") ? true : false;
                cbHotLyrics.Checked = ((string)xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).ToString().Equals("True") ? true : false;
                automaticFetch = xmlreader.GetValueAsBool("myLyrics", "automaticFetch", true);
                automaticUpdate = xmlreader.GetValueAsBool("myLyrics", "automaticUpdateWhenFirstFound", false);
                moveLyricFromMarkedDatabase = xmlreader.GetValueAsBool("myLyrics", "moveLyricFromMarkedDatabase", true);
            }

            this.Show();

            if (artist.Length != 0 && title.Length != 0)
            {
                if (automaticFetch)
                {
                    fetchLyric(originalArtist, originalTitle);
                    lvSearchResults.Focus();
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
        }

        private void openGUI()
        {
            btFind.Enabled = true;
            btCancel.Enabled = false;
        }

        private void fetchLyric(string artist, string title)
        {
            lockGUI();
            tbLyrics.Text = "";
            lvSearchResults.Items.Clear();

            counter = 0;

            sitesToSearch = new List<string>();

            if (cbLyricWiki.Checked)
            {
                sitesToSearch.Add("LyricWiki");
            }
            if (cbEvilLabs.Checked)
            {
                sitesToSearch.Add("EvilLabs");
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
            if (cbSeekLyrics.Checked)
            {
                sitesToSearch.Add("SeekLyrics");
            }

            lc = new LyricsController(this, m_EventStopThread, (string[])sitesToSearch.ToArray(), true);

            ThreadStart job = delegate
            {
                lc.Run();
            };

            m_LyricControllerThread = new Thread(job);
            m_LyricControllerThread.Name = "lyricSearch Thread";	// looks nice in Output window
            m_LyricControllerThread.Start();

            lc.AddNewLyricSearch(artist, title);
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
                fetchLyric(artist, title);
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
        private void updateStatusMethod(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound)
        {
        }

        private void lyricFoundMethod(String lyricStrings, String artist, String title, String site)
        {
            ListViewItem item = new ListViewItem(site);
            item.SubItems.Add("yes");
            item.SubItems.Add(lyricStrings);
            lvSearchResults.Items.Add(item);
            lvSearchResults.Items[lvSearchResults.Items.Count - 1].Selected = true;

            if (automaticUpdate)
            {
                btUpdate_Click(null, null);
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
                this.Invoke(m_DelegateStringUpdate, value);
            }
        }
        public Object[] UpdateStatus
        {
            set
            {
                //this.Invoke(m_DelegateStatusUpdate, value);
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
                this.Invoke(m_DelegateThreadException);
            }
        }
        #endregion

        private void btClose_Click(object sender, EventArgs e)
        {
            stopSearch();
            this.Close();
        }

        private void lvSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                tbLyrics.Text = LyricsEngine.LyricUtil.ReturnEnvironmentNewLine(lvSearchResults.SelectedItems[0].SubItems[2].Text);
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
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                stopSearch();
                ListViewItem item = lvSearchResults.Items[0];
                string site = lvSearchResults.SelectedItems[0].Text;
                string lyric = tbLyrics.Text;

                if (markedDatabase && moveLyricFromMarkedDatabase)
                {
                    parent.RemoveSong(originalArtist, originalTitle);
                    string key = DatabaseUtil.CorrectKeyFormat(originalArtist, originalTitle);
                    MyLyricsSettings.LyricsDB[key] = new LyricsItem(originalArtist, originalTitle, lyric, site);
                    DatabaseUtil.SerializeLyricDB();
                    parent.updateInfo();
                }
                else if (markedDatabase)
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsSettings.LyricsMarkedDB, originalArtist, originalTitle, lyric, site);
                    DatabaseUtil.SerializeDBs();
                    parent.updateInfo();
                    parent.highlightSong(originalArtist, originalTitle, false);
                }
                else
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsSettings.LyricsDB, originalArtist, originalTitle, lyric, site);
                    DatabaseUtil.SerializeDBs();
                    parent.updateInfo();
                    parent.highlightSong(originalArtist, originalTitle, false);
                }
                this.Close();
            }
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
    }
}