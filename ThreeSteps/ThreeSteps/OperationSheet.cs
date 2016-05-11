using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ThreeSteps
{
    enum ColType
    {
        srcGrid = 0,
        srcPosition = 1,
        dilutionTimes = 2
    }

    class OperationSheet
    {
        List<SampleInfo> diultionInfos;
        public List<SampleInfo>  Read(string sFile)
        {
            diultionInfos = new List<SampleInfo>();
            List<string> strs = File.ReadAllLines(sFile).ToList();
            strs.ForEach(x => AddDiultionInfo(x));
            return diultionInfos;
        }

        private void AddDiultionInfo(string s)
        {
            List<string> strs = s.Split(',').ToList();
            int srcGrid = int.Parse(strs[(int)ColType.srcGrid]);
            int srcPosition = int.Parse(strs[(int)ColType.srcPosition]);
            int dilutionTimes = int.Parse(strs[(int)ColType.dilutionTimes]);
            diultionInfos.Add(new SampleInfo(srcGrid, srcPosition, dilutionTimes));
        }
    }

    class SampleInfo
    {
        public int gridNum;
        public int position;
        public int diluteTimes;
        public SampleInfo(int gridNum, int position, int times)
        {
            this.gridNum = gridNum;
            this.position = position;
            this.diluteTimes = times;
        }
    }
}
