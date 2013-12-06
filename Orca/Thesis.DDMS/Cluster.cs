using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    class Cluster
    {
        public Record UpperBound { get; set; }
        public Record LowerBound { get; set; }

        public Cluster(Record init)
        {
            UpperBound = init.Clone();
            LowerBound = init.Clone();
        }
    }
}
