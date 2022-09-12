using Speciale.SuffixTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.Common
{
    public abstract class Trie
    {
        public Node root;
        public int[] SA;
        public string S;

        public Dictionary<int, Node> suffixIndexToLeaf;


        public List<int> GenerateOutputOfSearch(Node node)
        {
            var output = new List<int>();
            for (int i = node.lexigraphicalI; i <= node.lexigraphicalJ; i++)
                output.Add(SA[i]);
            return output;
        }


        public void FinalizeConstruction()
        {
            if (SA == null)
                throw new Exception("Cannot finalize construction with null SA");

            suffixIndexToLeaf = new Dictionary<int, Node>();
            int[] indexToLexi = Statics.IndexToLexigraphical(SA);
            SetPropertiesRecursive(root, indexToLexi);
        }

        // Something like 3*n for binary trees
        // Sets lexigraphical range
        private Tuple<int, int> SetPropertiesRecursive(Node node, int[] indexToLexi)
        {
            if (node.IsLeaf())
            {
                node.lexigraphicalJ = indexToLexi[node.suffixIndex];
                node.lexigraphicalI = indexToLexi[node.suffixIndex];
                suffixIndexToLeaf.Add(node.suffixIndex, node);

                return new Tuple<int, int>(node.lexigraphicalI, node.lexigraphicalI);
            }

            List<Tuple<int, int>> lexigraphicalorders = node.children.Select(x => SetPropertiesRecursive(x, indexToLexi)).ToList();

            var min = lexigraphicalorders.MinBy(x => x.Item1).Item1;
            var max = lexigraphicalorders.MaxBy(x => x.Item2).Item2;
            node.lexigraphicalI = min;
            node.lexigraphicalJ = max;

            return new Tuple<int, int>(min, max);

        }

        public List<Node> FindLeaves(Node node)
        {
            List<Node> leaves = new List<Node>();
            Stack<Node> stack = new Stack<Node>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                Node child = stack.Pop();

                if (child.IsLeaf())
                {
                    leaves.Add(child);
                }
                else
                {
                    foreach (var c in child.children)
                        stack.Push(c);
                }
            }
            return leaves;
        }


    }


}
