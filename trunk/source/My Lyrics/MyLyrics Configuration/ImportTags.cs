using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using LyricsEngine;
using LyricsEngine.LRC;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MyLyrics
{
    public partial class ImportTags : Form
    {
        private ArrayList artists;
        private List<Song> songs;
        private List<MusicTag> tags;
        private int totalSongsSearched;
        private int totalSongsToSearch;

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

            MusicDatabase mDB = MusicDatabase.Instance;
            tags = new List<MusicTag>();
            artists = new ArrayList();
            songs = new List<Song>();
            //mDB.GetArtists(0, "", ref artists);
            mDB.GetAllArtists(ref artists);
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
                bw.ReportProgress(counter, artist);

                mDB.GetSongsByArtist(artist, ref songs);

                foreach (Song song in songs)
                {
                    MusicTag tag = TagReader.ReadTag(song.FileName);
                    if (tag != null && tag.Lyrics != string.Empty)
                    {
                        string capArtist = LyricUtil.CapatalizeString(tag.Artist);
                        string capTitle = LyricUtil.CapatalizeString(tag.Title);

                        if (
                            DatabaseUtil.IsSongInLyricsDatabase(MyLyricsSettings.LyricsDB, capArtist, capTitle).Equals(
                                DatabaseUtil.LYRIC_FOUND))
                        {
                            // If lyric exists in LyricsDb then only import (and overwrite) if it isn't an LRC-file
                            string lyricsText =
                                (string)
                                MyLyricsSettings.LyricsDB[DatabaseUtil.CorrectKeyFormat(capArtist, capTitle)].Lyrics;
                            SimpleLRC lrc = new SimpleLRC(capArtist, capTitle, lyricsText);
                            if (!lrc.IsValid)
                            {
                                tags.Add(tag);
                                ++counter;
                            }
                        }
                        else
                        {
                            tags.Add(tag);
                            ++counter;
                        }
                    }
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
            if (tags.Count > 0)
            {
                DialogResult dlgResult =
                    MessageBox.Show(
                        string.Format(
                            "{0} lyric were found in the search.{1}Do you want to import these into your lyrics database?",
                            tags.Count, Environment.NewLine), "Import tags", MessageBoxButtons.YesNo);
                if (dlgResult.Equals(DialogResult.Yes))
                {
                    foreach (MusicTag tag in tags)
                    {
                        string capArtist = LyricUtil.CapatalizeString(tag.Artist);
                        string capTitle = LyricUtil.CapatalizeString(tag.Title);
                        DatabaseUtil.ReplaceInLyricsDatabase(MyLyricsSettings.LyricsDB, capArtist, capTitle, tag.Lyrics,
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