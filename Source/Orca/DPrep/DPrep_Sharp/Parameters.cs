using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    public class Parameters
    {
        public enum Scale
        {
            /// <summary>
            /// Scale continuous fields to range [0,1].
            /// </summary>
            ZeroToOne = 0,
            /// <summary>
            /// scale continuous fields to zero mean and unit standard devation.
            /// </summary>
            Std = 1,
            /// <summary>
            /// No scaling of continuous fields.
            /// </summary>
            None = 2
        }

        public enum Clean
        {
            Final = 0,
            During = 1,
            None = 2
        }

        public Parameters()
        {
            Scaling = Scale.ZeroToOne;
            Randomize = true;
            Iterations = 5;
            RandFiles = 10;
            Cleaning = Clean.Final;
            TempFileStem = "tmp";
            Seed = (int)DateTime.Now.Ticks;
        }

        private int _iterations;
        private int _randFiles;

        public string DataFile { get; set; }
        public string NamesFiles { get; set; }
        public string DestinationFile { get; set; }
        public string WeightFile { get; set; }

        public Scale Scaling { get; set; }
        public string ScaleFile { get; set; }

        public bool Randomize { get; set; }

        public int Iterations
        {
            get { return _iterations; }
            set 
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();
                _iterations = value;
            }
        }

        public int RandFiles
        {
            get { return _randFiles; }
            set 
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(); 
                _randFiles = value;
            }
        }

        public int Seed { get; set; }

        public Clean Cleaning { get; set; }

        public string TempFileStem { get; set; }

        public int MissingD { get; set; }
        public float MissingR { get; set; }
    }
}
