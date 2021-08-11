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
using System.Text.RegularExpressions;
using System.Threading;

namespace NWN_Script
{
    public partial class ScriptEditorWindow : Form
    {

        //Splash Screen function
        private void SplashScreen()
        {
            Application.Run(new SplashWindow());
        }

        private string FormTitle = "KotOR Scripting Tool";


        /* Helper Functions */
        public ScintillaNET.Scintilla GetSelectedEditor()
        {
            ScintillaNET.Scintilla tabEditor = null;
            foreach (Control control in scriptTabs.SelectedTab.Controls)
            {
                if (control.GetType() == typeof(ScintillaNET.Scintilla))
                {
                    tabEditor = (ScintillaNET.Scintilla)control;
                }
            }

            return tabEditor;
        }

        private void createNewPage()
        {
            TabPage newPage = new TabPage();
            newPage.Text = "New Page";
            newPage.Margin = new Padding(3);
            newPage.Padding = new Padding(3);

            //Text editor control
            ScintillaNET.Scintilla newEditor = new ScintillaNET.Scintilla();
            newPage.Controls.Add(newEditor);

            newEditor.Name = "Editor";
            newEditor.Dock = DockStyle.Fill;
            newEditor.ConfigurationManager.CustomLocation = "ScintillaNET.xml";
            newEditor.ConfigurationManager.Language = "cpp";
            newEditor.ConfigurationManager.Configure();

            newEditor.Indentation.TabWidth = Properties.Settings.Default.TabWidth;

            newEditor.TextChanged += new EventHandler(scriptEditor_TextChanged);
            newEditor.KeyUp += new KeyEventHandler(scriptEditor_KeyUp);
            newEditor.AutoCompleteAccepted += new EventHandler<ScintillaNET.AutoCompleteAcceptedEventArgs>(scriptEditor_AutoCompleteAccepted);
            newEditor.Scroll += new System.EventHandler<System.Windows.Forms.ScrollEventArgs>(scriptEditor_Scroll);
            //newEditor.AnnotationChanged += new EventHandler<ScintillaNET.AnnotationChangedEventArgs>();

            newEditor.Margins.Margin0.Width = 25;
            newEditor.Margins.Margin2.Width = 20;

            openedFile = null;

            scriptTabs.TabPages.Add(newPage);
            scriptTabs.SelectedTab = newPage;

            scriptTabs.SelectedTab.Tag = new FileSettings();

            UpdateFileInfo();


            ScintillaNET.Scintilla tabEditor = GetSelectedEditor();

            tabEditor.Text = "void main(){\n\n\n\n}";
            tabEditor.Focus();
            tabEditor.Selection.Start = 14;

        }

        private void UpdateFormInfo()
        {
            List<string> games = new List<string>();
            games.Add("Kotor I");
            games.Add("Kotor II");

            this.Text = FormTitle+" - [" + games[currentGame - 1] + " Mode]";
        }


        private void UpdateFileInfo()
        {
            updateFormTitle();
        }


