using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NWN_Script
{
    class ThemeManager
    {
        static int CurrentTheme = Properties.Settings.Default.WindowTheme;

        static public int GetCurrentTheme()
        {
            //return 0; // Light
            return ThemeManager.CurrentTheme; // Dark
        }


        static public void SetCurrentTheme(int index = 0)
        {
            ThemeManager.CurrentTheme = index;
            Properties.Settings.Default.WindowTheme = index;
            Properties.Settings.Default.Save();
        }

        static public Color GetControlBackgroundColor()
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                //return Color.FromArgb(22, 40, 49);
                return Color.FromArgb(30, 30, 30);
            }
            return Color.White;
        }

        static public Color GetControlAltBackgroundColor()
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                //return Color.FromArgb(22, 40, 49);
                return Color.FromArgb(60, 60, 60);
            }
            return Form.DefaultBackColor;
        }

        static public Color GetControlForeColor()
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                return Color.WhiteSmoke;
            }
            return Form.DefaultForeColor;
        }

        static public Color GetListBoxForeColor()
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                return Color.FromArgb(204, 204, 204);
            }
            return Form.DefaultForeColor;
        }

        static public MetroFramework.MetroThemeStyle GetMetroTheme()
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                return MetroFramework.MetroThemeStyle.Dark;
            }
            return MetroFramework.MetroThemeStyle.Light;
        }

        static public void ConfigureScintillaControlTheme(ScintillaNET.Scintilla control)
        {
            if (ThemeManager.GetCurrentTheme() == 1)
            {
                control.ConfigurationManager.CustomLocation = "ScintillaNET.xml";
                control.ConfigurationManager.Language = "nss_dark";
                control.ConfigurationManager.Configure();
                control.Caret.Color = Color.FromArgb(204, 204, 204);
                control.Margins.FoldMarginColor = ThemeManager.GetControlAltBackgroundColor();
                control.Margins.FoldMarginHighlightColor = ThemeManager.GetControlAltBackgroundColor();
                //control.BackColor = GetControlBackgroundColor(); //Breaks highlighting
                control.BorderStyle = System.Windows.Forms.BorderStyle.None;
                control.Update();
            }
            else
            {
                control.ConfigurationManager.CustomLocation = "ScintillaNET.xml";
                control.ConfigurationManager.Language = "nss_light";
                control.ConfigurationManager.Configure();
                control.Caret.Color = Color.Black;
                control.Margins.FoldMarginColor = Color.LightGray;
                control.Margins.FoldMarginHighlightColor = Color.LightGray;
                //control.BackColor = GetControlBackgroundColor(); //Breaks highlighting
                control.BorderStyle = System.Windows.Forms.BorderStyle.None;
                control.Update();
            }
            //control.BackColor = ThemeManager.GetControlBackgroundColor();
        }



        static public void UpdateMenuStripTheme(MenuStrip menuStrip)
        {
            menuStrip.BackColor = ThemeManager.GetControlAltBackgroundColor();
            menuStrip.ForeColor = ThemeManager.GetControlForeColor();
            foreach (ToolStripItem child in menuStrip.Items)
            {
                if (child.GetType() == typeof(ToolStripMenuItem))
                {
                    ThemeManager.UpdateMenuItemTheme((ToolStripMenuItem)child);
                }
                else if (child.GetType() == typeof(ToolStripSeparator))
                {
                    ThemeManager.UpdateMenuItemTheme((ToolStripSeparator)child);
                }
            }
        }

        static public void UpdateMenuItemTheme(ToolStripMenuItem menuItem)
        {
            menuItem.BackColor = ThemeManager.GetControlAltBackgroundColor();
            menuItem.ForeColor = ThemeManager.GetControlForeColor();
            foreach (ToolStripItem child in menuItem.DropDownItems)
            {
                if (child.GetType() == typeof(ToolStripMenuItem))
                {
                    ThemeManager.UpdateMenuItemTheme((ToolStripMenuItem)child);
                }
                else if (child.GetType() == typeof(ToolStripSeparator))
                {
                    ThemeManager.UpdateMenuItemTheme((ToolStripSeparator)child);
                }
            }
        }

        static public void UpdateMenuItemTheme(ToolStripSeparator menuItem)
        {
            menuItem.BackColor = ThemeManager.GetControlBackgroundColor();
            menuItem.ForeColor = ThemeManager.GetControlForeColor();
        }

    }



    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin
        {
            get { return Color.FromArgb(128, Color.Black); }
        }

        public override Color MenuStripGradientEnd
        {
            get { return Color.FromArgb(128, Color.Black); }
        }

        public override Color ButtonSelectedHighlight
        {
            get { return Color.FromArgb(64, Color.Black); }
        }

        // etc
    }

}
