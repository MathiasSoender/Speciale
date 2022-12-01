using Speciale.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Speciale.SuffixArray
{
    public static class SAWrapper
    {
        [DllImport("GenerateSA.dll", EntryPoint = "SingleGenerateDLL", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern void SingleGenerateDLL([MarshalAs(UnmanagedType.LPStr)] string data, int length, int[] output);

        public static int[] GenerateSuffixArrayDLL(string data, bool isFile = true)
        {
            string S = isFile ? File.ReadAllText(data) : data;

            int[] output = new int[S.Length];

            SingleGenerateDLL(S, S.Length, output);

            return output;
        }



    }
}
