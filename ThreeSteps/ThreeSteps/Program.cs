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
            Helper.WriteResult(true, "");
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
            
        }

        private static void Go()
        {
            Console.WriteLine(Helper.AssemblyCompany);
            Console.WriteLine(Helper.AssemblyCopyright);
            Console.WriteLine(Helper.AssemblyVersion);
            string outputFolder = Helper.GetOutputFolder();
            OperationSheet operationSheet = new OperationSheet();
            var directory = new DirectoryInfo(Configurations.Instance.WorkingFolder);
            var latestFile = directory.GetFiles("*csv")
             .OrderByDescending(f => f.LastWriteTime)
             .First();
            if (latestFile == null)
                throw new FileNotFoundException(string.Format("Cannot find any csv file at folder: {0}", Configurations.Instance.WorkingFolder));
            var sampleInfos = operationSheet.Read(latestFile.FullName);
            Console.WriteLine(string.Format("There are {0} samples.", sampleInfos.Count));
            worklist worklist = new worklist();
            var eachPlateSamplesInfo = worklist.SortInfos(sampleInfos);
            //Directory.Delete(outputFolder, true);
            //Directory.CreateDirectory(outputFolder);
            File.WriteAllText(outputFolder + "regionCnt.txt", eachPlateSamplesInfo.Count.ToString());
            for(int regionIndex = 0; regionIndex < eachPlateSamplesInfo.Count; regionIndex++)
            {
                string subFolder = outputFolder + string.Format("region{0}\\", regionIndex + 1);
                List<string> bufferPipettingStrs = new List<string>();
                List<List<string>> samplePipettingStrs = new List<List<string>>();
                for (int i = 0; i < 3; i++)
                    samplePipettingStrs.Add(new List<string>());
                List<string> readableStrs = new List<string>() { "srcLabware,srcWell,dstLabware,dstWell,volume" };
                worklist.DoJob(eachPlateSamplesInfo[regionIndex], bufferPipettingStrs, samplePipettingStrs, readableStrs, regionIndex);
                if (!Directory.Exists(subFolder))
                    Directory.CreateDirectory(subFolder);
                File.WriteAllLines(subFolder + "buffers.gwl", bufferPipettingStrs);
                for(int i = 0; i< samplePipettingStrs.Count; i++)
                {
                    File.WriteAllLines(string.Format(subFolder + "sample{0}.gwl", i + 1), samplePipettingStrs[i]);
                }
                File.WriteAllLines(subFolder + "buffersReadable.csv", readableStrs);
            }
            
        }

        
    }
}
