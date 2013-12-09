using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca
{
    public static class ScoreFunctions
    {
        private static readonly Func<IEnumerable<double>, double> _average = new Func<IEnumerable<double>, double>(dist => dist.Sum() / dist.Count());
        private static readonly Func<IEnumerable<double>, double> _sum = new Func<IEnumerable<double>, double>(dist => dist.Sum());
        private static readonly Func<IEnumerable<double>, double> _kthNeighbor = new Func<IEnumerable<double>, double>(dist => dist.FirstOrDefault());

        /// <summary>
        /// Average distance to k neighbors.
        /// </summary>
        public static Func<IEnumerable<double>, double> Average
        {
            get { return _average; }
        }

        /// <summary>
        /// Total distance to k neighbors (sum).
        /// </summary>
        public static Func<IEnumerable<double>, double> Sum
        {
            get { return _sum; }
        }

        /// <summary>
        /// Distance to kth neighbor.
        /// </summary>
        public static Func<IEnumerable<double>, double> KthNeighbor
        {
            get { return _kthNeighbor; }
        }
    }
}
