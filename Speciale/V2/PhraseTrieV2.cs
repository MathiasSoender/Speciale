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

            while (curPhraseIndex < patternPhrases.Length)
            {
                // Step 1, section 4.2
                if (!curNode.childrenMap.TryGetValue(patternPhrases[curPhraseIndex], out child))
                {
                    if (patternPhrases[curPhraseIndex].len == 0)
                    {
                        return new List<int>();
                    }


                    p_k = phraseIndexToDecompLength[curPhraseIndex];
                    r_k = p_k - patternPhrases[curPhraseIndex].pos;

                    return PhraseTrieV1.BinarySearchFromNode(curNode, SA, lcpDS, p_k, r_k, patternPhrases, curPhraseIndex, patternLength, S);
                }

                // Step 2
                int matched = 0;
                int i = child.leafPointer.suffixIndex;
                int edgeLength = child.length;
                p_k = phraseIndexToDecompLength[curPhraseIndex];
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

        public void Equals(PhraseTrieV2 otherTrie)
        {
            Stack<PTV2Node> nodes = new Stack<PTV2Node>();
            Stack<PTV2Node> otherNodes = new Stack<PTV2Node>();

            nodes.Push((PTV2Node)root);
            otherNodes.Push((PTV2Node)otherTrie.root);

            while (nodes.Count > 0)
            {
                PTV2Node node = nodes.Pop();
                PTV2Node otherNode = otherNodes.Pop();

                if (node.lexigraphicalI == otherNode.lexigraphicalI && node.lexigraphicalJ == otherNode.lexigraphicalJ && node.length == otherNode.length)
                {
                    foreach(var kv in node.childrenMap)
                    {
                        if (otherNode.childrenMap.ContainsKey(kv.Key))
                        {
                            nodes.Push(kv.Value);
                            otherNodes.Push(otherNode.childrenMap[kv.Key]);
                        }
                        else
                        {
                            throw new Exception("PTV2 trees are not equal");
                        }
                    }
                }
                else
                {
                    throw new Exception("PTV2 trees are not equal");
                }
            }


        }



    }




    public class PTV2Node : PTV1Node
    {
        public int length;
        public PTV2Node leafPointer;
        public int UUID; // Unused
        public int totalLength; // Used under construction


        public new Dictionary<Phrase, PTV2Node> childrenMap;

        public void AddChild(Phrase key, PTV2Node child)
        {
            childrenMap.Add(key, child);
            child.parent = this;
        }
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

        // Internal node constructors

        public PTV2Node(int length) : this()
        {
            this.length = length;
        }

        public PTV2Node(List<Phrase> phrases) : base(phrases)
        {
            childrenMap = new Dictionary<Phrase, PTV2Node>();
        }


        // Leaf constructors
        public PTV2Node(int length, int suffixIndex) : this(length)
        {
            this.suffixIndex = suffixIndex;
            leafPointer = this;
        }

        public PTV2Node(List<Phrase> phrases, int suffixIndex) : base(phrases, suffixIndex)
        {
            leafPointer = this;
            childrenMap = new Dictionary<Phrase, PTV2Node>();
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


        
        public void SanitizeNode()
        {
            if (phrases == null)
                return;

            this.length = Phrase.FindDecompressedLength(phrases.ToArray());
            phrases.Clear();
            phrases = null;
        }


    }
}
