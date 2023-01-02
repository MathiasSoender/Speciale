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
        ConstructPTV2Lexico,

        ConstructSTnaive,
        ConstructSTfast,

        SearchST,
        SearchPTV1,
        SearchPTV2,

        CompressionTest,

        None
    }
    public class TestingSuite
    {
        public static string timeOutputFile = "result_preprocess_time";
        public static string memoryOutputFile = "result_preprocess_memory";
        public static string searchTimeOutputFile = "result_search_time";


        private static void EnsureFolderStructure()
        {
            if (!Directory.Exists("results"))
            {
                Directory.CreateDirectory("results");
            }
            if (!Directory.Exists("results_lcp"))
            {
                Directory.CreateDirectory("results_lcp");
            }
            if (!Directory.Exists("results_search"))
            {
                Directory.CreateDirectory("results_search");
            }
        }

        public static string TestTypeToString(TestType type)
        {
            switch (type)
            {
                case TestType.SearchPTV1:
                    return "Search_PTV1";
                case TestType.SearchPTV2:
                    return "Search_PTV2";
                case TestType.SearchST:
                    return "SearchST";
                case TestType.ConstructPTV1CSC:
                    return "Construct_PTV1_CSC";
                case TestType.ConstructPTV1KKP3:
                    return "Construct_PTV1_KKP3";
                case TestType.ConstructPTV2KKP3:
                    return "Construct_PTV2_KKP3";
                case TestType.ConstructPTV2CSC:
                    return "Construct_PTV2_CSC";
                case TestType.ConstructSTnaive:
                    return "Construct_ST_naive";
                case TestType.ConstructSTfast:
                    return "Construct_ST_fast";
                case TestType.ConstructPTV2Lexico:
                    return "Construct_PTV2_Lexico";
                default:
                    throw new Exception("Type not understood");
            }
        }

        public static TestType StringToTestType(string testTypeAsString)
        {
            switch (testTypeAsString)
            {
                case "SearchST":
                    return TestType.SearchST;
                case "SearchPTV1":
                    return TestType.SearchPTV1;
                case "SearchPTV2":
                    return TestType.SearchPTV2;
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

        private static void WriteTempFile(string S_file, int partition)
        {
            FileStream filestream;
            StreamReader reader;
            string partitionS;
            char[] buffer;
            if (S_file == "repeat")
            {
                buffer = new char[partition / 2];
                filestream = File.OpenRead("proteins");
                reader = new StreamReader(filestream);
                reader.Read(buffer, 0, partition / 2);
                partitionS = new string(buffer) + new string(buffer);
            }
            else
            {
                buffer = new char[partition];
                filestream = File.OpenRead(S_file);
                reader = new StreamReader(filestream);
                reader.Read(buffer, 0, partition);
                partitionS = new string(buffer);
            }

            File.WriteAllText(S_file + ".tmp", partitionS);
            reader.Dispose();
            filestream.Dispose();

        }

        
        public static int SearchTest(string S_file, TestType type, int S_length)
        {
            var patternLengths = new List<int>() {5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000};

            int reps = 2500;
            EnsureFolderStructure();

            File.AppendAllText("results_search\\" + searchTimeOutputFile + "_" + TestTypeToString(type), "Filename: " + (S_file) + "\n");
            File.AppendAllText("results_search\\" + searchTimeOutputFile + "_" + TestTypeToString(type), "S length: " + (S_length) + "\n");


            WriteTempFile(S_file, S_length);
            var tester = type == TestType.SearchST ? new Tester(S_file + ".tmp", false) : new Tester(S_file + ".tmp", true);
            Trie trie;

            switch (type)
            {
                case TestType.SearchST:
                    trie = STConstructors.Construct(tester.S, tester.SA_S, tester.invSA, TestType.ConstructSTfast, null);
                    break;
                case TestType.SearchPTV1:
                    trie = PTV1Constructors.Construct(tester.S, tester.SA_S, tester.invSA, tester.lcpDS, TestType.ConstructPTV1CSC, null);
                    break;
                case TestType.SearchPTV2:
                    trie = PTV2Constructors.Construct(tester.S, tester.SA_S, tester.invSA, tester.lcpDS, TestType.ConstructPTV2CSC, null);
                    break;
                default:
                    throw new Exception("Bad type");
            }
            bool first = true;

            Console.Out.WriteLine("Doing: " + S_file + " S length: " + S_file + " search type: " + TestTypeToString(type));
            string S = File.ReadAllText(S_file);
            foreach (var p_length in patternLengths)
            {
                File.AppendAllText("results_search\\" + searchTimeOutputFile + "_" + TestTypeToString(type), "pattern length: " + (p_length) + "\n");

                double timeTakenTotalSeconds = 0;

                for (int i = 0; i < reps; i++)
                {
                    //if (i % 2 == 0)
                    string pattern = S.Substring(i, p_length);
                    //else
                    // string pattern = String.Join("", S.Substring(i, p_length), "X");

                    tester.UpdateP(pattern, false);


                    double curTime;
                    switch (type)
                    {
                        case TestType.SearchST:
                            tester.SearchST((SuffixTree.SuffixTree)trie, out curTime);
                            break;
                        case TestType.SearchPTV1:
                            tester.SearchPTV1((PhraseTrieV1)trie, out curTime);
                            break;
                        case TestType.SearchPTV2:
                            tester.SearchPTV2((PhraseTrieV2)trie, out curTime);
                            break;
                        default:
                            throw new Exception("Bad type");
                    }
                    if (first)
                    {
                        first = false;
                        i--;
                    }
                    else
                        timeTakenTotalSeconds += curTime;


                }
                Console.Out.WriteLine("Search: ST [" + TestTypeToString(type) + "]. Time taken: " + timeTakenTotalSeconds);

                File.AppendAllText("results_search\\" + searchTimeOutputFile + "_" + TestTypeToString(type), "time: " + (timeTakenTotalSeconds) + "\n");

            }



            return 0;

        }

        // Finds LCP sum
        public static int LCPTest(string S_file, int partition)
        {
            EnsureFolderStructure();
            File.AppendAllText("results_lcp\\LCP_sum", "Filename: " + (S_file) + "\n");
            File.AppendAllText("results_lcp\\LCP_sum", "partition: " + (partition) + "\n");


            WriteTempFile(S_file, partition);

            var tester = new Tester(S_file + ".tmp", false);
            DateTime t1 = DateTime.Now;
            var lcpArrSum = tester.LCPSum();
            Console.Out.WriteLine("Time for partition: " + partition + " time: " + (DateTime.Now - t1).TotalSeconds);
            File.AppendAllText("results_lcp\\LCP_sum", "sum: " + (lcpArrSum) + "\n");

            return 0;
        }




        public static int PreprocessingTest(string S_file, TestType type, int partition)
        {
            EnsureFolderStructure();

            File.AppendAllText("results_lcp\\LCP_time", "Filename: " + (S_file) + "\n");
            File.AppendAllText("results\\" + timeOutputFile + "_" + TestTypeToString(type), "Filename: " + (S_file) + "\n");
            File.AppendAllText("results\\" + memoryOutputFile + "_" + TestTypeToString(type), "Filename: " + (S_file) + "\n");

            Trie trie;


            Console.Out.WriteLine("Doing partition: " + partition);
            File.AppendAllText("results\\" + timeOutputFile + "_" + TestTypeToString(type), "parition: " + partition + "\n");
            File.AppendAllText("results\\" + memoryOutputFile + "_" + TestTypeToString(type), "parition: " + partition + "\n");

            WriteTempFile(S_file, partition);


            var tester = type == TestType.ConstructSTnaive || type == TestType.ConstructSTfast ? new Tester(S_file + ".tmp", false) : new Tester(S_file + ".tmp", true);
            trie = tester.BuildTrie(type);


            if (trie == null)
            {
                Console.Out.WriteLine("Cannot process anymore, due to constraints");
                return -1;
            }

            File.Delete(S_file + ".tmp");


            return 0;
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
        public static int TIME_OUT_SECONDS = 8 * 60;
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

        public void UpdateP(string P_file, bool isFile = true)
        {
            if (isFile) P = File.ReadAllText(P_file);
            else P = P_file;

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
                if (res != null)
                {
                    File.AppendAllText("results\\" + TestingSuite.timeOutputFile + "_" + typeAsString, (DateTime.Now - t1).TotalSeconds + "\n");
                    File.AppendAllText("results\\" + TestingSuite.memoryOutputFile + "_" + typeAsString, (MC.GetMaxMemory() / (1024.0 * 1024.0)) + "\n");
                    Console.Out.WriteLine("Preprocess:  [" + typeAsString + "]. Time taken: " + (DateTime.Now - t1).TotalSeconds);
                    Console.Out.WriteLine("Max memory used: " + MC.GetMaxMemory() / (1024 * 1024) + "MB.");
                }

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
            catch (Exception E)
            {
                return null;
            }
        }


        public List<int> SearchPTV1(PhraseTrieV1 PTV1naive, out double timetaken)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV1naive.Search(LZ_P);
            timetaken = (DateTime.Now - t1).TotalSeconds;

            // Console.Out.WriteLine("Search: PT_V1. Time taken: " + timetaken);

            return res;
        }

        #endregion


        #region PTV2

        public List<int> SearchPTV2(PhraseTrieV2 PTV2, out double timetaken)
        {
            DateTime t1 = DateTime.Now;
            var res = PTV2.Search(LZ_P);
            timetaken = (DateTime.Now - t1).TotalSeconds;

            //Console.Out.WriteLine("Search: PT_V2. Time taken: " + timetaken);

            return res;
        }

        #endregion



        #region ST
        
        public List<int> SearchST(SuffixTree.SuffixTree ST, out double timetaken)
        {
            DateTime t1 = DateTime.Now;
            var pattern = Phrase.DecompressLZ77Phrases(LZ_P);
            var res = ST.Search(pattern);
            timetaken = (DateTime.Now - t1).TotalSeconds;
            // Console.Out.WriteLine("Search: ST [Naive]. Time taken: " + timetaken);
            return res;
        }

        #endregion


        public long LCPSum()
        {
            try
            {
                LCPArray lcparr = new LCPArray(SA_S, S, invSA);
                long s = 0;
                foreach (var e in lcparr.lcpArr)
                    s += e;
                return s;

            }
            catch (Exception E)
            {
                return -1;
            }

        }

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
