﻿using System;
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
        public Parameters Parameters { get; set; }

        public OrcaAD()
        {
            Parameters = new Parameters();
        }

        public OrcaAD(Parameters parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null);

            Parameters = parameters;
        }

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
            using (BatchDataReader batchInFile = new BatchDataReader(cases, Parameters.BatchSize))
            // Reference database
            {
                List<Outlier> outliers = new List<Outlier>();
                bool done = false;
                double cutoff = Parameters.Cutoff;
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
                    int numOutliers = Parameters.NumOutliers;
                    if (outliers.Count > numOutliers &&
                        outliers[numOutliers - 1].Score > cutoff)
                    {
                        // New cutoff
                        cutoff = outliers[numOutliers - 1].Score;
                    }
                    done = !batchInFile.GetNextBatch();
                }

                return returnAll ? outliers : outliers.Take(Parameters.NumOutliers);
            }
        }

        private IList<Outlier> FindOutliers(BatchDataReader cases, IDataReader references, Weights weights, double cutoff)
        {
            Contract.Requires<ArgumentNullException>(cases != null);
            Contract.Requires<ArgumentNullException>(references != null);

            int k = Parameters.NeighborsCount; // number of neighbors
            
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
                    double dist = Parameters.DistanceFunction(records[j], descRecord, weights);

                    if (dist < minkDist[minkDist_i])
                    {
                        if (cases.Offset + candidates[candidates_i] != references.Index - 1)
                        {
                            BinaryHeap<double> kvec = neighborsDist[neighborsDist_i].Distances;
                            kvec.Push(dist);
                            kvec.Pop();
                            minkDist[minkDist_i] = kvec.Peek();

                            double score = Parameters.ScoreFunction(kvec);

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
                outlier.Score = Parameters.ScoreFunction(point.Distances);
                outliers.Add(outlier);
            }

            return outliers;
        }
    }
}