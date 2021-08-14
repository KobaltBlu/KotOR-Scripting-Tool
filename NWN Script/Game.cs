using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NWN_Script
{
    class Game
    {
        public string Diretory = null;
        public string Name = null;
        public string NWScript_Location = null;
        public string NWNNssComp_Location = null;
        public Bitmap Icon = null;

        public NWScriptParser NWScript = null;

        public Game(string name = null, string directory = null, string nwscript_location = null)
        {
            Name = name;
            Diretory = directory;
            NWScript_Location = nwscript_location;
            NWNNssComp_Location = nwscript_location;

        NWScript = new NWScriptParser();
        }

        public void CompileScript( FileSettings editorFile )
        {
            if (NWScript_Location != null)
            {
                Process nwnnsscomp = new Process();

                string sourcePath = editorFile.CompleteFileName;
                string directoryName = Path.GetDirectoryName(sourcePath);
                string outputPath = editorFile.FilePath + Path.GetFileNameWithoutExtension(sourcePath) + ".ncs";
                string[] pathArray = { Application.StartupPath, NWScript_Location, "nwnnsscomp.exe" };
                string compilerPath = Path.Combine(pathArray);

                nwnnsscomp.StartInfo.FileName = compilerPath;
                //MessageBox.Show("-c \"" + fileToCompile + "\"");
                nwnnsscomp.StartInfo.Arguments = "-c \"" + sourcePath + "\"";
                nwnnsscomp.StartInfo.RedirectStandardOutput = true;
                nwnnsscomp.StartInfo.UseShellExecute = false;
                nwnnsscomp.Start();
                MessageBox.Show(nwnnsscomp.StandardOutput.ReadToEnd());
            }
        }

        public void DecompileScript( string filePath )
        {
            string[] pathArray = { Application.StartupPath, NWScript_Location, "nwnnsscomp.exe" };
            string compilerPath = Path.Combine(pathArray);

            Process nwnnsscomp = new Process();
            nwnnsscomp.StartInfo.FileName = compilerPath;
            nwnnsscomp.StartInfo.Arguments = "-d \"" + filePath + "\" -o \""+ Path.Combine(Application.StartupPath, "tmp_decompiled.nss") + "\"";
            nwnnsscomp.StartInfo.RedirectStandardOutput = true;
            nwnnsscomp.StartInfo.UseShellExecute = false;
            nwnnsscomp.StartInfo.WorkingDirectory = Application.StartupPath;
            nwnnsscomp.Start();

            MessageBox.Show(nwnnsscomp.StandardOutput.ReadToEnd());
        }

        public void ParseNWScript()
        {
            NWScript.Parse(NWScript_Location);
        }

        public void SetMenuIconResource(Bitmap resource)
        {
            Icon = resource;
        }

        public void BuildMenuItem()
        {
            /*
            this.kOTORIToolStripMenuItem2.Checked = true;
            this.kOTORIToolStripMenuItem2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.kOTORIToolStripMenuItem2.Image = global::NWN_Script.Properties.Resources.k1;
            this.kOTORIToolStripMenuItem2.Name = "kOTORIToolStripMenuItem2";
            this.kOTORIToolStripMenuItem2.Size = new System.Drawing.Size(118, 22);
            this.kOTORIToolStripMenuItem2.Text = "KotOR I";
            this.kOTORIToolStripMenuItem2.Click += new System.EventHandler(this.kOTORIToolStripMenuItem2_Click);
            */
        }

    }

    class GameManager
    {
        //0 = KotOR I  - //DEFAULT
        //1 = KotOR II
        public static int CurrentGame = Properties.Settings.Default.CurrentGame;
        public static Game Game;
        public static List<Game> Games = new List<Game>();

        public static Game GetCurrentGame()
        {
            return Games[CurrentGame];
        }

        public static void SetCurrentGameIndex(int index)
        {
            CurrentGame = index;
            Properties.Settings.Default.CurrentGame = CurrentGame;
            Properties.Settings.Default.Save();
        }

        public static List<FunctionsListItem> GetFunctionsList()
        {
            return GetCurrentGame().NWScript.Functions;
        }

        public static List<ConstantListItem> GetConstantsList()
        {
            return GetCurrentGame().NWScript.Constants;
        }

    }

}
