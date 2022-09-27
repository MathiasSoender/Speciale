using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Speciale.Common
{
    public static class Statics
    {
        // Make sure to build GenerateSA.exe + LZ77.exe to debug/release folder
        // These are built directly to bin/debug or bin/release (so we must use "GetParent(...)")
        public static ProcessStartInfo GetStartInfo(string exeName, string arguments)
        {

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = GetDLLorEXEpath(exeName);

            psi.CreateNoWindow = true;
            psi.Arguments = arguments;
            return psi;
        }

        public static string GetDLLorEXEpath(string name)
        {
            string filename;
            if (new DirectoryInfo(AppContext.BaseDirectory).Name.Contains("net"))
            {
                var parentDir = Directory.GetParent(AppContext.BaseDirectory);
                filename = Path.Combine(parentDir.Parent.FullName, name);

            }
            else
            {
                filename = Path.Combine(AppContext.BaseDirectory, name);

            }
            return filename;
        }

        public static int[] InverseArray(int[] arr)
        {

            int[] indexToLexi = new int[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                indexToLexi[arr[i]] = i;

            return indexToLexi;
        }

        public static void PruneTextFile(string textfile, string patternfile, bool pruneDanish = true)
        {

            StringBuilder Pruner(string s, out int maxChar, out int maxCharPos)
            {
                List<char> unallowedEscapeSymbols = new List<char>() { '\n', '\r', '\t', '\a', '\b', '\f', '\v', '\'', '\"' };
                List<char> unallowedSymbols = new List<char>() { 'å', 'æ', 'ø' };
                maxChar = -1;
                maxCharPos = -1;
                StringBuilder newS = new StringBuilder();



                for (int i = 0; i < s.Length; i++)
                {
                    char? symbol = null;

                    if (s[i] == '“' || s[i] == '”')
                    {
                        symbol = '"';
                    }
                    else if (s[i] == '’')
                    {
                        symbol = (char)39;
                    }
                    else if (unallowedEscapeSymbols.Contains(s[i]))
                    {
                        symbol = ' ';
                    }
                    else
                    {
                        if (pruneDanish && !unallowedSymbols.Contains(s[i]))
                            symbol = s[i];
                    }

                    if (symbol != null)
                    {
                        if (maxChar < symbol)
                        {
                            maxChar = (int)symbol;
                            maxCharPos = i;
                        }
                        newS.Append(symbol);
                    }

                }
                return newS;
            }


            var s = File.ReadAllText(textfile);
            var p = File.ReadAllText(patternfile);

            int maxCharS;
            int maxCharPosS;
            int maxCharP;

            var newS = Pruner(s, out maxCharS, out maxCharPosS);
            var newP = Pruner(p, out _, out _);



            if (s[s.Length - 1] <= maxCharS && maxCharPosS != s.Length - 1)
            {
                Console.Out.WriteLine("The last symbol of S does not have greatest lexigraphical value (of S and P). Appending S with new symbol of greatest value");
                newS.Append((char)(maxCharS + 1));
            }

            File.WriteAllText(textfile, newS.ToString());
            File.WriteAllText(patternfile, newP.ToString());

        }
    }
}
