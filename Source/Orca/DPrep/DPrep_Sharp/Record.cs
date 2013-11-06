using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    struct Record
    {
        public int Index { get; set; }
        public IList<float> Real { get; set; }
        public IList<int> Discrete { get; set; }
    }
}
