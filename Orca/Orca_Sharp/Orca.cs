using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

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

                var o = FindOutliers(records, inFile);
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

        private IEnumerable<Outlier> FindOutliers(Record[] records, BinaryInFile inFile)
        {
            
            throw new NotImplementedException();
        }
    }
}
