using Speciale.Common;
using Speciale.SuffixArray;
using System;
using System.Collections;
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

        public static int[] FindPhraseIndexToDecompressedLength(Phrase[] phrases)
        {
            int[] indexToLength = new int[phrases.Length + 1];

            int matched = 0;
            for (int i = 0; i < phrases.Length; i++)
            {
                indexToLength[i] = matched;
                matched += phrases[i].len == 0 ? 1 : phrases[i].len;
            }
            indexToLength[phrases.Length] = matched;
            return indexToLength;

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


            string S = isFile ? File.ReadAllText(data) : data;

            int[] phraseLengths = new int[S.Length + 20];
            int[] phrasePositions = new int[S.Length + 20];

            int phraseCount;
            int[] SA2 = new int[SA.Length + 2]; // KKP3 requires two more entries after end of SA.
            SA.CopyTo(SA2, 0);




            if (algo == LZ77Algorithm.kkp3)
                phraseCount = LZ77DLL(S, SA2, S.Length,  phrasePositions,  phraseLengths, 1);
            else if (algo == LZ77Algorithm.kkp2)
                phraseCount = LZ77DLL(S, SA, S.Length,  phrasePositions,  phraseLengths, 2);
            else
                throw new Exception("Algorithm not supported for LZ77 generation DLL");



            var res = Phrase.ArraysToObject(phrasePositions, phraseLengths, phraseCount);

            // Force GC
            phraseLengths = null;
            phrasePositions = null;
            return res;


        }

    }
}
