using MetroFramework.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NWN_Script
{
    class EditorTabPage : MetroTabPage
    {

        public MetroScrollBar hScroll;
        public MetroScrollBar vScroll;
        public ScintillaNET.Scintilla editor;
        public FileSettings FileSettings;
        public bool AllowAutoComplete = true;
        private int scrollSize = 20;

        public EditorTabPage(FileSettings fileSettings)
        {
            this.FileSettings = fileSettings;
            this.Tag = fileSettings;
            this.UseStyleColors = true;
            this.Text = "Untitled";
            this.Margin = new Padding(3);
            this.Padding = new Padding(3);

            hScroll = new MetroScrollBar(MetroScrollOrientation.Horizontal, scrollSize);
            hScroll.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            hScroll.Size = new Size(Width, scrollSize);
            hScroll.Location = new Point(0, this.Height - scrollSize);
            hScroll.Scroll += new ScrollEventHandler(hscrollBar_OnScroll);
            hScroll.Theme = ThemeManager.GetMetroTheme();

            vScroll = new MetroScrollBar(MetroScrollOrientation.Vertical, scrollSize);
            vScroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            vScroll.Size = new Size(scrollSize, this.Height);
            vScroll.Location = new Point(this.Width - scrollSize, 0);
            vScroll.Scroll += new ScrollEventHandler(vscrollBar_OnScroll);
            vScroll.Theme = ThemeManager.GetMetroTheme();

            //Text editor control
            editor = new ScintillaNET.Scintilla();
            this.Controls.Add(editor);
            this.Controls.Add(vScroll);
            this.Controls.Add(hScroll);

            editor.Name = "Editor";
            editor.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            editor.Size = new Size(this.Width, this.Height);
            //editor.Dock = DockStyle.Fill;
            ThemeManager.ConfigureScintillaControlTheme(editor);
            editor.IsBraceMatching = true;
            editor.MatchBraces = true;
            editor.Snippets.IsEnabled = true;
            editor.Indentation.SmartIndentType = ScintillaNET.SmartIndent.CPP2;
            editor.Indentation.ShowGuides = true;
            editor.Indentation.TabWidth = Properties.Settings.Default.TabWidth;

            editor.TextChanged += new EventHandler(scriptEditor_TextChanged);
            editor.KeyUp += new KeyEventHandler(scriptEditor_KeyUp);
            editor.AutoCompleteAccepted += new EventHandler<ScintillaNET.AutoCompleteAcceptedEventArgs>(scriptEditor_AutoCompleteAccepted);
            editor.Scroll += new EventHandler<ScrollEventArgs>(scriptEditor_Scroll);
            editor.MouseWheel += Editor_MouseWheel;
            //editor.AnnotationChanged += new EventHandler<ScintillaNET.AnnotationChangedEventArgs>();
            editor.SelectionChanged += new EventHandler(scriptEditor_Selection);

            //editor.Font = new Font(editor.Font.Name, 14, editor.Font.Style);
            editor.Margins.Margin0.Width = 25;
            editor.Margins.Margin2.Width = 20;

            GotFocus += EditorTabPage_GotFocus;

            UpdateCurrentTabName();

            if (!String.IsNullOrEmpty(FileSettings.Content))
            {
                editor.Text = FileSettings.Content;
            }
            this.Focus();
            editor.Selection.Start = FileSettings.Selection;
            editor.SendToBack();
        }

        /*
         * Editor MouseWheel Event Handler
         */

        private void Editor_MouseWheel(object sender, MouseEventArgs e)
        {
            vScroll.Value = editor.Lines.FirstVisibleIndex;
            UpdateEditorScrollBars();
        }

        /*
         * Editor Focus Event Hander
         */

        private void EditorTabPage_GotFocus(object sender, EventArgs e)
        {
            Debug.WriteLine("Tab Focus");
            UpdateEditorScrollBars();
        }

        /*
         * Get the max width of the visible lines on screen
         */

        private int GetVisibleLinesMaxWidth()
        {
            int width = 0;
            var TabSpace = new String(' ', Properties.Settings.Default.TabWidth);

            foreach (ScintillaNET.Line line in editor.Lines.VisibleLines)
            {
                var NewString = line.Text.Replace("\t", TabSpace);
                int lineWidth = TextRenderer.MeasureText(NewString, editor.Font).Width;
                if(lineWidth > width)
                {
                    width = lineWidth;
                }
            }
            return width;
        }

        /*
         * Get the width of the editors margins
         */

        private int GetGutterWidth()
        {
            return editor.Margins.Margin0.Width + editor.Margins.Margin2.Width;
        }

        /*
         * Get The Width of the text area of the editor
         */

        private int GetEditorWidth()
        {
            return editor.Width - GetGutterWidth();
        }

        /*
         * Update ScrollBars
         */

        private void UpdateEditorScrollBars()
        {
            editor.Scrolling.ScrollBars = ScrollBars.None;
            bool canVScroll = (editor.Lines.VisibleCount < editor.Lines.Count);
            bool canHScroll = GetVisibleLinesMaxWidth() > GetEditorWidth();

            bool vScrollWasVisible = vScroll.Visible;
            bool hScrollWasVisible = hScroll.Visible;

            vScroll.Visible = canVScroll;
            vScroll.Minimum = 0;
            vScroll.Maximum = editor.Lines.Count;
            vScroll.LargeChange = editor.Lines.VisibleCount - 1;
            vScroll.Value = editor.Lines.FirstVisibleIndex;

            if (canVScroll)
            {
                editor.Width = Width - scrollSize;
            }
            else
            {
                editor.Width = Width;
            }

            hScroll.Visible = canHScroll;
            hScroll.Minimum = 0;
            hScroll.Maximum = GetVisibleLinesMaxWidth();
            hScroll.LargeChange = GetEditorWidth();
            hScroll.Value = editor.Scrolling.HorizontalScrollOffset;
            if (canHScroll)
            {
                editor.Height = Height - scrollSize;
            }
            else
            {
                editor.Height = Height;
            }

            if(canVScroll && canHScroll)
            {
                hScroll.Size = new Size(Width - scrollSize, scrollSize);
                vScroll.Size = new Size(scrollSize, this.Height - scrollSize);
            }
            else
            {
                hScroll.Size = new Size(Width, scrollSize);
                vScroll.Size = new Size(scrollSize, this.Height);
            }

            Debug.WriteLine(vScroll.Value + " <:> " + vScroll.Maximum + " <:> " + vScroll.LargeChange + " <:> " + (canVScroll ? 1 : 0));
            Debug.WriteLine(hScroll.Value + " <:> " + hScroll.Maximum + " <:> " + hScroll.LargeChange + " <:> " + editor.Width);
        }

        /*
         * Horizontal Scroll Bar Scroll Event
         */

        private void hscrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            editor.Scrolling.HorizontalScrollOffset = hScroll.Value;
            UpdateEditorScrollBars();
        }

        /*
         * Vertical Scroll Bar Scroll Event
         */

        private void vscrollBar_OnScroll(object sender, ScrollEventArgs e)
        {
            editor.Lines.FirstVisibleIndex = vScroll.Value;
            UpdateEditorScrollBars();
        }

        /*
         * Script Editor OnScroll Event
         */

        private void scriptEditor_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateEditorScrollBars();
            vScroll.Value = editor.Lines.FirstVisibleIndex;
            UpdateEditorMargins(editor);
        }

        /*
         * Apply the metro theme
         */

        public void ApplyMetroTheme(MetroFramework.Components.MetroStyleExtender metroStyleExtender)
        {
            metroStyleExtender.SetApplyMetroTheme(editor, true);
        }

        /*
         * Update The Tabs Name To Reflect It's Current State
         */

        public void UpdateCurrentTabName()
        {
            if (FileSettings.FileName == null)
            {
                FileSettings.FileName = "Untitled";

            }

            if (FileSettings.HasUnsavedChanges())
            {
                Text = FileSettings.FileName + "* ";
            }
            else
            {
                Text = FileSettings.FileName + " ";
            }
        }

        /*
         * TextChanged Event
         */

        private void scriptEditor_TextChanged(object sender, EventArgs e)
        {
            //FileSettings.UnsavedChanges = true;
            //Debug.WriteLine(editor.Scrolling.HorizontalScrollWidth + " <:> " + editor.Width);
            FileSettings.Content = editor.Text;
            UpdateEditorMargins(editor);
            UpdateCurrentTabName();
            UpdateEditorScrollBars();
        }

        /*
         * Auto Complete Event Handler
         */

        private void scriptEditor_KeyUp(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine(editor.Scrolling.HorizontalScrollWidth + " : " + editor.Width);
            try
            {

                if ((e.KeyCode != Keys.Up) && (e.KeyCode != Keys.Down) && (e.KeyCode != Keys.Back) && AllowAutoComplete)
                {
                    editor.AutoComplete.List.Clear();
                    editor.AutoComplete.Dispose();

                    if (e.KeyCode == Keys.OemOpenBrackets)
                    {
                        editor.Selection.Text = "}";
                        editor.Selection.Start -= 1;
                        editor.Selection.End = editor.Selection.Start;
                    }

                    if (e.KeyCode != Keys.Enter)
                    {
                        string line = editor.Lines.Current.Text;

                        int caret = (editor.Lines.Current.SelectionStartPosition - editor.Lines.Current.StartPosition);
                        string lineReversed;
                        if (line.Length > 1)
                        {
                            lineReversed = StringHelper.ReverseString(line.Substring(0, caret));
                        }
                        else
                        {
                            lineReversed = line;
                        }

                        Match functionMatch = Regex.Match(lineReversed, "(^[\\S][a-z0-9A-Z_]+)\\b");

                        List<FunctionsListItem> FunctionsList = GameManager.GetFunctionsList();

                        //Search Script Functions
                        foreach (FunctionsListItem item in FunctionsList)
                        {

                            if (item.Name.Contains(StringHelper.ReverseString(functionMatch.Value)) && functionMatch.Value != "")
                            {
                                editor.AutoComplete.List.Add(item.Name);
                            }

                        }

                        List<ConstantListItem> ConstantsList = GameManager.GetConstantsList();

                        //Search Script Constants
                        foreach (ConstantListItem item in ConstantsList)
                        {
                            //CurrentEditor.Lexing.
                            if (item.Name.Contains(StringHelper.ReverseString(functionMatch.Value)) && functionMatch.Value != "")
                            {
                                editor.AutoComplete.List.Add(item.Name);
                            }

                        }

                        if (editor.AutoComplete.List.Count > 0)
                        {
                            editor.AutoComplete.Show();
                            FunctionsListItem selectedFunction = FunctionsList.Find(func => func.Name == editor.AutoComplete.SelectedText);
                            if (selectedFunction != null)
                            {
                                //changeFunctionInfo(editor.AutoComplete.SelectedText);
                            }
                        }
                    }
                }
                else
                {
                    if (editor.AutoComplete.IsActive)
                    {
                        //changeFunctionInfo(editor.AutoComplete.SelectedText);
                    }
                }
                UpdateEditorMargins(editor);
            }
            catch (Exception exception)
            {

            }
        }

        /*
         * Auto Complete Accepted Event
         */

        private void scriptEditor_AutoCompleteAccepted(object sender, ScintillaNET.AutoCompleteAcceptedEventArgs e)
        {
            //if (changeFunctionInfo(e.Text))
            //{
            //    editor.SelectionChanged += new EventHandler(finishComplete);
            //}
        }

        /*
         * Auto Complete finished event
         */
        private void finishComplete(object sender, EventArgs e)
        {
            editor.Selection.Text = "()";
            editor.Selection.Start = (editor.Selection.Start);
            editor.Selection.End = editor.Selection.Start;
            editor.SelectionChanged -= finishComplete;
        }

        private void UpdateEditorMargins(ScintillaNET.Scintilla CurrentEditor)
        {

            if (CurrentEditor.Lines.Count >= 1000)
            {
                CurrentEditor.Margins.Margin0.Width = 60;
            }
            else if (CurrentEditor.Lines.Count >= 100)
            {
                CurrentEditor.Margins.Margin0.Width = 50;
            }
            else
            {
                CurrentEditor.Margins.Margin0.Width = 40;
            }
        }

        /*
         * Script Editor OnSelection Event
         */

        private void scriptEditor_Selection(object sender, EventArgs e)
        {
            //MessageBox.Show(e.NewValue.ToString());

            string line = editor.GetCurrentLine();
            int caretPos = editor.Selection.Start;
            int caretEnd = editor.Selection.End;

            int endPos = 0;
            int startPos = 0;
            Debug.WriteLine("Start: " + caretPos + "End: " + caretEnd + " Length: " + line.Length);
            //return;

            if (caretPos > line.Length - 1)
                return;

            int pos = caretPos;
            while (true)
            {
                if (pos >= 0 && pos < line.Length)
                {
                    int c = line[pos];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        pos++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            endPos = pos;

            pos = caretPos;
            while (true)
            {
                if (pos >= 0 && pos > 0)
                {
                    int c = line[pos];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        pos--;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            startPos = pos;
            try
            {
                Debug.WriteLine("Start: " + startPos + " End: " + endPos + "Text: " + line.Substring(startPos, endPos));
            }
            catch (Exception exception)
            {
                Debug.WriteLine(" - Start: " + startPos + " End: " + endPos);
            }
        }



        /*
         * Save & Compile Operations
         */

        public void SaveState()
        {
            if (FileSettings.HasUnsavedChanges())
            {
                FileSettings.Selection = editor.Selection.Start;
                FileSettings.SaveState();
            }
        }


        public void SaveFile()
        {
            if (FileSettings.CompleteFileName != null)
            {
                if (FileSettings.FileDataType == FileDataType.PLAIN_TEXT)
                {
                    using (StreamWriter outfile = new StreamWriter(FileSettings.CompleteFileName))
                    {
                        outfile.Write(editor.Text.ToString());
                        FileSettings.SavedContent = editor.Text.ToString();
                        FileSettings.Content = editor.Text.ToString();
                        SaveState();
                        UpdateCurrentTabName();
                    }
                }
                else
                {
                    MessageBox.Show("Error: You can't edit a compiled ncs file. Please open the nss version to make changes.");
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
                        FileSettings.FileName = fileName;
                        FileSettings.CompleteFileName = saveFile.FileName.ToString();
                        FileSettings.FilePath = filePath;
                        //FileSettings.UnsavedChanges = false;
                        FileSettings.FileDataType = FileDataType.PLAIN_TEXT;

                        outfile.Write(editor.Text.ToString());

                        FileSettings.SavedContent = editor.Text.ToString();
                        FileSettings.Content = editor.Text.ToString();
                        SaveState();
                        UpdateCurrentTabName();
                    }

                }
            }
        }

        public void SaveFileAs()
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
                    FileSettings.FileName = fileName;
                    FileSettings.CompleteFileName = saveFile.FileName.ToString();
                    FileSettings.FilePath = filePath;
                    //FileSettings.UnsavedChanges = false;

                    outfile.Write(editor.Text.ToString());
                    FileSettings.SavedContent = editor.Text.ToString();
                    FileSettings.Content = editor.Text.ToString();
                    SaveState();
                    UpdateCurrentTabName();
                }
            }
        }

        public void CompileFile()
        {
            if (FileSettings.FileDataType != FileDataType.COMPILED)
            {
                if (GameManager.GetCurrentGame() != null)
                {
                    if (FileSettings.CompleteFileName != null)
                    {
                        using (StreamWriter outfile = new StreamWriter(FileSettings.CompleteFileName))
                        {
                            outfile.Write(editor.Text.ToString());
                            outfile.Close();
                            GameManager.GetCurrentGame().CompileScript(FileSettings);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: You must save your script before you compile.");
                    }
                }
                else
                {
                    //Game selection is broken... but how?
                }
            }
            else
            {
                MessageBox.Show("Error: You can't compile an already compiled ncs file. Please open the nss version to recompile.");
            }
        }

        public bool CloseFile()
        {
            MetroTabControl scriptTabs = (MetroTabControl)Parent;
            int selectedIndex = scriptTabs.SelectedIndex;
            if (FileSettings.HasUnsavedChanges())
            {
                FileHistoryManager.CloseFile(
                    FileSettings.CompleteFileName
                );
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
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                FileHistoryManager.CloseFile(
                    FileSettings.CompleteFileName
                );
                
                scriptTabs.TabPages.Remove(scriptTabs.TabPages[scriptTabs.SelectedIndex]);

                if ((selectedIndex - 1) >= 0)
                {
                    scriptTabs.SelectedIndex = (selectedIndex - 1);
                }
                else
                {
                    scriptTabs.SelectedIndex = 0;
                }
                return true;
            }
            return false;
        }

    }
}
