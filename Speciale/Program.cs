using System;
using Speciale.SuffixArray;
using Speciale.LZ77;
using Speciale.Common;
using Speciale.V1;
using Speciale.V2;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Speciale.CSC;

namespace Speciale
{
    public class Program
    {
        static void Main(string[] args)
        {


            string S_file = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\t.txt";
            string P_file = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\tpattern.txt";
            Tests.GenerateData("ab", 100, S_file);
            // Tests.GenerateData("ab", 10, P_file);

            Statics.PruneTextFile(S_file, P_file);
            Tests.TestAllSubstringsOfData(S_file);


            /*
            string S = File.ReadAllText(S_file);
            int[] SA = SAWrapper.GenerateSuffixArrayDLL(S, false);
            LCP lcpDS = new LCP(S, SA, LCPType.fast);
            Tests.BuildPTV2Fast(S, SA, lcpDS);
            */

            //Tests.TestAllSubstringsOfData(S_file);
            //Tests.TestAll(S_file, P_file);

            // Tests.TestConsecutiveSuffixCompressors(File.ReadAllText(S_file), false);
            return;

















        }
    }
}