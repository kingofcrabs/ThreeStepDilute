﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ThreeSteps
{
    class Configurations
    {
        static Configurations instance;
        static public Configurations Instance
        {
            get
            {
                if (instance == null)
                    instance = new Configurations();
                return instance;
            }
        }

        private Configurations()
        {
            Ratio = double.Parse(ConfigurationManager.AppSettings["mixRatio"]);
            WorkingFolder = ConfigurationManager.AppSettings["workingFolder"];
            Plate1Vol = int.Parse(ConfigurationManager.AppSettings["plate1Vol"]);
            Plate2Vol = int.Parse(ConfigurationManager.AppSettings["plate2Vol"]);
            Plate3Vol = int.Parse(ConfigurationManager.AppSettings["plate3Vol"]);
            MixTimes = int.Parse(ConfigurationManager.AppSettings["mixTimes"]);

        }
        public int MixTimes { get; set; }
        public double Ratio { get; set; }
        public string WorkingFolder { get; set; }

        public int Plate1Vol { get; set; }
        public int Plate2Vol { get; set; }
        public int Plate3Vol { get; set; }
    }
}
