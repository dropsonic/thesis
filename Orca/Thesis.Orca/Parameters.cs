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
        /// Distance score type.
        /// </summary>
        public enum DistanceType
        {
            /// <summary>
            /// Average distance to k neighbors.
            /// </summary>
            Average,
            /// <summary>
            /// Distance to kth neighbor.
            /// </summary>
            KthNeighbor
        }

        public string DataFile { get; set; }

        /// <summary>
        /// Distance score type.
        /// </summary>
        public DistanceType ScoreF { get; set; }

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

        private int _k;
        public int K
        {
            get { return _k; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _k = value;
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

        public bool RecordNeighbors { get; set; }
        public float MissingR { get; set; }
        public float DistMR { get; set; }

        public Parameters()
        {
            // outlier options
            ScoreF = DistanceType.Average;
            NumOutliers = 30;
            K = 5;
            Cutoff = 0;

            // computation parameters
            BatchSize = 1000;

            // misc parameters
            RecordNeighbors = false;
            MissingR = float.NaN;
            DistMR = 0.4f;
        }

        public Parameters(string dataFile)
            : this()
        {
            DataFile = dataFile;
        }
    }
}
