using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca
{
    public struct Outlier : IComparable<Outlier>
    {
        public int Index { get; set; }
        public double Score { get; set; }
        //public IEnumerable<int> Neighbors { get; set; }

        public int CompareTo(Outlier other)
        {
            return Score.CompareTo(other.Score);
        }
    }
}
