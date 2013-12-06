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
        public Parameters()
        {
            Randomize = true;
            Iterations = 5;
            RandFiles = 10;
            Seed = (int)DateTime.Now.Ticks;
        }

        private int _iterations;
        private int _randFiles;

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
    }
}
