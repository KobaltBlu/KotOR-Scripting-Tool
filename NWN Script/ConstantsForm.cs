using MetroFramework.Forms;
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

    public partial class ConstantsForm : MetroForm
    {
        public string SelectedConstant { set; get; } // In .NET 3.0 or newer
        public ConstantsForm()
        {
            InitializeComponent();

            if(ThemeManager.GetCurrentTheme() == 0)
            {
                Theme = MetroFramework.MetroThemeStyle.Light;
            }
            else
            {
                Theme = MetroFramework.MetroThemeStyle.Dark;
            }

            foreach (ConstantListItem constant in ScriptEditorWindow.ConstantsList)
            {
                listBox1.Items.Add(constant.Name);
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


            foreach (ConstantListItem constant in ScriptEditorWindow.ConstantsList)
            {
                if (constant.Name.ToLower().Contains(constantsSearch.Text.ToLower()))
                {
                    listBox1.Items.Add(constant.Name);
                }
            }

            if (listBox1.Items.Count == 0)
            {
                listBox1.Items.Add("No functions found");
            }
        }
    }
}
