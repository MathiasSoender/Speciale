using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixTree;
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

namespace Speciale.V1
{
    public class PhraseTrieV1 : Trie
    {
        public LCP lcpDS;


        public static List<int> CheckLeafNeighbors(int iMax, int[] SA, LCP lcpDS, int patternLength)
        {
            // Go left and right
            List<int> output = new List<int>();

            int[] indexToLexi = Statics.InverseArray(SA);
            HashSet<int> visited = new HashSet<int>();
            Stack<int> neighbors = new Stack<int>();
            neighbors.Push(iMax);

            while (neighbors.Count > 0)
            {
                int curI = neighbors.Pop();
                visited.Add(curI);

                int val = lcpDS.GetPrefixLength(curI, iMax);


                if (val >= patternLength)
                {
                    if (indexToLexi[curI] - 1 >= 0 && !visited.Contains(SA[indexToLexi[curI] - 1]))
                        neighbors.Push(SA[indexToLexi[curI] - 1]);
                    if (indexToLexi[curI] + 1 < SA.Length && !visited.Contains(SA[indexToLexi[curI] + 1]))
                        neighbors.Push(SA[indexToLexi[curI] + 1]);

                    output.Add(curI);
                }

            }
            return output;
        }


        public static List<int> BinarySearchFromNode(Node w, int[] SA, LCP lcpDS, int p_k, int r_k, Phrase[] patternPhrases, int kthphrase, int patternLength, string S)
        {
            int left = w.lexigraphicalI;
            int right = w.lexigraphicalJ;
            int maxVal = -1;
            int iMax = -1;
            bool roundUp = false;

            while (true)
            {
                int middel = roundUp ? (left + right + 1) / 2 : (left + right) / 2;
                int i = SA[middel];

                int val = lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k);


                if (val > maxVal)
                {
                    iMax = i;
                    maxVal = val;
                }

                if (Math.Abs(left - right) == 1)
                {
                    roundUp = true;
                }

                if (val > patternPhrases[kthphrase].len || left == right)
                {
                    break;
                }

                int t = p_k + lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k);

                // P[t] smaller than S_i[t]?, left half
                // Remember lexigraphically
                if (i + t >= S.Length)
                {
                    left = middel;
                    continue;
                }
                // Should NOT be allowed!
                if (S[i + t - r_k] == S[i + t])
                {
                    if (S[p_k + i + val] < S[p_k + i + val - r_k])
                    {
                        left = middel;
                    }
                    else
                    {
                        right = middel;
                    }

                }
                else if (S[i + t - r_k] < S[i + t])
                {
                    if (right == middel)
                        break;
                    right = middel;
                }
                else
                {
                    left = middel;
                }
            }

            if (p_k + Math.Min(lcpDS.GetPrefixLength(iMax + p_k - r_k, iMax + p_k), patternPhrases[kthphrase].len) >= patternLength)
            {
                return CheckLeafNeighbors(iMax, SA, lcpDS, patternLength);

            }
            return new List<int>();
        }

        public List<int> Search(Phrase[] patternPhrases)
        {

            PTV1Node curNode = (PTV1Node)root;
            PTV1Node child;
            int curPhrase = 0; // Index in pattern

            // Variables from paper
            PTV1Node w = null; 
            int kthphrase = -1;
            bool locusIsEdge = false;

            // Traverse
            while (curPhrase < patternPhrases.Length)
            {
                // Could not match in node
                if (!curNode.childrenMap.TryGetValue(patternPhrases[curPhrase], out child))
                {
                    w = curNode;
                    kthphrase = curPhrase;
                    locusIsEdge = false;
                    break;
                }
                else
                {
                    // Match
                    List<Phrase> traversalPhrases = child.phrases;
                    int matchedLength = 0;
                    while (traversalPhrases.Count() > matchedLength && patternPhrases.Length > (matchedLength + curPhrase))
                    {
                        // Correct match
                        if (traversalPhrases[matchedLength].Equals(patternPhrases[matchedLength + curPhrase]))
                        {
                            matchedLength++;
                        }
                        // Incorrect match
                        else
                        {
                            locusIsEdge = true;
                            w = child;
                            kthphrase = curPhrase + matchedLength;
                            curPhrase = patternPhrases.Length;
                            break;

                        }


                    }
                    curPhrase += matchedLength;
                    curNode = child;
                }
            }

            // Everything was matched, ie locus etc is not set
            if (kthphrase == -1 && w == null)
            {
                return CheckLeafNeighbors(SA[curNode.lexigraphicalI], SA, lcpDS, Phrase.FindDecompressedLength(patternPhrases));
            }


            // Difference from paper, here we only return complete matches, and not longest prefix of P  (that is a substring of S) matches.
            // If we fell off (and next phrase is a character, resulting in a char which has not yet been seen), simply return empty list
            if (patternPhrases[kthphrase].len == 0)
            {
                return new List<int>();
            }
            else
            {

                int p_k = Phrase.FindDecompressedLength(patternPhrases.Take(kthphrase).ToArray());

                int patternLength = Phrase.FindDecompressedLength(patternPhrases);

                int r_k = p_k - patternPhrases[kthphrase].pos;

                if (locusIsEdge)
                {

                    int i = SA[w.lexigraphicalI];
                    int totalmatchedlength = p_k + Math.Min(lcpDS.GetPrefixLength(i + p_k - r_k, i + p_k), patternPhrases[kthphrase].len);

                    return totalmatchedlength >= patternLength ? GenerateOutputOfSearch(curNode) : new List<int>();

                }
                else
                {
                    return BinarySearchFromNode(w, SA, lcpDS, p_k, r_k, patternPhrases, kthphrase, patternLength, S);

                }

            }
            
        }


    }




    public class PTV1Node : Node
    {
        public List<Phrase> phrases;

        public Dictionary<Phrase, PTV1Node> childrenMap;

        public override IEnumerable<Node> children
        {
            get
            {
                _children = childrenMap.Values;
                return _children;
            }
            set { _children = value; }
        }


        // Root constructor
        public PTV1Node()
        {
            phrases = new List<Phrase>();
            childrenMap = new Dictionary<Phrase, PTV1Node>();
        }

        // Internal node constructor
        public PTV1Node(List<Phrase> phrases) : this()
        {
            this.phrases = phrases;
        }

        // Leaf constructor
        public PTV1Node(List<Phrase> phrases, int suffixIndex) : this(phrases)
        {
            this.suffixIndex = suffixIndex;
        }






        public override bool IsLeaf()
        {
            return childrenMap.Count == 0;
        }
    }
}
