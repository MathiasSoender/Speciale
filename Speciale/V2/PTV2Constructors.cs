using Speciale.Common;
using Speciale.CSC;
using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.SuffixTree;
using Speciale.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Speciale.V2
{

    public static class PTV2Constructors
    {
        // suffixPhrases format 2 cases: UNICODE 0 or P_i L_i
        // Unicode is a single Char
        // P_i is position of where to copy
        // L_i is the length to copy
        // Example: 100 0,200 0,300 0,0 2
        // (Comma separated phrases)


        // O(LZ(S)^2) preprocessing time (Need to look at each suffix phrases)
        // A simple transformation from O(n^2) space to O(n) space.

        public static PhraseTrieV2 NaiveConstruction(PhraseTrieV1 PTV1)
        {
            PhraseTrieV2 PTV2 = new PhraseTrieV2() { lcpDS = PTV1.lcpDS, S = PTV1.S, SA = PTV1.SA};

            PTV2Node leafref = null;
            PTV2Node root = new PTV2Node((PTV1Node)PTV1.root, out leafref);
            root.leafPointer = leafref;
            PTV2.root = root;

            return PTV2;

        }


        public static PhraseTrieV2 FastConstruction(string S, int[] SA, LCP lcpDS)
        {
            PTV2Node root = new PTV2Node();
            PhraseTrieV2 PTV2 = new PhraseTrieV2() { lcpDS = lcpDS, S = S, SA = SA, root = root };

            var csc = new CSC_v3(PTV2.S, PTV2.SA, PTV2.lcpDS);
            Phrase[] curSuffixPhrases = null;


            for (int curSuffixIndex = 0; curSuffixIndex < PTV2.S.Length; curSuffixIndex++)
            {
                curSuffixPhrases = csc.CompressOneSuffix(curSuffixIndex, curSuffixPhrases);
                var phraseIndexToDecompLength = Phrase.FindPhraseIndexToDecompressedLength(curSuffixPhrases);


                List<PTV2Node> visitedNodes = new List<PTV2Node>() { root };
                int curPhraseIndex = 0;
                PTV2Node curNode = (PTV2Node)root;
                PTV2Node child;
                while (curPhraseIndex < curSuffixPhrases.Length)
                {
                    // Mistmatch trying to find new edge -> add leaf
                    if (!curNode.childrenMap.TryGetValue(curSuffixPhrases[curPhraseIndex], out child))
                    {
                        int remainLength = Phrase.FindDecompressedLength(curSuffixPhrases.Skip(curPhraseIndex).ToArray());
                        int remainLength2 = phraseIndexToDecompLength[curSuffixPhrases.Length - 1] - phraseIndexToDecompLength[curPhraseIndex];

                        if (remainLength != remainLength2)
                            throw new Exception("DAMN");

                        PTV2Node leafNode = new PTV2Node(remainLength, curSuffixIndex);
                        curNode.childrenMap.Add(curSuffixPhrases[curPhraseIndex], leafNode);
                        UpdateLeafPointers(visitedNodes, leafNode);
                        break;
                    }
                    else
                    {
                        int matchedPhrases = 0;
                        int i = child.leafPointer.suffixIndex;
                        int edgeLength = child.length;
                        int p_k = Phrase.FindDecompressedLength(curSuffixPhrases.Take(curPhraseIndex).ToArray());
                        int p_k2 = phraseIndexToDecompLength[curPhraseIndex];
                        if (p_k != p_k2)
                            throw new Exception("DAMN");

                        int p_k_start = p_k;
                        bool matchFailed = false;

                        // Match as far as possible
                        while ((curPhraseIndex + matchedPhrases) < curSuffixPhrases.Length && (p_k - p_k_start) < edgeLength)
                        {
                            if (curSuffixPhrases[curPhraseIndex + matchedPhrases].len == 0)
                            {
                                if (curSuffixPhrases[curPhraseIndex + matchedPhrases].pos == S[i + p_k])
                                {
                                    matchedPhrases++;
                                    p_k++;
                                }
                                else
                                {
                                    matchFailed = true;
                                    break;
                                }
                            }
                            else
                            {
                                int r_k = p_k - curSuffixPhrases[curPhraseIndex + matchedPhrases].pos;
                                int l_k = curSuffixPhrases[curPhraseIndex + matchedPhrases].len;
                                int lcpVal = lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k);

                                if (lcpVal >= l_k)
                                {
                                    matchedPhrases++;
                                    p_k += l_k;
                                }
                                else
                                {
                                    matchFailed = true;
                                    break;
                                }
                            }

                        }

                        // Split edge
                        if (matchFailed)
                        {
                            int matchedChars = p_k - p_k_start;
                            int remainLength = Phrase.FindDecompressedLength(curSuffixPhrases.Skip(curPhraseIndex + matchedPhrases).ToArray());

                            PTV2Node splitNode = new PTV2Node(matchedChars);
                            PTV2Node leafNode = new PTV2Node(remainLength, curSuffixIndex);

                            curNode.childrenMap.Remove(curSuffixPhrases[curPhraseIndex]);
                            curNode.childrenMap.Add(curSuffixPhrases[curPhraseIndex], splitNode);

                            // HOW to add child??
                            Phrase splitPhrase = csc.CompressSpecificPhrase(p_k + matchedChars, curSuffixIndex);
                            splitNode.childrenMap.Add(splitPhrase, child);
                            splitNode.childrenMap.Add(curSuffixPhrases[curPhraseIndex + matchedPhrases], leafNode);

                            child.length = edgeLength - matchedChars;


                            visitedNodes.Add(splitNode);
                            UpdateLeafPointers(visitedNodes, leafNode);

                            break;
                        }


                        curPhraseIndex += matchedPhrases;
                        curNode = child;
                        visitedNodes.Add(curNode);

                    }

                }



            }

            PTV2.FinalizeConstruction();
            return PTV2;

        }

        private static void UpdateLeafPointers(List<PTV2Node> visitedNodes, PTV2Node newLeaf)
        {
            foreach (var node in visitedNodes)
                node.leafPointer = newLeaf;
        }       



    }
}
