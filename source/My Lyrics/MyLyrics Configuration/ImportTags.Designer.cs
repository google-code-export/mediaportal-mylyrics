namespace MyLyrics
{
    partial class ImportTags
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbInfo = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.bw = new System.ComponentModel.BackgroundWorker();
            this.btStart = new System.Windows.Forms.Button();
            this.gbMusicDBSearchStats = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.lbSongsToSearch2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbSongsToSearch = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbTotalSongs2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbTotalSongs = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbLyricsFound2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbLyricsFound = new MediaPortal.UserInterface.Controls.MPLabel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btClose = new System.Windows.Forms.Button();
            this.lbCurrentArtist = new System.Windows.Forms.Label();
            this.gbMusicDBSearchStats.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbInfo
            // 
            this.lbInfo.AutoSize = true;
            this.lbInfo.Location = new System.Drawing.Point(10, 12);
            this.lbInfo.MinimumSize = new System.Drawing.Size(150, 13);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(150, 13);
            this.lbInfo.TabIndex = 0;
            this.lbInfo.Text = "[text]";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(13, 53);
            this.progressBar.Maximum = 1000;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(249, 23);
            this.progressBar.Step = 1;
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 1;
            // 
            // bw
            // 
            this.bw.WorkerReportsProgress = true;
            this.bw.WorkerSupportsCancellation = true;
            this.bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bw_DoWork);
            this.bw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bw_RunWorkerCompleted);
            this.bw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bw_ProgressChanged);
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(13, 163);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(66, 23);
            this.btStart.TabIndex = 2;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // gbMusicDBSearchStats
            // 
            this.gbMusicDBSearchStats.Controls.Add(this.lbSongsToSearch2);
            this.gbMusicDBSearchStats.Controls.Add(this.lbSongsToSearch);
            this.gbMusicDBSearchStats.Controls.Add(this.lbTotalSongs2);
            this.gbMusicDBSearchStats.Controls.Add(this.lbTotalSongs);
            this.gbMusicDBSearchStats.Controls.Add(this.lbLyricsFound2);
            this.gbMusicDBSearchStats.Controls.Add(this.lbLyricsFound);
            this.gbMusicDBSearchStats.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbMusicDBSearchStats.Location = new System.Drawing.Point(13, 86);
            this.gbMusicDBSearchStats.Name = "gbMusicDBSearchStats";
            this.gbMusicDBSearchStats.Size = new System.Drawing.Size(249, 71);
            this.gbMusicDBSearchStats.TabIndex = 13;
            this.gbMusicDBSearchStats.TabStop = false;
            this.gbMusicDBSearchStats.Text = "Search stats";
            // 
            // lbSongsToSearch2
            // 
            this.lbSongsToSearch2.Location = new System.Drawing.Point(173, 31);
            this.lbSongsToSearch2.Name = "lbSongsToSearch2";
            this.lbSongsToSearch2.Size = new System.Drawing.Size(65, 15);
            this.lbSongsToSearch2.TabIndex = 33;
            this.lbSongsToSearch2.Text = "-";
            // 
            // lbSongsToSearch
            // 
            this.lbSongsToSearch.Location = new System.Drawing.Point(5, 30);
            this.lbSongsToSearch.Name = "lbSongsToSearch";
            this.lbSongsToSearch.Size = new System.Drawing.Size(140, 16);
            this.lbSongsToSearch.TabIndex = 32;
            this.lbSongsToSearch.Text = "Songs left to search:";
            // 
            // lbTotalSongs2
            // 
            this.lbTotalSongs2.Location = new System.Drawing.Point(172, 15);
            this.lbTotalSongs2.Name = "lbTotalSongs2";
            this.lbTotalSongs2.Size = new System.Drawing.Size(65, 15);
            this.lbTotalSongs2.TabIndex = 21;
            this.lbTotalSongs2.Text = "-";
            // 
            // lbTotalSongs
            // 
            this.lbTotalSongs.Location = new System.Drawing.Point(4, 15);
            this.lbTotalSongs.Name = "lbTotalSongs";
            this.lbTotalSongs.Size = new System.Drawing.Size(91, 15);
            this.lbTotalSongs.TabIndex = 20;
            this.lbTotalSongs.Text = "Total songs:";
            // 
            // lbLyricsFound2
            // 
            this.lbLyricsFound2.Location = new System.Drawing.Point(173, 46);
            this.lbLyricsFound2.Name = "lbLyricsFound2";
            this.lbLyricsFound2.Size = new System.Drawing.Size(65, 15);
            this.lbLyricsFound2.TabIndex = 15;
            this.lbLyricsFound2.Text = "-";
            // 
            // lbLyricsFound
            // 
            this.lbLyricsFound.Location = new System.Drawing.Point(5, 46);
            this.lbLyricsFound.Name = "lbLyricsFound";
            this.lbLyricsFound.Size = new System.Drawing.Size(162, 15);
            this.lbLyricsFound.TabIndex = 14;
            this.lbLyricsFound.Text = "Lyrics to be imported:";
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(85, 163);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(66, 23);
            this.btCancel.TabIndex = 14;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point(196, 163);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(66, 23);
            this.btClose.TabIndex = 15;
            this.btClose.Text = "Close";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // lbCurrentArtist
            // 
            this.lbCurrentArtist.AutoSize = true;
            this.lbCurrentArtist.Location = new System.Drawing.Point(10, 29);
            this.lbCurrentArtist.MinimumSize = new System.Drawing.Size(150, 13);
            this.lbCurrentArtist.Name = "lbCurrentArtist";
            this.lbCurrentArtist.Size = new System.Drawing.Size(150, 13);
            this.lbCurrentArtist.TabIndex = 16;
            this.lbCurrentArtist.Text = "[current artist]";
            // 
            // ImportTags
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 193);
            this.ControlBox = false;
            this.Controls.Add(this.lbCurrentArtist);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.gbMusicDBSearchStats);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lbInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportTags";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import lyrics from music tags";
            this.gbMusicDBSearchStats.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbInfo;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.ComponentModel.BackgroundWorker bw;
        private System.Windows.Forms.Button btStart;
        private MediaPortal.UserInterface.Controls.MPGroupBox gbMusicDBSearchStats;
        private MediaPortal.UserInterface.Controls.MPLabel lbSongsToSearch2;
        private MediaPortal.UserInterface.Controls.MPLabel lbSongsToSearch;
        private MediaPortal.UserInterface.Controls.MPLabel lbTotalSongs2;
        private MediaPortal.UserInterface.Controls.MPLabel lbTotalSongs;
        private MediaPortal.UserInterface.Controls.MPLabel lbLyricsFound2;
        private MediaPortal.UserInterface.Controls.MPLabel lbLyricsFound;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Label lbCurrentArtist;
    }
}