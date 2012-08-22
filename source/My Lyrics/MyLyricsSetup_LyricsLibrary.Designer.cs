namespace MyLyrics
{
    partial class MyLyricsSetup_LyricsLibrary
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btSave = new MediaPortal.UserInterface.Controls.MPButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btDelete = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.gbLibrary = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btSwitch = new MediaPortal.UserInterface.Controls.MPButton();
            this.btSearchSingle = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbSource = new MediaPortal.UserInterface.Controls.MPLabel();
            this.comboDatabase = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.lbDatabase = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbLRCTest = new System.Windows.Forms.Label();
            this.btImportDirs = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbSongs2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.tbLyrics = new MediaPortal.UserInterface.Controls.MPTextBox();
            this.lbSongs = new MediaPortal.UserInterface.Controls.MPLabel();
            this.btImportFiles = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbArtists2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbTitle = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbArtists = new MediaPortal.UserInterface.Controls.MPLabel();
            this.treeView = new System.Windows.Forms.TreeView();
            this.gbResetDatabase = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btResetMarkedLyricsDatabase = new System.Windows.Forms.Button();
            this.btResetLyricsDatabase = new System.Windows.Forms.Button();
            this.lbResetDatabase = new MediaPortal.UserInterface.Controls.MPLabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.gbLibrary.SuspendLayout();
            this.gbResetDatabase.SuspendLayout();
            this.SuspendLayout();
            // 
            // btSave
            // 
            this.btSave.Enabled = false;
            this.btSave.Location = new System.Drawing.Point(235, 301);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(65, 23);
            this.btSave.TabIndex = 7;
            this.btSave.Text = "&Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Text and LRC files|*-*.txt;*.lrc|Text files|*-*.txt|LRC files|*.lrc|All files|*.*" +
                "";
            this.openFileDialog1.InitialDirectory = "c:\\\\";
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.Title = "Select lyrics files to include";
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(85, 301);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(65, 23);
            this.btDelete.TabIndex = 3;
            this.btDelete.Text = "&Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(14, 301);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(65, 23);
            this.btAdd.TabIndex = 2;
            this.btAdd.Text = "&Add";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // gbLibrary
            // 
            this.gbLibrary.Controls.Add(this.btSwitch);
            this.gbLibrary.Controls.Add(this.btSearchSingle);
            this.gbLibrary.Controls.Add(this.lbSource);
            this.gbLibrary.Controls.Add(this.comboDatabase);
            this.gbLibrary.Controls.Add(this.lbDatabase);
            this.gbLibrary.Controls.Add(this.lbLRCTest);
            this.gbLibrary.Controls.Add(this.btImportDirs);
            this.gbLibrary.Controls.Add(this.lbSongs2);
            this.gbLibrary.Controls.Add(this.tbLyrics);
            this.gbLibrary.Controls.Add(this.lbSongs);
            this.gbLibrary.Controls.Add(this.btImportFiles);
            this.gbLibrary.Controls.Add(this.lbArtists2);
            this.gbLibrary.Controls.Add(this.lbTitle);
            this.gbLibrary.Controls.Add(this.lbArtists);
            this.gbLibrary.Controls.Add(this.treeView);
            this.gbLibrary.Controls.Add(this.btAdd);
            this.gbLibrary.Controls.Add(this.btDelete);
            this.gbLibrary.Controls.Add(this.btSave);
            this.gbLibrary.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbLibrary.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbLibrary.Location = new System.Drawing.Point(3, 3);
            this.gbLibrary.Name = "gbLibrary";
            this.gbLibrary.Size = new System.Drawing.Size(539, 362);
            this.gbLibrary.TabIndex = 8;
            this.gbLibrary.TabStop = false;
            this.gbLibrary.Text = "Editor";
            // 
            // btSwitch
            // 
            this.btSwitch.Location = new System.Drawing.Point(156, 330);
            this.btSwitch.Name = "btSwitch";
            this.btSwitch.Size = new System.Drawing.Size(65, 23);
            this.btSwitch.TabIndex = 29;
            this.btSwitch.Text = "&Move";
            this.btSwitch.UseVisualStyleBackColor = true;
            this.btSwitch.Click += new System.EventHandler(this.btSwitch_Click);
            // 
            // btSearchSingle
            // 
            this.btSearchSingle.Location = new System.Drawing.Point(156, 301);
            this.btSearchSingle.Name = "btSearchSingle";
            this.btSearchSingle.Size = new System.Drawing.Size(65, 23);
            this.btSearchSingle.TabIndex = 8;
            this.btSearchSingle.Text = "&Find";
            this.btSearchSingle.UseVisualStyleBackColor = true;
            this.btSearchSingle.Click += new System.EventHandler(this.btSearchSingle_Click);
            // 
            // lbSource
            // 
            this.lbSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSource.Location = new System.Drawing.Point(400, 297);
            this.lbSource.Name = "lbSource";
            this.lbSource.Size = new System.Drawing.Size(113, 15);
            this.lbSource.TabIndex = 28;
            this.lbSource.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboDatabase
            // 
            this.comboDatabase.BorderColor = System.Drawing.Color.Empty;
            this.comboDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatabase.FormattingEnabled = true;
            this.comboDatabase.Items.AddRange(new object[] {
            "Lyrics database",
            "Marked database"});
            this.comboDatabase.Location = new System.Drawing.Point(85, 15);
            this.comboDatabase.Name = "comboDatabase";
            this.comboDatabase.Size = new System.Drawing.Size(144, 21);
            this.comboDatabase.TabIndex = 1;
            this.comboDatabase.SelectedIndexChanged += new System.EventHandler(this.comboDatabase_SelectedIndexChanged);
            // 
            // lbDatabase
            // 
            this.lbDatabase.Location = new System.Drawing.Point(11, 18);
            this.lbDatabase.Name = "lbDatabase";
            this.lbDatabase.Size = new System.Drawing.Size(60, 26);
            this.lbDatabase.TabIndex = 26;
            this.lbDatabase.Text = "Database:";
            // 
            // lbLRCTest
            // 
            this.lbLRCTest.AutoSize = true;
            this.lbLRCTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLRCTest.Location = new System.Drawing.Point(453, 313);
            this.lbLRCTest.MaximumSize = new System.Drawing.Size(60, 13);
            this.lbLRCTest.MinimumSize = new System.Drawing.Size(60, 13);
            this.lbLRCTest.Name = "lbLRCTest";
            this.lbLRCTest.Size = new System.Drawing.Size(60, 13);
            this.lbLRCTest.TabIndex = 25;
            this.lbLRCTest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btImportDirs
            // 
            this.btImportDirs.Location = new System.Drawing.Point(85, 330);
            this.btImportDirs.Name = "btImportDirs";
            this.btImportDirs.Size = new System.Drawing.Size(65, 23);
            this.btImportDirs.TabIndex = 5;
            this.btImportDirs.Text = "Import &dirs";
            this.btImportDirs.UseVisualStyleBackColor = true;
            this.btImportDirs.Click += new System.EventHandler(this.btImportDIRS_Click);
            // 
            // lbSongs2
            // 
            this.lbSongs2.Location = new System.Drawing.Point(82, 65);
            this.lbSongs2.Name = "lbSongs2";
            this.lbSongs2.Size = new System.Drawing.Size(65, 15);
            this.lbSongs2.TabIndex = 23;
            this.lbSongs2.Text = "-";
            // 
            // tbLyrics
            // 
            this.tbLyrics.AllowDrop = true;
            this.tbLyrics.BorderColor = System.Drawing.Color.Empty;
            this.tbLyrics.Enabled = false;
            this.tbLyrics.Location = new System.Drawing.Point(235, 81);
            this.tbLyrics.Multiline = true;
            this.tbLyrics.Name = "tbLyrics";
            this.tbLyrics.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLyrics.Size = new System.Drawing.Size(298, 213);
            this.tbLyrics.TabIndex = 6;
            this.tbLyrics.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbLyrics_KeyUp);
            // 
            // lbSongs
            // 
            this.lbSongs.Location = new System.Drawing.Point(11, 65);
            this.lbSongs.Name = "lbSongs";
            this.lbSongs.Size = new System.Drawing.Size(60, 15);
            this.lbSongs.TabIndex = 22;
            this.lbSongs.Text = "Songs:";
            // 
            // btImportFiles
            // 
            this.btImportFiles.Location = new System.Drawing.Point(14, 330);
            this.btImportFiles.Name = "btImportFiles";
            this.btImportFiles.Size = new System.Drawing.Size(65, 23);
            this.btImportFiles.TabIndex = 4;
            this.btImportFiles.Text = "&Import files";
            this.btImportFiles.UseVisualStyleBackColor = true;
            this.btImportFiles.Click += new System.EventHandler(this.btImportSingle_Click);
            // 
            // lbArtists2
            // 
            this.lbArtists2.Location = new System.Drawing.Point(82, 50);
            this.lbArtists2.Name = "lbArtists2";
            this.lbArtists2.Size = new System.Drawing.Size(65, 15);
            this.lbArtists2.TabIndex = 21;
            this.lbArtists2.Text = "-";
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(238, 53);
            this.lbTitle.MaximumSize = new System.Drawing.Size(290, 26);
            this.lbTitle.MinimumSize = new System.Drawing.Size(290, 26);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(290, 26);
            this.lbTitle.TabIndex = 4;
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lbArtists
            // 
            this.lbArtists.Location = new System.Drawing.Point(11, 50);
            this.lbArtists.Name = "lbArtists";
            this.lbArtists.Size = new System.Drawing.Size(60, 15);
            this.lbArtists.TabIndex = 20;
            this.lbArtists.Text = "Artists:";
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(14, 81);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(215, 213);
            this.treeView.TabIndex = 2;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // gbResetDatabase
            // 
            this.gbResetDatabase.Controls.Add(this.btResetMarkedLyricsDatabase);
            this.gbResetDatabase.Controls.Add(this.btResetLyricsDatabase);
            this.gbResetDatabase.Controls.Add(this.lbResetDatabase);
            this.gbResetDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbResetDatabase.Location = new System.Drawing.Point(3, 371);
            this.gbResetDatabase.Name = "gbResetDatabase";
            this.gbResetDatabase.Size = new System.Drawing.Size(539, 76);
            this.gbResetDatabase.TabIndex = 11;
            this.gbResetDatabase.TabStop = false;
            this.gbResetDatabase.Text = "Reset database";
            // 
            // btResetMarkedLyricsDatabase
            // 
            this.btResetMarkedLyricsDatabase.Location = new System.Drawing.Point(375, 46);
            this.btResetMarkedLyricsDatabase.Name = "btResetMarkedLyricsDatabase";
            this.btResetMarkedLyricsDatabase.Size = new System.Drawing.Size(136, 23);
            this.btResetMarkedLyricsDatabase.TabIndex = 11;
            this.btResetMarkedLyricsDatabase.Text = "Reset marked database";
            this.btResetMarkedLyricsDatabase.UseVisualStyleBackColor = true;
            this.btResetMarkedLyricsDatabase.Click += new System.EventHandler(this.button1_Click);
            // 
            // btResetLyricsDatabase
            // 
            this.btResetLyricsDatabase.Location = new System.Drawing.Point(375, 17);
            this.btResetLyricsDatabase.Name = "btResetLyricsDatabase";
            this.btResetLyricsDatabase.Size = new System.Drawing.Size(136, 23);
            this.btResetLyricsDatabase.TabIndex = 10;
            this.btResetLyricsDatabase.Text = "Reset Lyrics database";
            this.btResetLyricsDatabase.UseVisualStyleBackColor = true;
            this.btResetLyricsDatabase.Click += new System.EventHandler(this.btResetDatabase_Click);
            // 
            // lbResetDatabase
            // 
            this.lbResetDatabase.Location = new System.Drawing.Point(11, 22);
            this.lbResetDatabase.Name = "lbResetDatabase";
            this.lbResetDatabase.Size = new System.Drawing.Size(320, 41);
            this.lbResetDatabase.TabIndex = 9;
            this.lbResetDatabase.Text = "Reset the lyrics database or the database with marked titles.\r\n\r\nAll data in a re" +
                "set database is lost forever.";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select a directory to search for lyrics or lrc-files. All files matching the patt" +
                "erns [Artist]-[Title].txt and *.lrc will be included to the lyrics database.";
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // MyLyricsSetup_LyricsLibrary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbResetDatabase);
            this.Controls.Add(this.gbLibrary);
            this.Name = "MyLyricsSetup_LyricsLibrary";
            this.Size = new System.Drawing.Size(545, 456);
            this.gbLibrary.ResumeLayout(false);
            this.gbLibrary.PerformLayout();
            this.gbResetDatabase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private MediaPortal.UserInterface.Controls.MPGroupBox gbLibrary;
        private MediaPortal.UserInterface.Controls.MPTextBox tbLyrics;
        private MediaPortal.UserInterface.Controls.MPGroupBox gbResetDatabase;
        private MediaPortal.UserInterface.Controls.MPLabel lbSongs;
        private MediaPortal.UserInterface.Controls.MPLabel lbArtists;
        internal MediaPortal.UserInterface.Controls.MPLabel lbArtists2;
        public MediaPortal.UserInterface.Controls.MPLabel lbSongs2;
        internal MediaPortal.UserInterface.Controls.MPButton btSave;
        internal System.Windows.Forms.Button btDelete;
        internal System.Windows.Forms.Button btAdd;
        internal MediaPortal.UserInterface.Controls.MPButton btImportFiles;
        internal MediaPortal.UserInterface.Controls.MPButton btImportDirs;
        private MediaPortal.UserInterface.Controls.MPLabel lbDatabase;
        internal System.Windows.Forms.Button btResetMarkedLyricsDatabase;
        internal System.Windows.Forms.Button btResetLyricsDatabase;
        private MediaPortal.UserInterface.Controls.MPLabel lbResetDatabase;
        private MediaPortal.UserInterface.Controls.MPComboBox comboDatabase;
        private System.Windows.Forms.Label lbLRCTest;
        private MediaPortal.UserInterface.Controls.MPLabel lbTitle;
        private MediaPortal.UserInterface.Controls.MPLabel lbSource;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        internal System.Windows.Forms.TreeView treeView;
        internal MediaPortal.UserInterface.Controls.MPButton btSearchSingle;
        internal MediaPortal.UserInterface.Controls.MPButton btSwitch;
    }
}
