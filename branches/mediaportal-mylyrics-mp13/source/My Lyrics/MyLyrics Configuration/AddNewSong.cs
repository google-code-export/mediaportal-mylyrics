using System;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class AddNewSong : Form
    {
        private LyricsLibrary parent;

        public AddNewSong(LyricsLibrary parent)
        {
            InitializeComponent();
            this.parent = parent;
            ShowDialog();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            parent.addNewSongToDatabase(tbArtist.Text, tbTitle.Text, tbLyric.Text);
            Close();
        }

        private void tbArtist_KeyUp(object sender, KeyEventArgs e)
        {
            validateText();
        }

        private void tbTitle_KeyUp(object sender, KeyEventArgs e)
        {
            validateText();
        }

        private void tbLyric_KeyUp(object sender, KeyEventArgs e)
        {
            validateText();
        }

        private void validateText()
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