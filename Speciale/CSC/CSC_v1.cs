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
    public class CSC_v1
    {

        LCP lcpDS;
        string data;
        int[] repeatLengths;

        // suffix index => pred SA index
        Dictionary<int, int> predecessorsLazy;
        Dictionary<int, int> successorsLazy;

        int[] SALazy;
        int[] SALazyInv;

        public CSC_v1(string data, int[] SA, int[] invSA, LCP lcpDS = null)
        {
            this.data = data;

            SALazy = SA;
            SALazyInv = invSA;
            this.lcpDS = lcpDS == null ? new LCP(data, SALazy, invSA, LCPType.fast) : lcpDS;
            ComputeRepeatLengths();
        }

        public Phrase[][] CompressAllSuffixes(bool safeResult = true)
        {
            var res = (safeResult) ? new Phrase[data.Length][] : new Phrase[1][];

            for(int i = 0; i < data.Length; i++)
            {
                var curRes = CompressOneSuffix(i);
                if (safeResult) res[i] = curRes;
            }
            return res;
        }

        // Should be used with care! Can only be used in suffix order; ie curSuffix = 0, curSuffix = 1, ..
        public Phrase[] CompressOneSuffix(int curSuffix)
        {
            HashSet<char> seenChars = new HashSet<char>();
            List<Phrase> res = new List<Phrase>();
            int matched = 0;
            int suffixLength = data.Length - curSuffix;

            while(matched <  suffixLength)
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
                int pred = predLazySAindexed == -1 ? -1 : SALazy[predLazySAindexed] - curSuffix;

                int succLazySAindexed = FindLazySuccessor(curSuffix, matched);
                int succ = succLazySAindexed == -1 ? -1 : SALazy[succLazySAindexed] - curSuffix;




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
