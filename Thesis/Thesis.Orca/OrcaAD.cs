using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Thesis.Collections;
using System.Diagnostics;

namespace Thesis.Orca
{
    public class OrcaAD
    {
        private Func<IEnumerable<double>, double> _scoreFunction;
        private DistanceMetric _distanceFunction;
        private int _numOutliers;
        private int _neighborsCount;
        private double _cutoff;
        private int _batchSize;

        #region Constructors
        /// <param name="distanceFunction">Distance function for calculating the distance between two examples with weights.</param>
        /// <param name="numOutliers">Number of outliers.</param>
        /// <param name="neighborsCount"></param>
        /// <param name="cutoff"></param>
        /// <param name="batchSize"></param>
        public OrcaAD(DistanceMetric distanceFunction,
            int numOutliers = 30, int neighborsCount = 5,
            double cutoff = 0, int batchSize = 1000)
            : this(ScoreFunctions.Average, distanceFunction, numOutliers,
                neighborsCount, cutoff, batchSize)
        { }

        /// <param name="scoreFunction">Distance score function.</param>
        /// <param name="numOutliers">Number of outliers.</param>
        /// <param name="neighborsCount"></param>
        /// <param name="cutoff"></param>
        /// <param name="batchSize"></param>
        public OrcaAD(Func<IEnumerable<double>, double> scoreFunction,
            int numOutliers = 30, int neighborsCount = 5,
            double cutoff = 0, int batchSize = 1000)
            : this(scoreFunction, DistanceFunctions.SqrEuсlid, numOutliers,
                neighborsCount, cutoff, batchSize)
        { }

        /// <param name="numOutliers">Number of outliers.</param>
        /// <param name="neighborsCount"></param>
        /// <param name="cutoff"></param>
        /// <param name="batchSize"></param>
        public OrcaAD(int numOutliers = 30, int neighborsCount = 5,
            double cutoff = 0, int batchSize = 1000)
            : this(ScoreFunctions.Average, DistanceFunctions.SqrEuсlid, numOutliers,
                neighborsCount, cutoff, batchSize)
        { }

