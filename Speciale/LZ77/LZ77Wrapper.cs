using Speciale.Common;
using Speciale.SuffixArray;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.LZ77
{

    public class Phrase
    {
        public int len;
        public int pos; // pos is unicode char if len == 0

        public static int FindDecompressedLength(Phrase[] phrases)
        {
            int l = 0;
            Array.ForEach(phrases, x => l += Math.Max(x.len, 1));
            return l;


        }

        public static Phrase[] ArraysToObject(int[] phrasePositions, int[] phraseLengths, int phraseCount)
        {
            if (phrasePositions.Length != phraseLengths.Length)
                throw new Exception("Positions and lengths are not equal");

            Phrase[] phrases = new Phrase[phraseCount];
            for (int i = 0; i < phraseCount; i++)
            {
                phrases[i] = new Phrase() { len = phraseLengths[i], pos = phrasePositions[i] };
            }

            return phrases;
        }

        public static Phrase[] PhraseFileToObject(string phrasefile)
        {
            return PhraseFileToObject(File.ReadAllLines(phrasefile));
        }

        public static Phrase[] PhraseFileToObject(string[] phrases)
        {
            Phrase[] phrasesParsed = new Phrase[phrases.Length];

            for(int i = 0; i < phrases.Length; i++)
            {
                var split = phrases[i].Split(" ");
                int loadpos = int.Parse(split[0]);
                int loadlen = int.Parse(split[1]);
                phrasesParsed[i] = new Phrase() { len = loadlen, pos = loadpos };

            }

            return phrasesParsed;
        }

        // O(n^2) time & space
        public static Phrase[][] GeneratePhrasesForAllSuffixes(string text)
        {
            [DllImport("GenerateSA.dll", EntryPoint = "Free", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            static extern void Free(int[] SA);


            var allPhrases = new Phrase[text.Length][];

            for (int i = 0; i < text.Length; i++)
            {
                string curSuffix = text.Substring(i);
                int[] SA = SAWrapper.GenerateSuffixArrayDLL(curSuffix, false);
                Phrase[] res = LZ77Wrapper.GenerateLZ77PhrasesDLL(curSuffix, false, SA, LZ77Wrapper.LZ77Algorithm.kkp3);
                allPhrases[i] = res;
            }



            return allPhrases;
        }

        public static string DecompressLZ77Phrases(Phrase[] phrases)
        {
            string outputString = "";

            foreach (Phrase phrase in phrases)
            {

                if (phrase.len == 0)
                {
                    char character = (char)phrase.pos;
                    outputString += character;
                }
                else
                {

                    for (int j = phrase.pos; j < (phrase.pos + phrase.len); j++)
                    {
                        outputString += outputString[j];
                    }

                }

            }


            return outputString;
        }



        public override int GetHashCode()
        {
            return len^pos;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as Phrase);
        }

        public bool Equals(Phrase obj)
        {
            return obj != null && obj.len == this.len && obj.pos == this.pos;
        }

        public override string ToString()
        {
            if (len == 0)
            {
                return (char)pos + " ";
            }

            return pos.ToString() + " " + len.ToString();
        }


    }

    public static class LZ77Wrapper
    {

        public enum LZ77Algorithm
        {
            kkp3,
            kkp2,
            kkp1s
        }


        public static Phrase[] GenerateLZ77PhrasesDLL(string data, bool isFile, int[] SA, LZ77Algorithm algo)
        {
            [DllImport("LZ77.dll", EntryPoint = "LZ77DLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            static extern int LZ77DLL([MarshalAs(UnmanagedType.LPStr)] string data, int[] SA, int length, int[] phrasePositions, int[] phraseLengths, int algorithm);

            [DllImport("LZ77.dll", EntryPoint = "Free", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            static extern void Free(int[] phrasePositions, int[] phraseLengths);



            string S;
            if (isFile)
                S = File.ReadAllText(data);
            else
                S = data;


            int[] phraseLengths = new int[S.Length];
            int[] phrasePositions = new int[S.Length];

            /*
            for (int i = 0; i < S.Length; i++)
            {
                phraseLengths[i] = -1;
                phrasePositions[i] = -1;
            }
            */
            int phraseCount;

            if (algo == LZ77Algorithm.kkp3)
                phraseCount = LZ77DLL(S, SA, S.Length, phrasePositions, phraseLengths, 1);
            else if (algo == LZ77Algorithm.kkp2)
                phraseCount = LZ77DLL(S, SA, S.Length, phrasePositions, phraseLengths, 2);
            else
                throw new Exception("Algorithm not supported for LZ77 generation DLL");



            var res = Phrase.ArraysToObject(phrasePositions, phraseLengths, phraseCount);


            phrasePositions = null;
            phraseLengths = null;
            // GC.Collect();

            return res;


        }



        #region unused .exe entry points


        public static void GenerateLZ77Phrases(string infile, string phrasesfile, LZ77Algorithm algo)
        {
            DateTime t1 = DateTime.Now;
            ProcessStartInfo psi = Statics.GetStartInfo("LZ77.exe", infile + " " + algo.ToString() + " " + phrasesfile + " single");
            Double time;
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                time = (process.ExitTime - process.StartTime).TotalSeconds;

                if (process.ExitCode != 0)
                    throw new Exception("LZ77.exe failed.. Exit code: " + process.ExitCode);
            }
            var otime = (DateTime.Now - t1);
            var q = 0;

        }



        public static void GenerateSuffixLZ77Phrases(string infile, string phrasesfile, LZ77Algorithm algo)
        {
            ProcessStartInfo psi = Statics.GetStartInfo("LZ77.exe", infile + " " + algo.ToString() + " " + phrasesfile + " all");
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception("LZ77.exe failed.. Exit code: " + process.ExitCode);
            }

        }

        #endregion


    }
}
