using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace NWN_Script
{
    class FileSettings
    {

        public string FileName = null;
        public string CompleteFileName = null;
        public FileDataType FileDataType = FileDataType.PLAIN_TEXT;
        public bool UnsavedChanges = false;
        public string FilePath = null;
        public string SavedContent = null;
        public string Content = null;
        public int Selection = 0;
        
        public bool HasUnsavedChanges()
        {
            return SavedContent != Content;
        }

        //Saves a copy of the file to cache
        public void SaveState()
        {
            try {
                string md5Hash = ToHashString(CompleteFileName);
                string json = JsonConvert.SerializeObject(this);
                using (StreamWriter outfile = new StreamWriter(Path.Combine(".nwscript", md5Hash + ".json")))
                {
                    outfile.Write(json);
                }
                SavedContent = Content;
            }catch(Exception exception)
            {
                Debug.WriteLine("SaveState(): " + exception.ToString());
            }
        }

        //Reloads the real file from disk
        public void ReloadState()
        {

        }

        public static FileSettings LoadState( string file )
        {
            string md5Hash = ToHashString(file);
            string cachePath = Path.Combine(".nwscript", md5Hash + ".json");
            FileSettings fileSettings;
            if ( File.Exists(cachePath) )
            {
                using (StreamReader reader = new StreamReader(cachePath))
                {
                    fileSettings = JsonConvert.DeserializeObject<FileSettings>(reader.ReadToEnd());
                    return fileSettings;
                }
            }
            fileSettings = new FileSettings();
            using (StreamReader reader = new StreamReader(file))
            {

                string fileExt = Path.GetExtension(file);
                string fileName = Path.GetFileName(file);
                string filePath = Path.GetFullPath(file);
                FileDataType dataType = (fileExt == ".ncs") ? FileDataType.COMPILED : FileDataType.PLAIN_TEXT;

                string str = reader.ReadToEnd();
                string text = "";
                if (dataType == FileDataType.COMPILED)
                {
                    Encoding ascii = Encoding.ASCII;
                    byte[] encodedBytes = ascii.GetBytes(str);
                    text = ascii.GetString(encodedBytes);
                }
                else
                {
                    text = str;
                }

                fileSettings.FileName = fileName;
                fileSettings.CompleteFileName = file;
                fileSettings.FilePath = filePath;
                fileSettings.FileDataType = dataType;
                fileSettings.SavedContent = text;
                fileSettings.Content = fileSettings.SavedContent;
            }
            return fileSettings;
        }

        public static string ToHashString(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        
        }

        public static FileSettings NewPageFile()
        {
            FileSettings fileSettings = new FileSettings();

            fileSettings.FileName = "untitled.nss";
            fileSettings.CompleteFileName = "untitled.nss";
            fileSettings.FilePath = "untitled.nss";
            fileSettings.FileDataType = FileDataType.PLAIN_TEXT;
            //fileSettings.SavedContent = text;
            fileSettings.Content = "void main(){\n\t\n\t\n\t\n}";
            fileSettings.Selection = 16;

            return fileSettings;
        }

    }

    enum FileDataType
    {
        PLAIN_TEXT,
        COMPILED
    }
}
