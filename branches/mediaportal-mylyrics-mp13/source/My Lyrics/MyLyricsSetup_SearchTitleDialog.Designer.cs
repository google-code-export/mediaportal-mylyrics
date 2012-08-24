namespace MyLyrics
{
    partial class MyLyricsSetup_SearchTitleDialog
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
            this.gbSearchInfo = new System.Windows.Forms.GroupBox();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.lbTitle = new System.Windows.Forms.Label();
            this.tbArtist = new System.Windows.Forms.TextBox();
            this.lbArtist = new System.Windows.Forms.Label();
            this.btFind = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvSearchResults = new System.Windows.Forms.ListView();
            this.cbSite = new System.Windows.Forms.ColumnHeader();
            this.cbResult = new System.Windows.Forms.ColumnHeader();
            this.chLyric = new System.Windows.Forms.ColumnHeader();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbLyrics = new System.Windows.Forms.TextBox();
            this.gbLyricSites = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.cbHotLyrics = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cbSeekLyrics = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cbLyricsOnDemand = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cbLyrics007 = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cbEvilLabs = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.cbLyricWiki = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.btClose = new System.Windows.Forms.Button();
            this.btUpdate = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.gbSearchInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbLyricSites.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbSearchInfo
            // 
            this.gbSearchInfo.Controls.Add(this.tbTitle);
            this.gbSearchInfo.Controls.Add(this.lbTitle);
            this.gbSearchInfo.Controls.Add(this.tbArtist);
            this.gbSearchInfo.Controls.Add(this.lbArtist);
            this.gbSearchInfo.Location = new System.Drawing.Point(12, 12);
            this.gbSearchInfo.Name = "gbSearchInfo";
            this.gbSearchInfo.Size = new System.Drawing.Size(432, 85);
            this.gbSearchInfo.TabIndex = 0;
            this.gbSearchInfo.TabStop = false;
            this.gbSearchInfo.Text = "Search information";
            // 
            // tbTitle
            // 
            this.tbTitle.Location = new System.Drawing.Point(73, 53);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(344, 20);
            this.tbTitle.TabIndex = 2;
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(15, 56);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(30, 13);
            this.lbTitle.TabIndex = 2;
            this.lbTitle.Text = "Title:";
            // 
            // tbArtist
            // 
            this.tbArtist.Location = new System.Drawing.Point(73, 26);
            this.tbArtist.Name = "tbArtist";
            this.tbArtist.Size = new System.Drawing.Size(344, 20);
            this.tbArtist.TabIndex = 1;
            // 
            // lbArtist
            // 
            this.lbArtist.AutoSize = true;
            this.lbArtist.Location = new System.Drawing.Point(15, 29);
            this.lbArtist.Name = "lbArtist";
            this.lbArtist.Size = new System.Drawing.Size(33, 13);
            this.lbArtist.TabIndex = 0;
            this.lbArtist.Text = "Artist:";
            // 
            // btFind
            // 
            this.btFind.Location = new System.Drawing.Point(281, 67);
            this.btFind.Name = "btFind";
            this.btFind.Size = new System.Drawing.Size(65, 23);
            this.btFind.TabIndex = 3;
            this.btFind.Text = "&Fetch";
            this.btFind.UseVisualStyleBackColor = true;
            this.btFind.Click += new System.EventHandler(this.btFind_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lvSearchResults);
            this.groupBox1.Location = new System.Drawing.Point(13, 207);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 147);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search results";
            // 
            // lvSearchResults
            // 
            this.lvSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cbSite,
            this.cbResult,
            this.chLyric});
            this.lvSearchResults.FullRowSelect = true;
            this.lvSearchResults.Location = new System.Drawing.Point(17, 21);
            this.lvSearchResults.MultiSelect = false;
            this.lvSearchResults.Name = "lvSearchResults";
            this.lvSearchResults.Size = new System.Drawing.Size(399, 111);
            this.lvSearchResults.TabIndex = 10;
            this.lvSearchResults.UseCompatibleStateImageBehavior = false;
            this.lvSearchResults.View = System.Windows.Forms.View.Details;
            this.lvSearchResults.DoubleClick += new System.EventHandler(this.lvSearchResults_DoubleClick);
            this.lvSearchResults.SelectedIndexChanged += new System.EventHandler(this.lvSearchResults_SelectedIndexChanged);
            // 
            // cbSite
            // 
            this.cbSite.Text = "Site";
            this.cbSite.Width = 113;
            // 
            // cbResult
            // 
            this.cbResult.Text = "Result";
            this.cbResult.Width = 58;
            // 
            // chLyric
            // 
            this.chLyric.Text = "Lyric";
            this.chLyric.Width = 223;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbLyrics);
            this.groupBox2.Location = new System.Drawing.Point(13, 360);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(431, 183);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Lyric";
            // 
            // tbLyrics
            // 
            this.tbLyrics.Location = new System.Drawing.Point(17, 19);
            this.tbLyrics.Multiline = true;
            this.tbLyrics.Name = "tbLyrics";
            this.tbLyrics.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLyrics.Size = new System.Drawing.Size(399, 153);
            this.tbLyrics.TabIndex = 0;
            // 
            // gbLyricSites
            // 
            this.gbLyricSites.Controls.Add(this.btCancel);
            this.gbLyricSites.Controls.Add(this.btFind);
            this.gbLyricSites.Controls.Add(this.cbHotLyrics);
            this.gbLyricSites.Controls.Add(this.cbSeekLyrics);
            this.gbLyricSites.Controls.Add(this.cbLyricsOnDemand);
            this.gbLyricSites.Controls.Add(this.cbLyrics007);
            this.gbLyricSites.Controls.Add(this.cbEvilLabs);
            this.gbLyricSites.Controls.Add(this.cbLyricWiki);
            this.gbLyricSites.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbLyricSites.Location = new System.Drawing.Point(12, 103);
            this.gbLyricSites.Name = "gbLyricSites";
            this.gbLyricSites.Size = new System.Drawing.Size(432, 98);
            this.gbLyricSites.TabIndex = 29;
            this.gbLyricSites.TabStop = false;
            this.gbLyricSites.Text = "Lyric sites to search";
            // 
            // cbHotLyrics
            // 
            this.cbHotLyrics.AutoSize = true;
            this.cbHotLyrics.Checked = true;
            this.cbHotLyrics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbHotLyrics.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbHotLyrics.Location = new System.Drawing.Point(146, 21);
            this.cbHotLyrics.Name = "cbHotLyrics";
            this.cbHotLyrics.Size = new System.Drawing.Size(71, 17);
            this.cbHotLyrics.TabIndex = 14;
            this.cbHotLyrics.Text = "Hot Lyrics";
            this.cbHotLyrics.UseVisualStyleBackColor = true;
            // 
            // cbSeekLyrics
            // 
            this.cbSeekLyrics.AutoSize = true;
            this.cbSeekLyrics.Checked = true;
            this.cbSeekLyrics.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSeekLyrics.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbSeekLyrics.Location = new System.Drawing.Point(146, 43);
            this.cbSeekLyrics.Name = "cbSeekLyrics";
            this.cbSeekLyrics.Size = new System.Drawing.Size(79, 17);
            this.cbSeekLyrics.TabIndex = 15;
            this.cbSeekLyrics.Text = "Seek Lyrics";
            this.cbSeekLyrics.UseVisualStyleBackColor = true;
            // 
            // cbLyricsOnDemand
            // 
            this.cbLyricsOnDemand.AutoSize = true;
            this.cbLyricsOnDemand.Checked = true;
            this.cbLyricsOnDemand.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLyricsOnDemand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbLyricsOnDemand.Location = new System.Drawing.Point(16, 43);
            this.cbLyricsOnDemand.Name = "cbLyricsOnDemand";
            this.cbLyricsOnDemand.Size = new System.Drawing.Size(108, 17);
            this.cbLyricsOnDemand.TabIndex = 12;
            this.cbLyricsOnDemand.Text = "Lyrics OnDemand";
            this.cbLyricsOnDemand.UseVisualStyleBackColor = true;
            // 
            // cbLyrics007
            // 
            this.cbLyrics007.AutoSize = true;
            this.cbLyrics007.Checked = true;
            this.cbLyrics007.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLyrics007.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbLyrics007.Location = new System.Drawing.Point(16, 66);
            this.cbLyrics007.Name = "cbLyrics007";
            this.cbLyrics007.Size = new System.Drawing.Size(72, 17);
            this.cbLyrics007.TabIndex = 13;
            this.cbLyrics007.Text = "Lyrics 007";
            this.cbLyrics007.UseVisualStyleBackColor = true;
            // 
            // cbEvilLabs
            // 
            this.cbEvilLabs.AutoSize = true;
            this.cbEvilLabs.Checked = true;
            this.cbEvilLabs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbEvilLabs.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbEvilLabs.Location = new System.Drawing.Point(146, 66);
            this.cbEvilLabs.Name = "cbEvilLabs";
            this.cbEvilLabs.Size = new System.Drawing.Size(67, 17);
            this.cbEvilLabs.TabIndex = 16;
            this.cbEvilLabs.Text = "Evil Labs";
            this.cbEvilLabs.UseVisualStyleBackColor = true;
            // 
            // cbLyricWiki
            // 
            this.cbLyricWiki.AutoSize = true;
            this.cbLyricWiki.Checked = true;
            this.cbLyricWiki.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLyricWiki.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cbLyricWiki.Location = new System.Drawing.Point(16, 21);
            this.cbLyricWiki.Name = "cbLyricWiki";
            this.cbLyricWiki.Size = new System.Drawing.Size(67, 17);
            this.cbLyricWiki.TabIndex = 11;
            this.cbLyricWiki.Text = "LyricWiki";
            this.cbLyricWiki.UseVisualStyleBackColor = true;
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point(354, 549);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(75, 23);
            this.btClose.TabIndex = 30;
            this.btClose.Text = "Close";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btUpdate
            // 
            this.btUpdate.Enabled = false;
            this.btUpdate.Location = new System.Drawing.Point(273, 549);
            this.btUpdate.Name = "btUpdate";
            this.btUpdate.Size = new System.Drawing.Size(75, 23);
            this.btUpdate.TabIndex = 11;
            this.btUpdate.Text = "Update";
            this.btUpdate.UseVisualStyleBackColor = true;
            this.btUpdate.Click += new System.EventHandler(this.btUpdate_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(352, 67);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(65, 23);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "&Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // MyLyricsSetup_SearchTitleDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 584);
            this.ControlBox = false;
            this.Controls.Add(this.btUpdate);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.gbLyricSites);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbSearchInfo);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MyLyricsSetup_SearchTitleDialog";
            this.ShowIcon = false;
            this.Text = "Fetch lyric dialog";
            this.gbSearchInfo.ResumeLayout(false);
            this.gbSearchInfo.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbLyricSites.ResumeLayout(false);
            this.gbLyricSites.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbSearchInfo;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.TextBox tbArtist;
        private System.Windows.Forms.Label lbArtist;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.Button btFind;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbLyrics;
        private MediaPortal.UserInterface.Controls.MPGroupBox gbLyricSites;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbHotLyrics;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbSeekLyrics;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbLyricsOnDemand;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbLyrics007;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbEvilLabs;
        internal MediaPortal.UserInterface.Controls.MPCheckBox cbLyricWiki;
        private System.Windows.Forms.ListView lvSearchResults;
        private System.Windows.Forms.ColumnHeader cbSite;
        private System.Windows.Forms.ColumnHeader cbResult;
        private System.Windows.Forms.ColumnHeader chLyric;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btUpdate;
        private System.Windows.Forms.Button btCancel;
    }
}