using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeSteps
{
    class worklist
    {
        int bufferPositionIndex = 0;
        public void DoJob(List<SampleInfo> sampleInfos,
            List<string> bufferPipettingsStrs, 
            List<string> samplePipettingStrs,
            List<string> rCommandsTransfer)
        {
           
            List<List<SampleInfo>> each96DiluteInfos = SortInfos(sampleInfos);
            for (int i = 0; i < each96DiluteInfos.Count; i++ )
            {
                var this96DiluteInfos = each96DiluteInfos[i];
                while (this96DiluteInfos.Count > 0)// pipetting eight samples together
                {
                    var thisBatchDiluteInfos = this96DiluteInfos.Take(8).ToList();
                    this96DiluteInfos = this96DiluteInfos.Skip(8).ToList();
                    var bufferPipettingsThisBatch = GenerateBufferPipettings(thisBatchDiluteInfos,i);
                    var samplePipettingsThisBatch = GenerateSamplePipettings(thisBatchDiluteInfos,i);
                    bufferPipettingsStrs.AddRange(FormatBuffer(bufferPipettingsThisBatch));
                    samplePipettingStrs.AddRange(FormatSample(samplePipettingsThisBatch,i));
                }
                rCommandsTransfer.AddRange(GenerateRCommand(this96DiluteInfos.Count, i));
            }
        }


        public List<string> GenerateRCommand(int sampleCnt, int regionIndex)
        {

            /*R;AspirateParameters;DispenseParameters;Volume;LiquidClass;NoOfDitiRe
            uses;NoOfMultiDisp;Direction[;ExcludeDestWell]*
            where:
            AspirateParameters =
            SrcRackLabel;SrcRackID;SrcRackType;SrcPosStart;SrcPosEnd;
            and
            DispenseParameters =
            DestRackLabel;DestRackID;DestRackType;DestPosStart;DestPosEnd;
            R;T2;;Trough 100ml;1;8;MTP96-2;;96 Well Microplate;1;96;100;Water;
            2;5;0*/
            List<string> strs = new List<string>();
            var plateNames = GetPlateNames(regionIndex);
            int multiDispenseTimes = 1;
            string aspParameters = string.Format("DP{0};;;1;{1}", plateNames[0],sampleCnt);
            string dispParameters = string.Format("{0};;;1;{1}", plateNames[1], sampleCnt);
            string rCommand = string.Format("R;{0};{1};{2};;1;{3};0", aspParameters, dispParameters, 260, multiDispenseTimes);
            strs.Add(rCommand);

            aspParameters = string.Format("DP{0};;;1;{1}", plateNames[1], sampleCnt);
            dispParameters = string.Format("{0};;;1;{1}", plateNames[2], sampleCnt);
            rCommand = string.Format("R;{0};{1};{2};;1;{3};0", aspParameters, dispParameters, 250, multiDispenseTimes);
            strs.Add(rCommand);
            return strs;
        }

        private List<string> FormatSample(List<PipettingInfo> samplePipettingsThisBatch, int regionIndex)
        {
            List<string> strs = new List<string>();
            var plateNames = GetPlateNames(regionIndex);
            var secondPlateName = plateNames[1];
            var thirdPlateName = plateNames[2];
            foreach (var pipettingInfo in samplePipettingsThisBatch)
            {
                strs.Add(GetAspirate(pipettingInfo.srcLabware, pipettingInfo.srcWellID, pipettingInfo.vol));
                strs.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.vol));
                double volume = Configurations.Instance.Ratio * pipettingInfo.vol;
                //mix
                strs.Add(GetAspirate(pipettingInfo.dstLabware, pipettingInfo.dstWellID, volume));
                strs.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, volume));
            }
            return strs;
        }

        private IEnumerable<string> FormatBuffer(List<PipettingInfo> bufferPipettingsThisBatch)
        {
            List<string> strs = new List<string>();
            foreach(var pipettingInfo in bufferPipettingsThisBatch)
            {
                strs.Add(GetAspirate(pipettingInfo.srcLabware, pipettingInfo.srcWellID, pipettingInfo.vol));
                strs.Add(GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.vol));
                strs.Add("W;");
            }
            return strs;
        }

        

        private string GetAspirate(string sLabware, int srcWellID, double vol)
        {
            string sAspirate = string.Format("A;{0};;;{1};;{2};;;",
                         sLabware,
                         srcWellID,
                         vol);
            return sAspirate;
        }

        private string GetDispense(string sLabware, int dstWellID, double vol)
        {
            string sDispense = string.Format("D;{0};;;{1};;{2};;;",
              sLabware,
              dstWellID,
              vol);
            return sDispense;
        }
        private List<string> GenerateGWL(PipettingInfo pipettingInfo)
        {
            List<string> strs = new List<string>();
            string asp = GetAspirate(pipettingInfo.srcLabware, pipettingInfo.srcWellID, pipettingInfo.vol);
            string disp = GetDispense(pipettingInfo.dstLabware, pipettingInfo.dstWellID, pipettingInfo.vol);
            strs.Add(asp);
            strs.Add(disp);
            strs.Add("W;");
            return strs;
        }

        private List<List<SampleInfo>> SortInfos(List<SampleInfo> diluteInfos)
        {
            List<List<SampleInfo>> eachBatchDiluteInfos = new List<List<SampleInfo>>();
            for(int i = 0; i< 4; i++)
            {
                int startGird = i * 6 + 1;
                int endGrid = i * 6 + 6;
                eachBatchDiluteInfos.Add(diluteInfos.Where(x => x.gridNum >= startGird && x.gridNum <= endGrid).ToList());
            }
            return eachBatchDiluteInfos;
        }

        private List<PipettingInfo> GenerateSamplePipettings(List<SampleInfo> thisBatchDiluteInfos, int regionIndex)
        {
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
            foreach (var sample in thisBatchDiluteInfos)
            {
                double vol = GetSampleVolume(sample);
                vol = Math.Round(vol, 1);
                pipettingInfos.Add(
                    new PipettingInfo(string.Format("sample{0}",sample.gridNum),
                    sample.position,
                    GetPlateNames(regionIndex).First(), 
                    GetDstPosition(sample),
                    vol));
            }
            return pipettingInfos;
        }

        private double GetSampleVolume(SampleInfo sample)
        {
            return 270 - GetBufferVolume(sample);
        }

        private List<PipettingInfo> GenerateBufferPipettings(List<SampleInfo> thisBatchDiluteInfos, int regionIndex)
        {
            List<PipettingInfo> pipettingInfos = new List<PipettingInfo>();
            List<SampleInfo> firstPlateDilutes = thisBatchDiluteInfos.Where(x => x.diluteTimes > 1).ToList();
            List<SampleInfo> secondPlateDilutes = thisBatchDiluteInfos.Where(x => x.diluteTimes > 50).ToList();
            List<SampleInfo> thirdPlateDilutes = thisBatchDiluteInfos.Where(x => x.diluteTimes > 2500).ToList();
            Dictionary<string, List<SampleInfo>> allSamples = new Dictionary<string, List<SampleInfo>>();
            List<string> plateNames = GetPlateNames(regionIndex);
            allSamples.Add(plateNames[0], firstPlateDilutes);
            allSamples.Add(plateNames[1], secondPlateDilutes);
            allSamples.Add(plateNames[2], thirdPlateDilutes);

            foreach (var pair in allSamples)
            {
                string plateName = pair.Key;
                var samplesSamePlate = pair.Value;
                foreach(var sample in samplesSamePlate)
                {
                    double vol = GetBufferVolume(sample);
                    vol = Math.Round(vol, 1);
                    pipettingInfos.Add(new PipettingInfo("buffer", GetBufferPosition(), plateName, GetDstPosition(sample), vol));
                }
            }
            return pipettingInfos;
        }

        private List<string> GetPlateNames(int regionIndex)
        {
            List<string> plateNames = new List<string>();
            int startID = 1 + regionIndex * 3;
            for(int i = 0; i< 3; i++)
            {
                plateNames.Add(string.Format("DP{0}", startID + i));
            }
            return plateNames;
        }

        private double GetBufferVolume(SampleInfo sample)
        {
            double thisPlateTimes = sample.diluteTimes;
            if(sample.diluteTimes > 50 && sample.diluteTimes <= 2500)
                thisPlateTimes = Math.Sqrt(thisPlateTimes);
            if(sample.diluteTimes > 2500 && sample.diluteTimes <= 125000)
                thisPlateTimes = Math.Log(thisPlateTimes,3);
            if (sample.diluteTimes > 125000)
                throw new Exception(string.Format("Invalid sample: {0},{1}", sample.gridNum, sample.position));

            return 270 * (thisPlateTimes - 1) / thisPlateTimes;
            
        }

        private int GetDstPosition(SampleInfo sample)
        {
            return (sample.gridNum - 1) * 16 + sample.position;
        }

        private int GetBufferPosition()
        {
            int pos = bufferPositionIndex % 8 + 1;
            bufferPositionIndex++;
            return pos;
        }

    }

    class PipettingInfo
    {

        public string srcLabware;
        public int srcWellID;
        public string dstLabware;
        public int dstWellID;
        public double vol;

        public PipettingInfo(string srcLabware,
            int srcWell, string dstLabware, int dstWell, double v)
        {
            this.srcLabware = srcLabware;
            this.dstLabware = dstLabware;
            this.srcWellID = srcWell;
            this.dstWellID = dstWell;
            this.vol = v;

        }

        public PipettingInfo(PipettingInfo pipettingInfo)
        {
            srcLabware = pipettingInfo.srcLabware;
            dstLabware = pipettingInfo.dstLabware;
            srcWellID = pipettingInfo.srcWellID;
            dstWellID = pipettingInfo.dstWellID;
            vol = pipettingInfo.vol;
        }
    }
}
