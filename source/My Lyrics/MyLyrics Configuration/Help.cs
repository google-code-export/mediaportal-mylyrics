using System.Diagnostics;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class Help : UserControl
    {
        private Form parent;

        public Help(Form parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void linkLabelForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://forum.team-mediaportal.com/forums/my-lyrics-plugin.163/");
        }
    }
}