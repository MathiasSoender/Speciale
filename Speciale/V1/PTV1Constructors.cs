using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.SuffixTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.V1
{

    public static class PTV1Constructors
    {
        // suffixPhrases format 2 cases: UNICODE 0 or P_i L_i
        // Unicode is a single Char
        // P_i is position of where to copy
        // L_i is the length to copy
        // Example: 100 0,200 0,300 0,0 2
        // (Comma separated phrases)



        public static PhaseTrieV1 NaiveConstruction(string textFile, int[] SA)
        {
            var S = File.ReadAllText(textFile);
            var suffixPhrases = Phrase.GeneratePhrasesForAllSuffixes2(textFile, S);
            var PT =  NaiveConstruction(suffixPhrases, SA);
            PT.S = S;

            PT.lcpDS = new LCP(PT, LCPType.naive);
            return PT;
        }


        // O(n^2) preprocessing time

        private static PhaseTrieV1 NaiveConstruction(Phrase[][] suffixPhrases, int[] SA)
        {

            PhaseTrieV1 PT = new PhaseTrieV1();
            PTNodeV1 root = new PTNodeV1();
            PT.root = root;
            PT.suffixPhrases = suffixPhrases;
            PT.SA = SA;

            for (int i = 0; i < suffixPhrases.Length; i++)
            {
                PTNodeV1 child;
                PTNodeV1 curNode = root;
                int curIndex = 0;
                Phrase[] curSuffixPhrases = suffixPhrases[i];


                while (curIndex < curSuffixPhrases.Length)
                {
                    if (!curNode.childrenMap.TryGetValue(curSuffixPhrases[curIndex], out child))
                    {
                        PTNodeV1 leafNode = new PTNodeV1(curSuffixPhrases.Skip(curIndex).ToList(), i);
                        curNode.childrenMap.Add(leafNode.phrases[0], leafNode);
                        curIndex = curSuffixPhrases.Length;
                    }
                    else
                    {
                        // Could match first phrase, try to traverse remaining
                        List<Phrase> traversalPhrases = child.phrases;
                        int matchedLength = 0;
                        while (traversalPhrases.Count() > matchedLength && curSuffixPhrases.Length > (matchedLength + curIndex) &&
                               traversalPhrases[matchedLength].Equals(curSuffixPhrases[matchedLength + curIndex]))
                        {
                            matchedLength++;
                        }

                        // we matched everything of edge to new internal node
                        if (traversalPhrases.Count() == matchedLength)
                        {
                            curIndex += matchedLength;
                            curNode = child;
                        }
                        // We fell off traversing edge
                        // Split edge
                        else
                        {
                            curNode.childrenMap.Remove(curSuffixPhrases[curIndex]);
                            PTNodeV1 splitNode = new PTNodeV1(child.phrases.Take(matchedLength).ToList());
                            curNode.childrenMap.Add(splitNode.phrases[0], splitNode);
                            child.phrases = child.phrases.Skip(matchedLength).ToList();
                            splitNode.childrenMap.Add(child.phrases[0], child);

                            PTNodeV1 leafNode = new PTNodeV1(curSuffixPhrases.Skip(matchedLength + curIndex).ToList(), i);
                            splitNode.childrenMap.Add(leafNode.phrases[0], leafNode);

                            curIndex = curSuffixPhrases.Length;


                        }



                    }
                }

            }


            PT.FinalizeConstruction();
            return PT;

        }

    }
}
