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
        public static int RunProgram(string[] args)
        {
            string inputfile = args[0];
            string enumtype = args[1];
            int partition = args[2] == "_" ? 0 : int.Parse(args[2]);
            string method = args[3];

            TestType type = enumtype == "_" ? TestType.None :  TestingSuite.StringToTestType(enumtype);

            switch (method)
            {
                case "preprocess":
                    return TestingSuite.PreprocessingTest(inputfile, type, partition);
                case "lcpsum":
                    return TestingSuite.LCPTest(inputfile, partition);
                case "search":
                    return TestingSuite.SearchTest(inputfile, type, partition);
                default:
                    return -1;
            }

        }

        static int Main(string[] args)
        {
            // Preprocess test:
            // file testype partition preprocess

            // LCP sum test:
            // file _ partition lcpsum
            
            // Search test:
            // file testype S_length search




            // Running from IDE
            return RunProgram(args);


            //List<string> testFiles = new List<string> { "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\t.txt" };
            // TestingSuite.PreprocessingTest("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt", TestType.ConstructSTnaive);




            /*
            Tester tFast = new Tester("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt");
            var suffixFast = tFast.BuildSTFast();

            Tester tNaive = new Tester("C:\\Users\\Mathi\\Desktop\\Speciale\\TestFiles\\temp.txt");
            var suffixNaive = tNaive.BuildSTNaive();

            suffixFast.Equals(suffixNaive);
            */

















        }
    }
}