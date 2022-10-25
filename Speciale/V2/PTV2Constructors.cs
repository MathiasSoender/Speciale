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

        private static void AddSuffixPhrase(Phrase[] curSuffixPhrases, PTV2Node curNode, int curSuffixIndex, int longestMatch, out PTV2Node leafNode, ref int UUID)
        {
            PTV2Node child;
            int curPhraseIndex = 0;
            leafNode = null;

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




        public static PhraseTrieV2 FastConstruction2(string S, int[] SA, LCP lcpDS)
        {
            int UUID = 0;
            PTV2Node root = new PTV2Node();
            root.UUID = UUID++;

            PhraseTrieV2 PTV2 = new PhraseTrieV2() { lcpDS = lcpDS, S = S, SA = SA, root = root };

            var csc = new CSC_v3(PTV2.S, PTV2.SA, PTV2.lcpDS);
            int[] invSA = Statics.InverseArray(SA);

            PTV2Node newLeaf = null;
            Phrase[] curSuffixPhrases = null;


            for (int curSuffixIndex = 0; curSuffixIndex < PTV2.S.Length; curSuffixIndex++)
            {

                int longestMatch;
                if (invSA[curSuffixIndex] == 0)
                    longestMatch = PTV2.lcpDS.GetPrefixLength(curSuffixIndex, SA[invSA[curSuffixIndex] + 1]);
                else if (invSA[curSuffixIndex] == PTV2.S.Length - 1)
                    longestMatch = PTV2.lcpDS.GetPrefixLength(SA[invSA[curSuffixIndex] - 1], curSuffixIndex);
                else
                    longestMatch = Math.Max(PTV2.lcpDS.GetPrefixLength(curSuffixIndex, SA[invSA[curSuffixIndex] + 1]), PTV2.lcpDS.GetPrefixLength(SA[invSA[curSuffixIndex] - 1], curSuffixIndex));

                curSuffixPhrases = csc.CompressOneSuffix(curSuffixIndex, curSuffixPhrases, longestMatch);
                AddSuffixPhrase(curSuffixPhrases, root, curSuffixIndex, longestMatch, out newLeaf, ref UUID);

            }
            SetLeafPointers(PTV2);
            SetLengths(PTV2);
            PTV2.FinalizeConstruction();

            return PTV2;
        }


        private static void SetLeafPointers(PhraseTrieV2 PTV2)
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

                if (node.IsLeaf())
                {
                    var parent = (PTV2Node)node.parent;
                    node.length = (PTV2.S.Length - node.suffixIndex) - parent.totalLength;
                    node.phrases = null;
                }
                else
                {
                    node.length = Phrase.FindDecompressedLength(node.phrases.ToArray());
                    node.phrases = null;
                    var parent = (PTV2Node)node.parent;
                    int parentLength = parent == null ? 0 : parent.totalLength;

                    node.totalLength = node.length + parentLength;
                    foreach (var c in node.childrenMap.Values)
                    {
                        queue.Enqueue(c);
                    }
                }
            }

            


        }


        private static void UpdateLeafPointers(PTV2Node newLeaf)
        {
            PTV2Node parent = (PTV2Node)newLeaf.parent;
            parent.leafPointer = newLeaf;
            
            while (parent.parent != null)
            {
                parent = (PTV2Node)parent.parent;
                parent.leafPointer = newLeaf;
            }
        }



        private static void SanitizeFinal(PTV2Node startNode, bool skipDescendants = false)
        {
            Stack<PTV2Node> stack = new Stack<PTV2Node>();
            stack.Push(startNode);
            while(stack.Count() > 0)
            {
                var node = stack.Pop();

                if (node.phrases != null)
                {
                    node.SanitizeNode();
                }
                else if (skipDescendants)
                    continue;

                foreach (var child in node.childrenMap.Values)
                {
                    stack.Push(child);
                }
            }
           
        }

        private static void SanitizeNodes(PTV2Node newLeaf, PTV2Node prevLeaf, LCP lcpDS)
        {
            if (prevLeaf == null) return;

            int lcpVal = lcpDS.GetPrefixLength(prevLeaf.suffixIndex, newLeaf.suffixIndex);

            List<PTV2Node> prevVisitedNodes = new List<PTV2Node>() { prevLeaf};
            List<PTV2Node> curVisitedNodes = new List<PTV2Node>() { newLeaf };

            while (prevLeaf.parent != null)
            {
                prevVisitedNodes.Add((PTV2Node)prevLeaf.parent);
                prevLeaf = (PTV2Node)prevLeaf.parent;
            }
            while (newLeaf.parent != null)
            {
                curVisitedNodes.Add((PTV2Node)newLeaf.parent);
                newLeaf = (PTV2Node)newLeaf.parent;
            }

            prevVisitedNodes.Reverse();
            curVisitedNodes.Reverse();

            int i = 0;
            int matchLength = 0;
            while (i < prevVisitedNodes.Count())
            {
                if (prevVisitedNodes[i].UUID != curVisitedNodes[i].UUID)
                    break;

                matchLength += Phrase.FindDecompressedLength(prevVisitedNodes[i].phrases.ToArray()); // Can be optimized while building (save lengths)
                i++;
            }


            int l = Math.Max(prevVisitedNodes[i].phrases[0].len, 1);

            for (int j = i; j < prevVisitedNodes.Count(); j++)
            {

                if (lcpVal > matchLength && lcpVal >= (matchLength + l))
                {
                    matchLength += Phrase.FindDecompressedLength(prevVisitedNodes[j].phrases.ToArray());
                }
                else
                {
                    SanitizeFinal(prevVisitedNodes[j], true);
                }
            }


        }


    }
}
