using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    public static class ClusterDistances
    {
        /// <summary>
        /// Distance from record to the center of cluster.
        /// </summary>
        public static double CenterDistance(Cluster cluster, Record record, Weights weights, DistanceMetric metric)
        {
            int realFieldsCount = record.Real.Length;
            int discreteFieldsCount = record.Discrete.Length;

            // Calculate center of the cluster
            Record center = new Record();
            center.Real = new float[realFieldsCount];
            center.Discrete = new int[discreteFieldsCount];
            for (int i = 0; i < realFieldsCount; i++)
                center.Real[i] = (cluster.LowerBound[i] + cluster.UpperBound[i]) / 2;
            for (int i = 0; i < discreteFieldsCount; i++)
                // if cluster contains value, keep it the same as in record
                center.Discrete[i] = cluster.DiscreteValues[i].Contains(record.Discrete[i]) ? record.Discrete[i] : -1;

            return metric(center, record, weights);
        }

        /// <summary>
        /// Distance from record to the nearest cluster boundary line.
        /// </summary>
        public static double NearestBoundDistance(Cluster cluster, Record record, Weights weights, DistanceMetric metric)
        {
            int realFieldsCount = record.Real.Length;
            int discreteFieldsCount = record.Discrete.Length;

            // Calculate nearest boundary of the cluster
            Record nearestBound = new Record();
            nearestBound.Real = new float[realFieldsCount];
            nearestBound.Discrete = new int[discreteFieldsCount];
            for (int i = 0; i < realFieldsCount; i++)
            {
                if (record.Real[i] >= cluster.LowerBound[i])
                {
                    if (record.Real[i] <= cluster.UpperBound[i])
                        nearestBound.Real[i] = record.Real[i];
                    else
                        nearestBound.Real[i] = cluster.UpperBound[i];
                }
                else
                {
                    nearestBound.Real[i] = cluster.LowerBound[i];
                }
            }
            for (int i = 0; i < discreteFieldsCount; i++)
                // if cluster contains value, keep it the same as in record
                nearestBound.Discrete[i] = cluster.DiscreteValues[i].Contains(record.Discrete[i]) ? record.Discrete[i] : -1;

            return metric(nearestBound, record, weights);
        }
    }
}
