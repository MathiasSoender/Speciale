using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixTree;
using Speciale.V1;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.V2
{
    // Section 4, Linear space phrase trie
    public class PhraseTrieV2 : Trie
    {
        public LCP lcpDS;



        public List<int> Search(Phrase[] patternPhrases)
        {

            var phraseIndexToDecompLength = Phrase.FindPhraseIndexToDecompressedLength(patternPhrases);
            PTV2Node curNode = (PTV2Node)root;
            PTV2Node child;
            int curPhraseIndex = 0; // Index in pattern
            int p_k;
            int r_k;
            int patternLength = phraseIndexToDecompLength[patternPhrases.Length];
            int patternLength2 = phraseIndexToDecompLength[patternPhrases.Length];

            if (patternLength != patternLength2)
                throw new Exception("DAMN");

            while (curPhraseIndex < patternPhrases.Length)
            {
                // Step 1, section 4.2
                if (!curNode.childrenMap.TryGetValue(patternPhrases[curPhraseIndex], out child))
                {
                    if (patternPhrases[curPhraseIndex].len == 0)
                    {
                        return new List<int>();
                    }


                    p_k = Phrase.FindDecompressedLength(patternPhrases.Take(curPhraseIndex).ToArray());
                    int pk3 = phraseIndexToDecompLength[curPhraseIndex];
                    if (p_k != pk3)
                        throw new Exception("DAMN");
                    r_k = p_k - patternPhrases[curPhraseIndex].pos;

                    return PhraseTrieV1.BinarySearchFromNode(curNode, SA, lcpDS, p_k, r_k, patternPhrases, curPhraseIndex, patternLength, S);
                }

                // Step 2
                int matched = 0;
                int i = child.leafPointer.suffixIndex;
                int edgeLength = child.length;
                p_k = Phrase.FindDecompressedLength(patternPhrases.Take(curPhraseIndex).ToArray());
                int pk2 = phraseIndexToDecompLength[curPhraseIndex];
                if (p_k != pk2)
                    throw new Exception("DAMN");

                int p_k_start = p_k;

                while ((curPhraseIndex+matched) < patternPhrases.Length && (p_k - p_k_start) < edgeLength) // && matched < edgeLength)
                {

                    // Single letter
                    if (patternPhrases[curPhraseIndex + matched].len == 0)
                    {
                        if (patternPhrases[curPhraseIndex + matched].pos == S[i + p_k])
                        {
                            matched++;
                            p_k++;
                        }
                        else
                        {
                            return new List<int>();
                        }
                    }
                    else
                    {
                        r_k = p_k - patternPhrases[curPhraseIndex + matched].pos;
                        int l_k = patternPhrases[curPhraseIndex + matched].len;

                        if (Math.Min(lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k), l_k) == l_k)
                        {
                            matched++;
                            p_k += l_k;
                        }
                        else
                        {
                            if (p_k + lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k) >= patternLength)
                            {
                                return GenerateOutputOfSearch(child);
                            }
                            else
                            {
                                return new List<int>();
                            }
                        }
                    }


                }

                // Completely matched the edge
                curNode = child;
                curPhraseIndex += matched;

            }

            // Matched everything
            return PhraseTrieV1.CheckLeafNeighbors(SA[curNode.lexigraphicalI], SA, lcpDS, patternLength);

        }


    }




    public class PTV2Node : Node
    {
        public int length;

        public PTV2Node leafPointer;


        public Dictionary<Phrase, PTV2Node> childrenMap;

        public override IEnumerable<Node> children
        {
            get
            {
                if (_children == null)
                    _children = childrenMap.Values;
                return _children;
            }
            set { _children = value; }
        }

        // Root constructor
        public PTV2Node()
        {
            childrenMap = new Dictionary<Phrase, PTV2Node>();

        }

        // Internal node constructor

        public PTV2Node(int length) : this()
        {
            this.length = length;
        }

        // Leaf constructor
        public PTV2Node(int length, int suffixIndex) : this(length)
        {
            this.suffixIndex = suffixIndex;
            leafPointer = this;
        }


        // Recursively changes type of all V1 nodes below argument node (only used for constructing PTV2 from PTV1)
        public PTV2Node(PTV1Node v1Node, out PTV2Node leafRef)
        {
            childrenMap = new Dictionary<Phrase, PTV2Node>();

            length = Phrase.FindDecompressedLength(v1Node.phrases.ToArray());
            suffixIndex = v1Node.suffixIndex;
            lexigraphicalI = v1Node.lexigraphicalI;
            lexigraphicalJ = v1Node.lexigraphicalJ;

            leafRef = null;

            foreach (var kv in v1Node.childrenMap)
            {
                childrenMap.Add(kv.Key, new PTV2Node(kv.Value, out leafRef));

            }

            if (IsLeaf())
            {
                leafPointer = this;
                leafRef = this;
            }
            else
            {
                leafPointer = leafRef;
            }



        }


        public override bool IsLeaf()
        {
            return childrenMap.Count == 0;
        }
    }
}
