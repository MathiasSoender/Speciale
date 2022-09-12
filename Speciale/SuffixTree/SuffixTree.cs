﻿using Speciale.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.SuffixTree
{
    // O(n) space, O(?) construction, depending on which constructor is used
    public class SuffixTree : Trie
    {

        public List<int> Search(string pattern)
        {

            STNode curNode = (STNode)root;
            STNode child;
            int curIndex = 0; // Index in pattern

            // Traverse
            while (curIndex < pattern.Length)
            {
                // Could not match in node
                if (!curNode.childrenMap.TryGetValue(pattern[curIndex], out child))
                {
                    return new List<int>();
                }
                else
                {
                    // Match
                    string traversalString = IndexOfS(child.indexI, child.indexJ);
                    int matchedLength = 0;
                    while (traversalString.Length > matchedLength && pattern.Length > (matchedLength + curIndex))
                    {
                        // Char has matched
                        if (traversalString[matchedLength] == pattern[matchedLength + curIndex])
                        {
                            matchedLength++;
                        }
                        // No match - falling off the edge
                        else
                        {
                            return new List<int>();
                        }
                    }
                    curIndex += matchedLength;
                    curNode = child;
                }
            }


            return GenerateOutputOfSearch(curNode);



        }

        public string IndexOfS(int i, int j)
        {
            if (S == null)
                throw new Exception("S not yet defined!");
            if (i > j)
                throw new Exception("cannot get substring when i is greater than j");
            if (i > S.Length || j > S.Length)
                throw new Exception("Indexes are out of range");

            return S.Substring(i, (j-i));
        }

        public char IndexOfS(int i)
        {
            if (S == null)
                throw new Exception("S not yet defined!");
            if (i > S.Length)
                throw new Exception("Indexes are out of range");

            return S[i];

        }



    }

    public class STNode : Node
    {
        public Dictionary<char, STNode> childrenMap;

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



        // i and j corresponds to the index of the substring, which the node covers in S.
        public int indexI;
        public int indexJ;

        // Constructor for root nodes
        public STNode()
        {
            childrenMap = new Dictionary<char, STNode>();
        }

        // Constructor for internal nodes
        public STNode(int i, int j)
        {
            childrenMap = new Dictionary<char, STNode>();
            indexI = i;
            indexJ = j;
        }

        // Constructor for leaves
        public STNode(int i, int j, int suffixIndex)
        {
            childrenMap = new Dictionary<char, STNode>();
            indexI = i;
            indexJ = j;
            this.suffixIndex = suffixIndex;
        }

        public override bool IsLeaf()
        {
            return childrenMap.Count == 0;
        }

    }
}
