namespace MyLyrics
{
  partial class LyricsLibrary
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gbResetDatabase = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btResetMarkedLyricsDatabase = new System.Windows.Forms.Button();
            this.btResetLyricsDatabase = new System.Windows.Forms.Button();
            this.lbResetDatabase = new MediaPortal.UserInterface.Controls.MPLabel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpGroupBox5 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpButton1 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpButton2 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpComboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpGroupBox6 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpButton3 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpButton4 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpGroupBox7 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpButton5 = new MediaPortal.UserInterface.Controls.MPButton();
            this.button3 = new System.Windows.Forms.Button();
            this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.mpGroupBox8 = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.mpButton6 = new MediaPortal.UserInterface.Controls.MPButton();
            this.mpButton7 = new MediaPortal.UserInterface.Controls.MPButton();
            this.gbEditLyrics = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btSave = new MediaPortal.UserInterface.Controls.MPButton();
            this.btSearchSingle = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbTitle = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbArtists = new MediaPortal.UserInterface.Controls.MPLabel();
            this.gbAddRemove = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btDelete = new System.Windows.Forms.Button();
            this.btSwitch = new MediaPortal.UserInterface.Controls.MPButton();
            this.btAdd = new System.Windows.Forms.Button();
            this.gbImport = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btImportFiles = new MediaPortal.UserInterface.Controls.MPButton();
            this.btImportDirs = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbArtists2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.treeView = new System.Windows.Forms.TreeView();
            this.comboDatabase = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.gbImportTags = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btImportTags = new MediaPortal.UserInterface.Controls.MPButton();
            this.gbExportTags = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.btExportTags = new MediaPortal.UserInterface.Controls.MPButton();
            this.lbSongs = new MediaPortal.UserInterface.Controls.MPLabel();
            this.tbLyrics = new System.Windows.Forms.TextBox();
            this.lbSongs2 = new MediaPortal.UserInterface.Controls.MPLabel();
            this.lbLRCTest = new System.Windows.Forms.Label();
            this.btRefresh = new System.Windows.Forms.Button();
            this.lbDatabase = new MediaPortal.UserInterface.Controls.MPLabel();
            this.gbResetDatabase.SuspendLayout();
            this.gbEditLyrics.SuspendLayout();
            this.gbAddRemove.SuspendLayout();
            this.gbImport.SuspendLayout();
            this.gbImportTags.SuspendLayout();
            this.gbExportTags.SuspendLayout();
            this.SuspendLayout();
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
            // gbResetDatabase
            // 
            this.gbResetDatabase.Controls.Add(this.btResetMarkedLyricsDatabase);
            this.gbResetDatabase.Controls.Add(this.btResetLyricsDatabase);
            this.gbResetDatabase.Controls.Add(this.lbResetDatabase);
            this.gbResetDatabase.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbResetDatabase.Location = new System.Drawing.Point(5, 427);
            this.gbResetDatabase.Name = "gbResetDatabase";
            this.gbResetDatabase.Size = new System.Drawing.Size(538, 48);
            this.gbResetDatabase.TabIndex = 11;
            this.gbResetDatabase.TabStop = false;
            this.gbResetDatabase.Text = "Reset database";
            // 
            // btResetMarkedLyricsDatabase
            // 
            this.btResetMarkedLyricsDatabase.Location = new System.Drawing.Point(457, 18);
            this.btResetMarkedLyricsDatabase.Name = "btResetMarkedLyricsDatabase";
            this.btResetMarkedLyricsDatabase.Size = new System.Drawing.Size(75, 21);
            this.btResetMarkedLyricsDatabase.TabIndex = 11;
            this.btResetMarkedLyricsDatabase.Text = "MarkedDB";
            this.btResetMarkedLyricsDatabase.UseVisualStyleBackColor = true;
            this.btResetMarkedLyricsDatabase.Click += new System.EventHandler(this.button1_Click);
            // 
            // btResetLyricsDatabase
            // 
            this.btResetLyricsDatabase.Location = new System.Drawing.Point(376, 18);
            this.btResetLyricsDatabase.Name = "btResetLyricsDatabase";
            this.btResetLyricsDatabase.Size = new System.Drawing.Size(75, 21);
            this.btResetLyricsDatabase.TabIndex = 10;
            this.btResetLyricsDatabase.Text = "LyricsDB";
            this.btResetLyricsDatabase.UseVisualStyleBackColor = true;
            this.btResetLyricsDatabase.Click += new System.EventHandler(this.btResetDatabase_Click);
            // 
            // lbResetDatabase
            // 
            this.lbResetDatabase.Location = new System.Drawing.Point(11, 14);
            this.lbResetDatabase.Name = "lbResetDatabase";
            this.lbResetDatabase.Size = new System.Drawing.Size(353, 30);
            this.lbResetDatabase.TabIndex = 9;
            this.lbResetDatabase.Text = "Reset the lyrics database or the database with\r\nmarked titles. All data in the da" +
    "tabase will be permanately lost.";
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select a directory to search for lyrics or lrc-files. All files matching the patt" +
    "erns [Artist]-[Title].txt and *.lrc will be included to the lyrics database.";
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(463, 317);
            this.label1.MaximumSize = new System.Drawing.Size(60, 13);
            this.label1.MinimumSize = new System.Drawing.Size(60, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 25;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mpLabel4
            // 
            this.mpLabel4.Location = new System.Drawing.Point(77, 70);
            this.mpLabel4.Name = "mpLabel4";
            this.mpLabel4.Size = new System.Drawing.Size(65, 15);
            this.mpLabel4.TabIndex = 23;
            this.mpLabel4.Text = "-";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.textBox1.CausesValidation = false;
            this.textBox1.Location = new System.Drawing.Point(252, 84);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(291, 213);
            this.textBox1.TabIndex = 35;
            // 
            // mpLabel5
            // 
            this.mpLabel5.Location = new System.Drawing.Point(6, 70);
            this.mpLabel5.Name = "mpLabel5";
            this.mpLabel5.Size = new System.Drawing.Size(60, 15);
            this.mpLabel5.TabIndex = 22;
            this.mpLabel5.Text = "Songs:";
            // 
            // mpGroupBox5
            // 
            this.mpGroupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox5.Location = new System.Drawing.Point(251, 362);
            this.mpGroupBox5.Name = "mpGroupBox5";
            this.mpGroupBox5.Size = new System.Drawing.Size(293, 52);
            this.mpGroupBox5.TabIndex = 34;
            this.mpGroupBox5.TabStop = false;
            this.mpGroupBox5.Text = "Import / Export lyrics from music tags";
            // 
            // mpButton1
            // 
            this.mpButton1.Location = new System.Drawing.Point(77, 19);
            this.mpButton1.Name = "mpButton1";
            this.mpButton1.Size = new System.Drawing.Size(68, 23);
            this.mpButton1.TabIndex = 6;
            this.mpButton1.Text = "Export";
            this.mpButton1.UseVisualStyleBackColor = true;
            // 
            // mpButton2
            // 
            this.mpButton2.Location = new System.Drawing.Point(4, 19);
            this.mpButton2.Name = "mpButton2";
            this.mpButton2.Size = new System.Drawing.Size(68, 23);
            this.mpButton2.TabIndex = 6;
            this.mpButton2.Text = "Import";
            this.mpButton2.UseVisualStyleBackColor = true;
            // 
            // mpComboBox1
            // 
            this.mpComboBox1.BorderColor = System.Drawing.Color.Empty;
            this.mpComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mpComboBox1.FormattingEnabled = true;
            this.mpComboBox1.Items.AddRange(new object[] {
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database",
            "Lyrics database",
            "Marked database"});
            this.mpComboBox1.Location = new System.Drawing.Point(80, 23);
            this.mpComboBox1.Name = "mpComboBox1";
            this.mpComboBox1.Size = new System.Drawing.Size(144, 21);
            this.mpComboBox1.TabIndex = 1;
            // 
            // treeView1
            // 
            this.treeView1.LineColor = System.Drawing.Color.Empty;
            this.treeView1.Location = new System.Drawing.Point(9, 84);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(235, 213);
            this.treeView1.TabIndex = 2;
            // 
            // mpLabel6
            // 
            this.mpLabel6.Location = new System.Drawing.Point(77, 55);
            this.mpLabel6.Name = "mpLabel6";
            this.mpLabel6.Size = new System.Drawing.Size(65, 15);
            this.mpLabel6.TabIndex = 21;
            this.mpLabel6.Text = "-";
            // 
            // mpGroupBox6
            // 
            this.mpGroupBox6.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox6.Location = new System.Drawing.Point(6, 362);
            this.mpGroupBox6.Name = "mpGroupBox6";
            this.mpGroupBox6.Size = new System.Drawing.Size(238, 52);
            this.mpGroupBox6.TabIndex = 33;
            this.mpGroupBox6.TabStop = false;
            this.mpGroupBox6.Text = "Import lyrics from files";
            // 
            // mpButton3
            // 
            this.mpButton3.Location = new System.Drawing.Point(6, 19);
            this.mpButton3.Name = "mpButton3";
            this.mpButton3.Size = new System.Drawing.Size(65, 23);
            this.mpButton3.TabIndex = 4;
            this.mpButton3.Text = "&Import files";
            this.mpButton3.UseVisualStyleBackColor = true;
            // 
            // mpButton4
            // 
            this.mpButton4.Location = new System.Drawing.Point(77, 19);
            this.mpButton4.Name = "mpButton4";
            this.mpButton4.Size = new System.Drawing.Size(65, 23);
            this.mpButton4.TabIndex = 5;
            this.mpButton4.Text = "Import &dirs";
            this.mpButton4.UseVisualStyleBackColor = true;
            // 
            // mpGroupBox7
            // 
            this.mpGroupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox7.Location = new System.Drawing.Point(6, 304);
            this.mpGroupBox7.Name = "mpGroupBox7";
            this.mpGroupBox7.Size = new System.Drawing.Size(238, 52);
            this.mpGroupBox7.TabIndex = 30;
            this.mpGroupBox7.TabStop = false;
            this.mpGroupBox7.Text = "Add / remove / move lyrics";
            // 
            // mpButton5
            // 
            this.mpButton5.Location = new System.Drawing.Point(148, 19);
            this.mpButton5.Name = "mpButton5";
            this.mpButton5.Size = new System.Drawing.Size(65, 23);
            this.mpButton5.TabIndex = 29;
            this.mpButton5.Text = "&Move";
            this.mpButton5.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 19);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(65, 23);
            this.button3.TabIndex = 2;
            this.button3.Text = "&Add";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // mpLabel7
            // 
            this.mpLabel7.Location = new System.Drawing.Point(6, 55);
            this.mpLabel7.Name = "mpLabel7";
            this.mpLabel7.Size = new System.Drawing.Size(60, 15);
            this.mpLabel7.TabIndex = 20;
            this.mpLabel7.Text = "Artists:";
            // 
            // mpLabel8
            // 
            this.mpLabel8.AutoSize = true;
            this.mpLabel8.Location = new System.Drawing.Point(252, 55);
            this.mpLabel8.MaximumSize = new System.Drawing.Size(290, 26);
            this.mpLabel8.MinimumSize = new System.Drawing.Size(290, 26);
            this.mpLabel8.Name = "mpLabel8";
            this.mpLabel8.Size = new System.Drawing.Size(290, 26);
            this.mpLabel8.TabIndex = 4;
            this.mpLabel8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // mpGroupBox8
            // 
            this.mpGroupBox8.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.mpGroupBox8.Location = new System.Drawing.Point(251, 304);
            this.mpGroupBox8.Name = "mpGroupBox8";
            this.mpGroupBox8.Size = new System.Drawing.Size(293, 52);
            this.mpGroupBox8.TabIndex = 32;
            this.mpGroupBox8.TabStop = false;
            this.mpGroupBox8.Text = "Edit / find lyrics";
            // 
            // mpButton6
            // 
            this.mpButton6.Location = new System.Drawing.Point(4, 19);
            this.mpButton6.Name = "mpButton6";
            this.mpButton6.Size = new System.Drawing.Size(68, 23);
            this.mpButton6.TabIndex = 7;
            this.mpButton6.Text = "&Save";
            this.mpButton6.UseVisualStyleBackColor = true;
            // 
            // mpButton7
            // 
            this.mpButton7.Location = new System.Drawing.Point(77, 19);
            this.mpButton7.Name = "mpButton7";
            this.mpButton7.Size = new System.Drawing.Size(68, 23);
            this.mpButton7.TabIndex = 31;
            this.mpButton7.Text = "&Find lyric";
            this.mpButton7.UseVisualStyleBackColor = true;
            // 
            // gbEditLyrics
            // 
            this.gbEditLyrics.Controls.Add(this.btSave);
            this.gbEditLyrics.Controls.Add(this.btSearchSingle);
            this.gbEditLyrics.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbEditLyrics.Location = new System.Drawing.Point(250, 290);
            this.gbEditLyrics.Name = "gbEditLyrics";
            this.gbEditLyrics.Size = new System.Drawing.Size(293, 48);
            this.gbEditLyrics.TabIndex = 32;
            this.gbEditLyrics.TabStop = false;
            this.gbEditLyrics.Text = "Edit / find lyrics";
            // 
            // btSave
            // 
            this.btSave.Enabled = false;
            this.btSave.Location = new System.Drawing.Point(4, 19);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(68, 21);
            this.btSave.TabIndex = 7;
            this.btSave.Text = "&Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btSearchSingle
            // 
            this.btSearchSingle.Enabled = false;
            this.btSearchSingle.Location = new System.Drawing.Point(77, 19);
            this.btSearchSingle.Name = "btSearchSingle";
            this.btSearchSingle.Size = new System.Drawing.Size(68, 21);
            this.btSearchSingle.TabIndex = 31;
            this.btSearchSingle.Text = "&Find lyric";
            this.btSearchSingle.UseVisualStyleBackColor = true;
            this.btSearchSingle.Click += new System.EventHandler(this.btSearchSingle_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(251, 40);
            this.lbTitle.MaximumSize = new System.Drawing.Size(290, 26);
            this.lbTitle.MinimumSize = new System.Drawing.Size(290, 26);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(290, 26);
            this.lbTitle.TabIndex = 4;
            this.lbTitle.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lbArtists
            // 
            this.lbArtists.Location = new System.Drawing.Point(5, 40);
            this.lbArtists.Name = "lbArtists";
            this.lbArtists.Size = new System.Drawing.Size(60, 15);
            this.lbArtists.TabIndex = 20;
            this.lbArtists.Text = "Artists:";
            // 
            // gbAddRemove
            // 
            this.gbAddRemove.Controls.Add(this.btDelete);
            this.gbAddRemove.Controls.Add(this.btSwitch);
            this.gbAddRemove.Controls.Add(this.btAdd);
            this.gbAddRemove.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbAddRemove.Location = new System.Drawing.Point(5, 290);
            this.gbAddRemove.Name = "gbAddRemove";
            this.gbAddRemove.Size = new System.Drawing.Size(238, 48);
            this.gbAddRemove.TabIndex = 30;
            this.gbAddRemove.TabStop = false;
            this.gbAddRemove.Text = "Add / remove / move lyrics";
            // 
            // btDelete
            // 
            this.btDelete.Location = new System.Drawing.Point(77, 19);
            this.btDelete.Name = "btDelete";
            this.btDelete.Size = new System.Drawing.Size(65, 21);
            this.btDelete.TabIndex = 3;
            this.btDelete.Text = "&Delete";
            this.btDelete.UseVisualStyleBackColor = true;
            this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // btSwitch
            // 
            this.btSwitch.Location = new System.Drawing.Point(148, 19);
            this.btSwitch.Name = "btSwitch";
            this.btSwitch.Size = new System.Drawing.Size(65, 21);
            this.btSwitch.TabIndex = 29;
            this.btSwitch.Text = "&Move";
            this.btSwitch.UseVisualStyleBackColor = true;
            this.btSwitch.Click += new System.EventHandler(this.btSwitch_Click);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(6, 19);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(65, 21);
            this.btAdd.TabIndex = 2;
            this.btAdd.Text = "&Add";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // gbImport
            // 
            this.gbImport.Controls.Add(this.btImportFiles);
            this.gbImport.Controls.Add(this.btImportDirs);
            this.gbImport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbImport.Location = new System.Drawing.Point(5, 343);
            this.gbImport.Name = "gbImport";
            this.gbImport.Size = new System.Drawing.Size(226, 50);
            this.gbImport.TabIndex = 33;
            this.gbImport.TabStop = false;
            this.gbImport.Text = "Import lyrics from files";
            // 
            // btImportFiles
            // 
            this.btImportFiles.Location = new System.Drawing.Point(6, 19);
            this.btImportFiles.Name = "btImportFiles";
            this.btImportFiles.Size = new System.Drawing.Size(65, 21);
            this.btImportFiles.TabIndex = 4;
            this.btImportFiles.Text = "&Import files";
            this.btImportFiles.UseVisualStyleBackColor = true;
            this.btImportFiles.Click += new System.EventHandler(this.btImportSingle_Click);
            // 
            // btImportDirs
            // 
            this.btImportDirs.Location = new System.Drawing.Point(77, 19);
            this.btImportDirs.Name = "btImportDirs";
            this.btImportDirs.Size = new System.Drawing.Size(65, 21);
            this.btImportDirs.TabIndex = 5;
            this.btImportDirs.Text = "Import &dirs";
            this.btImportDirs.UseVisualStyleBackColor = true;
            this.btImportDirs.Click += new System.EventHandler(this.btImportDIRS_Click);
            // 
            // lbArtists2
            // 
            this.lbArtists2.Location = new System.Drawing.Point(76, 40);
            this.lbArtists2.Name = "lbArtists2";
            this.lbArtists2.Size = new System.Drawing.Size(65, 15);
            this.lbArtists2.TabIndex = 21;
            this.lbArtists2.Text = "-";
            // 
            // treeView
            // 
            this.treeView.Location = new System.Drawing.Point(8, 69);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(235, 213);
            this.treeView.TabIndex = 2;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // comboDatabase
            // 
            this.comboDatabase.BorderColor = System.Drawing.Color.Empty;
            this.comboDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDatabase.FormattingEnabled = true;
            this.comboDatabase.Items.AddRange(new object[] {
            "Lyrics database",
            "Lyrics database (LRC only)",
            "Marked database"});
            this.comboDatabase.Location = new System.Drawing.Point(79, 8);
            this.comboDatabase.Name = "comboDatabase";
            this.comboDatabase.Size = new System.Drawing.Size(164, 21);
            this.comboDatabase.TabIndex = 1;
            this.comboDatabase.SelectedIndexChanged += new System.EventHandler(this.comboDatabase_SelectedIndexChanged);
            // 
            // gbImportTags
            // 
            this.gbImportTags.Controls.Add(this.btImportTags);
            this.gbImportTags.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbImportTags.Location = new System.Drawing.Point(237, 344);
            this.gbImportTags.Name = "gbImportTags";
            this.gbImportTags.Size = new System.Drawing.Size(152, 49);
            this.gbImportTags.TabIndex = 34;
            this.gbImportTags.TabStop = false;
            this.gbImportTags.Text = "Import lyrics from music tags";
            // 
            // btExportTags
            // 
            this.btExportTags.Location = new System.Drawing.Point(6, 17);
            this.btExportTags.Name = "btExportTags";
            this.btExportTags.Size = new System.Drawing.Size(68, 21);
            this.btExportTags.TabIndex = 6;
            this.btExportTags.Text = "Export";
            this.btExportTags.UseVisualStyleBackColor = true;
            this.btExportTags.Click += new System.EventHandler(this.btExportTags_Click);
            // 
            // btImportTags
            // 
            this.btImportTags.Location = new System.Drawing.Point(6, 18);
            this.btImportTags.Name = "btImportTags";
            this.btImportTags.Size = new System.Drawing.Size(68, 21);
            this.btImportTags.TabIndex = 6;
            this.btImportTags.Text = "Import";
            this.btImportTags.UseVisualStyleBackColor = true;
            this.btImportTags.Click += new System.EventHandler(this.btImportTags_Click);
            // 
            // lbSongs
            // 
            this.lbSongs.Location = new System.Drawing.Point(5, 55);
            this.lbSongs.Name = "lbSongs";
            this.lbSongs.Size = new System.Drawing.Size(60, 15);
            this.lbSongs.TabIndex = 22;
            this.lbSongs.Text = "Songs:";
            // 
            // tbLyrics
            // 
            this.tbLyrics.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tbLyrics.CausesValidation = false;
            this.tbLyrics.Location = new System.Drawing.Point(250, 69);
            this.tbLyrics.Multiline = true;
            this.tbLyrics.Name = "tbLyrics";
            this.tbLyrics.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbLyrics.Size = new System.Drawing.Size(293, 213);
            this.tbLyrics.TabIndex = 35;
            this.tbLyrics.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbLyrics_KeyUp);
            // 
            // lbSongs2
            // 
            this.lbSongs2.Location = new System.Drawing.Point(76, 55);
            this.lbSongs2.Name = "lbSongs2";
            this.lbSongs2.Size = new System.Drawing.Size(65, 15);
            this.lbSongs2.TabIndex = 23;
            this.lbSongs2.Text = "-";
            // 
            // lbLRCTest
            // 
            this.lbLRCTest.AutoSize = true;
            this.lbLRCTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLRCTest.Location = new System.Drawing.Point(479, 283);
            this.lbLRCTest.MaximumSize = new System.Drawing.Size(60, 11);
            this.lbLRCTest.MinimumSize = new System.Drawing.Size(60, 11);
            this.lbLRCTest.Name = "lbLRCTest";
            this.lbLRCTest.Size = new System.Drawing.Size(60, 11);
            this.lbLRCTest.TabIndex = 25;
            this.lbLRCTest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btRefresh
            // 
            this.btRefresh.Location = new System.Drawing.Point(250, 8);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(55, 21);
            this.btRefresh.TabIndex = 36;
            this.btRefresh.Text = "&Refresh";
            this.btRefresh.UseVisualStyleBackColor = true;
            this.btRefresh.Click += new System.EventHandler(this.btRefresh_Click);
            // 
            // lbDatabase
            // 
            this.lbDatabase.Location = new System.Drawing.Point(5, 11);
            this.lbDatabase.Name = "lbDatabase";
            this.lbDatabase.Size = new System.Drawing.Size(60, 26);
            this.lbDatabase.TabIndex = 26;
            this.lbDatabase.Text = "Database:";
            // 
            // gbExportTags
            // 
            this.gbExportTags.Controls.Add(this.btExportTags);
            this.gbExportTags.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbExportTags.Location = new System.Drawing.Point(395, 345);
            this.gbExportTags.Name = "gbExportTags";
            this.gbExportTags.Size = new System.Drawing.Size(142, 48);
            this.gbExportTags.TabIndex = 35;
            this.gbExportTags.TabStop = false;
            this.gbExportTags.Text = "Export lyrics to music tags";
            // 
            // LyricsLibrary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbExportTags);
            this.Controls.Add(this.lbLRCTest);
            this.Controls.Add(this.btRefresh);
            this.Controls.Add(this.gbResetDatabase);
            this.Controls.Add(this.tbLyrics);
            this.Controls.Add(this.gbImportTags);
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.gbImport);
            this.Controls.Add(this.lbArtists);
            this.Controls.Add(this.gbEditLyrics);
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.gbAddRemove);
            this.Controls.Add(this.lbArtists2);
            this.Controls.Add(this.comboDatabase);
            this.Controls.Add(this.lbSongs);
            this.Controls.Add(this.lbDatabase);
            this.Controls.Add(this.lbSongs2);
            this.Name = "LyricsLibrary";
            this.Size = new System.Drawing.Size(554, 479);
            this.gbResetDatabase.ResumeLayout(false);
            this.gbEditLyrics.ResumeLayout(false);
            this.gbAddRemove.ResumeLayout(false);
            this.gbImport.ResumeLayout(false);
            this.gbImportTags.ResumeLayout(false);
            this.gbExportTags.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbResetDatabase;
    internal System.Windows.Forms.Button btResetMarkedLyricsDatabase;
    internal System.Windows.Forms.Button btResetLyricsDatabase;
    private MediaPortal.UserInterface.Controls.MPLabel lbResetDatabase;
    private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    private MediaPortal.UserInterface.Controls.MPLabel lbDatabase;
    internal System.Windows.Forms.Button btRefresh;
    private System.Windows.Forms.Label lbLRCTest;
    public MediaPortal.UserInterface.Controls.MPLabel lbSongs2;
    private System.Windows.Forms.TextBox tbLyrics;
    private MediaPortal.UserInterface.Controls.MPLabel lbSongs;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbImportTags;
    internal MediaPortal.UserInterface.Controls.MPButton btExportTags;
    internal MediaPortal.UserInterface.Controls.MPButton btImportTags;
    private MediaPortal.UserInterface.Controls.MPComboBox comboDatabase;
    internal System.Windows.Forms.TreeView treeView;
    internal MediaPortal.UserInterface.Controls.MPLabel lbArtists2;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbImport;
    internal MediaPortal.UserInterface.Controls.MPButton btImportFiles;
    internal MediaPortal.UserInterface.Controls.MPButton btImportDirs;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbAddRemove;
    internal System.Windows.Forms.Button btDelete;
    internal MediaPortal.UserInterface.Controls.MPButton btSwitch;
    internal System.Windows.Forms.Button btAdd;
    private MediaPortal.UserInterface.Controls.MPLabel lbArtists;
    private MediaPortal.UserInterface.Controls.MPLabel lbTitle;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbEditLyrics;
    internal MediaPortal.UserInterface.Controls.MPButton btSave;
    internal MediaPortal.UserInterface.Controls.MPButton btSearchSingle;
    private System.Windows.Forms.Label label1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel4;
    private System.Windows.Forms.TextBox textBox1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel5;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox5;
    private MediaPortal.UserInterface.Controls.MPButton mpButton1;
    private MediaPortal.UserInterface.Controls.MPButton mpButton2;
    private MediaPortal.UserInterface.Controls.MPComboBox mpComboBox1;
    private System.Windows.Forms.TreeView treeView1;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel6;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox6;
    private MediaPortal.UserInterface.Controls.MPButton mpButton3;
    private MediaPortal.UserInterface.Controls.MPButton mpButton4;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox7;
    private MediaPortal.UserInterface.Controls.MPButton mpButton5;
    private System.Windows.Forms.Button button3;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel7;
    private MediaPortal.UserInterface.Controls.MPLabel mpLabel8;
    private MediaPortal.UserInterface.Controls.MPGroupBox mpGroupBox8;
    private MediaPortal.UserInterface.Controls.MPButton mpButton6;
    private MediaPortal.UserInterface.Controls.MPButton mpButton7;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbExportTags;
  }
}
