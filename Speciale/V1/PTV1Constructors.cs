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

        private static Phrase[] FindSuffixPhrase(int suffixIndex, Dictionary<int,int> SItoPImap, out int startIndex, string S, Phrase[] phrases)
        {
            Phrase[] newPhrase;
            int phraseIndex = SItoPImap[suffixIndex];
            startIndex = phraseIndex == phrases.Length - 1 ? -1 : phraseIndex + 1;

            Phrase curPhrase = phrases[phraseIndex];

            if (curPhrase.len == 0)
            {
                newPhrase = new Phrase[1] { curPhrase };
            }
            else if (curPhrase.len == 1)
            {
                newPhrase = new Phrase[1];
                newPhrase[0] = new Phrase() { len = 0, pos = (char)S[curPhrase.pos] };
            }
            else
            {
                newPhrase = new Phrase[2];
                newPhrase[0] = new Phrase() { len = 0, pos = (char)S[curPhrase.pos] };
                newPhrase[1] = new Phrase() { len = curPhrase.len - 1, pos = curPhrase.pos + 1 };
            }


            return newPhrase;
        }

        private static Dictionary<int, int> FindSuffixIndexToPhraseIndexMap(string S, Phrase[] phrases)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            int curPos = 0;
            int curPhrase = 0;

            for (int i = 0; i < S.Length; i++)
            {
                if (i > curPos)
                {
                    curPhrase++;
                    curPos += phrases[curPhrase].len == 0 ? 1 : phrases[curPhrase].len;

                }
                result.Add(i, curPhrase);
            }

            return result;

        }


        public static PhaseTrieV1 FastConstruction(string S, int[] SA)
        {
            var lcpDS = new LCP(S, SA, LCPType.fast);

            PhaseTrieV1 PT = new PhaseTrieV1();
            PTNodeV1 root = new PTNodeV1();
            PT.root = root;
            PT.SA = SA;
            PT.S = S;
            PT.lcpDS = lcpDS;

            var SAdata = SAWrapper.GenerateSuffixArrayDLL(S, false);
            Phrase[] phrasesData = LZ77Wrapper.GenerateLZ77PhrasesDLL(S, false, SAdata, LZ77Wrapper.LZ77Algorithm.kkp3);
            var SItoPhraseIndex = FindSuffixIndexToPhraseIndexMap(S, phrasesData);


            for (int i = 0; i < S.Length; i++)
            {
                int startIndex;
                var curPhrases = FindSuffixPhrase(i, SItoPhraseIndex, out startIndex, S, phrasesData);
            }




            return PT;


        }

        private static int[] FindSAFromPrevious(int[] curSA)
        {
            var SAlist = curSA.ToList();
            int killIndex = 0;
            for (int i = 0; i < SAlist.Count(); i++)
            {
                if (SAlist[i] == 0)
                {
                    killIndex = i;
                }
                SAlist[i] -= 1;
            }
            SAlist.RemoveAt(killIndex);

            return SAlist.ToArray();
        }

        // O(n^2) preprocessing time

        public static PhaseTrieV1 NaiveConstruction(string S, int[] SA)
        {
            var lcpDS = new LCP(S, SA, LCPType.fast);
            
            PhaseTrieV1 PT = new PhaseTrieV1();
            PTNodeV1 root = new PTNodeV1();
            PT.root = root;
            PT.SA = SA;
            PT.S = S;
            PT.lcpDS = lcpDS;

            int[] curSA = new int[SA.Length];
            Array.Copy(SA, curSA, SA.Length);

            for (int i = 0; i < S.Length; i++)
            {
                PTNodeV1 child;
                PTNodeV1 curNode = root;
                int curIndex = 0;

                int[] SAsuffix;
                if (i == 0)
                {
                    SAsuffix = curSA;
                    curSA = new int[SAsuffix.Length];
                    Array.Copy(SAsuffix, curSA, SAsuffix.Length);
                }
                else
                {
                    SAsuffix = FindSAFromPrevious(curSA);
                    curSA = new int[SAsuffix.Length];
                    Array.Copy(SAsuffix, curSA, SAsuffix.Length);
                }

                string curSub = S.Substring(i);
                // var SAsuffix = SAWrapper.GenerateSuffixArrayDLL(curSub, false);
                Phrase[] curSuffixPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curSub, false, SAsuffix, LZ77Wrapper.LZ77Algorithm.kkp3);


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
                            // Figure out which leaf vertex is actually correct:
                            // Very dummy way to do so (linearly search index):
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
            PT.SetPropertiesPhraseTrie(root, S.Length);
            return PT;

        }

    }
}