        /// <param name="scoreFunction">Distance score function.</param>
        /// <param name="distanceFunction">Distance function for calculating the distance between two examples with weights.</param>
        /// <param name="numOutliers">Number of outliers.</param>
        /// <param name="neighborsCount"></param>
        /// <param name="cutoff"></param>
        /// <param name="batchSize"></param>
        public OrcaAD(Func<IEnumerable<double>, double> scoreFunction,
            DistanceMetric distanceFunction,
            int numOutliers = 30, int neighborsCount = 5, 
            double cutoff = 0, int batchSize = 1000)
        {
            Contract.Requires<ArgumentNullException>(scoreFunction != null);
            Contract.Requires<ArgumentNullException>(distanceFunction != null);
            Contract.Requires<ArgumentOutOfRangeException>(numOutliers > 0);
            Contract.Requires<ArgumentOutOfRangeException>(neighborsCount > 0);
            Contract.Requires<ArgumentOutOfRangeException>(cutoff >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(batchSize > 0);

            _scoreFunction = scoreFunction;
            _distanceFunction = distanceFunction;
            _numOutliers = numOutliers;
            _neighborsCount = neighborsCount;
            _cutoff = cutoff;
            _batchSize = batchSize;
        }
        #endregion

        /// <param name="cases">Data reader for input data.</param>
        /// <param name="references">Data reader for reference data (can't be the same reader object).</param>
        /// <param name="returnAll">If true, returns score info for all records in input data.</param>
        /// <returns></returns>
        public IEnumerable<Outlier> Run(IDataReader cases, IDataReader references, bool returnAll = false)
        {
            Contract.Requires<ArgumentNullException>(cases != null);
            Contract.Requires<ArgumentNullException>(references != null);
            Contract.Requires<ArgumentException>(!object.ReferenceEquals(cases, references));

            // Test cases
            using (BatchDataReader batchInFile = new BatchDataReader(cases, _batchSize))
            // Reference database
            {
                List<Outlier> outliers = new List<Outlier>();
                bool done = false;
                double cutoff = _cutoff;
                Weights weights = cases.Fields.Weights();

                //-----------------------
                // run the outlier search 
                //
                done = !batchInFile.GetNextBatch(); //start batch
                while (!done)
                {
                    Trace.PrintRecords(batchInFile.CurrentBatch);

                    var o = FindOutliers(batchInFile, references, weights, cutoff);
                    outliers.AddRange(o);

                    references.Reset();

                    //-------------------------------
                    // sort the current best outliers 
                    // and keep the best
                    //
                    outliers.Sort();
                    outliers.Reverse(); // sorting in descending order
                    int numOutliers = _numOutliers;
                    if (outliers.Count > numOutliers &&
                        outliers[numOutliers - 1].Score > cutoff)
                    {
                        // New cutoff
                        cutoff = outliers[numOutliers - 1].Score;
                    }
                    done = !batchInFile.GetNextBatch();
                }

                return returnAll ? outliers : outliers.Take(_numOutliers);
            }
        }

        private IList<Outlier> FindOutliers(BatchDataReader cases, IDataReader references, Weights weights, double cutoff)
        {
            Contract.Requires<ArgumentNullException>(cases != null);
            Contract.Requires<ArgumentNullException>(references != null);

            int k = _neighborsCount; // number of neighbors
            
            var records = new List<Record>(cases.CurrentBatch);
            int batchRecCount = records.Count;


            // distance to neighbors — Neighbors(b) in original description
            var neighborsDist = new List<NeighborsDistance>(batchRecCount);
            // initialize distance score with max distance
            for (int i = 0; i < batchRecCount; i++)
            {
                var kDistDim = new NeighborsDistance() 
                { 
                    Record = records[i],
                    Distances = new BinaryHeap<double>(k) 
                };
                for (int j = 0; j < k; j++)
                    kDistDim.Distances.Push(double.MaxValue);
                neighborsDist.Add(kDistDim);
            }

            // vector to store furthest nearest neighbour
            var minkDist = new List<double>(batchRecCount);
            for (int i = 0; i < neighborsDist.Count; i++)
                minkDist.Add(double.MaxValue);

            // candidates stores the integer index
            var candidates = Enumerable.Range(0, batchRecCount).ToList();

            int neighborsDist_i;
            int minkDist_i;
            int candidates_i;

            // loop over objects in reference table
            foreach (var descRecord in references)
            {
                neighborsDist_i = 0;
                minkDist_i = 0;
                candidates_i = 0;

                for (int j = 0; j < batchRecCount; j++)
                {
                    double dist = _distanceFunction(records[j], descRecord, weights);

                    if (dist < minkDist[minkDist_i])
                    {
                        if (cases.Offset + candidates[candidates_i] != references.Index - 1)
                        {
                            BinaryHeap<double> kvec = neighborsDist[neighborsDist_i].Distances;
                            kvec.Push(dist);
                            kvec.Pop();
                            minkDist[minkDist_i] = kvec.Peek();

                            double score = _scoreFunction(kvec);

                            if (score <= cutoff)
                            {
                                candidates.RemoveAt(candidates_i--);
                                records.RemoveAt(j--); batchRecCount--;
                                neighborsDist.RemoveAt(neighborsDist_i--);
                                minkDist.RemoveAt(minkDist_i--);

                                if (candidates.Count == 0)
                                    break;
                            }
                        }
                    }

                    neighborsDist_i++;
                    minkDist_i++;
                    candidates_i++;
                }

                if (candidates.Count == 0)
                    break;

                Trace.Message(String.Format("Offset: {0} | Ref #{1} processed.", cases.Offset, references.Index));
            }

            //--------------------------------
            // update the list of top outliers 
            // 
            candidates_i = 0;
            List<Outlier> outliers = new List<Outlier>();

            foreach (var point in neighborsDist)
            {
                Outlier outlier = new Outlier();
                //outlier.Record = point.Record;
                outlier.Id = point.Record.Id;
                outlier.Score = _scoreFunction(point.Distances);
                outliers.Add(outlier);
            }

            return outliers;
        }
    }
}
