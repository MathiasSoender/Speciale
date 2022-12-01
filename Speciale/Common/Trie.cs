using Speciale.SuffixTree;
using Speciale.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Speciale.Common
{

    public abstract class Trie
    {
        public Node root;
        public int[] SA;
        public string S;
        public int[] invSA;
        public LCP lcpDS;



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

            SetPropertiesRecursive(root);
        }

        // Something like 3*n for binary trees
        // Sets lexigraphical range
        public Tuple<int, int> SetPropertiesRecursive(Node node)
        {
            if (node.IsLeaf())
            {
                node.lexigraphicalJ = invSA[node.suffixIndex];
                node.lexigraphicalI = invSA[node.suffixIndex];

                return new Tuple<int, int>(node.lexigraphicalI, node.lexigraphicalI);
            }

            List<Tuple<int, int>> lexigraphicalorders = node.children.Select(x => SetPropertiesRecursive(x)).ToList();

            var min = lexigraphicalorders.MinBy(x => x.Item1).Item1;
            var max = lexigraphicalorders.MaxBy(x => x.Item2).Item2;
            node.lexigraphicalI = min;
            node.lexigraphicalJ = max;

            return new Tuple<int, int>(min, max);

        }


        public void DFS(Action<Node> f)
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);


            while (queue.Count() > 0)
            {

                var node = queue.Dequeue();

                f(node);

                foreach (var c in node.children)
                {
                    queue.Enqueue(c);
                }

            }
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

        public List<Node> FindRootToLeafPath(Node leafNode)
        {
            var path = new List<Node>() { leafNode };
            Node node = leafNode;

            while (node.parent != null)
            {
                path.Add(node.parent);
                node = node.parent;
            }
            path.Reverse();
            return path;
        }




    }


}
