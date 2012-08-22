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
            this.label1 = new System.Windows.Forms.Label();
            this.lbInfo2 = new System.Windows.Forms.Label();
            this.lbInfo1 = new System.Windows.Forms.Label();
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
            this.lbIntro.Size = new System.Drawing.Size(480, 67);
            this.lbIntro.TabIndex = 0;
            this.lbIntro.Text = resources.GetString("lbIntro.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(29, 175);
            this.label1.MaximumSize = new System.Drawing.Size(480, 400);
            this.label1.MinimumSize = new System.Drawing.Size(480, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(480, 197);
            this.label1.TabIndex = 1;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // lbInfo2
            // 
            this.lbInfo2.AutoSize = true;
            this.lbInfo2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInfo2.Location = new System.Drawing.Point(26, 153);
            this.lbInfo2.Name = "lbInfo2";
            this.lbInfo2.Size = new System.Drawing.Size(456, 17);
            this.lbInfo2.TabIndex = 2;
            this.lbInfo2.Text = "This configuration section of the plugin consists of four parts:";
            // 
            // lbInfo1
            // 
            this.lbInfo1.AutoSize = true;
            this.lbInfo1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbInfo1.Location = new System.Drawing.Point(26, 30);
            this.lbInfo1.Name = "lbInfo1";
            this.lbInfo1.Size = new System.Drawing.Size(216, 17);
            this.lbInfo1.TabIndex = 3;
            this.lbInfo1.Text = "My Lyrics plugin, version 1.0";
            // 
            // Information
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbInfo1);
            this.Controls.Add(this.lbInfo2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbIntro);
            this.Name = "Information";
            this.Size = new System.Drawing.Size(539, 494);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbIntro;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbInfo2;
        private System.Windows.Forms.Label lbInfo1;
    }
}
