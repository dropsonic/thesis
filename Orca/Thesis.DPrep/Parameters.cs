using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

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

        public Parameters()
        {
            Scaling = Scale.ZeroToOne;
            Randomize = true;
            Iterations = 5;
            RandFiles = 10;
            TempFileStem = "tmp";
            Seed = (int)DateTime.Now.Ticks;
            MissingR = float.NaN;
            MissingD = -1;
            DiscreteWeight = 0.4f;
            RealWeight = 1.0f;
        }

        private int _iterations;
        private int _randFiles;

        public Scale Scaling { get; set; }

        public bool Randomize { get; set; }

        public int Iterations
        {
            get { return _iterations; }
            set 
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _iterations = value;
            }
        }

        public int RandFiles
        {
            get { return _randFiles; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _randFiles = value;
            }
        }

        public int Seed { get; set; }

        public string TempFileStem { get; set; }

        public int MissingD { get; set; }
        public float MissingR { get; set; }

        /// <summary>
        /// Weight value for all discrete variables.
        /// </summary>
        public float DiscreteWeight { get; set; }
        /// <summary>
        /// Weight value for all continuous variables.
        /// </summary>
        public float RealWeight { get; set; } 
    }
}
