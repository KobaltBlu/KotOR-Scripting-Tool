using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NWN_Script
{
    public partial class FindAndReplace : Form
    {
        public event EventHandler ReplaceEvent;
        public event EventHandler ReplaceAllEvent;
        public static bool bool_MatchCase = false;
        public static bool bool_InSelection = false;

        public string toFind = "";
        public FindAndReplace()
        {
            InitializeComponent();
        }

        protected void OnReplaceEvent()
        {
            this.ReplaceEvent(this, new ReplaceArgs(textBox_FindWhat.Text, textBox_ReplaceWith.Text));
        }

        protected void OnReplaceAllEvent()
        {
            this.ReplaceAllEvent(this, new ReplaceArgs(textBox_FindWhat.Text, textBox_ReplaceWith.Text));
        }

        public class ReplaceArgs : EventArgs
        {
            private string toFind;
            private string toReplace;
            private bool matchCase;
            private bool inSelection;

            public ReplaceArgs(string toFind, string toReplace)
            {
                this.toFind = toFind;
                this.toReplace = toReplace;
                this.matchCase = bool_MatchCase;
                this.inSelection = bool_InSelection;
            }

            // This is a straightforward implementation for 
            // declaring a public field
            public string ToFind
            {
                get
                {
                    return toFind;
                }
            }

            public string ToReplace
            {
                get
                {
                    return toReplace;
                }
            }

            public bool MatchCase
            {
                get
                {
                    return matchCase;
                }
            }

            public bool InSelection
            {
                get
                {
                    return inSelection;
                }
            }
        }

        private void btn_replace_Click(object sender, EventArgs e)
        {
            if (textBox_FindWhat.Text != "")
            {
                this.OnReplaceEvent();
            }
        }

        private void btn_ReplaceAll_Click(object sender, EventArgs e)
        {
            if (textBox_FindWhat.Text != "")
            {
                this.OnReplaceAllEvent();
            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            bool_MatchCase = checkMatchCase.Checked;
        }

        private void checkInSelection_CheckedChanged(object sender, EventArgs e)
        {
            bool_InSelection = checkInSelection.Checked;
        }

        private void FindAndReplace_Load(object sender, EventArgs e)
        {
            textBox_FindWhat.Text = toFind;
        }

        private void FindAndReplace_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        /*public void DrawRoundRect(Graphics g, Pen p, float X, float Y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            SolidBrush brush = new SolidBrush(SystemColors.ControlLight);


            gp.AddLine(X + radius, Y, X + width - (radius * 2), Y);
            gp.AddArc(X + width - (radius * 2), Y, radius * 2, radius * 2, 270, 90);
            gp.AddLine(X + width, Y + radius, X + width, Y + height - (radius * 2));
            gp.AddArc(X + width - (radius * 2), Y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            gp.AddLine(X + width - (radius * 2), Y + height, X + radius, Y + height);
            gp.AddArc(X, Y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddLine(X, Y + height - (radius * 2), X, Y + radius);
            gp.AddArc(X, Y, radius * 2, radius * 2, 180, 90);
            gp.CloseFigure();
            g.FillPath(brush, gp);
            g.DrawPath(p, gp);
            gp.Dispose();
        }*/



    }
}
