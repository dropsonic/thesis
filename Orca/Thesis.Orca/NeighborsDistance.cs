using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Collections;
using Thesis.Orca.Common;

namespace Thesis.Orca
{
    struct NeighborsDistance
    {
        public Record Record { get; set; }
        public BinaryHeap<double> Distances { get; set; }
    }
}
