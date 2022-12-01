using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Speciale.V2
{
    public static class PTV2LexicoConstructor
    {

        public static PhraseTrieV2 Construct(string S, int[] SA, int[] invSA, LCP lcpDS, MemoryCounter MC)
        {
            PTV2Node root = new PTV2Node();
            PhraseTrieV2 PTV2 = new PhraseTrieV2() { lcpDS = lcpDS, S = S, SA = SA, root = root, invSA = invSA };

            PTV2 = LexicoOrderedConstruction(PTV2, MC);

            MC.MeasureMemory();
            PTV2Constructors.SetLeafPointers(PTV2);

            PTV2.DFS((node) =>
            {
                var nodeCast = (PTV2Node)node;
                nodeCast.phrases = null;
            });
            PTV2.FinalizeConstruction();
            MC.MeasureMemory();
            return PTV2;
        }


        private static void PrunePathExceptFirstPhrases(IEnumerable<PTV2Node> path)
        {
            foreach (PTV2Node node in path)
            {
                PruneNodeExceptFirstPhrases(node);
            }

        }
        private static void PruneNodeExceptFirstPhrases(PTV2Node node)
        {
            node.phrases = node.phrases.Count() <= 1 ? node.phrases : new List<Phrase>() { node.phrases[0], node.phrases[1] };
        }



        private static void AddSuffixPhraseAndLengths(Phrase[] curSuffixPhrases, PTV2Node curNode, int curSuffixIndex, ref int UUID, out PTV2Node leafNode, string S)
        {
            PTV2Node child;
            leafNode = null;
            int curPhraseIndex = 0;

            while (curPhraseIndex < curSuffixPhrases.Length)
            {
                if (!curNode.childrenMap.TryGetValue(curSuffixPhrases[curPhraseIndex], out child))
                {
                    leafNode = new PTV2Node(curSuffixPhrases.Skip(curPhraseIndex).ToList(), curSuffixIndex);
                    curNode.childrenMap.Add(leafNode.phrases[0], leafNode);
                    leafNode.parent = curNode;
                    leafNode.UUID = UUID++;
                    leafNode.length = (S.Length - leafNode.suffixIndex) - curNode.totalLength;
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
                        int childLengthBefore = child.length;
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

                        splitNode.length = Phrase.FindDecompressedLength(splitNode.phrases.ToArray());
                        splitNode.totalLength = splitNode.length + curNode.totalLength;
                        child.length = childLengthBefore - splitNode.length;
                        leafNode.length = (S.Length - leafNode.suffixIndex) - splitNode.totalLength;


                        break;


                    }

                }
            }
        }



        public static PhraseTrieV2 LexicoOrderedConstruction(PhraseTrieV2 PTV2, MemoryCounter MC)
        {
            int UUID = 1;
            DateTime t1 = DateTime.Now;
            
            PTV2Node newLeaf = null;

            for (int curSuffixIndex = 0; curSuffixIndex < PTV2.S.Length; curSuffixIndex++)
            {
                Statics.Guard(curSuffixIndex, t1, MC);

                int curLexiIndex = PTV2.SA[curSuffixIndex];
                int phi = PTV2Constructors.FindLongestMatch(curLexiIndex, PTV2) + 1;
                string curS = PTV2.S.Substring(curLexiIndex, phi);
                int[] curSA = SAWrapper.GenerateSuffixArrayDLL(curS, false);
                var curSuffixPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curS, false, curSA, LZ77Wrapper.LZ77Algorithm.kkp3);


                AddSuffixPhraseAndLengths(curSuffixPhrases, (PTV2Node)PTV2.root, curLexiIndex, ref UUID, out PTV2Node leaf, PTV2.S);

                if (curSuffixIndex == 0)
                    newLeaf = leaf;
                else
                {
                    var oldLeaf = newLeaf;
                    newLeaf = leaf;

                    // Prune oldpath
                    var newPath = PTV2.FindRootToLeafPath(newLeaf).Cast<PTV2Node>().ToList();
                    List<PTV2Node> oldPath = PTV2.FindRootToLeafPath(oldLeaf).Cast<PTV2Node>().ToList();
                    int index = 0;

                    while (index < newPath.Count() && index < oldPath.Count())
                    {
                        if (oldPath[index].UUID != newPath[index].UUID)
                            break;
                        index++;
                    }
                    PrunePathExceptFirstPhrases(oldPath.Skip(index));
                }
            }

            return PTV2;
        }







    }
}
