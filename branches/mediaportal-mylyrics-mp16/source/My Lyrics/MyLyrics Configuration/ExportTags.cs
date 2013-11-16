using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using LyricsEngine;
using LyricsEngine.LRC;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MyLyrics
{
    public partial class ExportTags : Form
    {
        private ArrayList _artists;
        private List<Song> _songs;
        private int _totalSongsSearched;
        private int _totalSongsToSearch;

        public ExportTags()
        {
            InitializeComponent();
            lbInfo.Text = "Press the 'Start'-button to begin the export.";
            lbCurrentArtist.Text = "Note that the export cannot be undone.";
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            lbInfo.Text = string.Format("Currently exporting lyrics to musictags with:");

            btStart.Enabled = false;
            btCancel.Enabled = true;
            btClose.Enabled = false;

            var mDB = MusicDatabase.Instance;
            _artists = new ArrayList();
            _songs = new List<Song>();
            mDB.GetAllArtists(ref _artists);
            //mDB.GetArtists(0, "", ref artists);
            //mDB.Get

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
                Thread.Sleep(100); // Give the GUI thread some time to update
                bw.ReportProgress(counter, artist);
                try
                {
                    mDB.GetSongsByArtist(artist, ref _songs);

                    foreach (var song in _songs)
                    {
                        var capArtist = LyricUtil.CapatalizeString(song.Artist);
                        var capTitle = LyricUtil.CapatalizeString(song.Title);

                        if (DatabaseUtil.IsSongInLyricsDatabase(MyLyricsUtils.LyricsDB, capArtist, capTitle).Equals(DatabaseUtil.LyricFound))
                        {
                            var lyric = MyLyricsUtils.LyricsDB[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)].Lyrics;
                            var lrcInLyricsDb = new SimpleLRC(capArtist, capTitle, lyric);

                            // If the lyricsDB lyric is LRC always export
                            if (lrcInLyricsDb.IsValid)
                            {
                                if (TagReaderUtil.WriteLyrics(song.FileName, lyric))
                                {
                                    ++counter;
                                }
                                continue;
                            }

                            var tag = TagReader.ReadTag(song.FileName);

                            // If there is a musictag lyric
                            if (tag != null && !tag.Lyrics.Equals(string.Empty))
                            {
                                // if there is no LRC lyric in the tag, then simple export
                                var lrcInTag = new SimpleLRC(capArtist, capTitle, tag.Lyrics);
                                if (!lrcInTag.IsValid)
                                {
                                    if (TagReaderUtil.WriteLyrics(song.FileName, lyric))
                                    {
                                        ++counter;
                                    }
                                }
                            }
                                // Al if no lyric in musictag simple export
                            else
                            {
                                if (TagReaderUtil.WriteLyrics(song.FileName, lyric))
                                {
                                    ++counter;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    ;
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

            lbInfo.Text = "The export has ended.";
            lbCurrentArtist.Text = string.Empty;

            progressBar.PerformStep();

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