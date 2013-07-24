namespace MyLyrics
{
  partial class Information
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Information));
            this.lbIntro = new System.Windows.Forms.Label();
            this.lbInfo2 = new System.Windows.Forms.Label();
            this.lbInfo1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.supportedSitesPanel = new System.Windows.Forms.Panel();
            this.supportedSites = new System.Windows.Forms.DataGridView();
            this.Site = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Url = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label8 = new System.Windows.Forms.Label();
            this.linkLabelForum = new System.Windows.Forms.LinkLabel();
            this.label11 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.supportedSitesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.supportedSites)).BeginInit();
            this.SuspendLayout();
            // 
            // lbIntro
            // 
            this.lbIntro.AutoSize = true;
            this.lbIntro.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbIntro.Location = new System.Drawing.Point(29, 57);
            this.lbIntro.MaximumSize = new System.Drawing.Size(480, 0);
            this.lbIntro.MinimumSize = new System.Drawing.Size(480, 67);
            this.lbIntro.Name = "lbIntro";
            this.lbIntro.Size = new System.Drawing.Size(480, 80);
            this.lbIntro.TabIndex = 0;
            this.lbIntro.Text = resources.GetString("lbIntro.Text");
            // 
            // lbInfo2
            // 
            this.lbInfo2.AutoSize = true;
            this.lbInfo2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInfo2.ForeColor = System.Drawing.Color.Black;
            this.lbInfo2.Location = new System.Drawing.Point(26, 137);
            this.lbInfo2.Name = "lbInfo2";
            this.lbInfo2.Size = new System.Drawing.Size(117, 18);
            this.lbInfo2.TabIndex = 2;
            this.lbInfo2.Text = "Supported Sites:";
            // 
            // lbInfo1
            // 
            this.lbInfo1.AutoSize = true;
            this.lbInfo1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInfo1.ForeColor = System.Drawing.Color.Black;
            this.lbInfo1.Location = new System.Drawing.Point(26, 30);
            this.lbInfo1.Name = "lbInfo1";
            this.lbInfo1.Size = new System.Drawing.Size(201, 18);
            this.lbInfo1.TabIndex = 3;
            this.lbInfo1.Text = "MyLyrics plugin, version 1.8.0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 18);
            this.label2.MaximumSize = new System.Drawing.Size(460, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 4;
            // 
            // supportedSitesPanel
            // 
            this.supportedSitesPanel.AutoScroll = true;
            this.supportedSitesPanel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.supportedSitesPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.supportedSitesPanel.Controls.Add(this.supportedSites);
            this.supportedSitesPanel.Controls.Add(this.label8);
            this.supportedSitesPanel.Controls.Add(this.label2);
            this.supportedSitesPanel.Location = new System.Drawing.Point(29, 158);
            this.supportedSitesPanel.Name = "supportedSitesPanel";
            this.supportedSitesPanel.Size = new System.Drawing.Size(480, 145);
            this.supportedSitesPanel.TabIndex = 8;
            // 
            // supportedSites
            // 
            this.supportedSites.AllowUserToAddRows = false;
            this.supportedSites.AllowUserToDeleteRows = false;
            this.supportedSites.AllowUserToResizeColumns = false;
            this.supportedSites.AllowUserToResizeRows = false;
            this.supportedSites.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.supportedSites.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.supportedSites.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.supportedSites.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.supportedSites.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.supportedSites.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.supportedSites.ColumnHeadersVisible = false;
            this.supportedSites.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Site,
            this.Url});
            this.supportedSites.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.supportedSites.Location = new System.Drawing.Point(6, 3);
            this.supportedSites.MultiSelect = false;
            this.supportedSites.Name = "supportedSites";
            this.supportedSites.ReadOnly = true;
            this.supportedSites.RowHeadersVisible = false;
            this.supportedSites.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.supportedSites.Size = new System.Drawing.Size(450, 131);
            this.supportedSites.TabIndex = 13;
            this.supportedSites.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // Site
            // 
            this.Site.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Site.FillWeight = 30F;
            this.Site.HeaderText = "Site";
            this.Site.Name = "Site";
            this.Site.ReadOnly = true;
            this.Site.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Site.Width = 135;
            // 
            // Url
            // 
            this.Url.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Url.FillWeight = 70F;
            this.Url.HeaderText = "Url";
            this.Url.Name = "Url";
            this.Url.ReadOnly = true;
            this.Url.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Url.Width = 315;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 218);
            this.label8.MaximumSize = new System.Drawing.Size(460, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 13);
            this.label8.TabIndex = 11;
            // 
            // linkLabelForum
            // 
            this.linkLabelForum.AutoSize = true;
            this.linkLabelForum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelForum.ForeColor = System.Drawing.Color.Black;
            this.linkLabelForum.LinkColor = System.Drawing.SystemColors.MenuHighlight;
            this.linkLabelForum.Location = new System.Drawing.Point(354, 456);
            this.linkLabelForum.Name = "linkLabelForum";
            this.linkLabelForum.Size = new System.Drawing.Size(168, 15);
            this.linkLabelForum.TabIndex = 9;
            this.linkLabelForum.TabStop = true;
            this.linkLabelForum.Tag = "http://forum.team-mediaportal.com/forums/my-lyrics-plugin.163/";
            this.linkLabelForum.Text = "MyLyrics plugin forum section";
            this.linkLabelForum.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelForum_LinkClicked);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.Color.Black;
            this.label11.Location = new System.Drawing.Point(34, 456);
            this.label11.MaximumSize = new System.Drawing.Size(460, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(314, 15);
            this.label11.TabIndex = 14;
            this.label11.Text = "For bugs, issues and feature requests use the MP forum:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(118, 330);
            this.label1.MinimumSize = new System.Drawing.Size(300, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(300, 110);
            this.label1.TabIndex = 15;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(240, 306);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 18);
            this.label3.TabIndex = 16;
            this.label3.Text = "Credits:";
            // 
            // Information
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.linkLabelForum);
            this.Controls.Add(this.supportedSitesPanel);
            this.Controls.Add(this.lbInfo1);
            this.Controls.Add(this.lbInfo2);
            this.Controls.Add(this.lbIntro);
            this.Name = "Information";
            this.Size = new System.Drawing.Size(539, 494);
            this.supportedSitesPanel.ResumeLayout(false);
            this.supportedSitesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.supportedSites)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lbIntro;
    private System.Windows.Forms.Label lbInfo2;
    private System.Windows.Forms.Label lbInfo1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel supportedSitesPanel;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.LinkLabel linkLabelForum;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.DataGridView supportedSites;
      private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.DataGridViewTextBoxColumn SiteName;
    private System.Windows.Forms.DataGridViewTextBoxColumn Url;
    private System.Windows.Forms.DataGridViewTextBoxColumn Site;
  }
}
