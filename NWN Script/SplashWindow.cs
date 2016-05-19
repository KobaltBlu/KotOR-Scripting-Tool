using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NWN_Script
{
    public partial class SplashWindow : Form
    {
        public SplashWindow()
        {
            InitializeComponent();
            /*this.SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(System.Windows.Forms.ControlStyles.Opaque, true);

            this.SetStyle(System.Windows.Forms.ControlStyles.UserPaint, true );*/


        }

        private void SplashWindow_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            //this.TransparencyKey = Color.FromKnownColor(KnownColor.Control);
            this.Update();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
