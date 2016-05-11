using System;
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
        }
        public double Ratio { get; set; }
        public string WorkingFolder { get; set; }
    }
}
