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
    public class CSC_v3
    {

        LCP lcpDS;
        string data;
        int[] repeatLengths;

        // suffix index => pred SA index
        // predecessorsLazy[i] = -2 -> represents a value not being set yet
        // predecessorsLazy[i] = -1 -> represents that no pred can be found
        int[] predecessorsLazy;
        int[] successorsLazy;


        int[] SALazy;
        int[] SALazyInv;

        public CSC_v3(string data, int[] SA, int[] invSA, LCP lcpDS = null)
        {
            this.data = data;

            SALazy = SA;
            SALazyInv = invSA;
            this.lcpDS = lcpDS == null ? new LCP(data, SALazy, invSA, LCPType.fast) : lcpDS;
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

            if (len1 >= len2 && len1 >= len3)
            {
                return new Phrase() { len = len1, pos = pred };
            }
            else if (len2 >= len1 && len2 >= len3)
            {
                return new Phrase() { len = len2, pos = succ };

            }
            else if (len3 > len1 && len3 > len2)
            {
                return new Phrase() { len = len3, pos = (matched - 1) };
            }
            else
            {
                throw new Exception("Should never happen");
            }
        }

        public Phrase[][] CompressAllSuffixes(bool safeResult = true)
        {
            var res = safeResult ? new Phrase[data.Length][] : new Phrase[1][];
            Phrase[] curRes = null;
            for (int i = 0; i < data.Length; i++)
            {
                curRes = CompressOneSuffix(i, curRes);
                if (safeResult) res[i] = curRes;
            }
            return res;

        }

        // Should be used with care! Can only be used in suffix order; ie curSuffix = 0, curSuffix = 1, ..
        public Phrase[] CompressOneSuffix(int curSuffix, Phrase[] prevPhrases, int neededMatches = int.MaxValue)
        {
            HashSet<char> seenChars = new HashSet<char>();
            List<Phrase> res = new List<Phrase>();
            int matched = 0;
            int suffixLength = data.Length - curSuffix;

            int prevPhraseIndex = 1; // Skip the first phrase (never used)
            int prevPhraseMatched = 0;
            bool usePrevPhrases = prevPhrases != null;

            while (matched < suffixLength && neededMatches >= matched)
            {
                int i = matched + curSuffix;
                Phrase reusablePhrase;

                if (!seenChars.Contains(data[i]))
                {
                    seenChars.Add(data[i]);
                    res.Add(new Phrase() { len = 0, pos = data[i] });
                }
                else if (usePrevPhrases && CanPreviousPhrasesBeUsed(prevPhraseIndex, prevPhraseMatched, matched, prevPhrases, out reusablePhrase))
                {
                    res.Add(new Phrase() { len = reusablePhrase.len, pos = reusablePhrase.pos - 1 });
                }
                else
                {
                    int predLazySAindexed = FindLazyPredecessor(curSuffix, matched);
                    int succLazySAindexed = FindLazySuccessor(curSuffix, matched);

                    var bestPhrase = FindBestMatch(predLazySAindexed, succLazySAindexed, curSuffix, matched);
                    res.Add(bestPhrase);
                }
                matched += res.Last().len == 0 ? 1 : res.Last().len;

                UpdatePrevPhrases(ref prevPhraseIndex, ref prevPhraseMatched, matched, prevPhrases);

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


            if (successorsLazy[matched + curSuffix] == -2)
            {
                int succIndex = FindNextSucc(curSuffix, matched);
                successorsLazy[matched + curSuffix] = succIndex;
                return succIndex;
            }

            else
            {
                if (successorsLazy[matched + curSuffix] == -1)
                {
                    return -1;
                }
                // Compute new
                if (SALazy[successorsLazy[matched + curSuffix]] - curSuffix < 0)
                {
                    int succIndex = FindNextSucc(curSuffix, matched, successorsLazy[matched + curSuffix]);
                    successorsLazy[matched + curSuffix] = succIndex;
                    return succIndex;
                }


            }

            return successorsLazy[matched + curSuffix];

        }
        private int FindNextPred(int curSuffix, int matched, int startPos = -1)
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
        private int FindLazyPredecessor(int curSuffix, int matched)
        {

            if (predecessorsLazy[matched + curSuffix] == -2)
            {
                int predIndex = FindNextPred(curSuffix, matched);
                predecessorsLazy[matched + curSuffix] = predIndex;
                return predIndex;
            }

            else
            {
                if (predecessorsLazy[matched + curSuffix] == -1)
                {
                    return -1;
                }
                // Compute new
                if (SALazy[predecessorsLazy[matched + curSuffix]] - curSuffix < 0)
                {
                    int predIndex = FindNextPred(curSuffix, matched, predecessorsLazy[matched + curSuffix]);
                    predecessorsLazy[matched + curSuffix] = predIndex;
                    return predIndex;
                }


            }

            return predecessorsLazy[matched + curSuffix];

        }


        private bool CanPreviousPhrasesBeUsed(int prevPhraseIndex, int prevPhraseMatched, int matched, Phrase[] prevPhrases, out Phrase reusablePhrase)
        {
            reusablePhrase = null;
            if (prevPhrases.Length <= prevPhraseIndex)
                return false;

            // i+1 as we are now a suffix 1 smaller, meaning that prevPhrases is indexed according to prev suffix.
            if (prevPhraseMatched == matched && prevPhrases[prevPhraseIndex].pos != 0 && prevPhrases[prevPhraseIndex].len != 0)
            {
                reusablePhrase = prevPhrases[prevPhraseIndex];
                return true;

            }
            return false;
        }

        private void UpdatePrevPhrases(ref int prevPhraseIndex, ref int prevPhraseMatched, int matched, Phrase[] prevPhrases)
        {
            if (prevPhrases == null)
                return;

            // Setting prevPhraseIndex to large val ensures that we return false on CanPReviousPhraseBeUsed.
            if (prevPhrases.Length <= prevPhraseIndex)
            {
                prevPhraseIndex = int.MaxValue;
                return;
            }

            while (matched > prevPhraseMatched)
            {
                prevPhraseMatched += prevPhrases[prevPhraseIndex].len == 0 ? 1 : prevPhrases[prevPhraseIndex].len;
                prevPhraseIndex++;
            }

        }





    }
}
