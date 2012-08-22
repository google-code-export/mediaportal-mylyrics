namespace MyLyrics
{
    partial class MusicDatabaseBrowse
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MusicDatabaseBrowse));
            this.btDeselectAll = new System.Windows.Forms.Button();
            this.btSelectAll = new System.Windows.Forms.Button();
            this.lvSongs = new System.Windows.Forms.ListView();
            this.columnHeaderSong = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDB = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.lbArtistNumber = new System.Windows.Forms.Label();
            this.lvSelectedSongs = new System.Windows.Forms.ListView();
            this.columnHeaderArtist2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSong2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDatabase2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderStatus2 = new System.Windows.Forms.ColumnHeader();
            this.btAdd = new System.Windows.Forms.Button();
            this.gbMusicDatabase = new System.Windows.Forms.GroupBox();
            this.lbStats = new System.Windows.Forms.Label();
            this.btAddAll = new System.Windows.Forms.Button();
            this.lbSelectedArtist = new System.Windows.Forms.Label();
            this.lvArtists = new System.Windows.Forms.ListView();
            this.columnHeaderArtist = new System.Windows.Forms.ColumnHeader();
            this.gbSelected = new System.Windows.Forms.GroupBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btRemoveAll = new System.Windows.Forms.Button();
            this.btDeselectAll2 = new System.Windows.Forms.Button();
            this.btSelectAll2 = new System.Windows.Forms.Button();
            this.btRemove = new System.Windows.Forms.Button();
            this.btSearch = new System.Windows.Forms.Button();
            this.bwOnlineSearch = new System.ComponentModel.BackgroundWorker();
            this.bwMusicTagSearch = new System.ComponentModel.BackgroundWorker();
            this.gbMusicDatabase.SuspendLayout();
            this.gbSelected.SuspendLayout();
            this.SuspendLayout();
            // 
            // btDeselectAll
            // 
            this.btDeselectAll.Location = new System.Drawing.Point(437, 219);
            this.btDeselectAll.Name = "btDeselectAll";
            this.btDeselectAll.Size = new System.Drawing.Size(75, 23);
            this.btDeselectAll.TabIndex = 10;
            this.btDeselectAll.Text = "Deselect All";
            this.btDeselectAll.UseVisualStyleBackColor = true;
            this.btDeselectAll.Click += new System.EventHandler(this.btDeselectAll_Click);
            // 
            // btSelectAll
            // 
            this.btSelectAll.Location = new System.Drawing.Point(356, 219);
            this.btSelectAll.Name = "btSelectAll";
            this.btSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btSelectAll.TabIndex = 9;
            this.btSelectAll.Text = "Select All";
            this.btSelectAll.UseVisualStyleBackColor = true;
            this.btSelectAll.Click += new System.EventHandler(this.btSelectAll_Click);
            // 
            // lvSongs
            // 
            this.lvSongs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSong,
            this.columnHeaderDB,
            this.columnHeaderStatus});
            this.lvSongs.FullRowSelect = true;
            this.lvSongs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSongs.Location = new System.Drawing.Point(189, 33);
            this.lvSongs.Name = "lvSongs";
            this.lvSongs.Size = new System.Drawing.Size(332, 162);
            this.lvSongs.SmallImageList = this.imageList;
            this.lvSongs.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvSongs.TabIndex = 8;
            this.lvSongs.UseCompatibleStateImageBehavior = false;
            this.lvSongs.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderSong
            // 
            this.columnHeaderSong.Text = "Song";
            this.columnHeaderSong.Width = 204;
            // 
            // columnHeaderDB
            // 
            this.columnHeaderDB.Text = "Database";
            this.columnHeaderDB.Width = 63;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 44;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "yes.gif");
            // 
            // lbArtistNumber
            // 
            this.lbArtistNumber.AutoSize = true;
            this.lbArtistNumber.Location = new System.Drawing.Point(11, 16);
            this.lbArtistNumber.Name = "lbArtistNumber";
            this.lbArtistNumber.Size = new System.Drawing.Size(67, 13);
            this.lbArtistNumber.TabIndex = 7;
            this.lbArtistNumber.Text = "[no of artists]";
            // 
            // lvSelectedSongs
            // 
            this.lvSelectedSongs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderArtist2,
            this.columnHeaderSong2,
            this.columnHeaderDatabase2,
            this.columnHeaderStatus2});
            this.lvSelectedSongs.FullRowSelect = true;
            this.lvSelectedSongs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSelectedSongs.Location = new System.Drawing.Point(11, 19);
            this.lvSelectedSongs.Name = "lvSelectedSongs";
            this.lvSelectedSongs.Size = new System.Drawing.Size(510, 164);
            this.lvSelectedSongs.SmallImageList = this.imageList;
            this.lvSelectedSongs.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvSelectedSongs.TabIndex = 11;
            this.lvSelectedSongs.UseCompatibleStateImageBehavior = false;
            this.lvSelectedSongs.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderArtist2
            // 
            this.columnHeaderArtist2.Text = "Artist";
            this.columnHeaderArtist2.Width = 177;
            // 
            // columnHeaderSong2
            // 
            this.columnHeaderSong2.Text = "Song";
            this.columnHeaderSong2.Width = 205;
            // 
            // columnHeaderDatabase2
            // 
            this.columnHeaderDatabase2.Text = "Database";
            this.columnHeaderDatabase2.Width = 63;
            // 
            // columnHeaderStatus2
            // 
            this.columnHeaderStatus2.Text = "Status";
            this.columnHeaderStatus2.Width = 44;
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(275, 219);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(75, 23);
            this.btAdd.TabIndex = 12;
            this.btAdd.Text = "Add";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // gbMusicDatabase
            // 
            this.gbMusicDatabase.Controls.Add(this.lbStats);
            this.gbMusicDatabase.Controls.Add(this.btAddAll);
            this.gbMusicDatabase.Controls.Add(this.lbSelectedArtist);
            this.gbMusicDatabase.Controls.Add(this.lvArtists);
            this.gbMusicDatabase.Controls.Add(this.btAdd);
            this.gbMusicDatabase.Controls.Add(this.btDeselectAll);
            this.gbMusicDatabase.Controls.Add(this.lbArtistNumber);
            this.gbMusicDatabase.Controls.Add(this.btSelectAll);
            this.gbMusicDatabase.Controls.Add(this.lvSongs);
            this.gbMusicDatabase.Location = new System.Drawing.Point(5, 5);
            this.gbMusicDatabase.Name = "gbMusicDatabase";
            this.gbMusicDatabase.Size = new System.Drawing.Size(538, 249);
            this.gbMusicDatabase.TabIndex = 13;
            this.gbMusicDatabase.TabStop = false;
            this.gbMusicDatabase.Text = "Select songs from music database";
            // 
            // lbStats
            // 
            this.lbStats.AutoSize = true;
            this.lbStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStats.Location = new System.Drawing.Point(191, 201);
            this.lbStats.Name = "lbStats";
            this.lbStats.Size = new System.Drawing.Size(35, 13);
            this.lbStats.TabIndex = 17;
            this.lbStats.Text = "[stats]";
            // 
            // btAddAll
            // 
            this.btAddAll.Location = new System.Drawing.Point(194, 219);
            this.btAddAll.Name = "btAddAll";
            this.btAddAll.Size = new System.Drawing.Size(75, 23);
            this.btAddAll.TabIndex = 15;
            this.btAddAll.Text = "Add all";
            this.btAddAll.UseVisualStyleBackColor = true;
            this.btAddAll.Click += new System.EventHandler(this.btAddAll_Click);
            // 
            // lbSelectedArtist
            // 
            this.lbSelectedArtist.AutoSize = true;
            this.lbSelectedArtist.Location = new System.Drawing.Point(191, 16);
            this.lbSelectedArtist.Name = "lbSelectedArtist";
            this.lbSelectedArtist.Size = new System.Drawing.Size(78, 13);
            this.lbSelectedArtist.TabIndex = 14;
            this.lbSelectedArtist.Text = "[selected artist]";
            // 
            // lvArtists
            // 
            this.lvArtists.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderArtist});
            this.lvArtists.FullRowSelect = true;
            this.lvArtists.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvArtists.Location = new System.Drawing.Point(11, 33);
            this.lvArtists.MultiSelect = false;
            this.lvArtists.Name = "lvArtists";
            this.lvArtists.Size = new System.Drawing.Size(172, 162);
            this.lvArtists.TabIndex = 13;
            this.lvArtists.UseCompatibleStateImageBehavior = false;
            this.lvArtists.View = System.Windows.Forms.View.Details;
            this.lvArtists.SelectedIndexChanged += new System.EventHandler(this.listViewArtists_SelectedIndexChanged);
            // 
            // columnHeaderArtist
            // 
            this.columnHeaderArtist.Text = "Artist";
            this.columnHeaderArtist.Width = 150;
            // 
            // gbSelected
            // 
            this.gbSelected.Controls.Add(this.btCancel);
            this.gbSelected.Controls.Add(this.btRemoveAll);
            this.gbSelected.Controls.Add(this.btDeselectAll2);
            this.gbSelected.Controls.Add(this.btSelectAll2);
            this.gbSelected.Controls.Add(this.btRemove);
            this.gbSelected.Controls.Add(this.btSearch);
            this.gbSelected.Controls.Add(this.lvSelectedSongs);
            this.gbSelected.Location = new System.Drawing.Point(5, 257);
            this.gbSelected.Name = "gbSelected";
            this.gbSelected.Size = new System.Drawing.Size(538, 220);
            this.gbSelected.TabIndex = 14;
            this.gbSelected.TabStop = false;
            this.gbSelected.Text = "Selected songs from music database";
            // 
            // btCancel
            // 
            this.btCancel.Enabled = false;
            this.btCancel.Location = new System.Drawing.Point(95, 189);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 17;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btRemoveAll
            // 
            this.btRemoveAll.Location = new System.Drawing.Point(194, 191);
            this.btRemoveAll.Name = "btRemoveAll";
            this.btRemoveAll.Size = new System.Drawing.Size(75, 23);
            this.btRemoveAll.TabIndex = 16;
            this.btRemoveAll.Text = "Remove all";
            this.btRemoveAll.UseVisualStyleBackColor = true;
            this.btRemoveAll.Click += new System.EventHandler(this.btRemoveAll_Click);
            // 
            // btDeselectAll2
            // 
            this.btDeselectAll2.Location = new System.Drawing.Point(437, 191);
            this.btDeselectAll2.Name = "btDeselectAll2";
            this.btDeselectAll2.Size = new System.Drawing.Size(75, 23);
            this.btDeselectAll2.TabIndex = 15;
            this.btDeselectAll2.Text = "Deselect All";
            this.btDeselectAll2.UseVisualStyleBackColor = true;
            this.btDeselectAll2.Click += new System.EventHandler(this.btDeselectAll2_Click);
            // 
            // btSelectAll2
            // 
            this.btSelectAll2.Location = new System.Drawing.Point(356, 191);
            this.btSelectAll2.Name = "btSelectAll2";
            this.btSelectAll2.Size = new System.Drawing.Size(75, 23);
            this.btSelectAll2.TabIndex = 15;
            this.btSelectAll2.Text = "Select All";
            this.btSelectAll2.UseVisualStyleBackColor = true;
            this.btSelectAll2.Click += new System.EventHandler(this.btSelectAll2_Click);
            // 
            // btRemove
            // 
            this.btRemove.Location = new System.Drawing.Point(275, 191);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(75, 23);
            this.btRemove.TabIndex = 15;
            this.btRemove.Text = "Remove";
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // btSearch
            // 
            this.btSearch.Enabled = false;
            this.btSearch.Location = new System.Drawing.Point(14, 189);
            this.btSearch.Name = "btSearch";
            this.btSearch.Size = new System.Drawing.Size(75, 23);
            this.btSearch.TabIndex = 15;
            this.btSearch.Text = "Search";
            this.btSearch.UseVisualStyleBackColor = true;
            this.btSearch.Click += new System.EventHandler(this.btSearch_Click);
            // 
            // bwOnlineSearch
            // 
            this.bwOnlineSearch.WorkerReportsProgress = true;
            this.bwOnlineSearch.WorkerSupportsCancellation = true;
            this.bwOnlineSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bw_DoWork);
            this.bwOnlineSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bw_RunWorkerCompleted);
            this.bwOnlineSearch.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bw_ProgressChanged);
            // 
            // bwMusicTagSearch
            // 
            this.bwMusicTagSearch.WorkerReportsProgress = true;
            this.bwMusicTagSearch.WorkerSupportsCancellation = true;
            this.bwMusicTagSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwMusicTagSearch_DoWork);
            this.bwMusicTagSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwMusicTagSearch_RunWorkerCompleted);
            this.bwMusicTagSearch.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwMusicTagSearch_ProgressChanged);
            // 
            // MusicDatabaseBrowse
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbSelected);
            this.Controls.Add(this.gbMusicDatabase);
            this.Name = "MusicDatabaseBrowse";
            this.Size = new System.Drawing.Size(544, 486);
            this.Load += new System.EventHandler(this.ListboxArtistsUpdate);
            this.gbMusicDatabase.ResumeLayout(false);
            this.gbMusicDatabase.PerformLayout();
            this.gbSelected.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btDeselectAll;
        private System.Windows.Forms.Button btSelectAll;
        private System.Windows.Forms.ListView lvSongs;
        private System.Windows.Forms.ColumnHeader columnHeaderSong;
        private System.Windows.Forms.ColumnHeader columnHeaderDB;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Label lbArtistNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderSong2;
        private System.Windows.Forms.ColumnHeader columnHeaderDatabase2;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus2;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.GroupBox gbMusicDatabase;
        private System.Windows.Forms.GroupBox gbSelected;
        private System.Windows.Forms.ListView lvArtists;
        private System.Windows.Forms.ColumnHeader columnHeaderArtist;
        private System.Windows.Forms.Label lbSelectedArtist;
        private System.Windows.Forms.ColumnHeader columnHeaderArtist2;
        private System.Windows.Forms.Button btSearch;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.Button btDeselectAll2;
        private System.Windows.Forms.Button btSelectAll2;
        private System.ComponentModel.BackgroundWorker bwOnlineSearch;
        private System.Windows.Forms.ImageList imageList;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Button btAddAll;
        private System.Windows.Forms.Button btRemoveAll;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label lbStats;
        public System.Windows.Forms.ListView lvSelectedSongs;
        private System.ComponentModel.BackgroundWorker bwMusicTagSearch;
    }
}