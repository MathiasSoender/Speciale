using System.Diagnostics;
using System.Text;

namespace Speciale.Common
{
    public enum ConstructionType
    {
        fast,
        naive,
        KKP3,
        CSC,
        LexicoOrdered
    }

    public class MemoryCounter
    {
        readonly PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);
        private float maxMemory;
        public void MeasureMemory()
        {
            maxMemory = Math.Max(ramCounter.NextValue(), maxMemory);
        }

        public MemoryCounter()
        {
            MeasureMemory();
            maxMemory = 0;
        }

        public float GetMaxMemory()
        {
            return maxMemory;
        }

        
    }

    public static class Statics
    {
        public static void Guard(int compRounds, DateTime startTime, MemoryCounter MC)
        {
            if (MC == null)
                return;


            if (compRounds % 5000 == 0)
            {
                MC.MeasureMemory();
                if (MC.GetMaxMemory() / (1024.0 * 1024.0 * 1024.0) > Tester.MAX_GB)
                    throw new OutOfMemoryException("Construction has exceed maximum memory");
            }
            if ((DateTime.Now - startTime).TotalSeconds > Tester.TIME_OUT_SECONDS)
            {
                throw new TimeoutException("Construction has timed out.");
            }
        }


        public static void GenerateData(string alphabet, int n, string outfile)
        {
            Random random = new Random();

            var data = new string(Enumerable.Repeat(alphabet, n).Select(s => s[random.Next(s.Length)]).ToArray());

            File.WriteAllText(outfile, data);
        }

        public static int[] InverseArray(int[] arr)
        {

            int[] indexToLexi = new int[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                indexToLexi[arr[i]] = i;

            return indexToLexi;
        }

        public static void PruneTextFile(string textfile, string patternfile = null, bool pruneDanish = true)
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
            var p = patternfile == null ? null : File.ReadAllText(patternfile);

            int maxCharS;
            int maxCharPosS;
            int maxCharP;

            var newS = Pruner(s, out maxCharS, out maxCharPosS);
            var newP = p == null ? null : Pruner(p, out _, out _);



            if (s[s.Length - 1] <= maxCharS && maxCharPosS != s.Length - 1)
            {
                // Console.Out.WriteLine("The last symbol of S does not have greatest lexigraphical value (of S and P). Appending S with new symbol of greatest value");
                newS.Append((char)(maxCharS + 1));
            }

            File.WriteAllText(textfile, newS.ToString());
            if (p != null) File.WriteAllText(patternfile, newP.ToString());

        }
    }
}
