using Speciale.Common;
using Speciale.CSC;
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

        private static void AddSuffixPhrase(Phrase[] curSuffixPhrases, PTV1Node curNode, int curSuffixIndex)
        {
            PTV1Node child;
            int curPhraseIndex = 0;
            while (curPhraseIndex < curSuffixPhrases.Length)
            {
                if (!curNode.childrenMap.TryGetValue(curSuffixPhrases[curPhraseIndex], out child))
                {
                    PTV1Node leafNode = new PTV1Node(curSuffixPhrases.Skip(curPhraseIndex).ToList(), curSuffixIndex);
                    curNode.childrenMap.Add(leafNode.phrases[0], leafNode);
                    break;
                }
                else
                {
                    // Could match first phrase, try to traverse remaining
                    List<Phrase> traversalPhrases = child.phrases;
                    int matchedLength = 0;
                    while (traversalPhrases.Count() > matchedLength && curSuffixPhrases.Length > (matchedLength + curPhraseIndex) &&
                           traversalPhrases[matchedLength].Equals(curSuffixPhrases[matchedLength + curPhraseIndex]))
                    {
                        matchedLength++;
                    }

                    // we matched everything of edge to new internal node
                    if (traversalPhrases.Count() == matchedLength)
                    {
                        curPhraseIndex += matchedLength;
                        curNode = child;
                    }
                    // We fell off traversing edge
                    // Split edge
                    else
                    {
                        // Figure out which leaf vertex is actually correct:
                        // Very dummy way to do so (linearly search index):
                        curNode.childrenMap.Remove(curSuffixPhrases[curPhraseIndex]);
                        PTV1Node splitNode = new PTV1Node(child.phrases.Take(matchedLength).ToList());
                        curNode.childrenMap.Add(splitNode.phrases[0], splitNode);
                        child.phrases = child.phrases.Skip(matchedLength).ToList();
                        splitNode.childrenMap.Add(child.phrases[0], child);

                        PTV1Node leafNode = new PTV1Node(curSuffixPhrases.Skip(matchedLength + curPhraseIndex).ToList(), curSuffixIndex);
                        splitNode.childrenMap.Add(leafNode.phrases[0], leafNode);

                        break;


                    }

                }
            }
        }

        public static PhraseTrieV1 Construct(string S, int[] SA, int[] invSA, LCP lcpDS, TestType constructType, MemoryCounter MC)
        {
            PTV1Node root = new PTV1Node();

            PhraseTrieV1 PT = new PhraseTrieV1() { root = root, SA = SA, S = S, lcpDS = lcpDS, invSA = invSA };

            if (constructType == TestType.ConstructPTV1CSC)
                ConstructionByCSC(PT, MC);
            else if (constructType == TestType.ConstructPTV1KKP3)
                ConstructionByKKP3(PT, MC);
            else
                throw new Exception("Construction type not understood");

            MC.MeasureMemory();
            PT.FinalizeConstruction();
            MC.MeasureMemory();


            return PT;

        }


        private static void ConstructionByCSC(PhraseTrieV1 PT, MemoryCounter MC)
        {
            DateTime t1 = DateTime.Now;

            var csc = new CSC_v3(PT.S, PT.SA, PT.invSA, PT.lcpDS);
            Phrase[] curSuffixPhrases = null;

            for (int curSuffixIndex = 0; curSuffixIndex < PT.S.Length; curSuffixIndex++)
            {
                Statics.Guard(curSuffixIndex, t1, MC);


                curSuffixPhrases = csc.CompressOneSuffix(curSuffixIndex, curSuffixPhrases);

                AddSuffixPhrase(curSuffixPhrases, (PTV1Node)PT.root, curSuffixIndex);

            }



        }


        private static void ConstructionByKKP3(PhraseTrieV1 PT, MemoryCounter MC)
        {
            DateTime t1 = DateTime.Now;
            for (int curSuffixIndex = 0; curSuffixIndex < PT.S.Length; curSuffixIndex++)
            {
                Statics.Guard(curSuffixIndex, t1, MC);



                string curSub = PT.S.Substring(curSuffixIndex);
                var SAsuffix = SAWrapper.GenerateSuffixArrayDLL(curSub, false);
                Phrase[] curSuffixPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curSub, false, SAsuffix, LZ77Wrapper.LZ77Algorithm.kkp3);

                AddSuffixPhrase(curSuffixPhrases, (PTV1Node)PT.root, curSuffixIndex);

            }



        }

    }
}
