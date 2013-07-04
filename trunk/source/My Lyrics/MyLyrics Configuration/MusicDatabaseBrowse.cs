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

        public delegate void DelegateLyricFound(String s, String artist, String track, String site);

        public delegate void DelegateLyricNotFound(String artist, String title, String message, String site);

        public delegate void DelegateStatusUpdate(
            Int32 noOfLyricsToSearch, Int32 noOfLyricsSearched, Int32 noOfLyricsFound, Int32 noOfLyricsNotFound);

        public delegate void DelegateStringUpdate(String message, String site);

        public delegate void DelegateThreadException(Object o);

        public delegate void DelegateThreadFinished(String arist, String title, String message, String site);

        #endregion

        public DelegateLyricFound m_DelegateLyricFound;
        public DelegateLyricNotFound m_DelegateLyricNotFound;
        public DelegateStatusUpdate m_DelegateStatusUpdate;
        public DelegateStringUpdate m_DelegateStringUpdate;

        public DelegateThreadException m_DelegateThreadException;
        public DelegateThreadFinished m_DelegateThreadFinished;
        private ManualResetEvent m_EventStopThread;
        private string m_find = string.Empty;
        private LyricsController m_lc;
        private Thread m_LyricControllerThread;
        private MusicDatabase m_mDB = null;

        private int m_NO_OF_SEARCHES_ALLOWED = 5;
        private int m_noOfCurrentlySearches = 0;
        private int m_noOfSearchesCompleted = 0;
        private int m_noOfSearchesToComplete = 0;
        private MyLyricsSetup m_Parent;
        private string m_replace = string.Empty;

        private bool m_Searching = false;
        private string m_SelectedArtist = String.Empty;

        private Queue<string[]> m_Songs = null;
        private string[] m_strippedPrefixStrings = null;

        public MusicDatabaseBrowse(MyLyricsSetup parent)
        {
            m_Parent = parent;
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
                        Invoke(m_DelegateLyricFound, value);
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
                        Invoke(m_DelegateLyricNotFound, value);
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

                    DatabaseUtil.WriteToLyricsDatabase(MyLyricsSettings.LyricsDB, MyLyricsSettings.LyricsMarkedDB,
                                                       capArtist, capTitle, lyricStrings, site);
                    DatabaseUtil.SerializeLyricDB();

                    if (!site.Equals("music tag"))
                    {
                        using (Settings xmlreader = new Settings("MediaPortal.xml"))
                        {
                            if (xmlreader.GetValueAsBool("myLyrics", "automaticWriteToMusicTag", true))
                            {
                                TagReaderUtil.WriteLyrics(capArtist, capTitle, lyricStrings);
                            }
                        }
                    }

                    m_noOfCurrentlySearches -= 1;
                    ++m_noOfSearchesCompleted;

                    break;
                }
            }

            //lvSelectedSongs.Update();
        }

        private void lyricNotFoundMethod(String artist, String title, String message, String site)
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
                        DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, capArtist, capTitle).
                            Equals(DatabaseUtil.LYRIC_NOT_FOUND))
                    {
                        MyLyricsSettings.LyricsMarkedDB.Add(DatabaseUtil.CorrectKeyFormat(capArtist, capTitle),
                                                            new LyricsItem(capArtist, capTitle, "", ""));
                    }

                    DatabaseUtil.SerializeLyricMarkedDB();
                    m_noOfCurrentlySearches -= 1;
                    ++m_noOfSearchesCompleted;

                    break;
                }
            }

            //lvSelectedSongs.Update();
        }


        // Set initial state of controls.
        // Called from worker thread using delegate and Control.Invoke
        private void ThreadFinishedMethod(string artist, string title, string message, string site)
        {
            m_Searching = false;

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
            m_mDB = MusicDatabase.Instance;
            ArrayList artists = new ArrayList();
            //mdb.GetArtists(0, String.Empty, ref artists);
            m_mDB.GetAllArtists(ref artists);

            lvArtists.Items.Clear();
            lbSelectedArtist.Text = String.Empty;

            m_DelegateStringUpdate = new DelegateStringUpdate(updateStringMethod);
            m_DelegateStatusUpdate = new DelegateStatusUpdate(updateStatusMethod);
            m_DelegateLyricFound = new DelegateLyricFound(lyricFoundMethod);
            m_DelegateLyricNotFound = new DelegateLyricNotFound(lyricNotFoundMethod);
            m_DelegateThreadFinished = new DelegateThreadFinished(ThreadFinishedMethod);
            m_DelegateThreadException = new DelegateThreadException(ThreadExceptionMethod);

            foreach (object artist in artists)
            {
                ListViewItem lvi = new ListViewItem((string) artist);
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
                ListViewItem newLvi = new ListViewItem(m_SelectedArtist);

                foreach (ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
                {
                    newLvi.SubItems.Add(lvsi);
                }

                newLvi.ImageIndex = lvi.ImageIndex;

                bool alreadySelected = false;

                foreach (ListViewItem item in lvSelectedSongs.Items)
                {
                    if (item.SubItems[0].Text.Equals(m_SelectedArtist) &&
                        item.SubItems[1].Text.Equals(newLvi.SubItems[1].Text))
                    {
                        alreadySelected = true;
                        continue;
                    }
                }

                if (!alreadySelected &&
                    !lvSelectedSongs.Items.ContainsKey(m_SelectedArtist + "-" + lvi.SubItems[0].Text))
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
            m_SelectedArtist = ((ListView) (sender)).SelectedItems.Count > 0
                                   ? ((ListView) (sender)).SelectedItems[0].Text
                                   : "";
            mdb.GetSongsByArtist(m_SelectedArtist, ref songs);

            lbSelectedArtist.Text = String.Format("Artist: {0}", m_SelectedArtist);
            lvSongs.Items.Clear();

            foreach (Song song in songs)
            {
                string capatalizedArtist = LyricUtil.CapatalizeString(song.Artist);
                string capatalizedTitle = LyricUtil.CapatalizeString(song.Title);

                ListViewItem lvi = new ListViewItem(capatalizedTitle);
                lvi.Tag = capatalizedTitle;

                int status = DatabaseUtil.IsSongInLyricsDatabase(MyLyricsSettings.LyricsDB, capatalizedArtist,
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
                            DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, capatalizedArtist,
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
            m_Searching = true;

            MyLyricsSetup.UpdateLibraryUI = true;
            ChangeButtonsEnableState();

            m_Songs = new Queue<string[]>();

            List<ListViewItem> items = new List<ListViewItem>();

            int count = 0;

            foreach (ListViewItem lvi in lvSelectedSongs.Items)
            {
                ListViewItem lviClone = (ListViewItem) lvi.Clone();
                if (lvi.ImageIndex == -1)
                {
                    m_Songs.Enqueue(new string[3]
                                        {lviClone.SubItems[0].Text, lviClone.SubItems[1].Text, count.ToString()});
                    lviClone.SubItems[3].Text = "";
                    items.Add(lviClone);
                    ++count;
                }
            }

            lvSelectedSongs.Items.Clear();
            lvSelectedSongs.Items.AddRange(items.ToArray());

            m_noOfSearchesToComplete = m_Songs.Count;
            m_noOfCurrentlySearches = 0;
            m_noOfSearchesCompleted = 0;
        }

        private void SearchOnline()
        {
            // After the tag search is completed, we now recalculate the number of lyrics to search for during online search
            m_noOfSearchesToComplete -= m_noOfSearchesCompleted;
            m_noOfSearchesCompleted = 0;
            m_noOfCurrentlySearches = 0;

            if (bwOnlineSearch.IsBusy)
            {
                Thread.Sleep(2000);
            }

            List<string> sitesToSearch = new List<string>();

            using (Settings xmlreader = new Settings("MediaPortal.xml"))
            {
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLrcFinder", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LrcFinder");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useActionext", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Actionext");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyrDB", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LyrDB");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyrics007", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Lyrics007");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useLyricsOnDemand", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("LyricsOnDemand");
                if (((string) xmlreader.GetValueAsString("myLyrics", "useHotLyrics", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("HotLyrics");
                if (((string)xmlreader.GetValueAsString("myLyrics", "useShironet", "True")).ToString().Equals("True"))
                    sitesToSearch.Add("Shironet");

                m_find = xmlreader.GetValueAsString("myLyrics", "find", "");
                m_replace = xmlreader.GetValueAsString("myLyrics", "replace", "");
            }

            m_strippedPrefixStrings = MediaPortalUtil.GetStrippedPrefixStringArray();

            m_EventStopThread = new ManualResetEvent(false);

            // If automaticUpdate is set then return after the first positive search
            m_lc = new LyricsController(this, m_EventStopThread, (string[]) sitesToSearch.ToArray(), false, false,
                                        m_find, m_replace);

            m_lc.NoOfLyricsToSearch = m_noOfSearchesToComplete;

            ThreadStart job = delegate { m_lc.Run(); };

            m_LyricControllerThread = new Thread(job);
            m_LyricControllerThread.Start();

            m_lc.StopSearches = false;

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
                        DatabaseUtil.IsSongInLyricsMarkedDatabase(MyLyricsSettings.LyricsMarkedDB, artist, title).Equals
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
            if (m_Searching == true)
            {
                m_Parent.tabControl.SelectedIndexChanged -= new EventHandler(m_Parent.tabControl_SelectedIndexChanged);

                ((MyLyricsSetup) m_Parent).btClose.Enabled = false;
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
                m_Parent.tabControl.SelectedIndexChanged += new EventHandler(m_Parent.tabControl_SelectedIndexChanged);

                ((MyLyricsSetup) m_Parent).btClose.Enabled = true;
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
            while (m_Songs.Count != 0)
            {
                if (m_lc == null)
                    return;

                if (bwOnlineSearch.CancellationPending)
                {
                    return;
                }

                if (m_noOfCurrentlySearches < m_NO_OF_SEARCHES_ALLOWED && m_lc.StopSearches == false)
                {
                    m_noOfCurrentlySearches += 1;
                    string[] lyricID = (string[]) m_Songs.Dequeue();
                    string artist = lyricID[0];
                    string title = lyricID[1];
                    int rowNumberInListView = int.Parse(lyricID[2]);

                    bwOnlineSearch.ReportProgress(rowNumberInListView);

                    m_lc.AddNewLyricSearch(artist, title,
                                           MediaPortalUtil.GetStrippedPrefixArtist(artist, m_strippedPrefixStrings));
                }

                Thread.Sleep(200);
            }
        }

        private bool LyricFoundInMusicTag(string artist, string title)
        {
            Song song = new Song();

            m_mDB.GetSongByMusicTagInfo(artist, string.Empty, title, true, ref song);

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
            if (m_lc != null)
            {
                m_lc.FinishThread(m_SelectedArtist, string.Empty, "The search has been cancelled by the user.", "none");
                m_lc.Dispose();
                m_lc = null;
            }
            else if (m_EventStopThread != null)
            {
                m_EventStopThread.Set();
                ThreadFinishedMethod(m_SelectedArtist, string.Empty, "The search has been cancelled by the user.",
                                     "none");
            }

            bwMusicTagSearch.CancelAsync();
            bwOnlineSearch.CancelAsync();
            ChangeButtonsEnableState();
            m_Searching = false;
            Thread.Sleep(500);
            RefreshSongsListView();
            RefreshArtistStats();
            ChangeStatusOnSubItems(true);
        }


        private void bwMusicTagSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            Queue<string[]> m_SongsToSearchOnline = new Queue<string[]>();
            int rowNumberInListView = 0;

            foreach (string[] song in m_Songs)
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

            m_Songs = m_SongsToSearchOnline;
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
            if (m_noOfSearchesCompleted == m_noOfSearchesToComplete)
            {
                m_Searching = false;
                ChangeButtonsEnableState();
                return;
            }

            SearchOnline();
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            using (Settings xmlreader = new Settings("MediaPortal.xml"))
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