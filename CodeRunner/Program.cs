using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace Speciale
{
    public class Program
    {
        

        static void Main(string[] args)
        {
            List<string> fileNames = new List<string>() { "proteins", "book2" };
            List<string> testTypes = new List<string>() { "ConstructPTV2KKP3", "ConstructPTV2CSC", "ConstructPTV1CSC", "ConstructPTV1KKP3" };

            // Running from IDE

            foreach (var filename in fileNames)
            {
                foreach(var testype in testTypes)
                {
                    string exePath = "C:\\Users\\Mathi\\Desktop\\Speciale\\Speciale\\bin\\Debug\\net6.0\\Speciale.exe";
                    Process process = new Process();
                    process.StartInfo.Arguments = filename + " " + testype;
                    process.StartInfo.FileName = exePath;
                    process.Start();
                    process.WaitForExit();
                }
            }












        }
    }
}