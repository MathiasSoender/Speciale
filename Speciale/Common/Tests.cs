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

namespace Speciale.Common
{
    // Should probably use a stopwatch here..
    public static class Tests
    {
        public static void TestAll(string TextFile, string PatternFile, string phrasefile)
        {

            int[] SAText = TestSA(TextFile);

            TestLZ77(PatternFile, phrasefile);
            var STres = TestSTNaive(TextFile, phrasefile, SAText);

            PhaseTrieV1 PTV1;
            var PTV1res = TestPTV1Naive(TextFile, phrasefile, SAText, out PTV1);
            var PTV2res = TestPTV2Naive(PTV1, phrasefile);



            STres.Sort();
            PTV1res.Sort();
            PTV2res.Sort();

            bool countsGood = new List<int>() { STres.Count(), PTV1res.Count(), PTV2res.Count() }.Distinct().Count() == 1;

            if (!countsGood)
                throw new Exception("Non determinstic result");

            for (int i = 0; i < STres.Count(); i++)
            {
                if (!(STres[i] == PTV1res[i] && PTV1res[i] == PTV2res[i]))
                    throw new Exception("Non determinstic result");

            }

        }


        // Builds ST with naive algo + searches using naive algo
        public static List<int> TestSTNaive(string infile, string phrasefile, int[] SA)
        {

            Console.Out.WriteLine("Preprocess: ST [Naive]");
            DateTime t1 = DateTime.Now;
            var ST = STConstructors.NaiveConstruction(File.ReadAllText(infile), SA);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);


            string[] phrases = File.ReadAllLines(phrasefile);
            Console.Out.WriteLine("Search: ST [Naive]");
            t1 = DateTime.Now;
            var pattern = LZ77Wrapper.DecompressLZ77Phrases(phrases);
            var res = ST.Search(pattern);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;

        }

        public static List<int> TestPTV1Naive(string infile, string phrasefile, int[] SA, out PhaseTrieV1 PTV1)
        {
            Console.Out.WriteLine("Preprocess: PT_V1 [Naive]");
            DateTime t1 = DateTime.Now;
            PTV1 = PTV1Constructors.NaiveConstruction(infile, SA);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            var patternPhrases = Phrase.PhraseFileToObject(File.ReadAllLines(phrasefile));

            t1 = DateTime.Now;
            Console.Out.WriteLine("Search: PT_V1 [Naive]");
            var res = PTV1.Search(patternPhrases);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }

        public static List<int> TestPTV2Naive(PhaseTrieV1 PTV1, string phrasefile)
        {
            Console.Out.WriteLine("Preprocess: PT_V2 [Naive]");
            DateTime t1 = DateTime.Now;
            var PTV2 = PTV2Constructors.NaiveConstruction(PTV1);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            var patternPhrases = Phrase.PhraseFileToObject(File.ReadAllLines(phrasefile));

            t1 = DateTime.Now;
            Console.Out.WriteLine("Search: PT_V2 [Naive]");
            var res = PTV2.Search(patternPhrases);
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            return res;
        }




        public static int[] TestSA(string infile)
        {
            Console.Out.WriteLine("Generating suffix array for: " + infile);
            DateTime t1 = DateTime.Now;
            string outfile;
            SAWrapper.GenerateSuffixArray(infile, out outfile);
            Console.Out.WriteLine("Suffix array generated, time taken: " + (DateTime.Now - t1).TotalSeconds);

            return SAWrapper.SuffixArrayParser(outfile);

        }

        public static void TestLZ77(string infile, string phrasefile)
        {
            int[] SAPattern = TestSA(infile); // Must be called for compression (creates temp file)

            Console.Out.WriteLine("Generating LZ77 Phrases..");
            DateTime t1 = DateTime.Now;
            LZ77Wrapper.GenerateLZ77Phrases(infile, phrasefile, LZ77Wrapper.LZ77Algorithm.kkp3);
            Console.Out.WriteLine("LZ77 Phrases generated, time taken: " + (DateTime.Now - t1).TotalSeconds);

            //var decompressedString = LZ77Wrapper.DecompressLZ77Phrases(phrasefile);

        }




    }
}
