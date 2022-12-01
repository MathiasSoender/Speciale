using Speciale.CSC;
using Speciale.LZ77;
using Speciale.SuffixArray;
using Speciale.SuffixTree;
using Speciale.V1;
using Speciale.V2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.Common
{
    public enum TestType
    {
        ConstructPTV1CSC,
        ConstructPTV1KKP3,
        ConstructPTV2KKP3,
        ConstructPTV2CSC,
        ConstructSTnaive,
        ConstructSTfast,
        SearchST,
        SearchPTV1,
        SearchPTV2,
        CompressionTest,
        ConstructPTV2Lexico

    }
    public class TestingSuite
    {
        public static string timeOutputFile = "result_time";
        public static string memoryOutputFile = "result_memory";



        public static string TestTypeToString(TestType type)
        {
            string output = "Construct_";
            switch (type)
            {
                case TestType.ConstructPTV1CSC:
                    output += "PTV1_CSC";
                    break;
                case TestType.ConstructPTV1KKP3:
                    output += "PTV1_Naive";
                    break;
                case TestType.ConstructPTV2KKP3:
                    output += "PTV2_KKP3";
                    break;
                case TestType.ConstructPTV2CSC:
                    output += "PTV2_CSC";
                    break;
                case TestType.ConstructSTnaive:
                    output += "ST_naive";
                    break;
                case TestType.ConstructSTfast:
                    output += "ST_fast";
                    break;
                case TestType.ConstructPTV2Lexico:
                    output += "PTV2_Lexico";
                    break;
                default:
                    throw new Exception("Type not understood");
            }
            return output;
        }

        public static TestType StringToTestType(string testTypeAsString)
        {
            switch (testTypeAsString)
            {
                case "ConstructPTV1CSC":
                    return TestType.ConstructPTV1CSC;
                case "ConstructPTV1KKP3":
                    return TestType.ConstructPTV1KKP3;
                case "ConstructPTV2KKP3":
                    return TestType.ConstructPTV2KKP3;
                case "ConstructPTV2CSC":
                    return TestType.ConstructPTV2CSC;
                case "ConstructSTnaive":
                    return TestType.ConstructSTnaive;
                case "ConstructSTfast":
                    return TestType.ConstructSTfast;
                case "ConstructPTV2Lexico":
                    return TestType.ConstructPTV2Lexico;
                default:
                    throw new Exception("Type not understood");
            }
        }


        



        public static void PreprocessingTest(string S_file, TestType type)
        {
            if (!Directory.Exists("results"))
            {
                Directory.CreateDirectory("results");
            }
            if (!Directory.Exists("results_lcp"))
            {
                Directory.CreateDirectory("results_lcp");
            }
            File.AppendAllText("results_lcp\\LCP_time", "Filename: " + (S_file) + "\n");
            File.AppendAllText("results\\" + timeOutputFile + "_" + TestTypeToString(type), "Filename: " + (S_file) + "\n");
            File.AppendAllText("results\\" + memoryOutputFile + "_" + TestTypeToString(type), "Filename: " + (S_file) + "\n");


            int partition = 10000;

            var filestream = File.OpenRead(S_file);
            long length = filestream.Length;
            filestream.Dispose();
            Trie trie;


            while (partition < length)
            {
                File.AppendAllText("results\\" + timeOutputFile + "_" + TestTypeToString(type), "parition: " + partition + "\n");
                File.AppendAllText("results\\" + memoryOutputFile + "_" + TestTypeToString(type), "parition: " + partition + "\n");

                char[] buffer = new char[partition];
                filestream = File.OpenRead(S_file);
                var reader = new StreamReader(filestream);
                reader.ReadBlock(buffer, 0, partition);
                reader.Dispose();
                string partitionS = new string(buffer);

                File.WriteAllText(S_file + ".tmp", partitionS);


                var tester = type == TestType.ConstructSTnaive || type == TestType.ConstructSTfast ? new Tester(S_file + ".tmp", false) : new Tester(S_file + ".tmp", true);
                trie = tester.BuildTrie(type);


                if (trie == null)
                {
                    Console.Out.WriteLine("Cannot process anymore, due to time constraints");
                    break;
                }

                partition *= 2;
                trie = null;
                GC.Collect();
            }
            File.Delete(S_file + ".tmp");
            filestream.Dispose();

            GC.Collect();
        }


        public static void SearchTest()
        {

        }


    }


    public class Tester
    {
        public int[] SA_S;
        public int[] SA_P;
        public int[] invSA;
        public string S;
        public string P;
        public LCP lcpDS;
        public Phrase[] LZ_P;
        public static int TIME_OUT_SECONDS = 5 * 60;
        public static int MAX_GB = 4;


        public Tester(string S_file, string P_file, bool computeLCP = true)
        {
            UpdateP(P_file);
            UpdateS(S_file);

            DateTime t1 = DateTime.Now;
            lcpDS = computeLCP ? new LCP(S, SA_S, invSA, LCPType.fast) : null;
            File.AppendAllText("results_lcp\\LCP_time", "parition: " + (S.Length - 1) + "\nFilename: " + S_file + "\n");
            File.AppendAllText("results_lcp\\LCP_time", (DateTime.Now - t1).TotalSeconds.ToString() + "\n");

        }

        public Tester(string S_file, bool computeLCP = true)
        {
            Statics.PruneTextFile(S_file);
            UpdateS(S_file);
            DateTime t1 = DateTime.Now;
            lcpDS = computeLCP ? new LCP(S, SA_S, invSA, LCPType.fast) : null;
            File.AppendAllText("results_lcp\\LCP_time", "parition: " + (S.Length - 1) + "\n");
            File.AppendAllText("results_lcp\\LCP_time", (DateTime.Now - t1).TotalSeconds.ToString() + "\n");
        }

        public void UpdateP(string P_file)
        {
            P = File.ReadAllText(P_file);
            SA_P = SAWrapper.GenerateSuffixArrayDLL(P, false);
            LZ_P = LZ77Wrapper.GenerateLZ77PhrasesDLL(P, false, SA_P, LZ77Wrapper.LZ77Algorithm.kkp3);
            
        }

        public void UpdateS(string S_file)
        {
            S = File.ReadAllText(S_file);
            SA_S = SAWrapper.GenerateSuffixArrayDLL(S, false);
            invSA = Statics.InverseArray(SA_S);

        }



        public static void TestResults(List<List<int>> resultLists, bool sort = true)
        {
            List<int> counts = new List<int>();

            foreach (var l in resultLists)
            {
                if (sort) l.Sort();
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


        #region PTV1
        public Trie BuildTrie(TestType CT)
        {
            string typeAsString = TestingSuite.TestTypeToString(CT);
            Trie res;
            var t1 = DateTime.Now;
            try
            {
                var MC = new MemoryCounter();

                switch (CT)
                {
                    case TestType.ConstructPTV1CSC:
                        res = PTV1Constructors.Construct(S, SA_S, invSA, lcpDS, TestType.ConstructPTV1CSC, MC);
                        break;
                    case TestType.ConstructPTV1KKP3:
                        res = PTV1Constructors.Construct(S, SA_S, invSA, lcpDS, TestType.ConstructPTV1KKP3, MC);
                        break;
                    case TestType.ConstructPTV2KKP3:
                        res = PTV2Constructors.Construct(S, SA_S, invSA, lcpDS, TestType.ConstructPTV2KKP3, MC);
                        break;
                    case TestType.ConstructPTV2CSC:
                        res = PTV2Constructors.Construct(S, SA_S, invSA, lcpDS, TestType.ConstructPTV2CSC, MC);
                        break;
                    case TestType.ConstructPTV2Lexico:
                        res = PTV2LexicoConstructor.Construct(S, SA_S, invSA, lcpDS, MC);
                        break;
                    case TestType.ConstructSTnaive:
                        res = STConstructors.Construct(S, SA_S, invSA, TestType.ConstructSTnaive, MC);
                        break;
                    case TestType.ConstructSTfast:
                        res = STConstructors.Construct(S, SA_S, invSA, TestType.ConstructSTfast, MC);
                        break;
                    default:
                        throw new Exception("Type not understood");
                }
                File.AppendAllText("results\\" + TestingSuite.timeOutputFile + "_" + typeAsString, (DateTime.Now - t1).TotalSeconds + "\n");
                File.AppendAllText("results\\" + TestingSuite.memoryOutputFile + "_" + typeAsString, (MC.GetMaxMemory() / (1024.0 * 1024.0)) + "\n");
                Console.Out.WriteLine("Preprocess:  [" + typeAsString + "]. Time taken: " + (DateTime.Now - t1).TotalSeconds);
                Console.Out.WriteLine("Max memory used: " + MC.GetMaxMemory() / (1024 * 1024) + "MB.");

                return res;
            }
            catch (TimeoutException E)
            {
                Console.Out.WriteLine("Preprocess:  [" + typeAsString + "] has timed out.");
                return null;
            }
            catch (OutOfMemoryException E)
            {
                Console.Out.WriteLine("Preprocess:  [" + typeAsString + "] has exceeded memory.");
                return null;
            }
        }


        public List<int> SearchPTV1(PhraseTrieV1 PTV1naive)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV1naive.Search(LZ_P);
            Console.Out.WriteLine("Search: PT_V1. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }

        #endregion


        #region PTV2

        public List<int> SearchPTV2(PhraseTrieV2 PTV2)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV2.Search(LZ_P);
            Console.Out.WriteLine("Search: PT_V2. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }

        #endregion



        #region ST
        
        public List<int> SearchST(SuffixTree.SuffixTree ST)
        {
            DateTime t1 = DateTime.Now;
            var pattern = Phrase.DecompressLZ77Phrases(LZ_P);
            var res = ST.Search(pattern);
            Console.Out.WriteLine("Search: ST [Naive]. Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }

        #endregion


        public void TestConsecutiveSuffixCompressors(bool safeResult = true)
        {


            var watch = System.Diagnostics.Stopwatch.StartNew();
            watch.Stop();


            watch = System.Diagnostics.Stopwatch.StartNew();
            var csc_v3 = new CSC_v3(S, SA_S, invSA, lcpDS);
            var res_v3 = csc_v3.CompressAllSuffixes(safeResult);
            watch.Stop();
            Console.Out.WriteLine("Time taken for previous phrase usage + Lazy SA: " + (double)watch.ElapsedMilliseconds / 1000);

            watch = System.Diagnostics.Stopwatch.StartNew();
            var csc_v1 = new CSC_v1(S, SA_S, invSA, lcpDS);
            var res_v1 = csc_v1.CompressAllSuffixes(safeResult);
            watch.Stop();
            Console.Out.WriteLine("Time taken for Lazy SA: " + (double)watch.ElapsedMilliseconds / 1000);

            watch = System.Diagnostics.Stopwatch.StartNew();
            var res_bad = new Phrase[S.Length][];
            for (int i = 0; i < S.Length; i++)
            {
                string curSub = S.Substring(i);
                var SAsuffix = SAWrapper.GenerateSuffixArrayDLL(curSub, false);
                Phrase[] curSuffixPhrases = LZ77Wrapper.GenerateLZ77PhrasesDLL(curSub, false, SAsuffix, LZ77Wrapper.LZ77Algorithm.kkp3);
                res_bad[i] = curSuffixPhrases;
            }
            watch.Stop();
            Console.Out.WriteLine("Time taken for KKP3: " + (double)watch.ElapsedMilliseconds / 1000);

        }
    }

}
