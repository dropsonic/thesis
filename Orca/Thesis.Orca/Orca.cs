using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;
using System.Diagnostics.Contracts;
using Thesis.Collections;
using System.Diagnostics;

namespace Thesis.Orca
{
    public class Orca
    {
        public Parameters Parameters { get; set; }

        public Orca()
        {
            Parameters = new Parameters();
        }

        public Orca(Parameters parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null);

            Parameters = parameters;
        }

        /// <param name="dataFile">Binary file with input data from DPrep.</param>
        /// <param name="returnAll">If true, returns score info for all records in input data.</param>
        /// <returns></returns>
        public IEnumerable<Outlier> Run(string dataFile, bool returnAll = false)
        {
            // Test cases
            using (BatchInFile batchInFile = new BatchInFile(dataFile, 
                                                      Parameters.BatchSize))
            // Reference database (in this case - whole input data)
            using (OrcaBinaryReader inFile = new OrcaBinaryReader(dataFile))
            {
                List<Outlier> outliers = new List<Outlier>();
                bool done = false;
                double cutoff = Parameters.Cutoff;

                //-----------------------
                // run the outlier search 
                //
                done = !batchInFile.GetNextBatch(); //start batch
                while (!done)
                {
                    Trace.PrintRecords(batchInFile.CurrentBatch);

                    var o = FindOutliers(batchInFile, inFile, cutoff);
                    outliers.AddRange(o);

                    inFile.SeekPosition(0);

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

        private IList<Outlier> FindOutliers(BatchInFile batchFile, OrcaBinaryReader inFile, double cutoff)
        {
            Contract.Requires<ArgumentNullException>(batchFile != null);
            Contract.Requires<ArgumentNullException>(inFile != null);

            int k = Parameters.NeighborsCount; // number of neighbors
            
            var records = new List<Record>(batchFile.CurrentBatch);
            int batchRecCount = records.Count;
            int recCount = inFile.RecordsCount;


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
            for (int i = 0; i < recCount; i++)
            {
                Record descRecord = inFile.GetNextRecord();

                neighborsDist_i = 0;
                minkDist_i = 0;
                candidates_i = 0;

                for (int j = 0; j < batchRecCount; j++)
                {
                    double dist = Parameters.DistanceFunction(records[j], descRecord, inFile.Weights);

                    if (dist < minkDist[minkDist_i])
                    {
                        if (batchFile.Offset + candidates[candidates_i] != i)
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
                                    i = recCount; // exit top loop and return
                            }
                        }
                    }

                    neighborsDist_i++;
                    minkDist_i++;
                    candidates_i++;
                }

                Trace.Message(String.Format("Offset: {0} | Ref #{1} processed.", batchFile.Offset, i));
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
