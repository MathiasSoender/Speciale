using Speciale.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.SuffixTree
{
    public static class STConstructors
    {


        // |S| = n, O(n^2) preprocessing time
        public static SuffixTree NaiveConstruction(string S, int[] SA)
        {
            STNode root = new STNode();
            SuffixTree ST = new SuffixTree();
            ST.root = root;
            ST.S = S;
            ST.SA = SA;


            for (int i = 0; i < S.Length; i++)
            {
                // Variables
                STNode child;
                STNode curNode = root;
                int curIndex = i;
                string curSuffix = ST.IndexOfS(i, S.Length);

                // Match
                while (curIndex < S.Length)
                {
                    // No match!
                    if (!curNode.childrenMap.TryGetValue(ST.IndexOfS(curIndex), out child))
                    {
                        STNode leafNode = new STNode(curIndex, S.Length, i);
                        curNode.childrenMap.Add(ST.IndexOfS(curIndex), leafNode);
                        curIndex = S.Length;
                    }
                    else
                    {
                        // Could match first char, try to traverse remaining
                        string traversalString = ST.IndexOfS(child.indexI, child.indexJ);
                        int matchedLength = 0;
                        while (traversalString.Length > matchedLength && curSuffix.Length > (matchedLength + (curIndex - i)) &&
                               traversalString[matchedLength] == curSuffix[matchedLength + (curIndex - i)])
                        {
                            matchedLength++;
                        }

                        // we matched everything of edge to new internal node
                        if (traversalString.Length == matchedLength)
                        {
                            curIndex += matchedLength;
                            curNode = child;
                        }
                        // We fell off traversing edge
                        // Split edge
                        else
                        {
                            curNode.childrenMap.Remove(ST.IndexOfS(curIndex));
                            STNode splitNode = new STNode(child.indexI, child.indexI + matchedLength);
                            curNode.childrenMap.Add(ST.IndexOfS(splitNode.indexI), splitNode);
                            child.indexI = splitNode.indexJ;
                            splitNode.childrenMap.Add(ST.IndexOfS(child.indexI), child);

                            STNode leafNode = new STNode(matchedLength + curIndex, S.Length, i);
                            splitNode.childrenMap.Add(ST.IndexOfS(leafNode.indexI), leafNode);

                            curIndex = S.Length;


                        }



                    }



                }

                

            }
            ST.FinalizeConstruction();
            return ST;
        }



    }
}
