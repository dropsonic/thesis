﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;
using System.Diagnostics.Contracts;

namespace Thesis.Orca
{
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
            while (!done)
            {
                Record[] records;
                done = !batchInFile.GetNextBatch(out records);

                int count = outliers.Count;

                var o = FindOutliers(records, inFile, Parameters.K);
                outliers.AddRange(o);

                inFile.SeekPosition(0);

                //-------------------------------
                // sort the current best outliers 
                // and keep the best
                //
                outliers.Sort();
                int numOutliers = Parameters.NumOutliers;
                if (outliers.Count > numOutliers)
                {
                    if (outliers[numOutliers - 1].Score > cutoff)
                    {
                        // New cutoff
                        cutoff = outliers[numOutliers - 1].Score;
                    }
                }
            }

            return outliers;
        }

        private IEnumerable<Outlier> FindOutliers(Record[] records, BinaryInFile inFile, int k)
        {
            Contract.Requires<ArgumentNullException>(records != null);
            Contract.Requires<ArgumentNullException>(inFile != null);
            Contract.Requires<ArgumentOutOfRangeException>(k > 0);

            int batchRecCount = records.Length;
            int recCount = inFile.RecordsCount;
            //var kdist = new List<double[]>();

            // vectors to store distance of nearest neighbors
            var kDist = new double[batchRecCount, k];
            // initialize distance score with max distance
            for (int i = 0; i < kDist.GetLength(0); i++)
                for (int j = 0; j < kDist.GetLength(1); j++)
                    kDist[i, j] = double.MaxValue;

            // vector to store furthest nearest neighbour
            var minkDist = new double[batchRecCount];

            // candidates stores the integer index
            var candidates = new int[batchRecCount];
            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = i;

            // loop over objects in reference table
            for (int i = 0; i < recCount; i++)
            {
                Record descRecord = inFile.GetNextRecord();

                foreach (var record in records)
                {
                    //double dist = Distance();
                }
            }

            throw new NotImplementedException();
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
