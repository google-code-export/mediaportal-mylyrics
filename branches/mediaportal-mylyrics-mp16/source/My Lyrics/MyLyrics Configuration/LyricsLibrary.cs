using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using LyricsEngine;
using LyricsEngine.LRC;
using LyricsEngine.LyricsSites;
using MediaPortal.Configuration;
using MyLyrics.XmlSettings;

namespace MyLyrics
{
    public partial class LyricsLibrary : UserControl
    {
        internal static LyricsDatabase CurrentLyricsDatabase;
        private static string _mCurrentArtist = "";
        private static string _mCurrentTitle = "";
        private int _currentDbIndex;
        private SimpleLRC _lrc;

        private int _mNoOfArtists;
        private int _mNoOfTitles;

        private bool _mOnlyShowLRCs;
        private string _mOriginalLyric = "";

        private readonly MyLyricsSetup _parentMyLyricsSetup;


        public LyricsLibrary(Form form)
        {
            _parentMyLyricsSetup = form as MyLyricsSetup;
            InitializeComponent();
            comboDatabase.SelectedIndex = 0;
            UpdateLyricsTree(false);
        }

        internal void UpdateLyricsTree(bool onlyShowLRCs)
        {
            _mOnlyShowLRCs = onlyShowLRCs;

            if (CurrentLyricsDatabase == null)
            {
                return;
            }

            try
            {
                treeView.Nodes.Clear();
                _mNoOfArtists = 0;
                _mNoOfTitles = 0;

                if (onlyShowLRCs)
                {
                    foreach (var kvp in CurrentLyricsDatabase)
                    {
                        if (IsSelectedLyricLRC(kvp.Value.Artist, kvp.Value.Title))
                        {
                            try
                            {
                                AddSong(kvp.Value);
                            }
                            catch
                            {
                                ;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var kvp in CurrentLyricsDatabase)
                    {
                        try
                        {
                            AddSong(kvp.Value);
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
            }
            finally
            {
                treeView.Sort();
                UpdateLyricDatabaseStats();
            }
        }

        internal void UpdateLyricDatabaseStats()
        {
            lbArtists2.Text = _mNoOfArtists.ToString(CultureInfo.InvariantCulture);
            lbSongs2.Text = _mNoOfTitles.ToString(CultureInfo.InvariantCulture);
        }

        private void ResetFields()
        {
            lbArtists2.Text = "";
            lbTitle.Text = "";
            lbLRCTest.Text = "";
            lbSongs2.Text = "";
            tbLyrics.Text = "";
        }

        private static ArrayList GetTitlesByArtist(string artist)
        {
            var titles = new ArrayList();

            foreach (var kvp in CurrentLyricsDatabase)
            {
                if (kvp.Value.Artist.Equals(artist))
                {
                    titles.Add(kvp.Value.Title);
                }
            }
            return titles;
        }

        /// <summary>
        /// AddSong adds a lyric to the treeView and not to the lyric database
        /// </summary>
        /// <param name="lyricsItem">lyrics item</param>
        private void AddSong(LyricsItem lyricsItem)
        {
            var artist = LyricUtil.CapatalizeString(lyricsItem.Artist);
            var title = LyricUtil.CapatalizeString(lyricsItem.Title);

            // add artist, if it doesn't exists
            if (!treeView.Nodes.ContainsKey(artist))
            {
                treeView.Nodes.Add(artist, artist);
                ++_mNoOfArtists;
            }

            // add title, if it doesn't exists
            var artistIndex = treeView.Nodes.IndexOfKey(artist);
            if (artistIndex >= 0 && !treeView.Nodes[artistIndex].Nodes.ContainsKey(title))
            {
                treeView.Nodes[artistIndex].Nodes.Add(title, title);
                treeView.Nodes[artistIndex].Nodes[treeView.Nodes[artistIndex].Nodes.Count - 1].Tag = lyricsItem;
                ++_mNoOfTitles;
            }
        }

        /// <summary>
        /// AddSong with tree parameters adds a lyric to the treeView and the lyric database
        /// </summary>
        /// <param name="artist">artist</param>
        /// <param name="title">title</param>
        /// <param name="lyrics">lyircs</param>
        /// <param name="site">site</param>
        private bool AddSong(string artist, string title, string lyrics, string site)
        {
            var item = new LyricsItem(artist, title, lyrics, site);

            if (DatabaseUtil.IsSongInLyricsDatabase(CurrentLyricsDatabase, artist, title).Equals(DatabaseUtil.LyricNotFound))
            {
                CurrentLyricsDatabase.Add(DatabaseUtil.CorrectKeyFormat(artist, title), item);
                try
                {
                    AddSong(item);
                }
                catch
                {
                    ;
                }
                treeView.Update();
                DatabaseUtil.SerializeDB(CurrentLyricsDatabase);

                if (SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, true))
                {
                    TagReaderUtil.WriteLyrics(artist, title, lyrics);
                }

                return true;
            }
            return false;
        }

        public void RemoveSong(string artist, string title, bool serializeDB)
        {
            try
            {
                var artistIndex = treeView.Nodes.IndexOfKey(artist);
                var titleIndex = treeView.Nodes[artistIndex].Nodes.IndexOfKey(title);
                treeView.Nodes[artistIndex].Nodes.RemoveAt(titleIndex);
                --_mNoOfTitles;

                // remove title from treeView
                if (treeView.Nodes[artistIndex].Nodes.Count == 0)
                {
                    treeView.Nodes.RemoveAt(artistIndex);
                    --_mNoOfArtists;
                }
            }
            catch
            {
                if (artist.Length == 0 && title.Length == 0)
                {
                    treeView.Nodes.RemoveAt(0);
                    --_mNoOfArtists;
                    treeView.Update();
                }
            }
            finally
            {
                treeView.Update();

                // remove title from database
                CurrentLyricsDatabase.Remove(DatabaseUtil.CorrectKeyFormat(artist, title));

                if (serializeDB)
                {
                    DatabaseUtil.SerializeDB(CurrentLyricsDatabase);
                }
            }
        }

        private static bool IsSelectedLyricLRC(string artist, string title)
        {
            if (artist.Length != 0 && title.Length != 0)
            {
                var lyricsText = CurrentLyricsDatabase[DatabaseUtil.CorrectKeyFormat(artist, title)].Lyrics;

                var lrc = new SimpleLRC(artist, title, lyricsText);
                if (lrc.IsValid)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public void UpdateInfo()
        {
            _mCurrentArtist = "";
            _mCurrentTitle = "";
            tbLyrics.Text = "";
            lbTitle.Text = "";
            lbLRCTest.Text = "";

            tbLyrics.Enabled = false;

            // Selected a title
            if (treeView.SelectedNode != null && treeView.SelectedNode.Parent != null)
            {
                var artist = treeView.SelectedNode.Parent.Text;
                var title = treeView.SelectedNode.Text;

                if (artist.Length != 0 && title.Length != 0)
                {
                    _mCurrentArtist = LyricUtil.CapatalizeString(artist);
                    _mCurrentTitle = LyricUtil.CapatalizeString(title);

                    if (
                        DatabaseUtil.IsSongInLyricsDatabase(CurrentLyricsDatabase, _mCurrentArtist, _mCurrentTitle).Equals(DatabaseUtil.LyricFound))
                    {
                        var item = CurrentLyricsDatabase[DatabaseUtil.CorrectKeyFormat(_mCurrentArtist, _mCurrentTitle)];
                        var lyricsText = item.Lyrics;

                        lyricsText = LyricUtil.ReturnEnvironmentNewLine(lyricsText);

                        _mOriginalLyric = lyricsText;
                        tbLyrics.Text = _mOriginalLyric;
                        tbLyrics.Enabled = true;

                        lbTitle.Text = "\"" + _mCurrentArtist + " - " + _mCurrentTitle + "\"" + (!item.Source.Equals("") ? " found at " + item.Source : "");

                        btSearchSingle.Enabled = true;
                    }
                }
            }
                // Selected an artist
            else if (treeView.SelectedNode != null)
            {
                var artist = treeView.SelectedNode.Text;
                _mCurrentArtist = LyricUtil.CapatalizeString(artist);
                btSearchSingle.Enabled = false;
            }
            else
            {
                btSearchSingle.Enabled = false;
            }
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                /*if(components != null)
                        {
                            components.Dispose();
                        }*/
            }
            base.Dispose(disposing);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            var capArtist = LyricUtil.CapatalizeString(_mCurrentArtist);
            var capTitle = LyricUtil.CapatalizeString(_mCurrentTitle);

            var lyrics = LyricUtil.FixLyrics(tbLyrics.Text);

            CurrentLyricsDatabase[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)].Lyrics = lyrics;
            DatabaseUtil.SerializeDB(CurrentLyricsDatabase);
            
            if (SettingManager.GetParamAsBool(SettingManager.AutomaticWriteToMusicTag, true))
            {
                TagReaderUtil.WriteLyrics(capArtist, capTitle, lyrics);
            }

            if (CurrentLyricsDatabase.Equals(MyLyricsUtils.LyricsMarkedDB))
            {
                if (SettingManager.GetParamAsBool(SettingManager.MoveLyricFromMarkedDatabase, true))
                {
                    MoveLyricToOtherDatabase();
                }
            }

            var lrc = new SimpleLRC(capArtist, capTitle, lyrics);
            if (lrc.IsValid && _parentMyLyricsSetup.cbUploadLrcAutomatically.Checked)
            {
                LrcFinder.SaveLrcWithGuid(lyrics, _parentMyLyricsSetup.MGuid);
            }

            btSave.Enabled = false;
            treeView.Focus();
        }

        private void tbLyrics_KeyUp(object sender, KeyEventArgs e)
        {
            if (_mOriginalLyric.Equals(tbLyrics.Text) || _mCurrentTitle.Length == 0)
                btSave.Enabled = false;
            else
                btSave.Enabled = true;
        }


        private void btDelete_Click(object sender, EventArgs e)
        {
            if (_mCurrentTitle.Length == 0)
            {
                var titles = GetTitlesByArtist(_mCurrentArtist);
                if (titles != null)
                {
                    for (var i = 0; i < titles.Count; i++)
                    {
                        RemoveSong(_mCurrentArtist, (string) titles[i], i == titles.Count - 1);
                    }
                }
            }
            else
            {
                RemoveSong(_mCurrentArtist, _mCurrentTitle, true);
                HighlightSong(_mCurrentArtist, _mCurrentTitle, true);
            }
            UpdateLyricDatabaseStats();
            treeView.Focus();
            treeView.Select();
        }


        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateInfo();

            if (treeView.SelectedNode != null && treeView.SelectedNode.Parent != null)
            {
                var title = treeView.SelectedNode.Text;
                _mCurrentArtist = treeView.SelectedNode.Parent.Text;
                _mCurrentTitle = LyricUtil.CapatalizeString(title);

                if (IsSelectedLyricLRC(_mCurrentArtist, title))
                {
                    lbLRCTest.Text = "(valid LRC)";
                }
                else
                {
                    lbLRCTest.Text = "";
                }
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            new AddNewSong(this);
            UpdateLyricDatabaseStats();
        }

        internal void AddNewSongToDatabase(string artist, string title, string lyrics)
        {
            _mCurrentArtist = LyricUtil.CapatalizeString(artist);
            _mCurrentTitle = LyricUtil.CapatalizeString(title);

            if (AddSong(_mCurrentArtist, _mCurrentTitle, lyrics, "Manual added"))
            {
                HighlightSong(_mCurrentArtist, _mCurrentTitle, false);
                UpdateLyricDatabaseStats();
            }
            else
            {
                MessageBox.Show("The title \"" + artist + " - " + title + "\" is already in the database");
            }
        }

        internal void HighlightSong(string artist, string title, bool previousSong)
        {
            if (artist.Length == 0 || title.Length == 0)
            {
                return;
            }

            treeView.Select();
            treeView.Focus();

            var artistIndex = treeView.Nodes.IndexOfKey(artist);
            var artistNode = treeView.Nodes[artistIndex];

            if (title.Length == 0)
            {
                treeView.SelectedNode = artistNode;
                return;
            }

            var titleIndex = artistNode.Nodes.IndexOfKey(title);
            if (previousSong && titleIndex > 0)
                titleIndex -= 1;
            treeView.SelectedNode = artistNode.Nodes[titleIndex];
        }

        internal void HighlightNextSong(int artistIndex, int titleIndex)
        {
            var index = 0;
            var artistNode = treeView.Nodes[artistIndex];
            var nextSongIndex = titleIndex + 1;

            if (artistNode.Nodes.Count > nextSongIndex)
            {
                index = nextSongIndex;
            }
            else
            {
                var nextArtistIndex = ++artistIndex;
                if (treeView.Nodes.Count > nextArtistIndex)
                {
                    artistNode = treeView.Nodes[nextArtistIndex];
                    index = 0;
                }
            }

            treeView.SelectedNode = artistNode.Nodes[index];
        }

        private void MoveLyricToOtherDatabase()
        {
            var artist = "";
            var title = "";

            if (treeView.SelectedNode != null)
            {
                var temp = treeView.SelectedNode.Text;

                if (treeView.SelectedNode.Parent != null)
                {
                    artist = treeView.SelectedNode.Parent.Text;
                    title = temp;
                }
                else
                {
                    artist = temp;
                }
            }

            if (artist.Length == 0 && title.Length == 0)
            {
                MessageBox.Show("No artist or track selected");
            }
            else if (title.Length == 0)
            {
                var artistNode = treeView.SelectedNode;

                LyricsDatabase otherDatabase;
                if (CurrentLyricsDatabase.Equals(MyLyricsUtils.LyricsDB))
                {
                    otherDatabase = MyLyricsUtils.LyricsMarkedDB;
                }
                else
                {
                    otherDatabase = MyLyricsUtils.LyricsDB;
                }

                foreach (TreeNode node in artistNode.Nodes)
                {
                    var key = DatabaseUtil.CorrectKeyFormat(artist, node.Text);
                    var item = CurrentLyricsDatabase[key];
                    CurrentLyricsDatabase.Remove(key);

                    if (!DatabaseUtil.IsSongInLyricsDatabase(otherDatabase, artist, item.Title).Equals(DatabaseUtil.LyricNotFound))
                    {
                        otherDatabase.Add(key, item);
                    }
                    else
                    {
                        otherDatabase[key] = item;
                    }
                }
                UpdateLyricsTree(false);
                DatabaseUtil.SerializeDBs();
            }
            else
            {
                var key = DatabaseUtil.CorrectKeyFormat(artist, title);
                var item = CurrentLyricsDatabase[key];

                // remove song from treeview and current database
                RemoveSong(artist, title, true);

                // add song to other database and serialize it
                if (CurrentLyricsDatabase.Equals(MyLyricsUtils.LyricsDB))
                {
                    MyLyricsUtils.LyricsMarkedDB.Add(key, item);
                    DatabaseUtil.SerializeDB(MyLyricsUtils.LyricsMarkedDB);
                }
                else
                {
                    MyLyricsUtils.LyricsDB.Add(key, item);
                    DatabaseUtil.SerializeDB(MyLyricsUtils.LyricsDB);
                }
                UpdateLyricDatabaseStats();
            }

            treeView.Focus();
        }


        private void btImportSingle_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var files = openFileDialog1.FileNames;
                    for (var i = 0; i < files.Length; i++)
                    {
                        var fileName = new FileInfo(files[i]).Name;
                        if (fileName.Contains("-"))
                        {
                            if (InspectFileNameAndAddToDatabaseIfValidLyrics(files[i]) != (int) TypeOfLyrics.None)
                            {
                                UpdateLyricDatabaseStats();
                            }
                            else
                            {
                                MessageBox.Show("The title is already in the database");
                            }
                        }
                        else
                        {
                            MessageBox.Show("The file \"" + fileName + "\" does not have a valid filename ([Artist]-[Title].txt or *.lrc).");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private int InspectFileNameAndAddToDatabaseIfValidLyrics(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Extension.Equals(".txt"))
            {
                var fileName = fileInfo.Name;
                var index = fileName.IndexOf("-", StringComparison.Ordinal);
                var fileStringArtist = fileName.Substring(0, index);
                var fileStringTitle = fileName.Substring(index + 1);
                fileStringArtist = fileStringArtist.Trim();
                fileStringTitle = fileStringTitle.Trim();

                index = fileStringTitle.LastIndexOf('.');
                fileStringTitle = fileStringTitle.Substring(0, index);

                var textReader = new StreamReader(filePath);
                string line;
                var lyrics = "";

                while ((line = textReader.ReadLine()) != null)
                {
                    lyrics += line + Environment.NewLine;
                }
                lyrics = lyrics.Trim();
                textReader.Close();

                var capArtist = LyricUtil.CapatalizeString(fileStringArtist);
                var capTitle = LyricUtil.CapatalizeString(fileStringTitle);

                return AddSong(capArtist, capTitle, lyrics, "Text file") ? (int) TypeOfLyrics.Normal : (int) TypeOfLyrics.None;
            }
            
            _lrc = new SimpleLRC(filePath);

            if (_lrc.IsValid && _lrc.Artist.Length != 0 && _lrc.Title.Length != 0)
            {
                return AddSong(_lrc.Artist, _lrc.Title, _lrc.LyricAsLRC.Trim(), "LRC-file") ? (int) TypeOfLyrics.LRC : (int) TypeOfLyrics.None;
            }
            
            return (int) TypeOfLyrics.None;
        }


        private void btImportDIRS_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var path = folderBrowserDialog1.SelectedPath;

                    var numberOfLyrics = 0;
                    var numberOfLrcs = 0;
                    var numberOfExisting = 0;

                    var dirInfo = new DirectoryInfo(path);
                    var fileInfos = dirInfo.GetFiles("*-*.txt", SearchOption.AllDirectories);

                    foreach (var fileInfo in fileInfos)
                    {
                        int typeAdded;
                        if ((typeAdded = InspectFileNameAndAddToDatabaseIfValidLyrics(fileInfo.FullName)) == (int) TypeOfLyrics.Normal)
                        {
                            ++numberOfLyrics;
                        }
                        else if (typeAdded == (int) TypeOfLyrics.LRC)
                        {
                            ++numberOfLrcs;
                        }
                        else
                        {
                            ++numberOfExisting;
                        }
                    }

                    dirInfo = new DirectoryInfo(path);
                    fileInfos = dirInfo.GetFiles("*.lrc", SearchOption.AllDirectories);

                    foreach (var fileInfo in fileInfos)
                    {
                        int typeAdded;
                        if ((typeAdded = InspectFileNameAndAddToDatabaseIfValidLyrics(fileInfo.FullName)) == (int) TypeOfLyrics.Normal)
                        {
                            ++numberOfLyrics;
                        }
                        else if (typeAdded == (int) TypeOfLyrics.LRC)
                        {
                            ++numberOfLrcs;
                        }
                        else
                        {
                            ++numberOfExisting;
                        }
                    }


                    UpdateLyricDatabaseStats();

                    MessageBox.Show("Number of lyris added :" + (numberOfLrcs + numberOfLyrics + numberOfExisting) +
                                    Environment.NewLine
                                    + "Number of basic lyrics added:" + numberOfLyrics + Environment.NewLine
                                    + "Number of LRCs added: " + numberOfLrcs + Environment.NewLine
                                    + "Number of lyrics already in database: " + numberOfExisting);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        private void comboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDatabase.SelectedIndex == 0 && (_currentDbIndex != 0 || _mOnlyShowLRCs))
            {
                ResetFields();
                _currentDbIndex = 0;
                CurrentLyricsDatabase = MyLyricsUtils.LyricsDB;
                btImportFiles.Enabled = true;
                btImportDirs.Enabled = true;
                btImportTags.Enabled = true;
                btExportTags.Enabled = true;
                UpdateLyricsTree(false);
            }
            else if (comboDatabase.SelectedIndex == 1)
            {
                ResetFields();
                _currentDbIndex = 0;
                CurrentLyricsDatabase = MyLyricsUtils.LyricsDB;
                btImportFiles.Enabled = true;
                btImportDirs.Enabled = true;
                btImportTags.Enabled = false;
                btExportTags.Enabled = false;
                UpdateLyricsTree(true);
            }
            else if (comboDatabase.SelectedIndex == 2 && _currentDbIndex != 1)
            {
                ResetFields();
                _currentDbIndex = 1;
                CurrentLyricsDatabase = MyLyricsUtils.LyricsMarkedDB;
                btImportFiles.Enabled = false;
                btImportDirs.Enabled = false;
                btImportTags.Enabled = false;
                btExportTags.Enabled = false;
                UpdateLyricsTree(false);
            }
        }

        private void btResetDatabase_Click(object sender, EventArgs e)
        {
            if (
                MessageBox.Show(this, "Are you sure the Lyrics database should be deleted?", "Delete Lyrics database",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                var path = Config.GetFolder(Config.Dir.Database) + "\\" + MyLyricsUtils.LyricsDBName;
                var fs = new FileStream(path, FileMode.Create);
                var bf = new BinaryFormatter();
                MyLyricsUtils.LyricsDB = new LyricsDatabase();
                bf.Serialize(fs, MyLyricsUtils.LyricsDB);
                fs.Close();

                CurrentLyricsDatabase = MyLyricsUtils.LyricsDB;
                comboDatabase.SelectedIndex = 0;
                UpdateLyricsTree(false);
                UpdateInfo();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to delete the database with marked titles?",
                                "Delete title database", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                var path = Config.GetFolder(Config.Dir.Database) + "\\" + MyLyricsUtils.LyricsMarkedDBName;
                var fs = new FileStream(path, FileMode.Create);
                var bf = new BinaryFormatter();
                MyLyricsUtils.LyricsMarkedDB = new LyricsDatabase();
                bf.Serialize(fs, MyLyricsUtils.LyricsMarkedDB);
                fs.Close();

                CurrentLyricsDatabase = MyLyricsUtils.LyricsMarkedDB;
                comboDatabase.SelectedIndex = 2;
                UpdateLyricsTree(false);
                UpdateInfo();
            }
        }

        private void btSearchSingle_Click(object sender, EventArgs e)
        {
            var artist = "";
            var title = "";
            var treeArtistIndex = 0;
            var treeTitleIndex = 0;

            if (treeView.SelectedNode != null)
            {
                var temp = treeView.SelectedNode.Text;
                treeTitleIndex = treeView.SelectedNode.Index;

                if (treeView.SelectedNode.Parent != null)
                {
                    artist = treeView.SelectedNode.Parent.Text;
                    treeArtistIndex = treeView.SelectedNode.Parent.Index;
                    title = temp;
                }
                else
                {
                    artist = temp;
                }
            }

            new FindLyric(this, artist, title, CurrentLyricsDatabase.Equals(MyLyricsUtils.LyricsMarkedDB), treeArtistIndex, treeTitleIndex);
        }

        private void btSwitch_Click(object sender, EventArgs e)
        {
            MoveLyricToOtherDatabase();
        }

        private void btImportTags_Click(object sender, EventArgs e)
        {
            var importTags = new ImportTags();
            importTags.ShowDialog();
            UpdateLyricsTree(false);
        }

        private void btExportTags_Click(object sender, EventArgs e)
        {
            var exportTags = new ExportTags();
            exportTags.ShowDialog();
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            UpdateLyricsTree(_mOnlyShowLRCs);
        }

        #region Nested type: TYPEOFLYRICS

        private enum TypeOfLyrics
        {
            None,
            Normal,
            LRC
        } ;

        #endregion
    }
}