using System.Diagnostics;
using System.Windows.Forms;
using LyricsEngine.LyricsSites;

namespace MyLyrics
{
    public partial class Information : UserControl
    {
        private Form _parent;

        public Information(Form parent)
        {
            _parent = parent;
            InitializeComponent();
            InitSitesList();
        }

        private void InitSitesList()
        {
            supportedSites.AutoSize = true;

            var lyricsSitesNames = LyricsSiteFactory.LyricsSitesNames();

            foreach (var site in lyricsSitesNames)
            {
                supportedSites.Rows.Add();
                var rowIndex = supportedSites.RowCount - 1;
                var row = supportedSites.Rows[rowIndex];
                row.Cells["Site"].Value = site;
                row.Cells["Url"].Value = LyricsSiteFactory.GetBaseUrlFromSiteName(site);
            }
            supportedSites.Refresh();
        }

        private void linkLabelForum_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://forum.team-mediaportal.com/forums/my-lyrics-plugin.163/");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                Process.Start((string)supportedSites.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
        }
    }
}