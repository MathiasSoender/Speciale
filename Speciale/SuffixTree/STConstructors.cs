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

        public static SuffixTree Construct(string S, int[] SA, int[] invSA, TestType constructionType, MemoryCounter MC)
        {
            STNode root = new STNode();
            SuffixTree ST = new SuffixTree() { root = root, S = S, SA = SA, invSA = invSA };

            if (constructionType == TestType.ConstructSTnaive)
                NaiveConstruction(ST, MC);
            else if (constructionType == TestType.ConstructSTfast)
                FastConstruction(ST, MC);
            else
                throw new Exception("Construction type not understood");

            if (MC != null) MC.MeasureMemory();
            ST.FinalizeConstruction();
            if (MC != null) MC.MeasureMemory();
            return ST;

        }

        // O(n)
        private static void FastConstruction(SuffixTree ST, MemoryCounter MC)
        {
            DateTime t1 = DateTime.Now;
            LCPArray lcp = new LCPArray(ST.SA, ST.S, ST.invSA);
            var lcpArray = lcp.lcpArr;

            var root = (STNode)ST.root;
            root.labelDepth = 0;


            var p = new STNode(ST.SA[0], ST.S.Length, ST.SA[0]);
            p.labelDepth = p.indexJ - p.indexI;
            root.AddChild(ST.IndexOfS(ST.SA[0]), p);


            for (int i = 1; i < ST.S.Length; i++)
            {
                Statics.Guard(i, t1, MC);



                var l = lcpArray[i];
                STNode prevP = null;
                while(p.labelDepth > l)
                {
                    prevP = p;
                    p = (STNode)p.parent;
                }
                if (p.labelDepth == l)
                {
                    var q = new STNode(ST.SA[i] + l, ST.S.Length, ST.SA[i]);
                    q.labelDepth = ST.S.Length - ST.SA[i];
                    p.AddChild(ST.IndexOfS(ST.SA[i] + l), q);
                    p = q;
                }
                else
                {
                    int prevPdepth = prevP.labelDepth;
                    // Make Split node
                    var q = new STNode(ST.SA[i - 1] + p.labelDepth, ST.SA[i - 1] + l);
                    q.labelDepth = l;

                    // Unlink edge
                    prevP.parent = null;
                    p.childrenMap.Remove(ST.IndexOfS(ST.SA[i] + p.labelDepth));

                    // Update prevP
                    prevP.indexI = ST.SA[i - 1] + l;
                    prevP.indexJ = ST.SA[i - 1] + prevPdepth;

                    // Make new leaf
                    var r = new STNode(ST.SA[i] + l, ST.S.Length, ST.SA[i]);
                    r.labelDepth = ST.S.Length - ST.SA[i];

                    // Link split node
                    p.AddChild(ST.IndexOfS(q.indexI), q);
                    q.AddChild(ST.IndexOfS(r.indexI), r);
                    q.AddChild(ST.IndexOfS(prevP.indexI), prevP);

                    p = r;

                }


            }


        }


        // |S| = n, O(n^2) preprocessing time
        private static void NaiveConstruction(SuffixTree ST, MemoryCounter MC)
        {
            DateTime t1 = DateTime.Now;

            for (int i = 0; i < ST.S.Length; i++)
            {
                Statics.Guard(i, t1, MC);

                // Variables
                STNode child;
                STNode curNode = (STNode)ST.root;
                int curIndex = i;
                string curSuffix = ST.IndexOfS(i, ST.S.Length);

                // Match
                while (curIndex < ST.S.Length)
                {
                    // No match!
                    if (!curNode.childrenMap.TryGetValue(ST.IndexOfS(curIndex), out child))
                    {
                        STNode leafNode = new STNode(curIndex, ST.S.Length, i);
                        curNode.childrenMap.Add(ST.IndexOfS(curIndex), leafNode);
                        break;
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

                            STNode leafNode = new STNode(matchedLength + curIndex, ST.S.Length, i);
                            splitNode.childrenMap.Add(ST.IndexOfS(leafNode.indexI), leafNode);

                            break;


                        }



                    }



                }

                

            }
        }



    }
}
