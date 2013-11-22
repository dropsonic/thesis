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
        public string DataFile { get; set; }
        public string ReferenceFiles { get; set; }
        public string WeightFile { get; set; }

        /// <summary>
        /// Are data file and reference file the same.
        /// </summary>
        public bool DataAndRefNotSame
        {
            get { return DataFile == ReferenceFiles; }
        }

        /// <summary>
        /// Average or kth nearest neighbor.
        /// </summary>
        public int ScoreF { get; set; }

        /// <summary>
        /// Default weighting or weight file.
        /// </summary>
        public int DistF { get; set; }

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

        private int _startBatchSize;
        public int StartBatchSize
        {
            get { return _startBatchSize; }
            set
            {
                Contract.Requires<ArgumentOutOfRangeException>(value > 0);
                _startBatchSize = value;
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
            ScoreF = 0; // average
            DistF = 1; // weighted
            NumOutliers = 30;
            K = 5;
            Cutoff = 0;

            // computation parameters
            StartBatchSize = 1000;
            BatchSize = 1000;

            // misc parameters
            RecordNeighbors = false;
            MissingR = float.NaN;
            DistMR = 0.4f;
        }
    }
}
