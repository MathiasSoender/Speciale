using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Speciale
{
    public class Program
    {
        

        static void Main(string[] args)
        {

            PreprocessTest();

        }


        public static void SearchTest()
        {
            var fileNames = new List<string>() { "book", "proteins", "a" };
            var testTypes = new List<string>() { "SearchST", "SearchPTV1", "SearchPTV2" };
            var dataLength = 10000;


            foreach (var filename in fileNames)
            {
                foreach(var type in testTypes)
                {

                    if (RunFile(filename + " " + type + " " + dataLength + " search ") != 0)
                    {
                        throw new Exception("error");
                    }


                }


            }



        }



        public static void LCPSumTest()
        {
            List<string> fileNames = new List<string>() { "book", "proteins", "repeat" };


            foreach (var filename in fileNames)
            {
                int partition = 20000;

                while (RunFile(filename + " " + "_" + " " + partition.ToString() + " lcpsum", false) == 0)
                {
                    if (partition >= 1500000)
                        break;

                    partition += 20000;

                }



            }
        }


        public static void PreprocessTest()
        {
            List<string> fileNames = new List<string>() { "repeat" };
            var testTypesPTV2 = new List<string>() { "ConstructPTV2Lexico" };//"ConstructPTV2KKP3", "ConstructPTV2CSC", "ConstructPTV2Lexico" };
            var testTypesPTV1 = new List<string>();// { "ConstructPTV1KKP3", "ConstructPTV1CSC"};
            var testTypesST = "ConstructSTfast";


            int ptv1PartitionIncrease = 10000;
            int ptv2PartitionIncrease = 50000;


            // Running from IDE
            int partition;
            int ptv2MaxPartition = 0;
            int ptv1MaxPartition = 60000;

            foreach (var filename in fileNames)
            {
                /*
                int ptv1MaxPartition = 0;
                int ptv2MaxPartition = 0;

                foreach (var testypePTV1 in testTypesPTV1)
                {

                    partition = ptv1PartitionIncrease;

                    while (RunFile(filename + " " + testypePTV1 + " " + partition.ToString() + " preprocess") == 0)
                    {
                        partition += ptv1PartitionIncrease;
                    }


                    ptv1MaxPartition = Math.Max(ptv1MaxPartition, partition);
                }
                */
                foreach (var testypePTV2 in testTypesPTV2)
                {
                    partition = ptv1PartitionIncrease;

                    /*
                    while (RunFile(filename + " " + testypePTV2 + " " + partition.ToString() + " preprocess") == 0)
                    {
                        if (partition >= ptv1MaxPartition)
                            break;
                        partition += ptv1PartitionIncrease;
                    }
                    */

                    partition = ptv2PartitionIncrease;

                    while (RunFile(filename + " " + testypePTV2 + " " + partition.ToString() + " preprocess") == 0)
                    {
                        partition += ptv2PartitionIncrease;
                    }

                    ptv2MaxPartition = Math.Max(ptv2MaxPartition, partition);


                }



                partition = ptv1PartitionIncrease;

                while (RunFile(filename + " " + testTypesST + " " + partition.ToString() + " preprocess") == 0)
                {
                    if (partition >= ptv1MaxPartition)
                        break;
                    partition += ptv1PartitionIncrease;
                }



                partition = ptv2PartitionIncrease;
                while (RunFile(filename + " " + testTypesST + " " + partition.ToString() + " preprocess") == 0)
                {
                    partition += ptv2PartitionIncrease;

                    if (partition >= ptv2MaxPartition)
                        break;
                }



            }
        }








        public static int RunFile(string arguments, bool sleep = true)
        {
            if (sleep) Thread.Sleep(3000);
            string exePath = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\net6.0\\Speciale.exe";
            Process process = new Process();
            process.StartInfo.Arguments = arguments;
            process.StartInfo.FileName = exePath;
            process.Start();
            process.WaitForExit();
            int exitcode = process.ExitCode;
            process.Dispose();
            if (sleep) Thread.Sleep(3000);
            return exitcode;



        }



    }
}