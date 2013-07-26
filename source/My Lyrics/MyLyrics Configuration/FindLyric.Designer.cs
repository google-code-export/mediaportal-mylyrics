using System.Windows.Forms;

namespace MyLyrics
{
  partial class FindLyric
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
            this.btCancel = new System.Windows.Forms.Button();
            this.lbArtist = new System.Windows.Forms.Label();
            this.btFind = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvSearchResults = new System.Windows.Forms.ListView();
            this.cbSite = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cbResult = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLyric = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbLyrics = new System.Windows.Forms.TextBox();
            this.gbLyricSites = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.singleRunSitesList = new System.Windows.Forms.CheckedListBox();
            this.btClose = new System.Windows.Forms.Button();
            this.btUpdate = new System.Windows.Forms.Button();
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
            this.gbSearchInfo.Controls.Add(this.btCancel);
            this.gbSearchInfo.Controls.Add(this.lbArtist);
            this.gbSearchInfo.Controls.Add(this.btFind);
            this.gbSearchInfo.Location = new System.Drawing.Point(9, 12);
            this.gbSearchInfo.Name = "gbSearchInfo";
            this.gbSearchInfo.Size = new System.Drawing.Size(432, 97);
            this.gbSearchInfo.TabIndex = 0;
            this.gbSearchInfo.TabStop = false;
            this.gbSearchInfo.Text = "Song information";
            // 
            // tbTitle
            // 
            this.tbTitle.Location = new System.Drawing.Point(73, 42);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(344, 20);
            this.tbTitle.TabIndex = 2;
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(15, 45);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(30, 13);
            this.lbTitle.TabIndex = 2;
            this.lbTitle.Text = "Title:";
            // 
            // tbArtist
            // 
            this.tbArtist.Location = new System.Drawing.Point(73, 19);
            this.tbArtist.Name = "tbArtist";
            this.tbArtist.Size = new System.Drawing.Size(344, 20);
            this.tbArtist.TabIndex = 1;
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(354, 68);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(65, 23);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "&Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // lbArtist
            // 
            this.lbArtist.AutoSize = true;
            this.lbArtist.Location = new System.Drawing.Point(15, 22);
            this.lbArtist.Name = "lbArtist";
            this.lbArtist.Size = new System.Drawing.Size(33, 13);
            this.lbArtist.TabIndex = 0;
            this.lbArtist.Text = "Artist:";
            // 
            // btFind
            // 
            this.btFind.Location = new System.Drawing.Point(283, 68);
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
            this.groupBox1.Location = new System.Drawing.Point(9, 221);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(432, 177);
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
            this.lvSearchResults.Size = new System.Drawing.Size(399, 150);
            this.lvSearchResults.TabIndex = 10;
            this.lvSearchResults.UseCompatibleStateImageBehavior = false;
            this.lvSearchResults.View = System.Windows.Forms.View.Details;
            this.lvSearchResults.SelectedIndexChanged += new System.EventHandler(this.lvSearchResults_SelectedIndexChanged);
            this.lvSearchResults.DoubleClick += new System.EventHandler(this.lvSearchResults_DoubleClick);
            // 
            // cbSite
            // 
            this.cbSite.Text = "Site";
            this.cbSite.Width = 113;
            // 
            // cbResult
            // 
            this.cbResult.Text = "Result";
            this.cbResult.Width = 52;
            // 
            // chLyric
            // 
            this.chLyric.Text = "Lyric";
            this.chLyric.Width = 223;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tbLyrics);
            this.groupBox2.Location = new System.Drawing.Point(9, 404);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 136);
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
            this.tbLyrics.Size = new System.Drawing.Size(399, 111);
            this.tbLyrics.TabIndex = 0;
            // 
            // gbLyricSites
            // 
            this.gbLyricSites.Controls.Add(this.singleRunSitesList);
            this.gbLyricSites.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gbLyricSites.Location = new System.Drawing.Point(9, 115);
            this.gbLyricSites.Name = "gbLyricSites";
            this.gbLyricSites.Size = new System.Drawing.Size(432, 100);
            this.gbLyricSites.TabIndex = 29;
            this.gbLyricSites.TabStop = false;
            this.gbLyricSites.Text = "Lyric sites to search";
            // 
            // singleRunSitesList
            // 
            this.singleRunSitesList.FormattingEnabled = true;
            this.singleRunSitesList.Location = new System.Drawing.Point(7, 20);
            this.singleRunSitesList.MultiColumn = true;
            this.singleRunSitesList.Name = "singleRunSitesList";
            this.singleRunSitesList.ScrollAlwaysVisible = true;
            this.singleRunSitesList.Size = new System.Drawing.Size(419, 79);
            this.singleRunSitesList.TabIndex = 0;
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point(361, 546);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(65, 23);
            this.btClose.TabIndex = 30;
            this.btClose.Text = "Close";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.btClose_Click);
            // 
            // btUpdate
            // 
            this.btUpdate.Enabled = false;
            this.btUpdate.Location = new System.Drawing.Point(292, 546);
            this.btUpdate.Name = "btUpdate";
            this.btUpdate.Size = new System.Drawing.Size(65, 23);
            this.btUpdate.TabIndex = 11;
            this.btUpdate.Text = "Update";
            this.btUpdate.UseVisualStyleBackColor = true;
            this.btUpdate.Click += new System.EventHandler(this.btUpdate_Click);
            // 
            // FindLyric
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 573);
            this.ControlBox = false;
            this.Controls.Add(this.btUpdate);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.gbLyricSites);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbSearchInfo);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(464, 611);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(464, 611);
            this.Name = "FindLyric";
            this.ShowIcon = false;
            this.Text = "Fetch lyric dialog";
            this.gbSearchInfo.ResumeLayout(false);
            this.gbSearchInfo.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.gbLyricSites.ResumeLayout(false);
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
    private System.Windows.Forms.ListView lvSearchResults;
    private System.Windows.Forms.ColumnHeader cbSite;
    private System.Windows.Forms.ColumnHeader cbResult;
    private System.Windows.Forms.ColumnHeader chLyric;
    private System.Windows.Forms.Button btClose;
    private System.Windows.Forms.Button btUpdate;
    private System.Windows.Forms.Button btCancel;
    internal System.Windows.Forms.CheckedListBox singleRunSitesList;
  }
}