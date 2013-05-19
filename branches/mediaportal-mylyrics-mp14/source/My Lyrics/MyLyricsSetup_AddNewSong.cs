using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class MyLyricsSetup_AddNewSong : Form
    {

        MyLyricsSetup_LyricsLibrary parent;

        public MyLyricsSetup_AddNewSong(MyLyricsSetup_LyricsLibrary parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.Show();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            parent.addNewSongToDatabase(tbArtist.Text, tbTitle.Text, tbLyric.Text);
            this.Close();
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
    }
}