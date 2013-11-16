namespace MyLyrics
{
  partial class AddNewSong
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
      this.gbAddNew = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbLyric = new System.Windows.Forms.TextBox();
      this.lbLyric = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbTitle = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lbTitle = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbArtist = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.lbArtist = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btOK = new MediaPortal.UserInterface.Controls.MPButton();
      this.btClose = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbAddNew.SuspendLayout();
      this.SuspendLayout();
      // 
      // gbAddNew
      // 
      this.gbAddNew.Controls.Add(this.tbLyric);
      this.gbAddNew.Controls.Add(this.lbLyric);
      this.gbAddNew.Controls.Add(this.tbTitle);
      this.gbAddNew.Controls.Add(this.lbTitle);
      this.gbAddNew.Controls.Add(this.tbArtist);
      this.gbAddNew.Controls.Add(this.lbArtist);
      this.gbAddNew.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.gbAddNew.Location = new System.Drawing.Point(9, 12);
      this.gbAddNew.Name = "gbAddNew";
      this.gbAddNew.Size = new System.Drawing.Size(508, 283);
      this.gbAddNew.TabIndex = 3;
      this.gbAddNew.TabStop = false;
      this.gbAddNew.Text = "Song Info";
      // 
      // tbLyric
      // 
      this.tbLyric.Location = new System.Drawing.Point(64, 80);
      this.tbLyric.Multiline = true;
      this.tbLyric.Name = "tbLyric";
      this.tbLyric.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.tbLyric.Size = new System.Drawing.Size(428, 196);
      this.tbLyric.TabIndex = 5;
      this.tbLyric.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbLyric_KeyUp);
      // 
      // lbLyric
      // 
      this.lbLyric.AutoSize = true;
      this.lbLyric.Location = new System.Drawing.Point(9, 83);
      this.lbLyric.Name = "lbLyric";
      this.lbLyric.Size = new System.Drawing.Size(32, 13);
      this.lbLyric.TabIndex = 4;
      this.lbLyric.Text = "Lyric:";
      // 
      // tbTitle
      // 
      this.tbTitle.BorderColor = System.Drawing.Color.Empty;
      this.tbTitle.Location = new System.Drawing.Point(64, 47);
      this.tbTitle.Name = "tbTitle";
      this.tbTitle.Size = new System.Drawing.Size(226, 20);
      this.tbTitle.TabIndex = 3;
      this.tbTitle.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbTitle_KeyUp);
      // 
      // lbTitle
      // 
      this.lbTitle.AutoSize = true;
      this.lbTitle.Location = new System.Drawing.Point(9, 50);
      this.lbTitle.Name = "lbTitle";
      this.lbTitle.Size = new System.Drawing.Size(30, 13);
      this.lbTitle.TabIndex = 2;
      this.lbTitle.Text = "Title:";
      // 
      // tbArtist
      // 
      this.tbArtist.BorderColor = System.Drawing.Color.Empty;
      this.tbArtist.Location = new System.Drawing.Point(64, 17);
      this.tbArtist.Name = "tbArtist";
      this.tbArtist.Size = new System.Drawing.Size(226, 20);
      this.tbArtist.TabIndex = 1;
      this.tbArtist.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbArtist_KeyUp);
      // 
      // lbArtist
      // 
      this.lbArtist.AutoSize = true;
      this.lbArtist.Location = new System.Drawing.Point(9, 20);
      this.lbArtist.Name = "lbArtist";
      this.lbArtist.Size = new System.Drawing.Size(33, 13);
      this.lbArtist.TabIndex = 0;
      this.lbArtist.Text = "Artist:";
      // 
      // btOK
      // 
      this.btOK.Enabled = false;
      this.btOK.Location = new System.Drawing.Point(377, 301);
      this.btOK.Name = "btOK";
      this.btOK.Size = new System.Drawing.Size(59, 23);
      this.btOK.TabIndex = 3;
      this.btOK.Text = "&OK";
      this.btOK.UseVisualStyleBackColor = true;
      this.btOK.Click += new System.EventHandler(this.btOK_Click);
      // 
      // btClose
      // 
      this.btClose.Location = new System.Drawing.Point(442, 301);
      this.btClose.Name = "btClose";
      this.btClose.Size = new System.Drawing.Size(59, 23);
      this.btClose.TabIndex = 4;
      this.btClose.Text = "&Close";
      this.btClose.UseVisualStyleBackColor = true;
      this.btClose.Click += new System.EventHandler(this.btClose_Click);
      // 
      // AddNewSong
      // 
      this.AcceptButton = this.btOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(517, 331);
      this.ControlBox = false;
      this.Controls.Add(this.btClose);
      this.Controls.Add(this.btOK);
      this.Controls.Add(this.gbAddNew);
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(533, 367);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(533, 367);
      this.Name = "AddNewSong";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add new lyric";
      this.gbAddNew.ResumeLayout(false);
      this.gbAddNew.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox gbAddNew;
    private MediaPortal.UserInterface.Controls.MPLabel lbTitle;
    private MediaPortal.UserInterface.Controls.MPTextBox tbArtist;
    private MediaPortal.UserInterface.Controls.MPLabel lbArtist;
    private MediaPortal.UserInterface.Controls.MPButton btOK;
    private MediaPortal.UserInterface.Controls.MPTextBox tbTitle;
    private System.Windows.Forms.TextBox tbLyric;
    private MediaPortal.UserInterface.Controls.MPLabel lbLyric;
    private MediaPortal.UserInterface.Controls.MPButton btClose;
  }
}