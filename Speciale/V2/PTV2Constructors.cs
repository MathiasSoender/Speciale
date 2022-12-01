using Speciale.Common;
using Speciale.CSC;
using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.SuffixTree;
using Speciale.V1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

            PTV2Node leafref;
            PTV2Node root = new PTV2Node((PTV1Node)PTV1.root, out leafref);
            root.leafPointer = leafref;
            PTV2.root = root;

            return PTV2;

        }

        private static void AddSuffixPhrase(Phrase[] curSuffixPhrases, PTV2Node curNode, int curSuffixIndex, ref int UUID)
        {
            PTV2Node child;
            PTV2Node leafNode;
            int curPhraseIndex = 0;

            while (curPhraseIndex < curSuffixPhrases.Length)
            {
                if (!curNode.childrenMap.TryGetValue(curSuffixPhrases[curPhraseIndex], out child))
                {
                    leafNode = new PTV2Node(curSuffixPhrases.Skip(curPhraseIndex).ToList(), curSuffixIndex);
                    curNode.childrenMap.Add(leafNode.phrases[0], leafNode);
                    leafNode.parent = curNode;
                    leafNode.UUID = UUID++;
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
                        curNode.childrenMap.Remove(curSuffixPhrases[curPhraseIndex]);
                        PTV2Node splitNode = new PTV2Node(child.phrases.Take(matchedLength).ToList());
                        curNode.childrenMap.Add(splitNode.phrases[0], splitNode);
                        child.phrases = child.phrases.Skip(matchedLength).ToList();
                        splitNode.childrenMap.Add(child.phrases[0], child);

                        leafNode = new PTV2Node(curSuffixPhrases.Skip(matchedLength + curPhraseIndex).ToList(), curSuffixIndex);
                        splitNode.childrenMap.Add(leafNode.phrases[0], leafNode);


                        child.parent = splitNode;
                        leafNode.parent = splitNode;
                        splitNode.parent = curNode;

                        splitNode.UUID = UUID++;
                        leafNode.UUID = UUID++;

                        break;


                    }

                }
            }
        }


        


        public static PhraseTrieV2 Construct(string S, int[] SA, int[] invSA, LCP lcpDS, TestType constructionType, MemoryCounter MC)
        {
            PTV2Node root = new PTV2Node();
            PhraseTrieV2 PTV2 = new PhraseTrieV2() { lcpDS = lcpDS, S = S, SA = SA, root = root, invSA = invSA};

            if (constructionType == TestType.ConstructPTV2CSC)
                PTV2 = FastConstructionByCSC(PTV2, MC);
            else if (constructionType == TestType.ConstructPTV2KKP3)
                PTV2 = FastConstructionByKKP3(PTV2, MC);
            else
                throw new Exception("Construction type not allowed");

            MC.MeasureMemory();
            SetLeafPointers(PTV2);
            SetLengths(PTV2);
            PTV2.FinalizeConstruction();
            MC.MeasureMemory();
            return PTV2;
        }

        private static PhraseTrieV2 FastConstructionByCSC(PhraseTrieV2 PTV2, MemoryCounter MC)
        {
            int UUID = 1;

            var csc = new CSC_v3(PTV2.S, PTV2.SA, PTV2.invSA, PTV2.lcpDS);

            Phrase[] curSuffixPhrases = null;

            DateTime t1 = DateTime.Now;

            for (int curSuffixIndex = 0; curSuffixIndex < PTV2.S.Length; curSuffixIndex++)
            {
                Statics.Guard(curSuffixIndex, t1, MC);



                int longestMatch = FindLongestMatch(curSuffixIndex, PTV2);
                curSuffixPhrases = csc.CompressOneSuffix(curSuffixIndex, curSuffixPhrases, longestMatch);
                AddSuffixPhrase(curSuffixPhrases, (PTV2Node)PTV2.root, curSuffixIndex, ref UUID);


            }

            return PTV2;
        }

        private static PhraseTrieV2 FastConstructionByKKP3(PhraseTrieV2 PTV2, MemoryCounter MC)
        {
            int UUID = 1;
            DateTime t1 = DateTime.Now;

            for (int curSuffixIndex = 0; curSuffixIndex < PTV2.S.Length; curSuffixIndex++)
            {
                Statics.Guard(curSuffixIndex, t1, MC);

                int longestMatch = FindLongestMatch(curSuffixIndex, PTV2) + 1;
                string curS = PTV2.S.Substring(curSuffixIndex, longestMatch);
                int[] curSA = SAWrapper.GenerateSuffixArrayDLL(curS, false);

                var curSuffixPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curS, false, curSA, LZ77Wrapper.LZ77Algorithm.kkp3);
                AddSuffixPhrase(curSuffixPhrases, (PTV2Node)PTV2.root, curSuffixIndex, ref UUID);

            }
            return PTV2;
        }
       

        private static void PruneNode(PTV2Node node, string S)
        {
            if (!node.IsLeaf())
            {
                node.length = Phrase.FindDecompressedLength(node.phrases.ToArray());
                node.phrases = null;
                var parent = (PTV2Node)node.parent;
                int parentLength = parent == null ? 0 : parent.totalLength;

                node.totalLength = node.length + parentLength;
            }
            else
            {
                var parent = (PTV2Node)node.parent;
                node.length = (S.Length - node.suffixIndex) - parent.totalLength;
                node.phrases = null;
            }
        }

        public static int FindLongestMatch(int curSuffixIndex, PhraseTrieV2 PTV2)
        {
            int longestMatch;
            if (PTV2.invSA[curSuffixIndex] == 0)
                longestMatch = PTV2.lcpDS.GetPrefixLength(curSuffixIndex, PTV2.SA[PTV2.invSA[curSuffixIndex] + 1]);
            else if (PTV2.invSA[curSuffixIndex] == PTV2.S.Length - 1)
                longestMatch = PTV2.lcpDS.GetPrefixLength(PTV2.SA[PTV2.invSA[curSuffixIndex] - 1], curSuffixIndex);
            else
                longestMatch = Math.Max(PTV2.lcpDS.GetPrefixLength(curSuffixIndex, PTV2.SA[PTV2.invSA[curSuffixIndex] + 1]), PTV2.lcpDS.GetPrefixLength(PTV2.SA[PTV2.invSA[curSuffixIndex] - 1], curSuffixIndex));

            return longestMatch;
        }


        public static void SetLeafPointers(PhraseTrieV2 PTV2)
        {
            // Set leaf refs
            var queue = new Queue<PTV2Node>();
            var leaves = PTV2.FindLeaves(PTV2.root);
            foreach (var leaf in leaves) queue.Enqueue((PTV2Node)leaf);

            while (queue.Count() > 0)
            {
                var node = queue.Dequeue();

                if (node.IsLeaf())
                {
                    node.leafPointer = node;
                }
                else
                {
                    if (node.leafPointer == null)
                    {
                        node.leafPointer = node.childrenMap.Values.First().leafPointer;
                    }
                    // Leaf pointer for node has already been set, continue.
                    else
                    {
                        continue;
                    }
                }

                if (node.parent != null) queue.Enqueue((PTV2Node)node.parent);
            }
        }

        // updates length, and removes the remaining phrases
        private static void SetLengths(PhraseTrieV2 PTV2)
        {

            // Start by setting lengths and remove phrases

            Queue<PTV2Node> queue = new Queue<PTV2Node>();
            queue.Enqueue((PTV2Node)PTV2.root);

            while (queue.Count() > 0)
            {
                PTV2Node node = queue.Dequeue();

                PruneNode(node, PTV2.S);

                foreach (var c in node.childrenMap.Values)
                {
                    queue.Enqueue(c);
                }
                
            }




    }


    }
}
