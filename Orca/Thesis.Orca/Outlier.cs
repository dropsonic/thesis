using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.Orca
{
    [DebuggerDisplay("Id = {Record.Id}; Score = {Score}")]
    public struct Outlier : IComparable<Outlier>
    {
        public Record Record { get; set; }
        public double Score { get; set; }
        //public IEnumerable<int> Neighbors { get; set; }

        public int CompareTo(Outlier other)
        {
            return Score.CompareTo(other.Score);
        }
    }
}
