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
            string S;
            if (isFile)
                S = File.ReadAllText(data);
            else
                S = data;

            int[] output = new int[S.Length];

            SingleGenerateDLL(S, S.Length, output);

            return output;
        }



        public static void GenerateSuffixArray(string infile, out string SAfile)
        {
            ProcessStartInfo psi = Statics.GetStartInfo("GenerateSA.exe", "-single " + infile);


            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                var time = (process.ExitTime - process.StartTime).TotalSeconds;
                if (process.ExitCode == 0)
                    SAfile = infile + ".sa";
                else
                    throw new Exception("GenerateSA.exe failed.. Exit code: " + process.ExitCode);
            }
        }


        public static void GenerateAllSuffixArrays(string infile)
        {
            ProcessStartInfo psi = Statics.GetStartInfo("GenerateSA.exe", "-all " + infile);


            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                var time = (process.ExitTime - process.StartTime).TotalSeconds;
                if (process.ExitCode != 0)
                    throw new Exception("GenerateSA.exe failed.. Exit code: " + process.ExitCode);

            }
        }


        // Used to parse the generated SA file
        public static int[] SuffixArrayParser(string SAFile)
        {
            var bytes = File.ReadAllBytes(SAFile);
            int[] SA = new int[bytes.Length / 4];


            for(int i = 0; i < bytes.Length; i += 4)
            {
                SA[i / 4] = (int)(bytes[i+3] << 24 | bytes[i+2] << 16 | bytes[i+1] << 8 | bytes[i]);
            }

            return SA;


        }




    }
}
