using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Speciale.Common
{
    public enum LCPType
    {
        naive,
        fast
    }

    public class LCPArray
    {
        public int[] SA;
        public int[] inverseSA;
        private string S;

        // Ordered by SA, not S. So lcpArr[1] = Prefix length of SA[0] and SA[1].
        public int[] lcpArr;




        public LCPArray(int[] SA, string S)
        {
            this.SA = SA;
            this.S = S;
            Construct();
        }




        private void Construct()
        {
            lcpArr = new int[S.Length];


            inverseSA = Statics.InverseArray(SA);
            int l = 0;
            for (int i = 0; i < inverseSA.Length; i++)
            {
                int k = inverseSA[i];

                if (k <= 0)
                    continue;

                int j = SA[k - 1];

                while ((i + l) < S.Length && (j + l) < S.Length && S[i+l] == S[j + l])
                {
                    l++;
                }
                lcpArr[k] = l;

                if (l > 0) l--;



            }



        }

        public int Get(int i)
        {
            if (i == 0)
            {
                throw new Exception("Trying to access index 0 of LCP-Array");
            }
            return lcpArr[i];

        }


    }



    // Note this is not LCP array, but the complete LCP DS which supports O(1) query between any 2 indicies.
    public class LCP
    {

        private LCPType processType;
        private LCPNaive lcpNaive;
        private LCPFast lcpFast;
        private string S;


        public LCP(string S, int[] SA,LCPType processType)
        {
            this.S = S;
            DateTime t1 = DateTime.Now;
            if (processType == LCPType.naive)
                lcpNaive = new LCPNaive(S);

            if (processType == LCPType.fast)
                lcpFast = new LCPFast(S, SA);


            this.processType = processType;
            Console.Out.WriteLine("Time for LCP construction: " + (DateTime.Now - t1).TotalSeconds);
        }

        public int GetPrefixLength(int i, int j, bool suffixIndexed = true)
        {
            if (i == j)
                return S.Length - i;

            if (i > j)
            {
                int v1 = i;
                int v2 = j;
                i = v2;
                j = v1;
            }

            if (processType == LCPType.naive && lcpNaive != null)
                return lcpNaive.GetPrefixLength(i, j);

            if (processType == LCPType.fast && lcpFast != null)
                return lcpFast.GetPrefixLength(i, j, suffixIndexed);

            throw new Exception("Not yet implemented");

        }


    }

    public class LCPNaive
    {
        private int[][] NaiveLCP;


        public LCPNaive(string S)
        {
            ConstructNaiveLCP(S);
        }

        private void ConstructNaiveLCP(string S)
        {
            NaiveLCP = new int[S.Length][];

            for (int i = 0; i < S.Length; i++)
            {
                NaiveLCP[i] = new int[S.Length - i];


                for (int j = i + 1; j < S.Length; j++)
                {
                    NaiveLCP[i][j - i] = CommonPrefixNaive(S, i, j);
                }
            }
        }

        private int CommonPrefixNaive(string S, int i, int j)
        {
            int match = 0;

            while ((i + match) < S.Length && (j + match) < S.Length && S[i + match] == S[j + match])
                match++;

            return match;
        }

        public int GetPrefixLength(int i, int j)
        {
            if (i > j)
                throw new Exception("Index j is smaller than index i");
            return NaiveLCP[i][j - i];
        }

    }

    public class LCPFast
    {
        private FastRMQ RMQLCPArr;
        private LCPArray lcpArr;

        public LCPFast(string S, int[] SA)
        {
            lcpArr = new LCPArray(SA, S);
            RMQLCPArr = new FastRMQ(lcpArr.lcpArr);
        }

        // suffixIndexed = true: Transform Suffix index to SA index before use.
        public int GetPrefixLength(int i, int j, bool suffixIndexed = true)
        {
            if (suffixIndexed)
            {
                int v1 = lcpArr.inverseSA[i];
                int v2 = lcpArr.inverseSA[j];
                i = Math.Min(v1, v2) + 1;
                j = Math.Max(v1, v2);
            }


            return RMQLCPArr.GetRMQ(i, j);
        }


    }


    public class FastRMQ
    {
        int[] A;
        int[][] B_blocks;
        int[] A_prime;
        int[] B;
        int n;
        int n_div_s;
        int s;
        int[][] M;
        int[] T;

        // TypeToTable(i,j)
        Dictionary<int, int[][]> P;


        // C_p,q computes
        private Dictionary<Tuple<int, int>, int> computedBallots = new Dictionary<Tuple<int, int>, int>();



        public FastRMQ(int[] arr)
        {
            A = arr;
            ConstructFastRMQ();
        }


        public int GetRMQ(int i, int j)
        {
            int block_i = i / s;
            int block_j = j / s;

            if (block_i == block_j)
            {
                return InBlockQuery(i, j, block_i);
            }

            int blocksBetween = block_j - block_i - 1;
            int min_i;
            int min_j;

            if (blocksBetween == 0)
            {
                min_i = InBlockQuery(i, s * block_i + (s-1), block_i);
                min_j = InBlockQuery(s * block_j, j, block_j);

                return Math.Min(min_i, min_j);

            }

            min_i = InBlockQuery(i, s * block_i + (s - 1), block_i);
            min_j = InBlockQuery(s * block_j, j, block_j);


            int missing_blocks_i = block_i + 1;
            int missing_blocks_j = block_j - 1;




            // Edge cases (needs to be handled seperately as M[i][0] = first power of 2^1, and not 2^0:
            // Missing 1 block
            if (missing_blocks_i == missing_blocks_j)
            {
                return Math.Min(Math.Min(A_prime[missing_blocks_i], min_i), Math.Min(min_i, min_j));
            }

            int l = (int)Math.Floor(Math.Log2(missing_blocks_j - missing_blocks_i));
            int val1;
            int val2;

            // Missing two blocks
            if (l == 0)
            {
                val1 = A_prime[missing_blocks_i];
                val2 = A_prime[missing_blocks_j];
            }
            // Missing more than 2 blocks, use M
            else
            {
                val1 = M[block_i + 1][l - 1];
                val2 = M[block_j - (int)Math.Pow(2, l)][l - 1];
            }


            int min_3 = Math.Min(val1, val2);



            return Math.Min(Math.Min(min_i, min_j), Math.Min(min_j, min_3));
        }

        private int InBlockQuery(int i, int j, int block)
        {
            if (i == j)
                return A[i];

            int type = T[block];
            int[][] table = P[type];

            int localIindex = i - block * s;

            int localIndex = table[localIindex][(j - localIindex) - block * s];

            int res = A[block * s + localIndex];

            return res;
        }

        private void ConstructFastRMQ()
        {
            n = A.Length;
            s = (int)(Math.Ceiling(Math.Log2(n) / 4));
            n_div_s = (int)Math.Ceiling(n / (double)s);


            PadA();
            ParitionB_blocks();
            ConstructA_primeAndB();
            ConstructM();
            ConstructT();
            ConstructP();


        }
        // Makes life easier
        private void PadA()
        {
            if (s == 1) return;

            int paddingNeeded = n % s;

            int[] newA = new int[n + (s - paddingNeeded)];

            for (int i = 0; i < n ; i++)
            {
                newA[i] = A[i];
            }
            for (int i = n; i < newA.Length; i++)
            {
                newA[i] = int.MaxValue;
            }
            A = newA;

            n = A.Length;
            s = (int)(Math.Ceiling(Math.Log2(n) / 4));
            n_div_s = (int)Math.Ceiling(n / (double)s);

        }

        private void ParitionB_blocks()
        {
            B_blocks = new int[n_div_s][];

            for (int i = 0; i < n_div_s; i++)
            {
                int startIndex = i * s;

                B_blocks[i] = A.Skip(startIndex).Take(s).ToArray();

            }
        }

        private void ConstructA_primeAndB()
        {
            A_prime = new int[n_div_s];
            B = new int[n_div_s];


            for(int i = 0; i < n_div_s; i++)
            {
                int min = int.MaxValue;
                int index = -1;

                for (int j = 0; j < B_blocks[i].Length; j++)
                {
                    if (B_blocks[i][j] < min)
                    {
                        min = B_blocks[i][j];
                        index = j;
                    }
                }

                A_prime[i] = min;
                B[i] = (i * s) + index;
            }
        }

        private void ConstructM()
        {
            M = new int[n_div_s][];

            for (int i = 0; i < n_div_s; i++)
            {
                var l = (int)Math.Floor(Math.Log2(n_div_s));
                M[i] = new int[l];

                for (int j = 1; j <= l; j++)
                {
                    int min = A_prime[i];

                    for (int k = i; k <= i + Math.Pow(2,j) - 1 && k < (A_prime.Length); k++)
                        min = Math.Min(A_prime[k], min);

                    M[i][j - 1] = min;
                }
            }
        }


        // All arrays of b_blocks. Find res of all RMQ(i,j) for all i,j
        // Spends O(1) for each (i,j) pair (same comp. as space, which is O(n)).
        private void ConstructP()
        {

            // Could lose dict and use int[][][] (quite unreadable tho)
            P = new Dictionary<int, int[][]>();


            for (int tIndex = 0; tIndex < T.Length; tIndex++)
            {
                // Dont compute already computed types
                int type = T[tIndex];

                if (P.ContainsKey(type))
                    continue;

                int[] actualBlock = B_blocks[tIndex];


                // All (i,j) results
                var table = new int[s][];

                for (int i = 0; i < s; i++)
                {
                    table[i] = new int[s - i];
                    table[i][0] = i;

                    for (int j = (i+1); j < s; j++)
                    {
                        if (actualBlock[j] < actualBlock[table[i][j - i - 1]])
                        {
                            table[i][j - i] = j;
                        }
                        else
                        {
                            table[i][j - i] = table[i][j - i - 1];
                        }

                    }

                }

                P[type] = table;
            }



        }


        private void ConstructT()
        {
            T = new int[n_div_s];

            for (int i = 0; i < B_blocks.Length; i++)
            {
                T[i] = FindBlockType(B_blocks[i]);
            }
        }


        private int FindBlockType(int[] block)
        {
            int[] rp = new int[s + 1];
            rp[0] = int.MinValue;

            int q = s;
            int N = 0;

            for (int i = 0; i < s; i++)
            {
                while (rp[q + i - s] > block[i])
                {
                    N += Ballot_Number(s - (i + 1), q);
                    q -= 1;
                }

                rp[q + i + 1 - s] = block[i];
            }

            return N;

        }

        private int Ballot_Number(int p, int q)
        {
            int res;
            if (computedBallots.TryGetValue(new Tuple<int, int>(p, q), out res))
            {
                return res;
            }

            if (p == 0 && q == 0)
                res = 1;

            else if (p <= q && 0 <= p && q != 0)
            {
                res = Ballot_Number(p, q - 1) + Ballot_Number(p - 1, q);
            }
            else
            {
                res = 0;
            }

            computedBallots.Add(new Tuple<int, int>(p, q), res);

            return res;
        }

    }

}
