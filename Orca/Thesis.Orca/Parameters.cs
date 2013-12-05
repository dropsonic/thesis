using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca
{
    public class Parameters
    {
        /// <summary>
        /// Distance score function.
        /// </summary>
        public Func<IEnumerable<double>, double> ScoreFunction { get; set; }

        /// <summary>
        /// Distance function for calculating the distance between two examples with weights.
        /// </summary>
        public Func<Record, Record, Weights, double> DistanceFunction { get; set; }

        private int _numOutliers;
        /// <summary>
        /// Number of outliers.
        /// </summary>
        public int NumOutliers
        {
            get { return _numOutliers; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _numOutliers = value;
            }
        }

        private int _neighborsCount;
        public int NeighborsCount
        {
            get { return _neighborsCount; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _neighborsCount = value;
            }
        }

        private double _cutoff;
        public double Cutoff
        {
            get { return _cutoff; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value >= 0);
                _cutoff = value;
            }
        }

        private int _batchSize;
        public int BatchSize
        {
            get { return _batchSize; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _batchSize = value;
            }
        }

        public Parameters()
        {
            // outlier options
            NumOutliers = 30;
            NeighborsCount = 5;
            Cutoff = 0;

            // computation parameters
            BatchSize = 1000;

            ScoreFunction = ScoreFunctions.Average;
            DistanceFunction = DistanceFunctions.Euclid;
        }
    }
}