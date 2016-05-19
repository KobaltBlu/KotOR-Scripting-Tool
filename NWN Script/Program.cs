using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using System.IO;
using System.IO.Pipes;
using System.Threading;
using FlawlessCode;


namespace NWN_Script
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {



            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string[] args = Environment.GetCommandLineArgs();

            Application.SetCompatibleTextRenderingDefault(false);
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processItems = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process item in processItems)
            {
                if (item.Id != currentProcess.Id)
                {
                    MessageBox.Show("Another instance running");
                    return;
                }
            }

            Application.Run(new ScriptEditorWindow());  */

            Guid guid = new Guid("{6EAE2E61-E7EE-42bf-8EBE-BAB89DE5410F}");
            using (SingleInstance singleInstance = new SingleInstance(guid))
            {
                if (singleInstance.IsFirstInstance)
                {
                    singleInstance.ArgumentsReceived += singleInstance_ArgumentsReceived;
                    singleInstance.ListenForArgumentsFromSuccessiveInstances();

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(form = new ScriptEditorWindow());
                }
                else
                    singleInstance.PassArgumentsToFirstInstance(Environment.GetCommandLineArgs());
            }


        }

        static ScriptEditorWindow form;

        static void singleInstance_ArgumentsReceived(object sender, ArgumentsReceivedEventArgs e)
        {
            if (form == null)
                return;

            Action<String[]> updateForm = arguments =>
            {
                form.WindowState = FormWindowState.Normal;

                List<string> newArgs = arguments.ToList<string>();
                newArgs.RemoveAt(0);

                arguments = newArgs.ToArray<string>();

                form.OpenFiles(arguments);
            };
            form.Invoke(updateForm, (Object)e.Args); //Execute our delegate on the forms thread!
        }
    }
}
