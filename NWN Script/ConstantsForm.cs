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

    public partial class ConstantsForm : Form
    {
        public string SelectedConstant { set; get; } // In .NET 3.0 or newer
        public ConstantsForm()
        {
            InitializeComponent();


            foreach (string CONSTANT in ScriptEditorWindow.CONSTANTS_LIST)
            {
                listBox1.Items.Add(CONSTANT);
            }

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string selected = listBox1.SelectedItem.ToString();
            this.SelectedConstant = selected;
            this.DialogResult = DialogResult.OK; 
            
        }

        private void constantsSearch_KeyUp(object sender, KeyEventArgs e)
        {
            listBox1.Items.Clear();


            foreach (string constant in ScriptEditorWindow.CONSTANTS_LIST)
            {
                if (constant.ToLower().Contains(constantsSearch.Text.ToLower()))
                {
                    listBox1.Items.Add(constant);
                }
            }

            if (listBox1.Items.Count == 0)
            {
                listBox1.Items.Add("No functions found");
            }
        }
    }
}
