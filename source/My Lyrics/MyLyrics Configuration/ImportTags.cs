using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using LyricsEngine;
using LyricsEngine.LRC;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MyLyrics
{
    public partial class ImportTags : Form
    {
        private ArrayList _artists;
        private List<Song> _songs;
        private List<MusicTag> _tags;
        private int _totalSongsSearched;
        private int _totalSongsToSearch;

        public ImportTags()
        {
            InitializeComponent();
            lbInfo.Text = "Press the 'Start'-button to begin the import";
            lbCurrentArtist.Text = string.Empty;
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            lbInfo.Text = string.Format("Currently checking music tags for:");

            btStart.Enabled = false;
            btCancel.Enabled = true;
            btClose.Enabled = false;

            var mDB = MusicDatabase.Instance;
            _tags = new List<MusicTag>();
            _artists = new ArrayList();
            _songs = new List<Song>();
            //mDB.GetArtists(0, "", ref artists);
            mDB.GetAllArtists(ref _artists);
            _artists.Sort();

            progressBar.ResetText();
            progressBar.Enabled = true;
            progressBar.Value = 0;
            progressBar.Maximum = _artists.Count;
            _totalSongsToSearch = mDB.GetTotalSongs();
            lbTotalSongs2.Text = _totalSongsToSearch.ToString(CultureInfo.InvariantCulture);

            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var mDB = MusicDatabase.Instance;
            var counter = 0;

            for (var i = 0; i < _artists.Count; i++)
            {
                if (bw.CancellationPending)
                {
                    return;
                }

                var artist = (string) _artists[i];
                bw.ReportProgress(counter, artist);

                mDB.GetSongsByArtist(artist, ref _songs);

                foreach (var song in _songs)
                {
                    var tag = TagReader.ReadTag(song.FileName);
                    if (tag != null && tag.Lyrics != string.Empty)
                    {
                        var capArtist = LyricUtil.CapatalizeString(tag.Artist);
                        var capTitle = LyricUtil.CapatalizeString(tag.Title);

                        if (
                            DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, capArtist, capTitle).Equals(
                                DatabaseUtil.LyricFound))
                        {
                            // If lyric exists in LyricsDb then only import (and overwrite) if it isn't an LRC-file
                            var lyricsText = MyLyricsUtils.LyricsDB[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)].Lyrics;
                            var lrc = new SimpleLRC(capArtist, capTitle, lyricsText);
                            if (!lrc.IsValid)
                            {
                                _tags.Add(tag);
                                ++counter;
                            }
                        }
                        else
                        {
                            _tags.Add(tag);
                            ++counter;
                        }
                    }
                }
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var mDB = MusicDatabase.Instance;
            var artist = e.UserState as string;
            var songs = new List<Song>();
            mDB.GetSongsByArtist(artist, ref songs);
            _totalSongsSearched += songs.Count;
            lbCurrentArtist.Text = artist;
            progressBar.PerformStep();
            lbLyricsFound2.Text = e.ProgressPercentage.ToString(CultureInfo.InvariantCulture);
            lbSongsToSearch2.Text = string.Format("{0}", _totalSongsToSearch - _totalSongsSearched);
        }


        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbSongsToSearch2.Text = "0";
            if (_tags.Count > 0)
            {
                var dlgResult =
                    MessageBox.Show(
                        string.Format(
                            "{0} lyric were found in the search.{1}Do you want to import these into your lyrics database?",
                            _tags.Count, Environment.NewLine), "Import tags", MessageBoxButtons.YesNo);
                if (dlgResult.Equals(DialogResult.Yes))
                {
                    foreach (var tag in _tags)
                    {
                        var capArtist = LyricUtil.CapatalizeString(tag.Artist);
                        var capTitle = LyricUtil.CapatalizeString(tag.Title);
                        DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsUtils.LyricsDB, capArtist, capTitle, tag.Lyrics,
                                                             "music tag");
                    }
                    DatabaseUtil.SerializeLyricDB();
                }
            }

            lbInfo.Text = "The search has ended.";
            lbCurrentArtist.Text = string.Empty;

            btStart.Enabled = true;
            btCancel.Enabled = false;
            btClose.Enabled = true;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            progressBar.ResetText();
            progressBar.Value = 0;
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}