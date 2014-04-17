using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LyricsEngine;
using LyricsEngine.LyricsSites;
using MyLyrics.XmlSettings;

namespace MyLyrics
{
    public partial class FindLyric : Form, ILyricForm
    {
        #region Delegates

        public delegate void DelegateLyricFound(String s, String artist, String track, String site, int row);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site, int row);

        public delegate void DelegateStatusUpdate(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(Object o);

        public delegate void DelegateThreadFinished(String arist, String title, String message, String site);

        #endregion

        private readonly bool _mAutomaticFetch = true;
        private readonly bool _mAutomaticUpdate = true;
        private readonly bool _mAutomaticWriteToMusicTag = true;
        
        private readonly string _mFind = string.Empty;
        
        private readonly bool _mMoveLyricFromMarkedDatabase = true;
        private readonly string _mReplace = string.Empty;

        private readonly string[] _mStrippedPrefixStrings;
        private readonly bool _markedDatabase;
        private readonly string _originalArtist;
        private readonly string _originalTitle;
        private readonly LyricsLibrary _parent;

        public DelegateLyricFound MDelegateLyricFound;
        public DelegateLyricNotFound MDelegateLyricNotFound;
        public DelegateStatusUpdate MDelegateStatusUpdate;
        public DelegateStringUpdate MDelegateStringUpdate;
        public DelegateThreadException MDelegateThreadException;
        public DelegateThreadFinished MDelegateThreadFinished;
        private ManualResetEvent _mEventStopThread;

        private int _counter;
        private LyricsController _lyricsController;

        private Thread _mLyricControllerThread;

        private readonly List<string> _sitesToSearch = new List<string>();

        private readonly int _treeArtistIndex;
        private readonly int _treeTitleIndex;

        public FindLyric(LyricsLibrary parent, string artist, string title, bool markedDatabase, int treeArtistIndex, int treeTitleIndex)
        {
            InitializeComponent();

            Text = String.Format("Find a lyric for {0} - {1}", artist, title);

            _parent = parent;
            _markedDatabase = markedDatabase;
            _treeArtistIndex = treeArtistIndex;
            _treeTitleIndex = treeTitleIndex;

            // initialize delegates
            MDelegateStringUpdate = UpdateStringMethod;
            MDelegateStatusUpdate = UpdateStatusMethod;
            MDelegateLyricFound = LyricFoundMethod;
            MDelegateLyricNotFound = LyricNotFoundMethod;
            MDelegateThreadFinished = ThreadFinishedMethod;
            MDelegateThreadException = ThreadExceptionMethod;

            // initialize events
            _mEventStopThread = new ManualResetEvent(false);

            tbArtist.Text = artist;
            tbTitle.Text = title;

            _originalArtist = artist;
            _originalTitle = title;
            
            var lyricsSitesNames = LyricsSiteFactory.LyricsSitesNames();
            singleRunSitesList.Items.Clear();
            foreach (var site in lyricsSitesNames)
            {
                singleRunSitesList.Items.Add(site, SettingManager.GetParamAsBool(SettingManager.SitePrefix + site, false));
            }

            _mAutomaticFetch = SettingManager.GetParamAsBool(SettingManager.AutomaticFetch, true);
            _mAutomaticUpdate = SettingManager.GetParamAsBool(SettingManager.AutomaticUpdateWhenFirstFound, false);
            _mMoveLyricFromMarkedDatabase = SettingManager.GetParamAsBool(SettingManager.MoveLyricFromMarkedDatabase, true);
            _mAutomaticWriteToMusicTag = SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, true);

            _mFind = SettingManager.GetParamAsString(SettingManager.Find, "");
            _mReplace = SettingManager.GetParamAsString(SettingManager.Replace, "");

            _mStrippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            BeginSearchIfPossible(artist, title);
            ShowDialog();
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        internal void BeginSearchIfPossible(string artist, string title)
        {
            if (artist.Length != 0 && title.Length != 0)
            {
                if (_mAutomaticFetch)
                {
                    lvSearchResults.Focus();
                    FetchLyric(_originalArtist, _originalTitle, _mAutomaticUpdate);
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

        private void LockGui()
        {
            btFind.Enabled = false;
            btCancel.Enabled = true;
            btClose.Enabled = false;
        }

        private void OpenGui()
        {
            btFind.Enabled = true;
            btCancel.Enabled = false;
            btClose.Enabled = true;
        }

        private void FetchLyric(string artist, string title, bool automaticUpdate)
        {
            LockGui();
            tbLyrics.Text = "";
            lvSearchResults.Items.Clear();

            _counter = 0;

            _sitesToSearch.Clear();
            foreach (var site in singleRunSitesList.Items.Cast<object>().Where(site => singleRunSitesList.CheckedItems.Contains(site)))
            {
                _sitesToSearch.Add((string)site);
            }

            // If automaticUpdate is set then return after the first positive search
            _mEventStopThread = new ManualResetEvent(false);
            _lyricsController = new LyricsController(this, _mEventStopThread, _sitesToSearch.ToArray(), true, automaticUpdate, _mFind, _mReplace);

            ThreadStart job = delegate { _lyricsController.Run(); };

            _mLyricControllerThread = new Thread(job);
            _mLyricControllerThread.Name = "lyricSearch Thread"; // looks nice in Output window
            _mLyricControllerThread.Start();

            _lyricsController.AddNewLyricSearch(artist, title, MediaPortalUtil.GetStrippedPrefixArtist(artist, _mStrippedPrefixStrings));
        }

        private void StopSearch()
        {
            Monitor.Enter(this);
            try
            {
                if (_lyricsController != null)
                {
                    _lyricsController.FinishThread(_originalArtist, _originalTitle, "", "");
                    _lyricsController.Dispose();
                    _lyricsController = null;
                }
                else
                {
                    _mEventStopThread.Set();
                    ThreadFinishedMethod(_originalArtist, _originalTitle, "", "");
                }

                _mLyricControllerThread = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void btFind_Click(object sender, EventArgs e)
        {
            var artist = tbArtist.Text.Trim();
            var title = tbTitle.Text.Trim();

            if (artist.Length != 0 && title.Length != 0)
            {
                FetchLyric(artist, title, _mAutomaticUpdate);
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
            StopSearch();
            Close();

            _parent.HighlightNextSong(_treeArtistIndex, _treeTitleIndex);
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
            StopSearch();
            OpenGui();
        }

        #region delegate called methods

        // Called from worker thread using delegate and Control.Invoke
        private static void UpdateStringMethod(String message, String site)
        {
            //string m_message = message.ToString();
            //int siteIndex = System.Array.IndexOf<string>(Setup.AllSites(), site);
            //lyricSiteArray[siteIndex].Lyric = m_message;
            //lyricSiteArray[siteIndex].stop();
        }

        // Called from worker thread using delegate and Control.Invoke
        private static void UpdateStatusMethod(Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound)
        {
        }

        private void LyricFoundMethod(String lyricStrings, String artist, String title, String site, int row)
        {
            var item = new ListViewItem(site);
            item.SubItems.Add("yes");
            item.SubItems.Add(lyricStrings);
            lvSearchResults.Items.Add(item);
            lvSearchResults.Items[lvSearchResults.Items.Count - 1].Selected = true;

            if (_mAutomaticUpdate)
            {
                UpdateSong();
                Close();
            }
            else if (++_counter == _sitesToSearch.Count)
            {
                StopSearch();
                OpenGui();
            }
        }

        private void LyricNotFoundMethod(String artist, String title, String message, String site, int row)
        {
            var item = new ListViewItem(site);
            item.SubItems.Add("no");
            item.SubItems.Add("");
            lvSearchResults.Items.Add(item);

            if (++_counter == _sitesToSearch.Count)
            {
                StopSearch();
                OpenGui();
                btClose.Focus();
            }
        }

        private void UpdateSong()
        {
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                StopSearch();
                var site = lvSearchResults.SelectedItems[0].Text;
                var lyric = tbLyrics.Text;

                if (_markedDatabase && _mMoveLyricFromMarkedDatabase)
                {
                    _parent.RemoveSong(_originalArtist, _originalTitle, true);
                    var key = DatabaseUtil.CorrectKeyFormat(_originalArtist, _originalTitle);
                    MyLyricsUtils.LyricsDB[key] = new LyricsItem(_originalArtist, _originalTitle, lyric, site);
                    DatabaseUtil.SerializeLyricDB();
                    _parent.UpdateInfo();
                }
                else if (_markedDatabase)
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsUtils.LyricsMarkedDB, _originalArtist, _originalTitle,
                                                         lyric, site);
                    DatabaseUtil.SerializeDBs();
                    _parent.UpdateInfo();
                    _parent.HighlightSong(_originalArtist, _originalTitle, false);
                }
                else
                {
                    DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsUtils.LyricsDB, _originalArtist, _originalTitle, lyric,
                                                         site);
                    DatabaseUtil.SerializeDBs();
                    _parent.UpdateInfo();
                    _parent.HighlightNextSong(_treeArtistIndex, _treeTitleIndex);
                }

                if (_mAutomaticWriteToMusicTag)
                {
                    TagReaderUtil.WriteLyrics(_originalArtist, _originalTitle, lyric);
                }

                _parent.UpdateLyricDatabaseStats();
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
                    Invoke(MDelegateStringUpdate, value);
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
                        Invoke(MDelegateLyricFound, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
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
                        Invoke(MDelegateLyricNotFound, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
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
                        Invoke(MDelegateThreadFinished, value);
                    }
                    catch (InvalidOperationException)
                    {
                    }
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
                        Invoke(MDelegateThreadException);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
        }

        #endregion
    }
}