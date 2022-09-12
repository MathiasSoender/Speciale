using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.Common
{
    public enum LCPType
    {
        naive
    }

    // Note this is not LCP array, but the complete LCP DS which supports O(1) query between any 2 indicies.
    public class LCP
    {
        private int[][] NaiveLCP;

        private LCPType processType;

        public LCP(Trie T, LCPType processType)
        {
            if (processType == LCPType.naive)
                ConstructNaiveLCP(T);
            this.processType = processType;
        }


        // Does not work, aabbaa$
        // O(n^2) time + O(n^2) space
        /*
        private void ConstructNaiveLCP(Trie T)
        {
            NaiveLCP = new int[T.S.Length][];
            int[] indexToLexi = Statics.IndexToLexigraphical(T.SA);


            for (int i = 0; i < T.S.Length; i++)
            {
                int lexiIndexOfI = indexToLexi[i];
                NaiveLCP[i] = new int[T.S.Length - lexiIndexOfI];
                int matchIndex = 0;

                for (int j = (lexiIndexOfI + 1); j < T.S.Length; j++)
                {
                    NaiveLCP[i][j - lexiIndexOfI] = FindPrefix(T.S, i, T.SA[j], ref matchIndex);
                }
            }
        }

        private int FindPrefix(string S, int i, int j, ref int matchIndex)
        {
            // Suffix lengths
            int lenI = S.Length - i;
            int lenJ = S.Length - j;
            matchIndex = new List<int>(){matchIndex, (lenI - 1), (lenJ - 1)}.Min(); // Avoid out of bounds, example: abcabc$

            bool up = S[i + matchIndex] == S[j + matchIndex];

            if (up)
            {
                while ((i+matchIndex) < S.Length && (j+matchIndex) < S.Length && S[i + matchIndex] == S[j + matchIndex])
                {
                    matchIndex++;
                }
            }
            else
            {
                while (matchIndex > 0 && S[i + matchIndex] != S[j + matchIndex])
                {
                    matchIndex--;
                }

                // Edge case
                if (S[i + matchIndex] == S[j + matchIndex])
                    matchIndex++;
            }
            


            return matchIndex;
        }


        */

        // O(n^3) time + O(n^2) space
        // Simple brute force..
        private void ConstructNaiveLCP(Trie T)
        {
            NaiveLCP = new int[T.S.Length][];

            for (int i = 0; i < T.S.Length; i++)
            {
                NaiveLCP[i] = new int[T.S.Length - i];


                for (int j = i+1; j < T.S.Length; j++)
                {
                    NaiveLCP[i][j - i] = CommonPrefixNaive(T.S, i, j);
                }
            }
        }

        public int CommonPrefixNaive(string S, int i, int j)
        {
            int match = 0;

            while ((i+match) < S.Length && (j+match) < S.Length && S[i + match] == S[j + match])
                match++;

            return match;
        }


        public int GetPrefixLength(int i, int j)
        {
            if (i == j)
                return i;

            if (processType == LCPType.naive)
            {
                if (i > j)
                    throw new Exception("Index j is smaller than index i");
                return NaiveLCP[i][j - i];
            }



            throw new Exception("Not yet implemented");

        }



    }
}
