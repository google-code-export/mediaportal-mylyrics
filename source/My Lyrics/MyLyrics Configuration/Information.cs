using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace MyLyrics
{
    public partial class Information : UserControl
    {
        Form parent;

        public Information(Form parent)
        {
            this.parent = parent;
            InitializeComponent();
        }
    }
}
