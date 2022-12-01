using System;
using Speciale.SuffixArray;
using Speciale.LZ77;
using Speciale.Common;
using Speciale.V1;
using Speciale.V2;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Speciale.CSC;
using Speciale.SuffixTree;
using System.Diagnostics;

namespace Speciale
{
    public class Program
    {
        public static void RunProgram(string[] args)
        {
            string inputfile = args[0];
            string enumtype = args[1];
            TestType type = TestingSuite.StringToTestType(enumtype);

            TestingSuite.PreprocessingTest(inputfile, type);
        }

        static void Main(string[] args)
        {

            Tester T = new Tester("test.txt");

            var PTnew = (PhraseTrieV2)T.BuildTrie(TestType.ConstructPTV2Lexico);
            var PTold = (PhraseTrieV2)T.BuildTrie(TestType.ConstructPTV2CSC);

            PTnew.Equals(PTold);
            // TestingSuite.PreprocessingTest("a", TestType.ConstructPTV2CSC);


            // Running from IDE
            RunProgram(args);


            //List<string> testFiles = new List<string> { "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\t.txt" };
            // TestingSuite.PreprocessingTest("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt", TestType.ConstructSTnaive);




            /*
            Tester tFast = new Tester("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt");
            var suffixFast = tFast.BuildSTFast();

            Tester tNaive = new Tester("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt");
            var suffixNaive = tNaive.BuildSTNaive();

            suffixFast.Equals(suffixNaive);
            */
            return;

















        }
    }
}