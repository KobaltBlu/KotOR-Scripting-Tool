using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using MetroFramework.Controls;

namespace NWN_Script
{
    public partial class ScriptEditorWindow : MetroForm
    {

        private string FormTitle = "KotOR Scripting Tool";
        private int LastFindIndex = 0;
        private bool allowAutoComplete = true;
        public static List<FunctionsListItem> FunctionsList = new List<FunctionsListItem>();
        public static List<ConstantListItem> ConstantsList = new List<ConstantListItem>();
        System.Windows.Forms.Timer AutoSaveTimer = new System.Windows.Forms.Timer();

        //Splash Screen function
        private void SplashScreen()
        {
            SplashWindow theSplashForm = new SplashWindow();
            //theSplashForm.ShowDialog();
        }

        private void UpdateFormTheme()
        {
            Theme = ThemeManager.GetMetroTheme();
            metroStyleManager.Theme = ThemeManager.GetMetroTheme();

            //functionInfoView.BackColor = Color.FromArgb(48, 48, 61);

            listBox1.BackColor = ThemeManager.GetControlBackgroundColor();
            ThemeManager.ConfigureScintillaControlTheme(functionInfoView);
            foreach(EditorTabPage tab in scriptTabs.TabPages)
            {
                ThemeManager.ConfigureScintillaControlTheme(tab.editor);
                switch (ThemeManager.GetCurrentTheme())
                {
                    case 0:
                        tab.vScroll.Theme = MetroFramework.MetroThemeStyle.Light;
                        tab.hScroll.Theme = MetroFramework.MetroThemeStyle.Light;
                        break;
                    case 1:
                        tab.vScroll.Theme = MetroFramework.MetroThemeStyle.Dark;
                        tab.hScroll.Theme = MetroFramework.MetroThemeStyle.Dark;
                        break;
                }
            }

            //labelFunctions1.ForeColor = ThemeManager.GetControlForeColor();
            functionSearch.BackColor = ThemeManager.GetControlAltBackgroundColor();
            functionSearch.Refresh();

            splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            splitContainer1.Panel1.BackColor = ThemeManager.GetControlAltBackgroundColor();
            splitContainer1.Panel2.BackColor = ThemeManager.GetControlAltBackgroundColor();
            splitContainer2.Panel1.BackColor = ThemeManager.GetControlAltBackgroundColor();
            splitContainer2.Panel2.BackColor = ThemeManager.GetControlAltBackgroundColor();

            labelFunctionInfo.ForeColor = ThemeManager.GetControlForeColor();

            lightModeToolStripMenuItem.Checked = ThemeManager.GetCurrentTheme() == 0;
            darkModeToolStripMenuItem.Checked = ThemeManager.GetCurrentTheme() == 1;

            switch (ThemeManager.GetCurrentTheme())
            {
                case 0:
                    metroStyleManager.Style = MetroFramework.MetroColorStyle.Black;
                    break;
                case 1:
                    metroStyleManager.Style = MetroFramework.MetroColorStyle.White;
                    break;
            }

            this.Refresh();

        }

        //Creates and initializes new editor tab
        private EditorTabPage CreateNewEditorTab( FileSettings fileSettings )
        {
            EditorTabPage newPage = new EditorTabPage(fileSettings);
            scriptTabs.TabPages.Add(newPage);
            scriptTabs.SelectedTab = newPage;
            return newPage;
        }

        //Updates the Forms Title with the current selected games name
        private void UpdateFormInfo()
        {
            if (GameManager.GetCurrentGame() != null)
            {
                this.Text = FormTitle + " - [" + GameManager.GetCurrentGame().Name + " Mode]";
                this.Refresh();
            }
            else
            {
                //Oops we have a problem.
            }
        }

