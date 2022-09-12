using System;
using Speciale.SuffixArray;
using Speciale.LZ77;
using Speciale.Common;
using Speciale.V1;
using Speciale.V2;

namespace Speciale
{
    public class Program
    {
        static void Main(string[] args)
        {



            string TextFile = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\t.txt";
            string PatternFile = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\tpattern.txt";

            string phrasefile = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\tphrases.txt";




            Statics.PruneTextFile(TextFile, PatternFile);

            Tests.TestAll(TextFile, PatternFile, phrasefile);



            return;

















        }
    }
}