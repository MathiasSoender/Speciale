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
    public class PhaseTrieV2 : Trie
    {
        public LCP lcpDS;



        public List<int> Search(Phrase[] patternPhrases)
        {


            PTNodeV2 curNode = (PTNodeV2)root;
            PTNodeV2 child;
            int curPhrase = 0; // Index in pattern
            int p_k;
            int r_k;
            int patternLength = Phrase.FindDecompressedLength(patternPhrases);

            while (curPhrase < patternPhrases.Length)
            {
                // Step 1, section 4.2
                if (!curNode.childrenMap.TryGetValue(patternPhrases[curPhrase], out child))
                {
                    if (patternPhrases[curPhrase].len == 0)
                    {
                        return new List<int>();
                    }


                    p_k = Phrase.FindDecompressedLength(patternPhrases.Take(curPhrase).ToArray());
                    r_k = p_k - patternPhrases[curPhrase].pos;

                    return PhaseTrieV1.BinarySearchFromNode(curNode, SA, lcpDS, p_k, r_k, patternPhrases, curPhrase, patternLength, S);
                }

                // Step 2
                int matched = 0;
                int i = child.leafPointer.suffixIndex;
                int edgeLength = child.length;
                p_k = Phrase.FindDecompressedLength(patternPhrases.Take(curPhrase + matched).ToArray());


                while (matched < edgeLength && (curPhrase+matched) < patternPhrases.Length)
                {

                    // Single letter
                    if (patternPhrases[curPhrase + matched].len == 0)
                    {
                        if (patternPhrases[curPhrase + matched].pos == S[i + p_k])
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
                        r_k = p_k - patternPhrases[curPhrase + matched].pos;
                        int l_k = patternPhrases[curPhrase + matched].len;

                        if (Math.Min(lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k), l_k) == l_k)
                        {
                            matched++;
                            p_k += l_k;
                        }
                        else
                        {
                            if (p_k + lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k) >= patternLength)
                            {
                                return FindLeaves(child).Select(x => x.suffixIndex).ToList();
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
                curPhrase += matched;

            }
            return FindLeaves(curNode).Select(x => x.suffixIndex).ToList();

        }


    }




    public class PTNodeV2 : Node
    {
        public int length;

        public PTNodeV2 leafPointer;


        public Dictionary<Phrase, PTNodeV2> childrenMap;

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

        // Recursively changes type of all V1 nodes below argument node
        public PTNodeV2(PTNodeV1 v1Node, out PTNodeV2 leafRef)
        {
            childrenMap = new Dictionary<Phrase, PTNodeV2>();

            length = Phrase.FindDecompressedLength(v1Node.phrases.ToArray());
            suffixIndex = v1Node.suffixIndex;
            lexigraphicalI = v1Node.lexigraphicalI;
            lexigraphicalJ = v1Node.lexigraphicalJ;

            leafRef = null;

            foreach (var kv in v1Node.childrenMap)
            {
                childrenMap.Add(kv.Key, new PTNodeV2(kv.Value, out leafRef));
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
