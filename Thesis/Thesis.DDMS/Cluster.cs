using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class Cluster
    {
        public float[] UpperBound { get; set; }
        public float[] LowerBound { get; set; }

        /// <summary>
        /// Appropriate discrete values for this cluster.
        /// </summary>
        public HashSet<int>[] DiscreteValues { get; set; }

        internal Cluster(Record init)
        {
            LowerBound = (float[])init.Real.Clone();
            UpperBound = (float[])init.Real.Clone();
            DiscreteValues = init.Discrete.Select(i =>
                {
                    var hs = new HashSet<int>();
                    hs.Add(i);
                    return hs;
                }).ToArray();
        }

        /// <summary>
        /// Determines if record lies inside cluster boundaries.
        /// </summary>
        internal bool Contains(Record record)
        {
            int realFieldsCount = UpperBound.Length;
            for (int i = 0; i < realFieldsCount; i++)
            {
                if (record.Real[i] > UpperBound[i])
                    return false;
                if (record.Real[i] < LowerBound[i])
                    return false;
            }

            int discreteFieldsCount = DiscreteValues.Length;
            for (int i = 0; i < discreteFieldsCount; i++)
            {
                if (!DiscreteValues[i].Contains(record.Discrete[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Adds record to the cluster (expands cluster bounds, if necessary).
        /// </summary>
        internal void Add(Record record)
        {
            // Expand cluster bounds
            int realFieldsCount = UpperBound.Length;
            for (int i = 0; i < realFieldsCount; i++)
            {
                if (record.Real[i] > UpperBound[i])
                    UpperBound[i] = record.Real[i];
                else if (record.Real[i] < LowerBound[i])
                    LowerBound[i] = record.Real[i];
            }

            // Add discrete values to cluster appropriate values
            int discreteFieldsCount = DiscreteValues.Length;
            for (int i = 0; i < discreteFieldsCount; i++)
                DiscreteValues[i].Add(record.Discrete[i]);
        }
    }
}
