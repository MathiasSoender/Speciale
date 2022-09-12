using Speciale.Common;
using Speciale.SuffixArray;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public static Phrase[][] GeneratePhrasesForAllSuffixes(string textFile, string text)
        {
            List<string> suffixes = new List<string>();

            for (int i = 0; i < text.Length; i++)
            {
                suffixes.Add(text.Substring(i));
            }

            var allPhrases = new Phrase[text.Length][];

            for (int i = 0; i < suffixes.Count(); i++)
            {
                string tempFile = textFile + ".temp";
                string tempFilePhrases = tempFile + ".phrases";

                File.WriteAllText(tempFile, suffixes[i]);

                SAWrapper.GenerateSuffixArray(tempFile, out _);
                LZ77Wrapper.GenerateLZ77Phrases(tempFile, tempFilePhrases, LZ77Wrapper.LZ77Algorithm.kkp3);
                var tempPhrases = File.ReadAllLines(tempFilePhrases);
                allPhrases[i] = PhraseFileToObject(tempPhrases);

                File.Delete(tempFile);
                File.Delete(tempFilePhrases);
            }



            return allPhrases;
        }


        public static Phrase[][] GeneratePhrasesForAllSuffixes2(string textFile, string text)
        {

            SAWrapper.GenerateAllSuffixArrays(textFile);
            string phraseFile = textFile + "phrase";
            LZ77Wrapper.GenerateSuffixLZ77Phrases(textFile, phraseFile, LZ77Wrapper.LZ77Algorithm.kkp3);
            Phrase[][] allPhrases = new Phrase[text.Length][];

            for (int i = 0; i < text.Length; i++)
            {
                string tempPhraseFile = phraseFile + i;

                allPhrases[i] = PhraseFileToObject(tempPhraseFile);


                // Cleanup
                if (File.Exists(tempPhraseFile))
                    File.Delete(tempPhraseFile);

                if (File.Exists(textFile + i + ".sa"))
                    File.Delete(textFile + i + ".sa");

            }







            return allPhrases;
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




        public static string DecompressLZ77Phrases(string[] phrases)
        {
            string outputString = "";

            foreach(string phrase in phrases)
            {
                var split = phrase.Split(" ");
                int repeatLen = int.Parse(split[1]);

                if (repeatLen == 0)
                {
                    char character = (char)int.Parse(split[0]);
                    outputString += character;
                }
                else
                {
                    int startPos = int.Parse(split[0]);

                    for(int j = startPos; j < (startPos + repeatLen); j++)
                    {
                        outputString += outputString[j];
                    }

                }

            }


            return outputString;
        }
    }
}
