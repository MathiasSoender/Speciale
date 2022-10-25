using Speciale.Common;
using Speciale.LZ77;
using Speciale.SuffixArray;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Speciale.CSC
{
    public class CSC_SAOrder
    {

        LCP lcpDS;
        string data;
        int[] repeatLengths;


        // predecessorsLazy[i] = -2 -> represents a value not being set yet
        // predecessorsLazy[i] = -1 -> represents that no pred can be found
        int[] predecessorsLazy;
        int[] successorsLazy;


        int[] SALazy;
        public int[] SALazyInv;

        public CSC_SAOrder(string data, int[] SA, LCP lcpDS = null)
        {
            this.data = data;

            SALazy = SA;
            SALazyInv = Statics.InverseArray(SALazy);
            this.lcpDS = lcpDS == null ? new LCP(data, SALazy, LCPType.fast) : lcpDS;
            ComputeRepeatLengths();

            predecessorsLazy = new int[SALazy.Length];
            successorsLazy = new int[SALazy.Length];

            for (int i = 0; i < SALazy.Length; i++)
            {
                predecessorsLazy[i] = -2; // Placeholder representing not being set yet
                successorsLazy[i] = -2;
            }
        }


        private Phrase FindBestMatch(int predSAIndexed, int succSAIndexed, int curSuffix, int matched)
        {
            int i = curSuffix + matched;
            int pred = predSAIndexed == -1 ? -1 : SALazy[predSAIndexed] - curSuffix;
            int succ = succSAIndexed == -1 ? -1 : SALazy[succSAIndexed] - curSuffix;


            int len1 = pred == -1 ? 0 : lcpDS.GetPrefixLength(pred + curSuffix, i, true);
            int len2 = succ == -1 ? 0 : lcpDS.GetPrefixLength(i, succ + curSuffix, true);
            int len3 = repeatLengths[i];


            // Ensure that closest suffix is chosen
            if (len1 == len2 && len1 >= len3)
            {
                if (data[pred + len1 + curSuffix] > data[succ + len1 + curSuffix])
                {
                    return new Phrase() { len = len1, pos = pred };
                }
                else
                {
                    return new Phrase() { len = len1, pos = succ };
                }
            }
            else if (len1 >= len2 && len1 >= len3)
            {
                return new Phrase() { len = len1, pos = pred };
            }
            else if (len2 >= len1 && len2 >= len3)
            {
                return new Phrase() { len = len2, pos = succ };

            }
            else
            {
                return new Phrase() { len = len3, pos = (matched - 1) };
            }

        }


        public Phrase[][] CompressAllSuffixes(bool safeResult = true)
        {
            var res = safeResult ? new Phrase[data.Length][] : new Phrase[1][];
            Phrase[] curRes = null;
            int prevSuffixIndex = -1;

            for (int i = 0; i < data.Length; i++)
            {
                var suffixIndex = SALazy[i];
                curRes = CompressOneSuffix(suffixIndex, prevSuffixIndex, curRes);
                if (safeResult) res[suffixIndex] = curRes;
                prevSuffixIndex = suffixIndex;

                // Can delete by knowing which SA indexes were used.
                for (int j = 0; j < SALazy.Length; j++)
                {
                    predecessorsLazy[j] = -2; // Placeholder representing not being set yet
                    successorsLazy[j] = -2;
                }


            }
            return res;

        }

        // Should be used with care! Can only be used in suffix order; ie curSuffix = 0, curSuffix = 1, ..
        // Add LCP from previous phrase (previous SA index)
        public Phrase[] CompressOneSuffix(int curSuffix, int prevSuffixIndex, Phrase[] prevPhrases)
        {
            HashSet<char> seenChars = new HashSet<char>();
            List<Phrase> res = new List<Phrase>();
            int matched = 0;
            int suffixLength = data.Length - curSuffix;

            // ReuseByLCP(curSuffix, prevSuffixIndex, prevPhrases, ref res, ref seenChars, ref matched);

            while (matched < suffixLength)
            {
                int i = matched + curSuffix;
                if (!seenChars.Contains(data[i]))
                {
                    seenChars.Add(data[i]);
                    res.Add(new Phrase() { len = 0, pos = data[i] });
                }
                else
                {
                    int predLazySAindexed = FindLazyPredecessor(curSuffix, matched);
                    int succLazySAindexed = FindLazySuccessor(curSuffix, matched);

                    var bestPhrase = FindBestMatch(predLazySAindexed, succLazySAindexed, curSuffix, matched);
                    res.Add(bestPhrase);
                }
                matched += Math.Max(res.Last().len, 1);


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

        private int FindNextSucc(int curSuffix, int matched, int startPos = -1)
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

        private int FindLazySuccessor(int curSuffix, int matched)
        {

            int i = SALazyInv[curSuffix + matched] + 1;

            while (i < SALazy.Length)
            {
                if (SALazy[i] - curSuffix < 0)
                {
                    i++;
                    continue;
                }

                else if (SALazy[i] < curSuffix + matched)
                {
                    successorsLazy[matched + curSuffix] = i;
                    return i;
                }
                // Bigger element is found, check if pointer can be used
                else
                {
                    // The pointer could not be found (but has been tried to), which means that nothing smaller can be found
                    if (successorsLazy[i] == -1)
                    {
                        break;
                    }
                    // Pointer has not been set
                    else if (successorsLazy[i] == -2)
                    {
                        i++;
                    }
                    // Pointer has been set, use.
                    else
                    {
                        i = successorsLazy[i];
                    }


                }

            }

            successorsLazy[SALazyInv[curSuffix + matched]] = -1;
            return -1;
        }

        private int FindLazyPredecessor(int curSuffix, int matched)
        {

            int i = SALazyInv[curSuffix + matched] - 1;

            while(i >= 0)
            {
                if (SALazy[i] - curSuffix < 0)
                {
                    i--;
                    continue;
                }

                else if (SALazy[i] < curSuffix + matched)
                {
                    predecessorsLazy[matched + curSuffix] = i;
                    return i;
                }
                // Bigger element is found, check if pointer can be used
                else
                {
                    // The pointer could not be found (but has been tried to), which means that nothing smaller can be found
                    if (predecessorsLazy[i] == -1)
                    {
                        break;
                    }
                    // Pointer has not been set
                    else if (predecessorsLazy[i] == -2)
                    {
                        i--;
                    }
                    // Pointer has been set, use.
                    else
                    {
                        i = predecessorsLazy[i];
                    }


                }

            }

            predecessorsLazy[SALazyInv[curSuffix + matched]] = -1;
            return -1;

        }


        // Also check if self-referencing could be better
        private void ReuseByLCP(int curSuffix, int prevSuffixIndex, Phrase[] prevPhrases, ref List<Phrase> res, ref HashSet<char> seenChars, ref int matched)
        {
            if (prevPhrases == null || prevSuffixIndex == -1) return;

            int lcpVal = lcpDS.GetPrefixLength(curSuffix, prevSuffixIndex);
            int prevPhrasesIndex = 0;


            while(prevPhrasesIndex + 1 < prevPhrases.Length && matched + Math.Max(prevPhrases[prevPhrasesIndex + 1].len, 1) <= lcpVal && matched + Math.Max(prevPhrases[prevPhrasesIndex].len, 1) <= lcpVal)
            {
                res.Add(prevPhrases[prevPhrasesIndex]);

                if (prevPhrases[prevPhrasesIndex].len == 0)
                    seenChars.Add((char)prevPhrases[prevPhrasesIndex].pos);

                matched += Math.Max(prevPhrases[prevPhrasesIndex].len, 1);
                prevPhrasesIndex++;

            }



        }





    }
}
