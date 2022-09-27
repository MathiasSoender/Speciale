using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.SuffixTree;
using Speciale.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.V2
{

    public static class PTV2Constructors
    {
        // suffixPhrases format 2 cases: UNICODE 0 or P_i L_i
        // Unicode is a single Char
        // P_i is position of where to copy
        // L_i is the length to copy
        // Example: 100 0,200 0,300 0,0 2
        // (Comma separated phrases)


        // O(n^2) preprocessing time (Need to look at each suffix phrases)
        // A simple transformation from O(n^2) space to O(n) space.

        public static PhaseTrieV2 NaiveConstruction(PhaseTrieV1 PTV1)
        {
            PhaseTrieV2 PTV2 = new PhaseTrieV2();
            PTV2.SA = PTV1.SA;
            PTV2.S = PTV1.S;
            PTV2.lcpDS = PTV1.lcpDS;
            PTV2.DFSIndexToSuffixIndex = PTV1.DFSIndexToSuffixIndex;

            PTNodeV2 leafref = null;
            PTNodeV2 root = new PTNodeV2((PTNodeV1)PTV1.root, out leafref);
            root.leafPointer = leafref;
            PTV2.root = root;

            return PTV2;

        }





    }
}
