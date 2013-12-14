using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca
{
    public static class ScoreFunctions
    {
        private static readonly ScoreFunction _average = new ScoreFunction(dist => dist.Sum() / dist.Count());
        private static readonly ScoreFunction _sum = new ScoreFunction(dist => dist.Sum());
        private static readonly ScoreFunction _kthNeighbor = new ScoreFunction(dist => dist.FirstOrDefault());

        /// <summary>
        /// Average distance to k neighbors.
        /// </summary>
        public static ScoreFunction Average
        {
            get { return _average; }
        }

        /// <summary>
        /// Total distance to k neighbors (sum).
        /// </summary>
        public static ScoreFunction Sum
        {
            get { return _sum; }
        }

        /// <summary>
        /// Distance to kth neighbor.
        /// </summary>
        public static ScoreFunction KthNeighbor
        {
            get { return _kthNeighbor; }
        }
    }
}
