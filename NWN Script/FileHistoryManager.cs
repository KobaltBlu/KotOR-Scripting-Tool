using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NWN_Script
{
    class FileHistoryManager
    {

        private static int nMaxRecentFiles = 15;
        public static List<string> RecentFiles = new List<string>();
        public static List<string> OpenFiles = new List<string>();

        private static Action RecentFilesChanged = null;

        public static void Initialize()
        {
            string recentFilesStored = Properties.Settings.Default.RecentFiles;
            string openFilesStored = Properties.Settings.Default.OpenFiles;

            RecentFiles = recentFilesStored.Split(',').ToList<string>();
            OpenFiles = openFilesStored.Split(',').ToList<string>();

            string path = @".nwscript"; // or whatever 
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
                di.Attributes = FileAttributes.Directory;// | FileAttributes.Hidden;
            }
        }

        public static void OnRecentFilesChanged(Action action)
        {
            RecentFilesChanged = action;
        }

        private static bool RemoveRecent( string fileName)
        {
            return RecentFiles.Remove(fileName);
        }

        public static void OpenFile( string fileName )
        {
            CloseFile(fileName);
            RemoveRecent(fileName);
            RecentFiles.Insert(0, fileName);
            RecentFiles = RecentFiles.Take(nMaxRecentFiles).ToList<string>();
            OpenFiles.Add(fileName);
            if(RecentFilesChanged != null)
            {
                RecentFilesChanged();
            }
            Save();
        }

        public static bool CloseFile( string fileName )
        {
            bool removed = OpenFiles.Remove(fileName);
            Save();
            try
            {
                string md5Hash = FileSettings.ToHashString(fileName);
                string cacheFile = Path.Combine(".nwscript", md5Hash + ".json");
                File.Delete(cacheFile);
            }
            catch (Exception exception) { }
            return removed;
        }

        public static void Save()
        {
            string[] recentFiles = RecentFiles.Take(nMaxRecentFiles).ToArray();
            string[] openFiles = OpenFiles.ToArray();

            Properties.Settings.Default.RecentFiles = String.Join(",", recentFiles);
            Properties.Settings.Default.OpenFiles = String.Join(",", openFiles);
            Properties.Settings.Default.Save();
        }

    }
}
