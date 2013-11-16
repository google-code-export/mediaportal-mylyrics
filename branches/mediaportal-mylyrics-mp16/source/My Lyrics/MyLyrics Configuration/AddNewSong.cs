using System;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class AddNewSong : Form
    {
        private readonly LyricsLibrary _parentLyricsLibrary;

        public AddNewSong(LyricsLibrary parentLyricsLibrary)
        {
            InitializeComponent();
            _parentLyricsLibrary = parentLyricsLibrary;
            ShowDialog();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            _parentLyricsLibrary.AddNewSongToDatabase(tbArtist.Text, tbTitle.Text, tbLyric.Text);
            Close();
        }

        private void tbArtist_KeyUp(object sender, KeyEventArgs e)
        {
            ValidateText();
        }

        private void tbTitle_KeyUp(object sender, KeyEventArgs e)
        {
            ValidateText();
        }

        private void tbLyric_KeyUp(object sender, KeyEventArgs e)
        {
            ValidateText();
        }

        private void ValidateText()
        {
            if (tbArtist.Text.Length != 0 && tbTitle.Text.Length != 0 && tbLyric.Text.Length != 0)
                btOK.Enabled = true;
            else
                btOK.Enabled = false;
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}