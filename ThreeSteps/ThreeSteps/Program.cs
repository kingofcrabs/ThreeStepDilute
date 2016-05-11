using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ThreeSteps
{
    class Program
    {
        static void Main(string[] args)
        {
            Helper.WriteResult(true, "");
#if DEBUG
#else

            try
#endif
            {
                Go();
            }
#if DEBUG
#else
            catch(Exception ex)
            {
                Console.WriteLine("Error happened: " + ex.Message + ex.StackTrace);
                Helper.WriteResult(false, ex.Message);
            }
#endif
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
            
        }

        private static void Go()
        {
            Console.WriteLine(Helper.AssemblyCompany);
            Console.WriteLine(Helper.AssemblyCopyright);
            Console.WriteLine(Helper.AssemblyVersion);
            string outputFolder = Helper.GetOutputFolder();
            //delete the old ones
            File.Delete(outputFolder + "buffers.gwl");
            File.Delete(outputFolder + "samples.gwl");
            File.Delete(outputFolder + "transfer.gwl");

            OperationSheet operationSheet = new OperationSheet();
            var directory = new DirectoryInfo(Configurations.Instance.WorkingFolder);
            var latestFile = directory.GetFiles("*csv")
             .OrderByDescending(f => f.LastWriteTime)
             .First();
            if (latestFile == null)
                throw new FileNotFoundException(string.Format("Cannot find any csv file at folder: {0}", Configurations.Instance.WorkingFolder));
            var sampleInfos = operationSheet.Read(latestFile.FullName);
            worklist worklist = new worklist();
            List<string> bufferPipettingStrs = new List<string>();
            List<string> samplePipettingStrs = new List<string>();
            List<string> rCommandStrs = new List<string>();
            worklist.DoJob(sampleInfos, bufferPipettingStrs, samplePipettingStrs, rCommandStrs);
           
            File.WriteAllLines(outputFolder + "buffers.gwl", bufferPipettingStrs);
            File.WriteAllLines(outputFolder + "samples.gwl", samplePipettingStrs);
            File.WriteAllLines(outputFolder + "transfer.gwl", rCommandStrs);
        }

        
    }
}
