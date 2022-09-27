using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.Common
{
    public abstract class Node
    {
        protected IEnumerable<Node> _children;

        abstract public IEnumerable<Node> children { get; set; }


        // lexigraphical I, J corresponds to the range of nodes which a node covers (in lexigraphical order)
        public int lexigraphicalI = int.MaxValue;
        public int lexigraphicalJ = int.MinValue;
        public int suffixIndex;
        public int dfsI = int.MaxValue;
        public int dfsJ = int.MinValue;

        public abstract bool IsLeaf();

    }
}
