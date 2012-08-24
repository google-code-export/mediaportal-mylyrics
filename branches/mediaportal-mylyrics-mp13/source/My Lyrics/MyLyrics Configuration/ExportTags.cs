using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        private ArrayList artists;
        private List<Song> songs;
        private int totalSongsSearched;
        private int totalSongsToSearch;

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

            MusicDatabase mDB = MusicDatabase.Instance;
            artists = new ArrayList();
            songs = new List<Song>();
            mDB.GetAllArtists(ref artists);
            //mDB.GetArtists(0, "", ref artists);
            //mDB.Get

            artists.Sort();

            progressBar.ResetText();
            progressBar.Enabled = true;
            progressBar.Value = 0;
            progressBar.Maximum = artists.Count;
            totalSongsToSearch = mDB.GetTotalSongs();
            lbTotalSongs2.Text = totalSongsToSearch.ToString();

            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            MusicDatabase mDB = MusicDatabase.Instance;
            int counter = 0;

            for (int i = 0; i < artists.Count; i++)
            {
                if (bw.CancellationPending)
                {
                    return;
                }

                string artist = (string) artists[i];
                Thread.Sleep(100); // Give the GUI thread some time to update
                bw.ReportProgress(counter, artist);
                try
                {
                    mDB.GetSongsByArtist(artist, ref songs);

                    foreach (Song song in songs)
                    {
                        string capArtist = LyricUtil.CapatalizeString(song.Artist);
                        string capTitle = LyricUtil.CapatalizeString(song.Title);

                        if (
                            DatabaseUtil.IsSongInLyricsDatabase(MyLyricsSettings.LyricsDB, capArtist, capTitle).Equals(
                                DatabaseUtil.LYRIC_FOUND))
                        {
                            string lyric =
                                MyLyricsSettings.LyricsDB[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)].Lyrics;
                            SimpleLRC lrcInLyricsDb = new SimpleLRC(capArtist, capTitle, lyric);

                            // If the lyricsDB lyric is LRC always export
                            if (lrcInLyricsDb.IsValid)
                            {
                                if (TagReaderUtil.WriteLyrics(song.FileName, lyric))
                                {
                                    ++counter;
                                }
                                continue;
                            }

                            MusicTag tag = TagReader.ReadTag(song.FileName);

                            // If there is a musictag lyric
                            if (tag != null && !tag.Lyrics.Equals(string.Empty))
                            {
                                // if there is no LRC lyric in the tag, then simple export
                                SimpleLRC lrcInTag = new SimpleLRC(capArtist, capTitle, tag.Lyrics);
                                if (!lrcInTag.IsValid)
                                {
                                    if (TagReaderUtil.WriteLyrics(song.FileName, lyric))
                                    {
                                        ++counter;
                                    }
                                }

                                continue;
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
                catch (Exception e2)
                {
                    string s = e2.Message;
                }
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            MusicDatabase mDB = MusicDatabase.Instance;
            string artist = e.UserState as string;
            List<Song> songs = new List<Song>();
            mDB.GetSongsByArtist(artist, ref songs);
            totalSongsSearched += songs.Count;
            lbCurrentArtist.Text = artist;
            progressBar.PerformStep();
            lbLyricsFound2.Text = e.ProgressPercentage.ToString();
            lbSongsToSearch2.Text = string.Format("{0}", totalSongsToSearch - totalSongsSearched);
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