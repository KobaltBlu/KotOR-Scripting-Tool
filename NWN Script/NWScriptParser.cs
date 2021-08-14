using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NWN_Script
{
    class NWScriptParser
    {

        public List<FunctionsListItem> Functions = new List<FunctionsListItem>(); //Holds all the Functions for the parsed NWScript.nss
        public List<ConstantListItem> Constants = new List<ConstantListItem>(); //Holds all the CONSTANTS for the parsed NWScript.nss

        public void Parse( string nwscript_location = @"" )
        {

            string[] pathArray = { Application.StartupPath, nwscript_location, @"nwscript.nss" };
            using ( StreamReader reader = new StreamReader( Path.Combine(pathArray) ) )
            {

                string source = reader.ReadToEnd().ToString();
                string line;
                int counter = 0;

                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();

                List<string> sourcelist = source.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
                sourcelist.Reverse();

                while ((line = reader.ReadLine()) != null)
                {
                    string voidPattern = "^void.*\\(.*\\);";
                    string objectPattern = "^object.*\\(.*\\);";
                    string intPattern = "^int.*\\(.*\\);";
                    string floatPattern = "^float.*\\(.*\\);";
                    string effectPattern = "^effect.*\\(.*\\);";
                    string eventPattern = "^event.*\\(.*\\);";
                    string stringPattern = "^string.*\\(.*\\);";
                    string vectorPattern = "^vector.*\\(.*\\);";
                    string locationPattern = "^location.*\\(.*\\);";

                    if (Regex.IsMatch(line, "^(effect|void|int|object|event|location|float|string|vector).*\\(.*\\);"))
                    {
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
                        }
                        else if (Regex.IsMatch(line, effectPattern))
                        {
                            Functtype = "effect";
                        }
                        else if (Regex.IsMatch(line, eventPattern))
                        {
                            Functtype = "event";
                        }
                        else if (Regex.IsMatch(line, stringPattern))
                        {
                            Functtype = "string";
                        }
                        else if (Regex.IsMatch(line, vectorPattern))
                        {
                            Functtype = "vector";
                        }
                        else if (Regex.IsMatch(line, locationPattern))
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
                            Functions.Add(new FunctionsListItem(Functtype, Name, Line, About, args, LineNumber));
                        }
                    }
                    counter++;
                }


                //GET CONSTANTS
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();
                int lineNum = 0;

                Constants.Add(new ConstantListItem("object", "OBJECT_SELF", "0"));

                while (((line = reader.ReadLine()) != null))
                {
                    string conts_pattern = "^(int|float|string)\\s+\\b([A-Z0-9_d]+)\\s+=\\s+(.*);";
                    if (Regex.IsMatch(line, conts_pattern))
                    {
                        Match match = Regex.Match(line, conts_pattern);

                        ConstantListItem item = new ConstantListItem(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
                        item.LineNumber = lineNum;

                        Constants.Add(item);
                    }

                    if(Regex.IsMatch(line, "^string\\s+sLanguage\\s+=\\s+\"nwscript\";"))
                    {
                        break;
                    }

                    lineNum++;
                }
            }

            //Constants.Sort();
        }
    }

    public class ConstantListItem
    {
        public string Type = null;
        public string Name = null;
        public string Value = null;
        public int LineNumber = 0;

        public ConstantListItem(string type, string name, string value)
        {
            Type = type;
            Name = name;
            Value = value;
        }

    }


    public class FunctionsListItem
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

}
