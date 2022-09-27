using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixArray;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.CSC
{
    public class ConsecutiveSuffixCompressorOld
    {

        LCP lcpDS;
        int[] SA; // Changed during execution
        int[] invSA;
        string data;
        Dictionary<int, int> predecessors; // Could just be arrays
        Dictionary<int, int> successors;
        int[] repeatLengths;

        // suffix index => pred SA index
        Dictionary<int, int> predecessorsLazy;
        Dictionary<int, int> successorsLazy;

        int[] SALazy;
        int[] SALazyInv;

        public ConsecutiveSuffixCompressorOld(string data)
        {
            this.data = data;
            SA = SAWrapper.GenerateSuffixArrayDLL(data, false);
            invSA = Statics.InverseArray(SA);

            SALazy = new int[SA.Length];
            Array.Copy(SA, SALazy, SA.Length);
            SALazyInv = Statics.InverseArray(SALazy);

            lcpDS = new LCP(data, SA, LCPType.fast);
            ComputePredecessorAndSucessor();
            ComputeRepeatLengths();
            CompressAllSuffixes();
        }

        public void Test(Phrase[][] phrases)
        {
            for (int i = 0; i < data.Length; i++)
            {
                string suffix = data.Substring(i);
                var curSA = SAWrapper.GenerateSuffixArrayDLL(suffix, false);
                var correctLZ = LZ77Wrapper.GenerateLZ77PhrasesDLL(suffix, false, curSA, LZ77Wrapper.LZ77Algorithm.kkp3);


                if (correctLZ.Length != phrases[i].Length)
                {
                    throw new Exception("Error");
                }

                for (int j = 0; j < correctLZ.Length; j++)
                {
                    if (!correctLZ[j].Equals(phrases[i][j]))
                    {
                        throw new Exception("Error");
                    }
                }

            }
        }

        public Phrase[][] CompressAllSuffixes()
        {
            DateTime t1 = DateTime.Now;
            var res = new Phrase[data.Length][];

            for (int i = 0; i < data.Length; i++)
            {
                var curRes = CompressOneSuffix(i);
                res[i] = curRes;

                if (i != data.Length - 1) // Dont update on last iteration (will always be single char)
                    UpdateDS(i);


            }
            Console.Out.WriteLine("Time taken: " + (DateTime.Now - t1).TotalSeconds);

            Test(res);
            return res;

        }

        private void UpdateDS(int curSuffix)
        {
            var SAlist = SA.ToList();
            int killIndex = 0;
            for (int i = 0; i < SAlist.Count(); i++)
            {
                if (SAlist[i] == 0)
                {
                    killIndex = i;
                }
                SAlist[i] -= 1;
            }
            SAlist.RemoveAt(killIndex);

            SA = SAlist.ToArray();

            ComputePredecessorAndSucessor();
            invSA = Statics.InverseArray(SA);

        }

        private Phrase[] CompressOneSuffix(int curSuffix)
        {
            HashSet<char> seenChars = new HashSet<char>();
            List<Phrase> res = new List<Phrase>();
            int matched = 0;
            int suffixLength = data.Length - curSuffix;

            while (matched < suffixLength)
            {
                int i = matched + curSuffix;
                if (!seenChars.Contains(data[i]))
                {
                    seenChars.Add(data[i]);
                    res.Add(new Phrase() { len = 0, pos = data[i] });
                    matched++;
                    continue;
                }
                int predLazySAindexed = FindLazyPredecessor(curSuffix, matched);
                int predLazySuffixIndexed = predLazySAindexed == -1 ? -1 : SALazy[predLazySAindexed] - curSuffix;

                int succLazySAindexed = FindLazySuccessor(curSuffix, matched);
                int succLazySuffixIndexed = succLazySAindexed == -1 ? -1 : SALazy[succLazySAindexed] - curSuffix;


                int pred = predecessors[invSA[matched]] == -1 ? -1 : SA[predecessors[invSA[matched]]];
                int succ = successors[invSA[matched]] == -1 ? -1 : SA[successors[invSA[matched]]];

                if (pred != predLazySuffixIndexed)
                {
                    int k = 10;
                }
                if (succ != succLazySuffixIndexed)
                {
                    int k = 10;
                }


                int len1 = pred == -1 ? 0 : lcpDS.GetPrefixLength(pred + curSuffix, i, true);
                int len2 = succ == -1 ? 0 : lcpDS.GetPrefixLength(i, succ + curSuffix, true);
                int len3 = repeatLengths[i];

                // Ensure that closest suffix is chosen
                if (len1 == len2 && len1 >= len3)
                {
                    if (data[pred + len1 + curSuffix] > data[succ + len1 + curSuffix])
                    {
                        res.Add(new Phrase() { len = len1, pos = pred });
                    }
                    else
                    {
                        res.Add(new Phrase() { len = len1, pos = succ });
                    }
                    matched += len1;
                    continue;
                }



                if (len1 >= len2 && len1 >= len3)
                {
                    res.Add(new Phrase() { len = len1, pos = pred });
                    matched += len1;
                }
                else if (len2 >= len1 && len2 >= len3)
                {
                    res.Add(new Phrase() { len = len2, pos = succ });
                    matched += len2;

                }
                else if (len3 > len1 && len3 > len2)
                {
                    res.Add(new Phrase() { len = len3, pos = (matched - 1) });
                    matched += len3;
                }
                else
                {
                    int k = 0; // ??
                }

            }


            return res.ToArray();
        }

        // Naive O(n^2) - can be improved to O(nlg(n)) or O(nlglg(n))
        private void ComputePredecessorAndSucessor()
        {
            predecessors = new Dictionary<int, int>();
            successors = new Dictionary<int, int>();

            for (int i = 0; i < SA.Length; i++)
            {
                int predIndex = -1;
                int sucIndex = -1;

                for (int j = i - 1; j >= 0; j--)
                {
                    if (SA[j] < SA[i])
                    {
                        predIndex = j;
                        break;
                    }
                }

                for (int j = i + 1; j < SA.Length; j++)
                {
                    if (SA[j] < SA[i])
                    {
                        sucIndex = j;
                        break;
                    }
                }
                predecessors.Add(i, predIndex);
                successors.Add(i, sucIndex);
            }
        }

        // O(n)
        private void ComputeRepeatLengths()
        {
            HashSet<char> seenChars = new HashSet<char>();

            repeatLengths = new int[data.Length];
            int totalMatched = 0;
            while (totalMatched < data.Length)
            {
                if (!seenChars.Contains(data[totalMatched]))
                {
                    seenChars.Add(data[totalMatched]);
                    totalMatched++;
                    continue;
                }

                if (data[totalMatched - 1] != data[totalMatched])
                {
                    totalMatched++;
                    continue;
                }

                int curMatched = 0;

                while (curMatched + totalMatched + 1 < data.Length
                    && data[curMatched + totalMatched] == data[curMatched + totalMatched + 1])
                {
                    curMatched++;
                }

                for (int i = totalMatched; i <= (totalMatched + curMatched); i++)
                {
                    repeatLengths[i] = (totalMatched + curMatched) - i + 1;
                }

                totalMatched += curMatched + 1;
            }

        }


        private int FindLazySuccessor(int curSuffix, int matched)
        {

            int FindNextSucc(int startPos = -1)
            {
                int succIndex = -1;
                int startIndex = startPos == -1 ? SALazyInv[matched + curSuffix] : startPos;

                for (int i = startIndex + 1; i < SALazy.Length; i++)
                {
                    if (SALazy[i] < matched + curSuffix && SALazy[i] - curSuffix >= 0)
                    {
                        succIndex = i;
                        break;
                    }
                }

                return succIndex;
            }


            if (successorsLazy == null)
                successorsLazy = new Dictionary<int, int>();


            int succIndex;
            if (!successorsLazy.TryGetValue(matched + curSuffix, out succIndex))
            {
                succIndex = FindNextSucc();
                successorsLazy[matched + curSuffix] = succIndex;
            }

            else
            {
                if (succIndex == -1)
                {
                    return -1;
                }
                // Compute new
                if (SALazy[succIndex] - curSuffix < 0)
                {
                    successorsLazy.Remove(matched + curSuffix);
                    succIndex = FindNextSucc(succIndex);
                    successorsLazy[matched + curSuffix] = succIndex;
                }


            }

            return succIndex;

        }

        private int FindLazyPredecessor(int curSuffix, int matched)
        {
            int FindNextPred(int startPos = -1)
            {
                int predIndex = -1;
                int startIndex = startPos == -1 ? SALazyInv[matched + curSuffix] : startPos;

                for (int i = startIndex - 1; i >= 0; i--)
                {
                    if (SALazy[i] < matched + curSuffix && SALazy[i] - curSuffix >= 0)
                    {
                        predIndex = i;
                        break;
                    }
                }

                return predIndex;
            }



            if (predecessorsLazy == null)
                predecessorsLazy = new Dictionary<int, int>();

            int predIndex;
            if (!predecessorsLazy.TryGetValue(matched + curSuffix, out predIndex))
            {
                predIndex = FindNextPred();
                predecessorsLazy[matched + curSuffix] = predIndex;
            }

            else
            {
                if (predIndex == -1)
                {
                    return -1;
                }
                // Compute new
                if (SALazy[predIndex] - curSuffix < 0)
                {
                    predecessorsLazy.Remove(matched + curSuffix);
                    predIndex = FindNextPred(predIndex);
                    predecessorsLazy[matched + curSuffix] = predIndex;
                }


            }

            return predIndex;



        }





    }
}