        private void updateFormTitle()
        {

            string fileName = ((FileSettings)scriptTabs.SelectedTab.Tag).FileName;

            if (fileName == null)
            {
                fileName = "New File";
                
            }

            if (((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges)
            {
                scriptTabs.SelectedTab.Text = fileName + "* ";
            }
            else
            {
                scriptTabs.SelectedTab.Text = fileName + " ";
            }
        }

        private void CompileScript()
        {
            string fileToCompile = ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName;

            if (fileToCompile != null)
            {
                

                using (StreamWriter outfile = new StreamWriter(fileToCompile))
                {
                    ScintillaNET.Scintilla tabEditor = GetSelectedEditor();
                    outfile.Write(tabEditor.Text.ToString());

                    outfile.Close();

                    Process nwnnsscomp = new Process();

                    string myPath = fileToCompile;
                    string directoryName = Path.GetDirectoryName(myPath);
                    string outPutPath = (((FileSettings)scriptTabs.SelectedTab.Tag).FilePath + Path.GetFileNameWithoutExtension(myPath)) + ".ncs";
                    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    
                    if(currentGame == 1)
                    {
                        nwnnsscomp.StartInfo.FileName = @"k1\nwnnsscomp.exe";
                        MessageBox.Show("-c \"" + fileToCompile + "\"");
                        nwnnsscomp.StartInfo.Arguments = "-c \"" + fileToCompile + "\"";
                    }
                    else
                    {
                        nwnnsscomp.StartInfo.FileName = @"k2\nwnnsscomp.exe";
                        MessageBox.Show("-c \"" + fileToCompile + "\"");
                        nwnnsscomp.StartInfo.Arguments = "-c \"" + fileToCompile + "\"";
                    }

                    nwnnsscomp.StartInfo.RedirectStandardOutput = true;
                    nwnnsscomp.StartInfo.UseShellExecute = false;
                    nwnnsscomp.Start();

                    MessageBox.Show(nwnnsscomp.StandardOutput.ReadToEnd());
                }

            }
            else
            {
                MessageBox.Show("Error: You must save your script before you compile.");
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

        private class FileSettings
        {
            public string FileName = null;
            public string CompleteFileName = null;
            public bool UnsvedChanges = false;
            public string FilePath = null;

        }

        private void ParseNSS()
        {
            //string nssSource = "nwscript-kotor.nss";
            List<string> sourceList = new List<string>();

            sourceList.Add("k1\\nwscript.nss");
            sourceList.Add("k2\\nwscript.nss");


            int sourceIndex = 0;
            foreach (string nssSource in sourceList)
            {
                using (StreamReader reader = new StreamReader(Application.StartupPath +@"\"+ nssSource))
                {

                    string source = reader.ReadToEnd().ToString();
                    string line;
                    int counter = 0;

                    reader.BaseStream.Position = 0;
                    reader.DiscardBufferedData();

                    List<string> sourcelist = source.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
                    sourcelist.Reverse();
                    listBox1.Items.Clear();

                    while ((line = reader.ReadLine()) != null)
                    {
                        string voidPattern = "^void.*\\(.*\\);";
                        string objectPattern = "^int.*\\(.*\\);";
                        string intPattern = "^object.*\\(.*\\);";
                        string floatPattern = "^float.*\\(.*\\);";
                        string effectPattern = "^effect.*\\(.*\\);";
                        string eventPattern = "^event.*\\(.*\\);";
                        string stringPattern = "^string.*\\(.*\\);";
                        string vectorPattern = "^vector.*\\(.*\\);";
                        string locationPattern = "^location.*\\(.*\\);";

                        if(Regex.IsMatch(line, "^(effect|void|int|object|event|location|float|string|vector).*\\(.*\\);")){
                            string Functtype = "";

                            if (Regex.IsMatch(line, voidPattern))
                            {
                                Functtype = "void";
                            }
                            else if (Regex.IsMatch(line, intPattern))
                            {
                                Functtype = "int";
                            }
                            else if (Regex.IsMatch(line, objectPattern))
                            {
                                Functtype = "object";
                            }
                            else if (Regex.IsMatch(line, floatPattern))
                            {
                                Functtype = "float";
                            }else if (Regex.IsMatch(line, effectPattern))
                            {
                                Functtype = "effect";
                            }else if (Regex.IsMatch(line, eventPattern))
                            {
                                Functtype = "event";
                            }else if (Regex.IsMatch(line, stringPattern))
                            {
                                Functtype = "string";
                            }else if (Regex.IsMatch(line, vectorPattern))
                            {
                                Functtype = "vector";
                            }else if (Regex.IsMatch(line, locationPattern))
                            {
                                Functtype = "location";
                            }
                            //Match match = Regex.Match(line, @"([A-Z][a-zA-Z_]+|[a-z][0-9]+)");
                            Match match = Regex.Match(line, @"([a-zA-Z_0-9]+)\(");

                            List<string> about = new List<string>();
                            int index = sourcelist.FindIndex(f => f.ToString() == line);
                            int i = index;
                            while (sourcelist[i].ToString() != "")
                            {
                                about.Add(sourcelist[i].ToString());
                                i++;
                            }
                            about.Reverse();

                            /*
                             * ARGS GRABBER
                             * */

                            //\(([^\)]+)\) //ARGS Inside Parentheses

                            //\b(object|int|location|float|vector|string|effect)\b //ARGS

                            List<string> args = new List<string>();

                            Match lineParen = Regex.Match(line, "\\(([^\\)]+)\\)");

                            MatchCollection match_args = Regex.Matches(lineParen.Value, "\\b(object|int|location|float|vector|string|effect)\\b");

                            foreach (Match arg in match_args)
                            {
                                args.Add(arg.Value);
                            }




                            // Here we check the Match instance.
                            if (match.Success)
                            {
                                string Name = match.Value.Substring(0, match.Value.Length - 1);
                                string Line = Regex.Match(line, @"([A-Z][a-zA-Z_]+|[a-z][0-9]+)+\(.*\);").Value;
                                string About = string.Join("\r\n", about.ToArray());
                                int LineNumber = counter;
                                if (sourceIndex == 0)
                                {
                                    FunctionsList_kotor.Add(new FunctionsListItem(Functtype, Name, Line, About, args, LineNumber));
                                }
                                else
                                {
                                    FunctionsList_tsl.Add(new FunctionsListItem(Functtype, Name, Line, About, args, LineNumber));
                                }
                            }
                        }
                        counter++;
                    }


                    //GET CONSTANTS
                    reader.BaseStream.Position = 0;
                    reader.DiscardBufferedData();
                    int lineNum = 0;
                    int maxLine = 1662;
                    if (sourceIndex == 1)
                    {
                        maxLine = 2041;
                    }


                    while (((line = reader.ReadLine()) != null) && lineNum <= maxLine)
                    {
                        string conts_pattern = "(int|float)\\s*\\b([A-Z0-9_d]+)";
                        if (Regex.IsMatch(line, conts_pattern))
                        {
                            Match match = Regex.Match(line, "(int|float)\\s*\\b([A-Z0-9_d]+)");
                            Match constant = Regex.Match(match.ToString(), "\\b([A-Z0-9_d]+)\\b");

                            if (sourceIndex == 0)
                            {
                                kotor_CONSTANTS.Add(constant.ToString());
                            }
                            else
                            {
                                tsl_CONSTANTS.Add(constant.ToString());
                            }
                        }

                        lineNum++;
                    }
                }

                sourceIndex++;
            }
            kotor_CONSTANTS.Sort();
            tsl_CONSTANTS.Sort();
        }

        private void updateGameInfo()
        {
            listBox1.Items.Clear();
            if (currentGame == 1)
            {
                FunctionsList = FunctionsList_kotor;
                CONSTANTS_LIST = kotor_CONSTANTS;
            }
            else
            {
                FunctionsList = FunctionsList_tsl;
                CONSTANTS_LIST = tsl_CONSTANTS;
            }

            FunctionsList = FunctionsList.OrderBy(c => c.Name).ToList();

            foreach (FunctionsListItem item in FunctionsList)
            {
                listBox1.Items.Add(item.Name);
            }

            UpdateFormInfo();

        }

        static class StringHelper
        {
            /// <summary>
            /// Receives string and returns the string with its letters reversed.
            /// </summary>
            public static string ReverseString(string s)
            {
                char[] arr = s.ToCharArray();
                Array.Reverse(arr);
                return new string(arr);
            }
        }






        private int LastFindIndex = 0;
        private string openedFile = null;
        private bool allowAutoComplete = true;

        //1 = Kotor I //DEFAULT
        //2 = Kotor 2
        public static int currentGame = 1;

        private List<FunctionsListItem> FunctionsList = new List<FunctionsListItem>();

        private List<FunctionsListItem> FunctionsList_kotor = new List<FunctionsListItem>();
        private List<FunctionsListItem> FunctionsList_tsl = new List<FunctionsListItem>();

        private class FunctionsListItem
        {
            public string Type = null;
            public string Name = null;
            public string Code = null;
            public string About = null;
            public int LineNumber;
            List<string> Args = new List<string>();


            public FunctionsListItem(string type, string name, string code, string about, List<string> args, int lineNumber)
            {
                Type = type;
                Name = name;
                Code = code;
                About = about;
                Args = args;
                LineNumber = lineNumber;
            }
            
        }

        public static List<string> CONSTANTS_LIST = new List<string>(); //Holds the CONSTANTS for the current game
        public static List<string> kotor_CONSTANTS = new List<string>(); //Holds all the CONSTANTS for KotOR I
        public static List<string> tsl_CONSTANTS = new List<string>(); //Holds all the CONSTANTS for KotOR II

        public ScriptEditorWindow()
        {
            InitializeComponent();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            //Parse the games NSS files and store all functions and cosntants info
            ParseNSS();

            updateGameInfo();

            //Open any files that started the application
            string[] args = Environment.GetCommandLineArgs();
            List<string> newArgs = args.ToList<string>();
            //Strip the first string from the list because the first arg is the app itself
            newArgs.RemoveAt(0);

            args = newArgs.ToArray<string>();
            

            if (args.Length >= 1)
            {
                OpenFiles(args);
            }
            else
            {
                createNewPage();
            }

            updateFormTitle();
            this.BringToFront();
            this.Focus();
            this.BringToFront();
            this.Show();
            this.WindowState = FormWindowState.Normal;

            UpdateTabSpacing();

            //FileAssociation.Associate(".nss", "nss.nwscript", "SWKotOR Script Source", "\\Resources\\icon.ico", "Kotor Scripting Tool.exe");
            //FileAssociation.Associate(".ncs", "ncs.nwscript", "SWKotOR Script Compiled", "\\Resources\\icon.ico", "Kotor Scripting Tool.exe");
        }


        private void changeFunctionInfo(string Name)
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
        }

        /*
         * Script Editor OnScroll Event
         */

        private void scriptEditor_Scroll(object sender, ScrollEventArgs e)
        {
            //MessageBox.Show(e.NewValue.ToString());

            int LinesHeight = (GetSelectedEditor().Size.Height / GetSelectedEditor().Lines.Current.Height);
            int CurrentValue = (LinesHeight + (e.NewValue-1));

            if (CurrentValue >= 1000)
            {
                int Margin = 1;
                GetSelectedEditor().Margins[0].Width = 25 + (5*Margin);
            }
            else
            {
                GetSelectedEditor().Margins[0].Width = 25;
            }
        }

        /*
         * Auto Complete Accepted Event
         */

        private void scriptEditor_AutoCompleteAccepted(object sender, ScintillaNET.AutoCompleteAcceptedEventArgs e)
        {

            changeFunctionInfo(e.Text);
            GetSelectedEditor().SelectionChanged += new EventHandler(finishComplete);
        }

        /*
         * Auto Complete finished event
         */
        private void finishComplete(object sender, EventArgs e)
        {
            GetSelectedEditor().Selection.Text = "();";
            GetSelectedEditor().Selection.Start = (GetSelectedEditor().Selection.Start - 2);
            GetSelectedEditor().Selection.End = GetSelectedEditor().Selection.Start;
            GetSelectedEditor().SelectionChanged -= finishComplete;
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

            foreach (Control control in scriptTabs.SelectedTab.Controls)
            {
                if (control.GetType() == typeof(ScintillaNET.Scintilla))
                {
                    ScintillaNET.Scintilla tabEditor = (ScintillaNET.Scintilla)control;
                    tabEditor.Indentation.TabWidth = Properties.Settings.Default.TabWidth;
                }
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
         * OnKeyUp Event for the FucntionsList Search TextBox
         */

        private void functionSearch_KeyUp(object sender, KeyEventArgs e)
        {
            listBox1.Items.Clear();
            FunctionsList.ForEach(delegate(FunctionsListItem f) {

                if (f.Name.ToLower().Contains(functionSearch.Text.ToLower()))
                {
                    listBox1.Items.Add(f.Name);
                }

            });

            if(listBox1.Items.Count == 0){
                listBox1.Items.Add("No functions found");
            }
        }

        /*
         * OnClick Event for the Quick Compile Button
         */

        private void btn_QuickCompile_Click(object sender, EventArgs e)
        {
            CompileScript();
        }

        /*
         * Auto Complete Event Handler
         */

        private void scriptEditor_KeyUp(object sender, KeyEventArgs e)
        {
            try {
                ScintillaNET.Scintilla CurrentEditor = GetSelectedEditor();

                if ((e.KeyCode != Keys.Up) && (e.KeyCode != Keys.Down) && (e.KeyCode != Keys.Back) && allowAutoComplete)
                {
                    CurrentEditor.AutoComplete.List.Clear();
                    CurrentEditor.AutoComplete.Dispose();

                    if (e.KeyCode != Keys.Enter)
                    {
                        string line = CurrentEditor.Lines.Current.Text;

                        int caret = (CurrentEditor.Lines.Current.SelectionStartPosition - CurrentEditor.Lines.Current.StartPosition);
                        string lineReversed;
                        if (line.Length > 1)
                        {
                            lineReversed = StringHelper.ReverseString(line.Substring(0, caret));
                        }
                        else
                        {
                            lineReversed = line;
                        }

                        Match functionMatch = Regex.Match(lineReversed, "(^[\\S][a-z0-9A-Z]+)\\b");

                        foreach (FunctionsListItem item in FunctionsList)
                        {


                            if (item.Name.Contains(StringHelper.ReverseString(functionMatch.Value)) && functionMatch.Value != "")
                            {
                                CurrentEditor.AutoComplete.List.Add(item.Name);
                            }

                        }

                        if (CurrentEditor.AutoComplete.List.Count > 0)
                        {
                            CurrentEditor.AutoComplete.Show();
                            changeFunctionInfo(CurrentEditor.AutoComplete.SelectedText);
                        }
                    }
                }
                else
                {
                    if (CurrentEditor.AutoComplete.IsActive)
                    {
                        changeFunctionInfo(CurrentEditor.AutoComplete.SelectedText);
                    }
                }
            }catch(Exception exception)
            {

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

            FunctionsListItem item = FunctionsList.Find(delegate(FunctionsListItem f) { return f.Name == ((ListBox)sender).Items[e.Index].ToString(); });

            if (item.Type == "int")
            {
                g.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Blue), new PointF(e.Bounds.X, e.Bounds.Y));
            }
            else if (item.Type == "object")
            {
                g.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Purple), new PointF(e.Bounds.X, e.Bounds.Y));
            }
            else if (item.Type == "void")
            {
                g.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(Color.Black), new PointF(e.Bounds.X, e.Bounds.Y));
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

        private void scriptEditor_Load(object sender, EventArgs e)
        {
            /*scriptEditor.Focus();
            scriptEditor.Selection.Start = 14;*/
        }

        private void scriptEditor_TextChanged(object sender, EventArgs e)
        {
            ((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges = true;
            updateFormTitle();
        }



        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ConstantsForm constantsForm = new ConstantsForm();

            if (constantsForm.ShowDialog() == DialogResult.OK) //popup form is loaded here and parent form will wait for dialogresult from popup window.
            {
                GetSelectedEditor().Selection.Text = constantsForm.SelectedConstant;
            }


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
                createNewPage();
            }
            GetSelectedEditor().Focus();
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

        //CHange game to Kotor I
        private void kOTORIToolStripMenuItem2_Click(object sender, EventArgs e)
        {

            if (kOTORIToolStripMenuItem2.CheckState == CheckState.Unchecked)
            {
                kOTORIToolStripMenuItem2.CheckState = CheckState.Checked;
                currentGame = 1;
                kOTORIIToolStripMenuItem2.CheckState = CheckState.Unchecked;
            }
            else
            {
                kOTORIToolStripMenuItem2.CheckState = CheckState.Unchecked;
                currentGame = 2;
                kOTORIIToolStripMenuItem2.CheckState = CheckState.Checked;
            }

            updateGameInfo();
        }

        //Change game to Kotor II
        private void kOTORIIToolStripMenuItem2_Click(object sender, EventArgs e)
        {

            if (kOTORIIToolStripMenuItem2.CheckState == CheckState.Unchecked)
            {
                kOTORIIToolStripMenuItem2.CheckState = CheckState.Checked;
                currentGame = 2;
                kOTORIToolStripMenuItem2.CheckState = CheckState.Unchecked;
            }
            else
            {
                kOTORIIToolStripMenuItem2.CheckState = CheckState.Unchecked;
                currentGame = 1;
                kOTORIToolStripMenuItem2.CheckState = CheckState.Checked;
            }

            updateGameInfo();
        }

        //Show/Hide Functions List
        private void showFunctionListToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showFunctionListToolStripMenuItem1.Checked = !showFunctionListToolStripMenuItem1.Checked;
            splitContainer2.Panel2Collapsed = !splitContainer2.Panel2Collapsed;
        }

        //Show/Hide Function Info
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
            createNewPage();
            ScintillaNET.Scintilla tabEditor = GetSelectedEditor();

            tabEditor.Text = "void main(){\n\n\n\n}";
            tabEditor.Focus();
            tabEditor.Selection.Start = 14;
        }

        //Open Files
        public void OpenFiles(string[] filesToOpen)
        {
            foreach (string openFileName in filesToOpen)
            {
                if (!String.IsNullOrEmpty(openFileName))
                {
                    //Stream myStream = null;
                    try
                    {
                        //if ((myStream == File.OpenRead(openFileName)) != null)
                        //{

                        string myPath = openFileName;
                        string extension = Path.GetExtension(myPath);
                        string fileName = Path.GetFileName(myPath);
                        string filePath = Path.GetFullPath(myPath);

                        if (extension == ".ncs")
                        {

                            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                            Process nwnnsscomp = new Process();
                            nwnnsscomp.StartInfo.FileName = @"nwnnsscomp.exe";
                            nwnnsscomp.StartInfo.Arguments = "-d \"" + openFileName + "\" --outputdir \"" + Path.GetDirectoryName(appPath) + "\" -o \"tmp_decompiled.nss\"";
                            nwnnsscomp.StartInfo.RedirectStandardOutput = true;
                            nwnnsscomp.StartInfo.UseShellExecute = false;
                            nwnnsscomp.StartInfo.WorkingDirectory = Path.GetDirectoryName(appPath);

                            nwnnsscomp.Start();

                            MessageBox.Show(nwnnsscomp.StandardOutput.ReadToEnd());

                            using (StreamReader reader = new StreamReader(Path.GetDirectoryName(appPath) + "\\tmp_decompiled.nss"))
                            {
                                createNewPage();
                                ScintillaNET.Scintilla tabEditor = GetSelectedEditor();

                                ((FileSettings)scriptTabs.SelectedTab.Tag).FileName = fileName;
                                ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName = openFileName;
                                ((FileSettings)scriptTabs.SelectedTab.Tag).FilePath = filePath;

                                string str = reader.ReadToEnd();
                                System.Text.Encoding ascii = System.Text.Encoding.ASCII;
                                Byte[] encodedBytes = ascii.GetBytes(str);

                                tabEditor.Text = ascii.GetString(encodedBytes);

                                openedFile = openFileName;
                                UpdateFileInfo();
                            }
                        }
                        else
                        {

                            using (StreamReader reader = new StreamReader(openFileName))
                            {
                                createNewPage();
                                ScintillaNET.Scintilla tabEditor = GetSelectedEditor();

                                ((FileSettings)scriptTabs.SelectedTab.Tag).FileName = fileName;
                                ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName = openFileName;
                                ((FileSettings)scriptTabs.SelectedTab.Tag).FilePath = filePath;

                                string contents = reader.ReadToEnd();
                                tabEditor.Text = contents;
                                openedFile = openFileName;
                                UpdateFileInfo();
                            }

                        }
                        //}
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
            }
            this.Focus();
        }

        //Open file in new tab
        private void openToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //Stream myStream = null;
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "SWKotOR Source Script (*.nss*)|*.nss|SWKotOR Compiled Script (*.ncs*)|*.ncs|SWKotOR Script Files|*.nss;*.ncs|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            OpenFile.FilterIndex = 3;
            OpenFile.Multiselect = true;
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                OpenFiles(OpenFile.FileNames);
            }
        }

        //Save current tab
        private void saveToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName != null)
            {
                string fileToSave = ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName;
                using (StreamWriter outfile = new StreamWriter(fileToSave))
                {
                    ScintillaNET.Scintilla tabEditor = GetSelectedEditor();

                    outfile.Write(tabEditor.Text.ToString());
                    ((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges = false;
                    updateFormTitle();
                }

            }
            else
            {
                SaveFileDialog saveFile = new SaveFileDialog();

                saveFile.Filter = "SWKotOR Script Files (*.nss*)|*.nss|txt files (*.txt)|*.txt";
                if (saveFile.ShowDialog() == DialogResult.OK)
                {

                    string myPath = saveFile.FileName;
                    string extension = Path.GetExtension(myPath);
                    string fileName = Path.GetFileName(myPath);
                    string filePath = Path.GetFullPath(myPath);

                    using (StreamWriter outfile = new StreamWriter(saveFile.FileName.ToString()))
                    {
                        ((FileSettings)scriptTabs.SelectedTab.Tag).FileName = fileName;
                        ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName = saveFile.FileName.ToString();
                        ((FileSettings)scriptTabs.SelectedTab.Tag).FilePath = filePath;
                        ((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges = false;

                        ScintillaNET.Scintilla tabEditor = GetSelectedEditor();
                        outfile.Write(tabEditor.Text.ToString());

                        UpdateFileInfo();
                    }

                }
            }
        }

        private void saveCurrentFile()
        {
            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.Filter = "SWKotOR Script Files (*.nss*)|*.nss|txt files (*.txt)|*.txt";
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                string myPath = saveFile.FileName;
                string extension = Path.GetExtension(myPath);
                string fileName = Path.GetFileName(myPath);
                string filePath = Path.GetFullPath(myPath);

                using (StreamWriter outfile = new StreamWriter(saveFile.FileName.ToString()))
                {
                    ((FileSettings)scriptTabs.SelectedTab.Tag).FileName = fileName;
                    ((FileSettings)scriptTabs.SelectedTab.Tag).CompleteFileName = saveFile.FileName.ToString();
                    ((FileSettings)scriptTabs.SelectedTab.Tag).FilePath = filePath;
                    ((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges = false;

                    ScintillaNET.Scintilla tabEditor = GetSelectedEditor();
                    outfile.Write(tabEditor.Text.ToString());

                    UpdateFileInfo();
                }
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveCurrentFile();
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TabPage page in scriptTabs.TabPages)
            {
                scriptTabs.SelectedTab = page;
                saveCurrentFile();
            }
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
                if (((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges)
                {
                    if (MessageBox.Show("You have unsaved changes.\nAre you sure you want to close?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        scriptTabs.SelectedTab.Dispose();
                    }

                }
                else
                {
                    scriptTabs.SelectedTab.Dispose();
                }
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
            FindWindow findWindow = new FindWindow();
            findWindow.FindEvent += new EventHandler(childWindow_FindTextEvent);
            findWindow.toFind = GetSelectedEditor().Selection.Text;
            findWindow.Show();

            LastFindIndex = 0;
        }

        void childWindow_FindTextEvent(object sender, EventArgs e)
        {
            string toFind = ((FindWindow.FindArgs)e).Message;
            bool matchCase = ((FindWindow.FindArgs)e).MatchCase;

            CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;

            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;

            if(matchCase){
                stringCompare = StringComparison.CurrentCulture;
            }


            if (GetSelectedEditor().Text.IndexOf(toFind, stringCompare) == 0)
            {
                int startIndex = GetSelectedEditor().Text.IndexOf(toFind, LastFindIndex, stringCompare);
                int endIndex = toFind.Length;

                GetSelectedEditor().Selection.Start = startIndex;
                GetSelectedEditor().Selection.End = (startIndex + endIndex);

                LastFindIndex = GetSelectedEditor().Selection.End;
                if (GetSelectedEditor().Text.IndexOf(toFind, LastFindIndex, stringCompare) == -1)
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

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {

            FindAndReplace replaceWindow = new FindAndReplace();
            replaceWindow.ReplaceEvent += new EventHandler(childWindow_replaceOnceEvent);
            replaceWindow.ReplaceAllEvent += new EventHandler(childWindow_replaceAllEvent);
            replaceWindow.toFind = GetSelectedEditor().Selection.Text;
            replaceWindow.Show();

            LastFindIndex = 0;
        }

        private void childWindow_replaceOnceEvent(object sender, EventArgs e)
        {

            string toFind = ((FindAndReplace.ReplaceArgs)e).ToFind;
            string toRepalce = ((FindAndReplace.ReplaceArgs)e).ToReplace;
            bool matchCase = ((FindAndReplace.ReplaceArgs)e).MatchCase;

            if (GetSelectedEditor().Text.IndexOf(toFind) != -1)
            {
                if (GetSelectedEditor().Text.IndexOf(toFind, LastFindIndex) == -1)
                {
                    LastFindIndex = 0;
                }

                int startIndex = GetSelectedEditor().Text.IndexOf(toFind, LastFindIndex);
                int endIndex = toFind.Length;

                GetSelectedEditor().Selection.Start = startIndex;
                GetSelectedEditor().Selection.End = (startIndex + endIndex);
                LastFindIndex = GetSelectedEditor().Selection.End;
                if (String.Compare(GetSelectedEditor().Selection.Text, toFind, matchCase) == 0)
                {
                    GetSelectedEditor().Selection.Text = toRepalce;
                }
                else
                {
                    GetSelectedEditor().Selection.Dispose();
                }



            }
            else
            {
                MessageBox.Show("Query not found.");
            }

        }

        private void childWindow_replaceAllEvent(object sender, EventArgs e)
        {

            string toFind = ((FindAndReplace.ReplaceArgs)e).ToFind;
            string toRepalce = ((FindAndReplace.ReplaceArgs)e).ToReplace;
            bool matchCase = ((FindAndReplace.ReplaceArgs)e).MatchCase;
            bool inSelection = ((FindAndReplace.ReplaceArgs)e).InSelection;
            int searchStart = 0;
            int SearchEnd = GetSelectedEditor().Text.Length;
            int Replaced = 0;

            if (inSelection)
            {
                searchStart = GetSelectedEditor().Selection.Start;
                SearchEnd = GetSelectedEditor().Selection.End;
            }
            

            if (GetSelectedEditor().Text.IndexOf(toFind) != -1)
            {
                while (searchStart <= SearchEnd)
                {
                    int startIndex = searchStart;
                    int endIndex = toFind.Length;

                    GetSelectedEditor().Selection.Start = startIndex;
                    GetSelectedEditor().Selection.End = (startIndex + endIndex);

                    if (String.Compare(GetSelectedEditor().Selection.Text, toFind, matchCase) == 0)
                    {

                        GetSelectedEditor().Selection.Text = toRepalce;
                        searchStart = GetSelectedEditor().Selection.End;
                        Replaced++;
                    }
                    else
                    {
                        GetSelectedEditor().Selection.Dispose();
                        searchStart++;
                    }
                }
                GetSelectedEditor().Selection.Dispose();
                MessageBox.Show(Replaced.ToString() + " occurrences matching '"+toFind+"' were replaced with '"+toRepalce+"' ");
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


        private void scriptTabs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show();
                Point point = PointToScreen(new Point(e.X, e.Y));
                contextMenuStrip1.Left = point.X;
                contextMenuStrip1.Top = point.Y + contextMenuStrip1.Height;

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
                if (((FileSettings)scriptTabs.SelectedTab.Tag).UnsvedChanges)
                {
                    if (MessageBox.Show("You have unsaved changes.\nAre you sure you want to close?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        scriptTabs.TabPages.Remove(scriptTabs.TabPages[scriptTabs.SelectedIndex]);

                        if ((selectedIndex - 1) >= 0)
                        {
                            scriptTabs.SelectedIndex = (selectedIndex - 1);
                        }
                        else
                        {
                            scriptTabs.SelectedIndex = 0;
                        }
                    }

                }
                else
                {
                    scriptTabs.TabPages.Remove(scriptTabs.TabPages[scriptTabs.SelectedIndex]);

                    if ((selectedIndex - 1) >= 0)
                    {
                        scriptTabs.SelectedIndex = (selectedIndex - 1);
                    }
                    else
                    {
                        scriptTabs.SelectedIndex = 0;
                    }
                }
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(GetSelectedEditor().Selection.Text);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(GetSelectedEditor().Selection.Text);
            GetSelectedEditor().Selection.Text = "";
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetSelectedEditor().Selection.Text = System.Windows.Forms.Clipboard.GetText();
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

    }
}
