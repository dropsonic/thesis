﻿using System;
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
    static class Trace
    {
        [Conditional("DEBUG")]
        public static void PrintRecords(IEnumerable<Record> records)
        {
            Console.WriteLine("-----------TRACE-----------");
            IEnumerable<Record> pRecords = records.Count() <= 10 ? records :
                records.Take(10);
            foreach (var record in pRecords)
            {
                Console.Write("#{0}: ", record.Id);
                foreach (var real in record.Real)
                    Console.Write("{0} ", real);
                Console.Write("| ");
                foreach (var discrete in record.Discrete)
                    Console.Write("{0} ", discrete);
                Console.WriteLine();
            }
            Console.WriteLine("-----------END TRACE-----------");
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Console.WriteLine("TRACE: " + message);
        }
    }

    public class Orca
    {
        public Parameters Parameters { get; set; }

        public Orca(Parameters parameters)
        {
            Parameters = parameters;
        }

        public IEnumerable<Outlier> Run()
        {
            // Test cases
            BatchInFile batchInFile = new BatchInFile(Parameters.DataFile, 
                                                      Parameters.BatchSize);
            // Reference database (in this case - whole input data)
            BinaryInFile inFile = new BinaryInFile(Parameters.DataFile);

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

            return outliers;
        }

        private IList<Outlier> FindOutliers(BatchInFile batchFile, BinaryInFile inFile, double cutoff)
        {
            Contract.Requires<ArgumentNullException>(batchFile != null);
            Contract.Requires<ArgumentNullException>(inFile != null);

            int k = Parameters.K;
            
            var records = new List<Record>(batchFile.CurrentBatch);
            int batchRecCount = records.Count;
            int recCount = inFile.RecordsCount;


            // distance to neighbors — Neighbors(b) in original description
            var kDist = new List<BinaryHeap<double>>(batchRecCount);
            // initialize distance score with max distance
            for (int i = 0; i < batchRecCount; i++)
            {
                var kDistDim = new BinaryHeap<double>(k);
                for (int j = 0; j < k; j++)
                    kDistDim.Push(double.MaxValue);
                    //kDistDim.Push(10000000000.0);
                kDist.Add(kDistDim);
            }

            // vector to store furthest nearest neighbour
            var minkDist = new List<double>(batchRecCount);
            for (int i = 0; i < kDist.Count; i++)
                minkDist.Add(double.MaxValue);
                //minkDist.Add(10000000000.0);

            // candidates stores the integer index
            var candidates = Enumerable.Range(0, batchRecCount).ToList();

            int kDist_i;
            int minkDist_i;
            int candidates_i;

            // loop over objects in reference table
            for (int i = 0; i < recCount; i++)
            {
                Record descRecord = inFile.GetNextRecord();

                kDist_i = 0;
                minkDist_i = 0;
                candidates_i = 0;

                for (int j = 0; j < batchRecCount; j++)
                {
                    double dist = Distance(records[j], descRecord, inFile.Weights);

                    if (dist < minkDist[minkDist_i])
                    {
                        if (batchFile.Offset + candidates[candidates_i] != i)
                        {
                            BinaryHeap<double> kvec = kDist[kDist_i];
                            kvec.Push(dist);
                            kvec.Pop();
                            minkDist[minkDist_i] = kvec.Peek();

                            double score = 0;
                            switch (Parameters.ScoreF)
                            {
                                case Parameters.DistanceType.Average:
                                    for (int it = 0; it < k; it++)
                                        score += kvec[it];
                                    break;
                                case Parameters.DistanceType.KthNeighbor:
                                    score = kvec.Peek();
                                    break;
                            }

                            if (score <= cutoff)
                            {
                                candidates.RemoveAt(candidates_i--);
                                records.RemoveAt(j--); batchRecCount--;
                                kDist.RemoveAt(kDist_i--);
                                minkDist.RemoveAt(minkDist_i--);

                                if (candidates.Count == 0)
                                    i = recCount; // exit top loop and return
                            }
                        }
                    }

                    kDist_i++;
                    minkDist_i++;
                    candidates_i++;
                }

                Trace.Message(String.Format("Record #{0} processed.", i));
            }

            //--------------------------------
            // update the list of top outliers 
            // 
            candidates_i = 0;
            List<Outlier> outliers = new List<Outlier>();

            foreach (var kvec in kDist)
            {
                double sum = 0;
                switch (Parameters.ScoreF)
                {
                    case Parameters.DistanceType.Average:
                        for (int j = 0; j < kvec.Count; j++)
                            sum += kvec[j];
                        break;
                    case Parameters.DistanceType.KthNeighbor:
                        sum = kvec[kvec.Count - 1];
                        break;
                }

                Outlier outlier = new Outlier();
                outlier.Index = batchFile.Offset + candidates_i++;
                outlier.Score = sum;
                outliers.Add(outlier);
            }

            return outliers;
        }

        /// <summary>
        /// Calculates the distance between two examples with weights.
        /// </summary>
        private double Distance(Record a, Record b, Weights weights)
        {
            Contract.Requires(a.Real.Length == b.Real.Length);
            Contract.Requires(a.Discrete.Length == b.Discrete.Length);
            Contract.Requires(a.Real.Length == weights.Real.Length);
            Contract.Requires(a.Discrete.Length == weights.Discrete.Length);

            int realFieldsCount = a.Real.Length;
            int discreteFieldsCount = a.Discrete.Length;

            double d = 0;

            // real 
            for (int i = 0; i < realFieldsCount; i++)
            {
                // check for missing values
                int missingCount = 0;
                if (a.Real[i] == Parameters.MissingR)
                    missingCount++;
                if (b.Real[i] == Parameters.MissingR)
                    missingCount++;

                if (missingCount == 0)
                {
                    double diff = a.Real[i] - b.Real[i];
                    d += diff * diff * weights.Real[i];
                }
                // one value is missing
                else if (missingCount == 1)
                {
                    d += Parameters.DistMR;
                }
            }

            // discrete
            for (int i = 0; i < discreteFieldsCount; i++)
            {
                if (a.Discrete[i] != b.Discrete[i])
                    d += weights.Discrete[i];
            }

            return d;
        }
    }
}
