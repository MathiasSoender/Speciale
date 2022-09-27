using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Speciale.SuffixTree;
using Speciale.V2;
using System.IO;
using System.Runtime.InteropServices;

namespace Speciale.Common
{
    // Should probably use a stopwatch here..
    public static class Tests
    {
        public static void GenerateData(string alphabet, int n, string outfile)
        {
            Random random = new Random();

            var data = new string(Enumerable.Repeat(alphabet, n).Select(s => s[random.Next(s.Length)]).ToArray());

            File.WriteAllText(outfile, data);
        }


        private static void Wait()
        {
            return;
            GC.Collect();
            Thread.Sleep(500);
        }

        public static void TestAllSubstringsOfPattern(string S_file)
        {
            string S = File.ReadAllText(S_file);

            Console.Out.WriteLine("Generating SA for S");
            int[] SAText = TestSA(S);


            var ST = BuildSTNaive(S, SAText);
            var PTV1 = BuildPTV1Naive(S, SAText);
            var PTV2 = BuildPTV2Naive(PTV1);



            for (int i = 0; i < S.Length; i++)
            {
                for (int j = (i+1); j < S.Length; j++)
                {
                    try
                    {
                        string curPattern = S.Substring(i, j - i);

                        int[] SAPattern = SAWrapper.GenerateSuffixArrayDLL(curPattern, false);
                        Phrase[] patternPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curPattern, false, SAPattern, LZ77Wrapper.LZ77Algorithm.kkp3);


                        var STRes = SearchST(ST, patternPhrases);
                        var PTV1Res = SearchPTV1(PTV1, patternPhrases);
                        var PTV2Res = SearchPTV2(PTV2, patternPhrases);

                        TestResults(new List<List<int>>() { STRes, PTV1Res, PTV2Res });

                    }
                    catch (Exception E)
                    {
                        int k = 0;
                    }
                }
            }

        }


        public static void TestAll(string S_file, string P_file)
        {
            string S = File.ReadAllText(S_file);
            string P = File.ReadAllText(P_file);


            Console.Out.WriteLine("Generating SA for S");
            int[] SAText = TestSA(S);



            Console.Out.WriteLine("Generating SA for P");
            int[] SAPattern = TestSA(P);

            Console.Out.WriteLine("Generating LZ77-phrases for P");
            Phrase[] patternPhrases = TestLZ77(P, SAPattern);


            Wait();
            var ST = BuildSTNaive(S, SAText);
            // JIT
            var STres = SearchST(ST, patternPhrases);
            SearchST(ST, patternPhrases);
            Wait();

            var PTV1 = BuildPTV1Naive(S, SAText);
            // JIT
            var PTV1res = SearchPTV1(PTV1, patternPhrases);
            SearchPTV1(PTV1, patternPhrases);

            Wait();

            var PTV2 = BuildPTV2Naive(PTV1);
            // JIT
            var PTV2res = SearchPTV2(PTV2, patternPhrases);
            SearchPTV2(PTV2, patternPhrases);

            TestResults(new List<List<int>>() { STres, PTV1res, PTV2res });

        }


        public static void TestResults(List<List<int>> resultLists)
        {
            List<int> counts = new List<int>();

            foreach (var l in resultLists)
            {
                l.Sort();
                counts.Add(l.Count());
            }


            bool countsGood = counts.Distinct().Count() == 1;

            if (!countsGood)
                throw new Exception("Non determinstic result");



            for (int i = 1; i < resultLists.Count(); i++)
            {
                for (int j = 0; j < resultLists[0].Count(); j++)
                {
                    if (resultLists[0][j] != resultLists[i][j])
                    {
                        throw new Exception("Non determinstic result");
                    }
                }

            }
        }

        public static SuffixTree.SuffixTree BuildSTNaive(string S, int[] SA)
        {
            DateTime t1 = DateTime.Now;
            var ST = STConstructors.NaiveConstruction(S, SA);
            Console.Out.WriteLine("Preprocess: ST [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return ST;
        }

        public static List<int> SearchST(SuffixTree.SuffixTree ST, Phrase[] patternPhrases)
        {
            DateTime t1 = DateTime.Now;
            var pattern = Phrase.DecompressLZ77Phrases(patternPhrases);
            var res = ST.Search(pattern);
            Console.Out.WriteLine("Search: ST [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }


        public static PhaseTrieV1 BuildPTV1Naive(string S, int[] SA)
        {
            DateTime t1 = DateTime.Now;
            var PTV1 = PTV1Constructors.NaiveConstruction(S, SA);
            Console.Out.WriteLine("Preprocess: PT_V1 [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return PTV1;
        }

        public static List<int> SearchPTV1(PhaseTrieV1 PTV1, Phrase[] patternPhrases)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV1.Search(patternPhrases);
            Console.Out.WriteLine("Search: PT_V1 [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }

        public static PhaseTrieV2 BuildPTV2Naive(PhaseTrieV1 PTV1)
        {
            DateTime t1 = DateTime.Now;
            var PTV2 = PTV2Constructors.NaiveConstruction(PTV1);
            Console.Out.WriteLine("Preprocess: PT_V2 [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return PTV2;
        }
        public static List<int> SearchPTV2(PhaseTrieV2 PTV2, Phrase[] patternPhrases)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV2.Search(patternPhrases);
            Console.Out.WriteLine("Search: PT_V2. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }


        public static int[] TestSA(string data)
        {
            DateTime t1 = DateTime.Now;
            var res = SAWrapper.GenerateSuffixArrayDLL(data, false);
            Console.Out.WriteLine("Suffix array generated, time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;

        }

        public static Phrase[] TestLZ77(string data, int[] SA)
        {

            DateTime t1 = DateTime.Now;
            var res = LZ77Wrapper.GenerateLZ77PhrasesDLL(data, false, SA, LZ77Wrapper.LZ77Algorithm.kkp3);
            Console.Out.WriteLine("LZ77 Phrases generated, time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;

        }
        public static void TestRMQ()
        {
            int n = 1000;
            int Min = -200;
            int Max = 200;
            Random randNum = new Random();
            int[] test = Enumerable.Repeat(0, n).Select(i => randNum.Next(Min, Max)).ToArray();

            DateTime t1 = DateTime.Now;
            FastRMQ rmq = new FastRMQ(test);

            Console.Out.WriteLine("RMQ construction n=" + n + ", time taken: " + (DateTime.Now - t1).TotalSeconds);

            for (int i = 0; i < test.Length; i++)
            {
                for (int j = (i + 1); j < test.Length; j++)
                {
                    int testRes = rmq.GetRMQ(i, j - 1);
                    int realRes = test.Skip(i).Take(j - i).ToArray().Min();


                    if (testRes != realRes)
                    {
                        Console.Out.WriteLine("Error found in RMQ test");
                        throw new Exception("RMQ Test failed");
                    }


                }
            }


        }


        public static void TestLCP(string data, int[] SA)
        {
            LCP lcpFast = new LCP(data, SA, LCPType.fast);
            LCP lcpNaive = new LCP(data, SA, LCPType.naive);

            for (int i = 0; i < SA.Length; i++)
            {
                for (int j = i + 1; j < SA.Length; j++)
                {
                    var testVal = lcpFast.GetPrefixLength(i, j);
                    var realVal = lcpNaive.GetPrefixLength(i, j);

                    if (testVal != realVal)
                    {
                        Console.Out.WriteLine("Error found in LCP test");
                        throw new Exception("LCP Test failed");

                    }


                }
            }


        }



    }
}
