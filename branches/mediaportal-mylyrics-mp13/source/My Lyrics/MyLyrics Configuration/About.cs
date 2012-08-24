using System.Diagnostics;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class Information : UserControl
    {
        private Form parent;

        public Information(Form parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void linkLabelForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "iexplore";
            proc.StartInfo.Arguments = "http://forum.team-mediaportal.com/my_lyrics_plugin-f163.html";
            proc.Start();
        }
    }
}