using System;
using Speciale.SuffixArray;
using Speciale.LZ77;
using Speciale.Common;
using Speciale.V1;
using Speciale.V2;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Speciale
{
    public class Program
    {
        static void Main(string[] args)
        {


            string S_file = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\t.txt";
            string P_file = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\tpattern.txt";
            Tests.GenerateData("abcdefg", 1000, S_file);
            //Tests.GenerateData("ab", 100, P_file);

            Statics.PruneTextFile(S_file, P_file);

            // Tests.TestAll(S_file, P_file);

            string S = File.ReadAllText(S_file);

            CSC.ConsecutiveSuffixCompressor csc = new CSC.ConsecutiveSuffixCompressor(S);


            var SA = SAWrapper.GenerateSuffixArrayDLL(S, false);
            var ph = LZ77Wrapper.GenerateLZ77PhrasesDLL(S, false, SA, LZ77Wrapper.LZ77Algorithm.kkp3);


            PTV1Constructors.FastConstruction(S, SA);

            return;

            Tests.TestAll(S_file, P_file);

            Tests.TestAllSubstringsOfPattern(S_file);

            

            




            //var SA = SAWrapper.GenerateSuffixArrayDLL(S_file);

            //Tests.TestLCP(File.ReadAllText(S_file), SA);


            // var lcparr = new LCPArray(SA, File.ReadAllText(S_file));





















        }
    }
}