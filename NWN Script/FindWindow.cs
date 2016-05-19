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
    public partial class FindWindow : Form
    {
        public event EventHandler FindEvent;
        public static bool bool_MatchCase = false;
        public string toFind = "";

        public FindWindow()
        {
            InitializeComponent();
        }

        protected void OnFindEvent()
        {            
            this.FindEvent(this, new FindArgs(textBox1.Text));
        }

        public class FindArgs : EventArgs
        {
            private string message;
            private bool matchCase;

            public FindArgs(string message)
            {
                this.message = message;
                this.matchCase = bool_MatchCase;
            }

            // This is a straightforward implementation for 
            // declaring a public field
            public string Message
            {
                get
                {
                    return message;
                }
            }

            public bool MatchCase
            {
                get
                {
                    return matchCase;
                }
            }
        }


        private void btn_find_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != ""){
                this.OnFindEvent();
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btn_find_Click(btn_find, EventArgs.Empty);
            }
        }

        private void checkMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            bool_MatchCase = checkMatchCase.Checked;
        }

        private void FindWindow_Load(object sender, EventArgs e)
        {
            textBox1.Text = toFind;
        }


    }
    
}
