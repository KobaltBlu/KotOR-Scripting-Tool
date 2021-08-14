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

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private void SplashWindow_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            //this.TransparencyKey = Color.FromKnownColor(KnownColor.Control);
            this.Update();
            timer.Interval = 2500;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
