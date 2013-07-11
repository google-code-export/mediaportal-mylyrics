using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using LyricsEngine;
using MediaPortal.Music.Database;
using MediaPortal.Profile;
using MediaPortal.TagReader;

namespace MyLyrics
{
    public partial class MusicDatabaseBrowse : UserControl, ILyricForm
    {
        #region Delegates

        public delegate void DelegateLyricFound(String s, String artist, String track, String site, int row);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site, int row);

        public delegate void DelegateStatusUpdate(
            Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(Object o);

        public delegate void DelegateThreadFinished(String arist, String title, String message, String site);

        #endregion

        public DelegateLyricFound MDelegateLyricFound;
        public DelegateLyricNotFound MDelegateLyricNotFound;
        public DelegateStatusUpdate MDelegateStatusUpdate;
        public DelegateStringUpdate MDelegateStringUpdate;

        public DelegateThreadException MDelegateThreadException;
        public DelegateThreadFinished MDelegateThreadFinished;
        private ManualResetEvent _mEventStopThread;
        private string _mFind = string.Empty;
        private LyricsController _lyricsController;
        private Thread _mLyricControllerThread;
        private MusicDatabase _mMDb;

        private const int MNoOfSearchesAllowed = 5;
        private int _mNoOfCurrentlySearches;
        private int _mNoOfSearchesCompleted;
        private int _mNoOfSearchesToComplete;
        private readonly MyLyricsSetup _mParent;
        private string _mReplace = string.Empty;

        private bool _mSearching;
        private string _mSelectedArtist = String.Empty;

        private Queue<string[]> _mSongs;
        private string[] _mStrippedPrefixStrings;

        public MusicDatabaseBrowse(MyLyricsSetup parent)
        {
            _mParent = parent;
            InitializeComponent();
        }

        #region ILyricForm Members

        public object[] UpdateString
        {
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public object[] UpdateStatus
        {
            set { ; }
        }

        public object[] LyricFound
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
                    ;
                }
            }
        }

        public object[] LyricNotFound
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
                    ;
                }
            }
        }

        public object[] ThreadFinished
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
                        Invoke(MDelegateThreadException);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    ;
                }
            }
        }

        #endregion

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

        private void lyricFoundMethod(String lyricStrings, String artist, String title, String site, int row)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                if (lvi.Text.Equals(artist) && lvi.SubItems[1].Text.Equals(title))
                {
                    if (lvi.SubItems[3].Text.Equals("miss"))
                    {
                        MessageBox.Show("This is an error and should not be able to happen!");
                    }

                    lvi.ImageIndex = 0;
                    lvi.SubItems[2].Text = "LyricsDB";
                    lvi.SubItems[3].Text = "OK";
                    lvi.EnsureVisible();

                    string capArtist = LyricUtil.CapatalizeString(artist);
                    string capTitle = LyricUtil.CapatalizeString(title);

                    DatabaseUtil.WriteToLyricsDatabase(MyLyricsUtils.LyricsDB, MyLyricsUtils.LyricsMarkedDB,
                                                       capArtist, capTitle, lyricStrings, site);
                    DatabaseUtil.SerializeLyricDB();

                    if (!site.Equals("music tag"))
                    {
                        using (var xmlreader = MyLyricsCore.MediaPortalSettings)
                        {
                            if (xmlreader.GetValueAsBool("myLyrics", "automaticWriteToMusicTag", true))
                            {
                                TagReaderUtil.WriteLyrics(capArtist, capTitle, lyricStrings);
                            }
                        }
                    }

                    _mNoOfCurrentlySearches -= 1;
                    ++_mNoOfSearchesCompleted;

                    break;
                }
            }

            //lvSelectedSongs.Update();
        }

        private void lyricNotFoundMethod(String artist, String title, String message, String site, int row)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                if (lvi.Text.Equals(artist) && lvi.SubItems[1].Text.Equals(title) && !lvi.SubItems[3].Text.Equals("OK"))
                {
                    lvi.SubItems[2].Text = "MarkedDB";
                    lvi.SubItems[3].Text = "miss";
                    lvi.EnsureVisible();

                    string capArtist = LyricUtil.CapatalizeString(artist);
                    string capTitle = LyricUtil.CapatalizeString(title);

                    if (
                        DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, capArtist, capTitle).
                            Equals(DatabaseUtil.LYRIC_NOT_FOUND))
                    {
                        MyLyricsUtils.LyricsMarkedDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                                                            new LyricsItem(capArtist, capTitle, "", ""));
                    }

                    DatabaseUtil.SerializeLyricMarkedDB();
                    _mNoOfCurrentlySearches -= 1;
                    ++_mNoOfSearchesCompleted;

                    break;
                }
            }

            //lvSelectedSongs.Update();
        }


        // Set initial state of controls.
        // Called from worker thread using delegate and Control.Invoke
        private void ThreadFinishedMethod(string artist, string title, string message, string site)
        {
            _mSearching = false;

            ChangeButtonsEnableState();
            RefreshSongsListView();
            ChangeStatusOnSubItems(false);

            lvSelectedSongs.Update();
        }

        private void ThreadExceptionMethod(Object o)
        {
            lvSelectedSongs.Update();
        }

        #endregion

        private void ListboxArtistsUpdate(object sender, EventArgs e)
        {
            _mMDb = MusicDatabase.Instance;
            var artists = new ArrayList();
            //mdb.GetArtists(0, String.Empty, ref artists);
            _mMDb.GetAllArtists(ref artists);

            lvArtists.Items.Clear();
            lbSelectedArtist.Text = String.Empty;

            MDelegateStringUpdate = updateStringMethod;
            MDelegateStatusUpdate = updateStatusMethod;
            MDelegateLyricFound = lyricFoundMethod;
            MDelegateLyricNotFound = lyricNotFoundMethod;
            MDelegateThreadFinished = ThreadFinishedMethod;
            MDelegateThreadException = ThreadExceptionMethod;

            foreach (var artist in artists)
            {
                var lvi = new ListViewItem((string) artist);
                lvArtists.Items.Add(lvi);
            }

            lvArtists.Sorting = SortOrder.Ascending;
            lvSongs.Items.Clear();
            lbArtistNumber.Text = String.Format("{0} artists found", lvArtists.Items.Count);
        }

        private void btSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSongs.Items)
            {
                lvi.Selected = true;
            }
            lvSongs.Select();
        }

        private void btDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSongs.Items)
            {
                lvi.Selected = false;
            }
            lvSongs.Select();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSongs.SelectedItems)
            {
                AddToSelected(lvi);
            }

            if (lvSelectedSongs.Items.Count > 0)
            {
                btSearch.Enabled = true;
                btRemove.Enabled = true;
                lvSelectedSongs.Sort();
            }
        }

        private void AddToSelected(ListViewItem lvi)
        {
            if (lvi.ImageIndex == -1)
            {
                var newLvi = new ListViewItem(_mSelectedArtist);

                foreach (ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
                {
                    newLvi.SubItems.Add(lvsi);
                }

                newLvi.ImageIndex = lvi.ImageIndex;

                var alreadySelected = false;

                foreach (ListViewItem item in lvSelectedSongs.Items)
                {
                    if (item.SubItems[0].Text.Equals(_mSelectedArtist) &&
                        item.SubItems[1].Text.Equals(newLvi.SubItems[1].Text))
                    {
                        alreadySelected = true;
                    }
                }

                if (!alreadySelected &&
                    !lvSelectedSongs.Items.ContainsKey(_mSelectedArtist + "-" + lvi.SubItems[0].Text))
                {
                    lvSelectedSongs.Items.Add(newLvi);
                }
            }
        }

        private void btAddAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSongs.Items)
            {
                AddToSelected(lvi);
            }

            if (lvSelectedSongs.Items.Count > 0)
            {
                btSearch.Enabled = true;
                btRemove.Enabled = true;
                lvSelectedSongs.Sort();
            }
        }


        private void listViewArtists_SelectedIndexChanged(object sender, EventArgs e)
        {
            MusicDatabase mdb = MusicDatabase.Instance;
            List<Song> songs = new List<Song>();
            _mSelectedArtist = ((ListView) (sender)).SelectedItems.Count > 0
                                   ? ((ListView) (sender)).SelectedItems[0].Text
                                   : "";
            mdb.GetSongsByArtist(_mSelectedArtist, ref songs);

            lbSelectedArtist.Text = String.Format("Artist: {0}", _mSelectedArtist);
            lvSongs.Items.Clear();

            foreach (Song song in songs)
            {
                string capatalizedArtist = LyricUtil.CapatalizeString(song.Artist);
                string capatalizedTitle = LyricUtil.CapatalizeString(song.Title);

                ListViewItem lvi = new ListViewItem(capatalizedTitle);
                lvi.Tag = capatalizedTitle;

                int status = DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, capatalizedArtist,
                                                                 capatalizedTitle);
                switch (status)
                {
                    case DatabaseUtil.LYRIC_FOUND:
                        lvi.ImageIndex = 0;
                        lvi.SubItems.Add("LyricsDB");
                        lvi.SubItems.Add("-");
                        break;
                    case DatabaseUtil.LYRIC_MARKED:
                        lvi.SubItems.Add("MarkedDB");
                        lvi.SubItems.Add("-");
                        break;
                    case DatabaseUtil.LYRIC_NOT_FOUND:
                        if (
                            DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, capatalizedArtist,
                                                                      capatalizedTitle).Equals(DatabaseUtil.LYRIC_MARKED))
                        {
                            lvi.SubItems.Add("MarkedDB");
                            lvi.SubItems.Add("-");
                        }
                        else
                        {
                            lvi.SubItems.Add("-");
                            lvi.SubItems.Add("-");
                        }
                        break;
                    default:
                        lvi.SubItems.Add("no");
                        lvi.SubItems.Add("-");
                        break;
                }

                bool alreadyInCollection = false;

                foreach (ListViewItem lviColl in lvSongs.Items)
                {
                    if (lvi.Tag.Equals(lviColl.Tag))
                    {
                        alreadyInCollection = true;
                        break;
                    }
                }

                if (!alreadyInCollection)
                {
                    lvSongs.Items.Add(lvi);
                }
            }
            RefreshArtistStats();
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.SelectedItems)
            {
                lvSelectedSongs.Items.Remove(lvi);
            }

            if (lvSelectedSongs.Items.Count == 0)
            {
                btSearch.Enabled = false;
                btRemove.Enabled = false;
            }
        }

        private void SearchMusicTags()
        {
            if (bwMusicTagSearch.IsBusy)
            {
                Thread.Sleep(2000);
            }

            bwMusicTagSearch.RunWorkerAsync();
        }

        private void Search_Init()
        {
            _mSearching = true;

            MyLyricsSetup.UpdateLibraryUI = true;
            ChangeButtonsEnableState();

            _mSongs = new Queue<string[]>();

            List<ListViewItem> items = new List<ListViewItem>();

            int count = 0;

            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                ListViewItem lviClone = (ListViewItem) lvi.Clone();
                if (lvi.ImageIndex == -1)
                {
                    _mSongs.Enqueue(new string[3]
                                        {lviClone.SubItems[0].Text, lviClone.SubItems[1].Text, count.ToString()});
                    lviClone.SubItems[3].Text = "";
                    items.Add(lviClone);
                    ++count;
                }
            }

            lvSelectedSongs.Items.Clear();
            lvSelectedSongs.Items.AddRange(items.ToArray());

            _mNoOfSearchesToComplete = _mSongs.Count;
            _mNoOfCurrentlySearches = 0;
            _mNoOfSearchesCompleted = 0;
        }

        private void SearchOnline()
        {
            // After the tag search is completed, we now recalculate the number of lyrics to search for during online search
            _mNoOfSearchesToComplete -= _mNoOfSearchesCompleted;
            _mNoOfSearchesCompleted = 0;
            _mNoOfCurrentlySearches = 0;

            if (bwOnlineSearch.IsBusy)
            {
                Thread.Sleep(2000);
            }

            List<string> sitesToSearch = new List<string>();

            using (Settings xmlreader = MyLyricsCore.MediaPortalSettings)
            {
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLrcFinder", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LrcFinder");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useActionext", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Actionext");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyrDB", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LyrDb");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Lyrics007");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LyricsOnDemand");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("HotLyrics");
                if (((string)xmlreader.GetValueAsString("myLyrics", "useShironet", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Shironet");

                _mFind = xmlreader.GetValueAsString("myLyrics", "find", "");
                _mReplace = xmlreader.GetValueAsString("myLyrics", "replace", "");
            }

            _mStrippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            _mEventStopThread = new ManualResetEvent(false);

            // If automaticUpdate is set then return after the first positive search
            _lyricsController = new LyricsController(this, _mEventStopThread, (string[]) sitesToSearch.ToArray(), false, false,
                                        _mFind, _mReplace);

            _lyricsController.NoOfLyricsToSearch = _mNoOfSearchesToComplete;

            ThreadStart job = delegate { _lyricsController.Run(); };

            _mLyricControllerThread = new Thread(job);
            _mLyricControllerThread.Start();

            _lyricsController.StopSearches = false;

            bwOnlineSearch.RunWorkerAsync();
        }

        private void btSelectAll2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                lvi.Selected = true;
            }
            lvSelectedSongs.Select();
        }

        private void btDeselectAll2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                lvi.Selected = false;
            }
            lvSelectedSongs.Select();
        }

        public void RefreshSongsListView()
        {
            if (lvArtists.SelectedIndices.Count > 0)
            {
                int index = lvArtists.SelectedIndices[0];
                lvArtists.Items[index].Selected = false;
                lvArtists.Items[index].Selected = true;
            }
        }

        public void RefreshArtistStats()
        {
            int lyricsInLyricDB = 0;
            int lyricsInMarkedDB = 0;

            foreach (ListViewItem lvi in lvSongs.Items)
            {
                if (lvi.SubItems[1].Text.Equals("LyricsDB"))
                {
                    ++lyricsInLyricDB;
                }
                else if (lvi.SubItems[1].Text.Equals("MarkedDB"))
                {
                    ++lyricsInMarkedDB;
                }
            }
            lbStats.Text = string.Format("Lyrics: {0} - Lyrics in LyricsDB: {1} - Lyrics in MarkedDB: {2}",
                                         lvSongs.Items.Count, lyricsInLyricDB, lyricsInMarkedDB);
        }

        private void ChangeStatusOnSubItems(bool cancelled)
        {
            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                if (lvi.SubItems[3].Text.Equals("search"))
                {
                    string artist = lvi.SubItems[0].Text;
                    string title = lvi.SubItems[1].Text;
                    if (
                        DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsUtils.LyricsMarkedDB, artist, title).Equals
                            (DatabaseUtil.LYRIC_MARKED))
                    {
                        lvi.SubItems[2].Text = "MarkedDB";
                        if (cancelled)
                        {
                            lvi.SubItems[3].Text = "-";
                        }
                        else
                        {
                            lvi.SubItems[3].Text = "miss";
                        }
                    }
                    else
                    {
                        lvi.SubItems[2].Text = "-";
                        lvi.SubItems[3].Text = "-";
                    }
                }
            }
        }

        private void ChangeButtonsEnableState()
        {
            if (_mSearching == true)
            {
                _mParent.tabControl.SelectedIndexChanged -= new EventHandler(_mParent.tabControl_SelectedIndexChanged);

                ((MyLyricsSetup) _mParent).btClose.Enabled = false;
                btSearch.Enabled = false;
                btCancel.Enabled = true;
                btAdd.Enabled = false;
                btAddAll.Enabled = false;
                btDeselectAll.Enabled = false;
                btDeselectAll2.Enabled = false;
                btRemove.Enabled = false;
                btRemoveAll.Enabled = false;
                btSelectAll.Enabled = false;
                btSelectAll2.Enabled = false;
            }
            else
            {
                _mParent.tabControl.SelectedIndexChanged += new EventHandler(_mParent.tabControl_SelectedIndexChanged);

                ((MyLyricsSetup) _mParent).btClose.Enabled = true;
                btSearch.Enabled = true;
                btCancel.Enabled = false;
                btAdd.Enabled = true;
                btAddAll.Enabled = true;
                btDeselectAll.Enabled = true;
                btDeselectAll2.Enabled = true;
                btRemove.Enabled = true;
                btRemoveAll.Enabled = true;
                btSelectAll.Enabled = true;
                btSelectAll2.Enabled = true;
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_mSongs.Count != 0)
            {
                if (_lyricsController == null)
                    return;

                if (bwOnlineSearch.CancellationPending)
                {
                    return;
                }

                if (_mNoOfCurrentlySearches < MNoOfSearchesAllowed && _lyricsController.StopSearches == false)
                {
                    _mNoOfCurrentlySearches += 1;
                    string[] lyricID = (string[]) _mSongs.Dequeue();
                    string artist = lyricID[0];
                    string title = lyricID[1];
                    int rowNumberInListView = int.Parse(lyricID[2]);

                    bwOnlineSearch.ReportProgress(rowNumberInListView);

                    _lyricsController.AddNewLyricSearch(artist, title,
                                           MediaPortalUtil.GetStrippedPrefixArtist(artist, _mStrippedPrefixStrings));
                }

                Thread.Sleep(200);
            }
        }

        private bool LyricFoundInMusicTag(string artist, string title)
        {
            Song song = new Song();

            _mMDb.GetSongByMusicTagInfo(artist, string.Empty, title, true, ref song);

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

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lvSelectedSongs.Items[e.ProgressPercentage].SubItems[2].Text = "-";
            lvSelectedSongs.Items[e.ProgressPercentage].SubItems[3].Text = "search";
            lvSelectedSongs.Items[e.ProgressPercentage].EnsureVisible();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }


        private void btRemoveAll_Click(object sender, EventArgs e)
        {
            ClearSelectedSongs();
        }

        public void ClearSelectedSongs()
        {
            lvSelectedSongs.Items.Clear();

            if (lvSelectedSongs.Items.Count == 0)
            {
                btSearch.Enabled = false;
                btRemove.Enabled = false;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            if (_lyricsController != null)
            {
                _lyricsController.FinishThread(_mSelectedArtist, string.Empty, "The search has been cancelled by the user.", "none");
                _lyricsController.Dispose();
                _lyricsController = null;
            }
            else if (_mEventStopThread != null)
            {
                _mEventStopThread.Set();
                ThreadFinishedMethod(_mSelectedArtist, string.Empty, "The search has been cancelled by the user.",
                                     "none");
            }

            bwMusicTagSearch.CancelAsync();
            bwOnlineSearch.CancelAsync();
            ChangeButtonsEnableState();
            _mSearching = false;
            Thread.Sleep(500);
            RefreshSongsListView();
            RefreshArtistStats();
            ChangeStatusOnSubItems(true);
        }


        private void bwMusicTagSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            Queue<string[]> m_SongsToSearchOnline = new Queue<string[]>();
            int rowNumberInListView = 0;

            foreach (string[] song in _mSongs)
            {
                if (bwMusicTagSearch.CancellationPending)
                {
                    return;
                }

                if (!LyricFoundInMusicTag(song[0], song[1]))
                {
                    m_SongsToSearchOnline.Enqueue(new string[3] {song[0], song[1], rowNumberInListView.ToString()});
                }
                else
                {
                    bwMusicTagSearch.ReportProgress(rowNumberInListView);
                }

                ++rowNumberInListView;
            }

            _mSongs = m_SongsToSearchOnline;
        }

        private void bwMusicTagSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lvSelectedSongs.Items[e.ProgressPercentage].SubItems[2].Text = "LyricsDB";
            lvSelectedSongs.Items[e.ProgressPercentage].SubItems[3].Text = "OK";
            lvSelectedSongs.Items[e.ProgressPercentage].EnsureVisible();
        }

        private void bwMusicTagSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // If all found in music tags, then no need for online search
            if (_mNoOfSearchesCompleted == _mNoOfSearchesToComplete)
            {
                _mSearching = false;
                ChangeButtonsEnableState();
                return;
            }

            SearchOnline();
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            using (Settings xmlreader = MyLyricsCore.MediaPortalSettings)
            {
                if (xmlreader.GetValueAsBool("myLyrics", "automaticReadFromMusicTag", true))
                {
                    Search_Init();
                    SearchMusicTags();
                }
                else
                {
                    Search_Init();
                    SearchOnline();
                }
            }
        }
    }
}