        //Compiles the current tab's script
        private void CompileScript()
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                tab.CompileFile();
            }
        }

        private void updateGameInfo()
        {
            FunctionsList = GameManager.GetCurrentGame().NWScript.Functions;
            ConstantsList = GameManager.GetCurrentGame().NWScript.Constants;
            switch (GameManager.CurrentGame)
            {
                case 0:
                    kOTORIToolStripMenuItem2.CheckState = CheckState.Checked;
                    kOTORIIToolStripMenuItem2.CheckState = CheckState.Unchecked;
                    break;
                case 1:
                    kOTORIToolStripMenuItem2.CheckState = CheckState.Unchecked;
                    kOTORIIToolStripMenuItem2.CheckState = CheckState.Checked;
                    break;
            }

            //Sort the FunctionsList
            FunctionsList = FunctionsList.OrderBy(c => c.Name).ToList();

            //Clear the FunctionsList listBox control
            listBox1.Items.Clear();
            foreach (FunctionsListItem item in FunctionsList)
            {
                listBox1.Items.Add(item.Name);
            }

            //Sort the FunctionsList
            ConstantsList = ConstantsList.OrderBy(c => c.Name).ToList();

            //Clear the ConstantsList listBox control
            listBox2.Items.Clear();
            foreach (ConstantListItem item in ConstantsList)
            {
                listBox2.Items.Add(item.Name);
            }

            UpdateFormInfo();
        }

        public ScriptEditorWindow()
        {
            InitializeComponent();
            functionInfoView = new ScintillaNET.Scintilla();
        }

        private void InitializeGames()
        {
            GameManager.Games.Clear();

            Game gKotOR = new Game(
                "KotOR I",
                String.IsNullOrEmpty(Properties.Settings.Default.KotORI_Directory) ? Properties.Settings.Default.KotORI_Directory : null,
                "k1"
            );
            gKotOR.SetMenuIconResource(Properties.Resources.k1);
            //Parse the KotOR's nwscript.nss file and store all the info for functions and constants
            gKotOR.ParseNWScript();
            GameManager.Games.Add(gKotOR);

            Game gTSL = new Game(
                "KotOR II",
                String.IsNullOrEmpty(Properties.Settings.Default.KotORII_Directory) ? Properties.Settings.Default.KotORII_Directory : null,
                "k2"
            );
            gTSL.SetMenuIconResource(Properties.Resources.k2);
            //Parse the TSL's nwscript.nss file and store all the info for functions and constants
            gTSL.ParseNWScript();
            GameManager.Games.Add(gTSL);

            updateGameInfo();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AutoSaveTimer.Interval = 5000;
            AutoSaveTimer.Tick += new EventHandler(AutoSaveOpenFileStates);
            InitializeGames();
            UpdateFormTheme();

            FileHistoryManager.Initialize();
            if(FileHistoryManager.OpenFiles.Count > 0)
            {
                OpenFiles(FileHistoryManager.OpenFiles.ToArray());
                scriptTabs.SelectedIndex = Properties.Settings.Default.SelectedTabIndex;
            }

            //Open any files that started the application
            string[] args = Environment.GetCommandLineArgs();
            List<string> newArgs = args.ToList<string>();
            //Strip the first string from the list because the first arg is the app itself
            newArgs.RemoveAt(0);

            args = newArgs.ToArray<string>();
            

            if (args.Length >= 1)
            {
                OpenFiles(args, true);
            }
            else if(FileHistoryManager.OpenFiles.Count == 0)
            {
                CreateNewEditorTab(FileSettings.NewPageFile());
            }

            FileHistoryManager.OnRecentFilesChanged(() =>
            {
                UpdateRecentFilesMenu();
            });

            UpdateRecentFilesMenu();

            this.BringToFront();
            this.Focus();
            this.BringToFront();
            this.Show();
            this.WindowState = FormWindowState.Normal;

            UpdateTabSpacing();

            scriptTabs.SelectedIndexChanged += (sender2, e2) =>
            {
                Properties.Settings.Default.SelectedTabIndex = scriptTabs.SelectedIndex;
                Properties.Settings.Default.Save();
            };

            //I moved functionInfoView here because it breaks the Form Designer otherwise
            // 
            // functionInfo
            // 
            functionInfoView.Anchor = ((((AnchorStyles.Top | AnchorStyles.Bottom)
                        | AnchorStyles.Left)
                        | AnchorStyles.Right));
            ThemeManager.ConfigureScintillaControlTheme(functionInfoView);
            functionInfoView.IsReadOnly = true;
            functionInfoView.Location = new Point(7, 23);
            functionInfoView.Name = "functionInfo Window";
            functionInfoView.Size = new Size(1323, 276);
            functionInfoView.Styles.BraceBad.Size = 6F;
            functionInfoView.Styles.BraceLight.Size = 6F;
            functionInfoView.Styles.ControlChar.Size = 6F;
            functionInfoView.Styles.Default.Size = 6F;
            functionInfoView.Styles.IndentGuide.Size = 6F;
            functionInfoView.Styles.LastPredefined.Size = 6F;
            functionInfoView.Styles.LineNumber.Size = 6F;
            //functionInfoView.Dock = DockStyle.Fill;
            functionInfoView.Styles.Max.Size = 6F;
            functionInfoView.TabIndex = 11;
            functionInfoView.Margins.Margin0.Width = 25;
            functionInfoView.Margins.Margin2.Width = 20;
            splitContainer1.Panel2.Controls.Add(functionInfoView);
            splitContainer1.Panel2.Controls.SetChildIndex(functionInfoView, 0);

            SplashScreen();
            //FileAssociation.Associate(".nss", "nss.nwscript", "SWKotOR Script Source", "\\Resources\\icon.ico", "Kotor Scripting Tool.exe");
            //FileAssociation.Associate(".ncs", "ncs.nwscript", "SWKotOR Script Compiled", "\\Resources\\icon.ico", "Kotor Scripting Tool.exe");
            AutoSaveTimer.Enabled = true;
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            FileHistoryManager.Save();
            AutoSaveOpenFileStates(null, null);
        }

        private void AutoSaveOpenFileStates(object Sender, EventArgs e)
        {
            foreach (EditorTabPage tab in scriptTabs.TabPages)
            {
                tab.SaveState();
            }
        }

        private void UpdateRecentFilesMenu()
        {
            Debug.WriteLine("UpdateRecentFilesMenu: "+ FileHistoryManager.RecentFiles.Count);
            int count = 0;
            foreach(ToolStripMenuItem recentControl in recentToolStripMenuItem.DropDownItems)
            {
                recentControl.Visible = false;
                if (count < FileHistoryManager.RecentFiles.Count)
                {
                    string file = FileHistoryManager.RecentFiles[count];
                    if (!String.IsNullOrEmpty(file))
                    {
                        recentControl.Text = file;
                        recentControl.Enabled = true;
                        recentControl.Visible = true;
                        recentControl.Tag = file;
                    }
                }

                count++;
            }

            recentToolStripMenuItem.Invalidate();
        }

        private void recentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem recentControl = (ToolStripMenuItem)sender;
            OpenFile((string)recentControl.Tag, true);
        }

        private bool changeFunctionInfo(string Name)
        {
            functionInfoView.IsReadOnly = false;
            FunctionsListItem item = FunctionsList.Find(delegate(FunctionsListItem f) { return f.Name == Name; });
            if (item != null)
            {
                functionInfoView.Text = item.About;
            }
            functionInfoView.IsReadOnly = true;

            //Scroll to the bottom
            functionInfoView.Scrolling.ScrollBy(0, functionInfoView.Lines.Count);
            return (item != null);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void spacingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TabWidth = 2;
            Properties.Settings.Default.Save();
            UpdateTabSpacing();
        }

        private void spacingToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TabWidth = 4;
            Properties.Settings.Default.Save();
            UpdateTabSpacing();
        }

        private void UpdateTabSpacing()
        {
            if(Properties.Settings.Default.TabWidth == 2)
            {
                spacingToolStripMenuItem1.Checked = true;
                spacingToolStripMenuItem2.Checked = false;
            }
            else
            {
                spacingToolStripMenuItem1.Checked = false;
                spacingToolStripMenuItem2.Checked = true;
            }

            foreach (EditorTabPage tab in scriptTabs.TabPages)
            {
                tab.editor.Indentation.TabWidth = Properties.Settings.Default.TabWidth;
            }
        }

        /*
         * OnDoubleClick Event for the ListBox
         */

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string selected = listBox1.SelectedItem.ToString();

            FunctionsListItem item = FunctionsList.Find(delegate(FunctionsListItem f) { return f.Name == selected; });
            if (item != null)
            {
                ScintillaNET.Scintilla tabEditor = null;
                foreach (Control control in scriptTabs.SelectedTab.Controls)
                {
                    if (control.GetType() == typeof(ScintillaNET.Scintilla))
                    {
                        tabEditor = (ScintillaNET.Scintilla)control;
                    }
                }

                tabEditor.InsertText(tabEditor.Caret.Anchor, item.Code + "\n");
                tabEditor.Selection.Start = (tabEditor.Selection.Start + (item.Code.Length + 1));
            }
        }

        /*
         * OnDoubleClick Event for the ListBox
         */

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string selected = listBox2.SelectedItem.ToString();

            ConstantListItem item = ConstantsList.Find(delegate (ConstantListItem f) { return f.Name == selected; });
            if (item != null)
            {
                ScintillaNET.Scintilla tabEditor = null;
                foreach (Control control in scriptTabs.SelectedTab.Controls)
                {
                    if (control.GetType() == typeof(ScintillaNET.Scintilla))
                    {
                        tabEditor = (ScintillaNET.Scintilla)control;
                    }
                }

                tabEditor.InsertText(tabEditor.Caret.Anchor, item.Name + "\n");
                tabEditor.Selection.Start = (tabEditor.Selection.Start + (item.Name.Length + 1));
            }
        }

        /*
         * OnKeyUp Event for the FucntionsList Search TextBox
         */

        private void functionSearch_KeyUp(object sender, KeyEventArgs e)
        {
            listBox1.Items.Clear();
            bool textDifference = !functionSearch.Text.Equals(constantsSearch.Text);
            constantsSearch.Text = functionSearch.Text;
            if (textDifference)
            {
                constantsSearch_KeyUp(sender, e);
            }

            //Filter Functions
            try
            {
                FunctionsList.ForEach(delegate (FunctionsListItem f) {

                    if (f.Name.ToLower().Contains(functionSearch.Text.ToLower()))
                    {
                        listBox1.Items.Add(f.Name);
                    }

                });

                if (listBox1.Items.Count == 0) {
                    listBox1.Items.Add("No functions found");
                }
            }catch(Exception exception)
            {
                listBox1.Items.Add("No functions found");
            }
        }

        /*
         * OnKeyUp Event for the FucntionsList Search TextBox
         */

        private void constantsSearch_KeyUp(object sender, KeyEventArgs e)
        {
            listBox2.Items.Clear();
            bool textDifference = !functionSearch.Text.Equals(constantsSearch.Text);
            functionSearch.Text = constantsSearch.Text;
            if (textDifference)
            {
                constantsSearch_KeyUp(sender, e);
            }

            //Filter Constants
            try
            {
                ConstantsList.ForEach(delegate (ConstantListItem f) {

                    if (f.Name.ToLower().Contains(functionSearch.Text.ToLower()))
                    {
                        listBox2.Items.Add(f.Name);
                    }

                });

                if (listBox2.Items.Count == 0)
                {
                    listBox2.Items.Add("No constants found");
                }
            }
            catch (Exception exception)
            {
                listBox2.Items.Add("No constants found");
            }
        }

        /*
         * OnMouseClick Event for the FunctionsList ListBox
         */

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                functionInfoView.IsReadOnly = false;
                string selected = listBox1.SelectedItem.ToString();
                FunctionsListItem item = FunctionsList.Find(delegate (FunctionsListItem f) { return f.Name == selected; });
                if (item != null)
                {
                    functionInfoView.Text = item.About;
                }
                functionInfoView.IsReadOnly = true;
            }

        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            Font textFont = new Font(listBox1.Font.FontFamily, 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            try
            {
                FunctionsListItem item = FunctionsList.Find(delegate (FunctionsListItem f) { return f.Name == ((ListBox)sender).Items[e.Index].ToString(); });
                SolidBrush typeColor = new SolidBrush(ThemeManager.GetListBoxForeColor());
                if (item.Type == "int")
                {
                    typeColor = new SolidBrush(Color.Blue);
                }
                else if (item.Type == "float")
                {
                    typeColor = new SolidBrush(Color.Purple);
                }
                else if (item.Type == "void")
                {
                    typeColor = new SolidBrush(Color.Gray);
                }
                else if (item.Type == "object")
                {
                    typeColor = new SolidBrush(Color.SteelBlue);
                }
                else if (item.Type == "talent")
                {
                    typeColor = new SolidBrush(Color.Orange);
                }
                else if (item.Type == "effect")
                {
                    typeColor = new SolidBrush(Color.Red);
                }
                else if (item.Type == "location")
                {
                    typeColor = new SolidBrush(Color.YellowGreen);
                }
                else if (item.Type == "vector")
                {
                    typeColor = new SolidBrush(Color.Green);
                }

                string typeLabel = item.Type + " ";
                g.DrawString(typeLabel, textFont, typeColor, new PointF(e.Bounds.X, e.Bounds.Y));
                Size textSize = TextRenderer.MeasureText("location ", textFont);
                g.DrawString(((ListBox)sender).Items[e.Index].ToString(), textFont, new SolidBrush(ThemeManager.GetListBoxForeColor()), new PointF(e.Bounds.X + textSize.Width, e.Bounds.Y));
            }
            catch (Exception exception)
            {
                g.DrawString("No functions found...", textFont, new SolidBrush(ThemeManager.GetListBoxForeColor()), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Graphics g = e.Graphics;
            Font textFont = new Font(listBox2.Font.FontFamily, 12.0f, FontStyle.Regular, GraphicsUnit.Point);
            try
            {
                ConstantListItem item = ConstantsList.Find(delegate (ConstantListItem f) { return f.Name == ((ListBox)sender).Items[e.Index].ToString(); });
                SolidBrush typeColor = new SolidBrush(ThemeManager.GetListBoxForeColor());
                if (item.Type == "int")
                {
                    typeColor = new SolidBrush(Color.Blue);
                }
                else if (item.Type == "float")
                {
                    typeColor = new SolidBrush(Color.Purple);
                }
                else if (item.Type == "void")
                {
                    typeColor = new SolidBrush(Color.Gray);
                }
                else if (item.Type == "object")
                {
                    typeColor = new SolidBrush(Color.SteelBlue);
                }
                else if (item.Type == "talent")
                {
                    typeColor = new SolidBrush(Color.Orange);
                }
                else if (item.Type == "effect")
                {
                    typeColor = new SolidBrush(Color.Red);
                }
                else if (item.Type == "location")
                {
                    typeColor = new SolidBrush(Color.YellowGreen);
                }
                else if (item.Type == "vector")
                {
                    typeColor = new SolidBrush(Color.Green);
                }

                string typeLabel = item.Type + " ";
                g.DrawString(typeLabel, textFont, typeColor, new PointF(e.Bounds.X, e.Bounds.Y));
                Size textSize = TextRenderer.MeasureText("float ", textFont);
                g.DrawString(((ListBox)sender).Items[e.Index].ToString(), textFont, new SolidBrush(ThemeManager.GetListBoxForeColor()), new PointF(e.Bounds.X + textSize.Width, e.Bounds.Y));
            }
            catch (Exception exception)
            {
                g.DrawString("No constants found...", textFont, new SolidBrush(ThemeManager.GetListBoxForeColor()), new PointF(e.Bounds.X, e.Bounds.Y));
            }
        }

        private void kOTORIToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void kOTORIIToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            functionInfoView.IsReadOnly = false;
            string selected = listBox1.SelectedItem.ToString();
            FunctionsListItem item = FunctionsList.Find(delegate(FunctionsListItem f) { return f.Name == selected; });
            if (item != null)
            {
                functionInfoView.Text = item.About;
            }
            functionInfoView.IsReadOnly = true;

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            return;
        }

        private void scriptEditor_Load(object sender, EventArgs e)
        {
            /*scriptEditor.Focus();
            scriptEditor.Selection.Start = 14;*/
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void showFunctionListToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void showFunctionInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void scriptTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (scriptTabs.TabPages.Count == 0)
            {
                CreateNewEditorTab(FileSettings.NewPageFile());
            }
            EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
            tab.editor.Focus();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == (System.Windows.Forms.Keys.LControlKey))
            {
                if (e.KeyCode == System.Windows.Forms.Keys.D1)
                {
                    MessageBox.Show("Right");
                }
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        /*
         * 
         * Menubar OPTIONS
         * 
         */

        //Change the target game to KotOR I
        private void kOTORIToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            GameManager.SetCurrentGameIndex(0);
            kOTORIToolStripMenuItem2.CheckState = CheckState.Checked;
            kOTORIIToolStripMenuItem2.CheckState = CheckState.Unchecked;
            updateGameInfo();
        }

        //Change the target game to KotOR II
        private void kOTORIIToolStripMenuItem2_Click(object sender, EventArgs e)
        {

            GameManager.SetCurrentGameIndex(1);
            kOTORIIToolStripMenuItem2.CheckState = CheckState.Checked;
            kOTORIToolStripMenuItem2.CheckState = CheckState.Unchecked;
            updateGameInfo();
        }

        //Show/Hide Functions/Constants List Pane
        private void showFunctionListToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showFunctionListToolStripMenuItem1.Checked = !showFunctionListToolStripMenuItem1.Checked;
            splitContainer2.Panel2Collapsed = !splitContainer2.Panel2Collapsed;
        }

        //Show/Hide Function Info Pane
        private void showFunctionInfoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showFunctionInfoToolStripMenuItem1.Checked = !showFunctionInfoToolStripMenuItem1.Checked;
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        //Allow AutoComplete
        private void showAutoCompleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAutoCompleteToolStripMenuItem.Checked = !showAutoCompleteToolStripMenuItem.Checked;
            allowAutoComplete = showAutoCompleteToolStripMenuItem.Checked;
        }

        /*
         * 
         * Menubar FILE
         * 
         */

        //Create a new tab
        private void newToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            CreateNewEditorTab(FileSettings.NewPageFile());
        }

        public void OpenFile(string openFileName, bool record = false)
        {
            if (!String.IsNullOrEmpty(openFileName))
            {
                try
                {
                    string myPath = openFileName;
                    string extension = Path.GetExtension(myPath);
                    string fileName = Path.GetFileName(myPath);
                    string filePath = Path.GetFullPath(myPath);

                    if (extension == ".ncs")
                    {
                        if (GameManager.GetCurrentGame() != null)
                        {
                            GameManager.GetCurrentGame().DecompileScript(openFileName);
                            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            using (StreamReader reader = new StreamReader(Path.GetDirectoryName(appPath) + "\\tmp_decompiled.nss"))
                            {
                                FileSettings fileSettings = FileSettings.NewPageFile();
                                string str = reader.ReadToEnd();
                                Encoding ascii = Encoding.ASCII;
                                Byte[] encodedBytes = ascii.GetBytes(str);
                                string text = ascii.GetString(encodedBytes);
                                fileSettings.SavedContent = text;
                                fileSettings.Content = text;

                                EditorTabPage tabPage = CreateNewEditorTab(fileSettings);

                                fileSettings.FileName = fileName;
                                fileSettings.CompleteFileName = openFileName;
                                fileSettings.FilePath = filePath;
                                fileSettings.FileDataType = FileDataType.COMPILED;
                                
                                if (record)
                                {
                                    FileHistoryManager.OpenFile(openFileName);
                                }
                            }
                        }
                    }
                    else
                    {
                        FileSettings fileSettings = FileSettings.LoadState(openFileName);
                        if(fileSettings != null)
                        {
                            CreateNewEditorTab(fileSettings);
                            if (record)
                            {
                                FileHistoryManager.OpenFile(openFileName);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        //Open Files
        public void OpenFiles(string[] filesToOpen, bool record = false)
        {
            foreach (string openFileName in filesToOpen)
            {
                OpenFile(openFileName, record);
            }
            this.Focus();
        }

        //Open a file in new tab
        private void openToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //Stream myStream = null;
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "SWKotOR Source Script (*.nss*)|*.nss|SWKotOR Compiled Script (*.ncs*)|*.ncs|SWKotOR Script Files|*.nss;*.ncs|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            OpenFile.FilterIndex = 3;
            OpenFile.Multiselect = true;
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                OpenFiles(OpenFile.FileNames, true);
            }
        }

        //Save current tab
        private void saveToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            saveCurrentFile();
        }

        private void saveCurrentFile()
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                tab.SaveFile();
            }
        }

        private void saveCurrentFileAs()
        {
            if(scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                tab.SaveFileAs();
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveCurrentFileAs();
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int oldIndex = scriptTabs.SelectedIndex;
            foreach (EditorTabPage page in scriptTabs.TabPages)
            {
                scriptTabs.SelectedTab = page;
                saveCurrentFile();
            }
            scriptTabs.SelectedIndex = oldIndex;
        }

        //Compile script in current tab
        private void compileToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            CompileScript();
        }

        //Close current tab
        private void closeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                tab.CloseFile();
            }
        }

        //Exit scripting application
        private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox theAboutForm = new AboutBox();
            theAboutForm.ShowDialog();
        }

        /*
         * 
         * Menubar EDIT
         * 
         */

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                FindWindow findWindow = new FindWindow();
                findWindow.FindEvent += new EventHandler(childWindow_FindTextEvent);
                findWindow.toFind = tab.editor.Selection.Text;
                findWindow.Show();

                LastFindIndex = 0;
            }
        }

        void childWindow_FindTextEvent(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                string toFind = ((FindWindow.FindArgs)e).Message;
                bool matchCase = ((FindWindow.FindArgs)e).MatchCase;

                CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;
                StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;

                if (matchCase)
                {
                    stringCompare = StringComparison.CurrentCulture;
                }


                if (tab.editor.Text.IndexOf(toFind, stringCompare) == 0)
                {
                    int startIndex = tab.editor.Text.IndexOf(toFind, LastFindIndex, stringCompare);
                    int endIndex = toFind.Length;

                    tab.editor.Selection.Start = startIndex;
                    tab.editor.Selection.End = (startIndex + endIndex);

                    LastFindIndex = tab.editor.Selection.End;
                    if (tab.editor.Text.IndexOf(toFind, LastFindIndex, stringCompare) == -1)
                    {
                        LastFindIndex = 0;
                        MessageBox.Show("No more references found");
                    }

                }
                else
                {
                    MessageBox.Show("Query not found.");
                }
            }
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                FindAndReplace replaceWindow = new FindAndReplace();
                replaceWindow.ReplaceEvent += new EventHandler(childWindow_replaceOnceEvent);
                replaceWindow.ReplaceAllEvent += new EventHandler(childWindow_replaceAllEvent);
                replaceWindow.toFind = tab.editor.Selection.Text;
                replaceWindow.Show();

                LastFindIndex = 0;
            }
        }

        private void childWindow_replaceOnceEvent(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                string toFind = ((FindAndReplace.ReplaceArgs)e).ToFind;
                string toRepalce = ((FindAndReplace.ReplaceArgs)e).ToReplace;
                bool matchCase = ((FindAndReplace.ReplaceArgs)e).MatchCase;

                if (tab.editor.Text.IndexOf(toFind) != -1)
                {
                    if (tab.editor.Text.IndexOf(toFind, LastFindIndex) == -1)
                    {
                        LastFindIndex = 0;
                    }

                    int startIndex = tab.editor.Text.IndexOf(toFind, LastFindIndex);
                    int endIndex = toFind.Length;

                    tab.editor.Selection.Start = startIndex;
                    tab.editor.Selection.End = (startIndex + endIndex);
                    LastFindIndex = tab.editor.Selection.End;
                    if (String.Compare(tab.editor.Selection.Text, toFind, matchCase) == 0)
                    {
                        tab.editor.Selection.Text = toRepalce;
                    }
                    else
                    {
                        tab.editor.Selection.Dispose();
                    }
                }
                else
                {
                    MessageBox.Show("Query not found.");
                }
            }
        }

        private void childWindow_replaceAllEvent(object sender, EventArgs e)
        {
            if (scriptTabs.SelectedTab != null)
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                string toFind = ((FindAndReplace.ReplaceArgs)e).ToFind;
                string toRepalce = ((FindAndReplace.ReplaceArgs)e).ToReplace;
                bool matchCase = ((FindAndReplace.ReplaceArgs)e).MatchCase;
                bool inSelection = ((FindAndReplace.ReplaceArgs)e).InSelection;
                int searchStart = 0;
                int SearchEnd = tab.editor.Text.Length;
                int Replaced = 0;

                if (inSelection)
                {
                    searchStart = tab.editor.Selection.Start;
                    SearchEnd = tab.editor.Selection.End;
                }


                if (tab.editor.Text.IndexOf(toFind) != -1)
                {
                    while (searchStart <= SearchEnd)
                    {
                        int startIndex = searchStart;
                        int endIndex = toFind.Length;

                        tab.editor.Selection.Start = startIndex;
                        tab.editor.Selection.End = (startIndex + endIndex);

                        if (String.Compare(tab.editor.Selection.Text, toFind, matchCase) == 0)
                        {

                            tab.editor.Selection.Text = toRepalce;
                            searchStart = tab.editor.Selection.End;
                            Replaced++;
                        }
                        else
                        {
                            tab.editor.Selection.Dispose();
                            searchStart++;
                        }
                    }
                    tab.editor.Selection.Dispose();
                    MessageBox.Show(Replaced.ToString() + " occurrences matching '" + toFind + "' were replaced with '" + toRepalce + "' ");
                }
                else
                {
                    if (inSelection)
                    {
                        MessageBox.Show("Nothing to replace in selection");
                    }
                    else
                    {
                        MessageBox.Show("Nothing to replace");
                    }
                }
            }
        }


        private void scriptTabs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show();
                Point point = PointToScreen(new Point(e.X, e.Y));
                contextMenuStrip1.Left = point.X + 25;
                contextMenuStrip1.Top = point.Y + contextMenuStrip1.Height + 60;

                // iterate through all the tab pages
                for (int i = 0; i <  scriptTabs.TabCount; i++)
                {
                    // get their rectangle area and check if it contains the mouse cursor
                    Rectangle r = scriptTabs.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        scriptTabs.SelectedIndex = i;                       
                    }
                }


            }
        }

        private void toolStripCloseTab_Click(object sender, EventArgs e)
        {
            int selectedIndex = scriptTabs.SelectedIndex;

            if (scriptTabs.SelectedTab != null)
            {
                ((EditorTabPage)scriptTabs.SelectedTab).CloseFile();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {

            EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
            Clipboard.SetText(tab.editor.Selection.Text);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
            Clipboard.SetText(tab.editor.Selection.Text);
            tab.editor.Selection.Text = "";
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
            tab.editor.Selection.Text = Clipboard.GetText();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        private void ScriptEditorWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Cleanup genreated files
            File.Delete("tmp_decompiled.nss");
        }

        private void lightModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThemeManager.SetCurrentTheme(0);
            UpdateFormTheme();
        }

        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThemeManager.SetCurrentTheme(1);
            UpdateFormTheme();
        }

        private void compileToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            CompileScript();
        }

        private void constantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConstantsForm constantsForm = new ConstantsForm();

            if (constantsForm.ShowDialog() == DialogResult.OK) //popup form is loaded here and parent form will wait for dialogresult from popup window.
            {
                EditorTabPage tab = (EditorTabPage)scriptTabs.SelectedTab;
                tab.editor.Selection.Text = constantsForm.SelectedConstant;
            }
        }

        private void splitContainer1_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, ThemeManager.GetControlAltBackgroundColor(), ButtonBorderStyle.Solid);
        }

        private void splitContainer2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, ThemeManager.GetControlAltBackgroundColor(), ButtonBorderStyle.Solid);
        }
    }
}
