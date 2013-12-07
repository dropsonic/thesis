using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    class ClusterDatabase
    {
        private List<Cluster> _clusters = new List<Cluster>();
        private double _eps;
        private Weights _weights;
        private Func<Record, Record, Weights, double> _distance;

        private bool IsEmpty
        {
            get { return _clusters.Count == 0; }
        }

        public int Size
        {
            get { return _clusters.Count; }
        }

        public IReadOnlyCollection<Cluster> Clusters
        {
            get { return _clusters.AsReadOnly(); }
        }

        public ClusterDatabase(double eps, Weights weights, Func<Record, Record, Weights, double> distanceFunc)
        {
            Contract.Requires<ArgumentOutOfRangeException>(eps >= 0);
            Contract.Requires<ArgumentNullException>(weights != null);
            Contract.Requires<ArgumentNullException>(distanceFunc != null);

            _eps = eps;
            _weights = weights;
            _distance = distanceFunc;
        }

        public void AddRecord(Record record)
        {
            if (IsEmpty) // if cluster database empty
            {
                // form input vector into cluster and insert into cluster database
                AddCluster(new Cluster(record));
            }
            else
            {
                // if record is inside at least one cluster, do nothing;
                // else:
                if (!_clusters.Any(c => c.Contains(record)))
                {
                    double dist;
                    Cluster closest = FindClosest(record, out dist);
                    // if distance to the closest cluster is greater then epsilon
                    if (dist > _eps)
                        // add new cluster initialized by this record
                        AddCluster(new Cluster(record));
                    else
                        // add record to the closest cluster
                        closest.Add(record);
                }
            }
        }

        /// <summary>
        /// Determines if record is inside or close enough
        /// to at least one cluster in database.
        /// </summary>
        public bool Contains(Record record)
        {
            // if distance to the closest cluster is less then epsilon, return true
            return Distance(record) <= _eps;
        }

        /// <summary>
        /// Calculates distance from record to the closest cluster. 
        /// </summary>
        public double Distance(Record record)
        {
            if (record == null)
                return double.PositiveInfinity;

            // if record is inside at least one cluster
            if (_clusters.Any(c => c.Contains(record)))
                return 0;

            double dist;
            Cluster closest = FindClosest(record, out dist);
            return dist;
        }

        private void AddCluster(Cluster cluster)
        {
            if (cluster != null)
                _clusters.Add(cluster);
        }

        /// <summary>
        /// Calculates distance between record and cluster.
        /// </summary>
        private double Distance(Cluster cluster, Record record)
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

            return _distance(center, record, _weights);
        }

        /// <summary>
        /// Finds closest cluster from specified record.
        /// </summary>
        private Cluster FindClosest(Record record, out double dist)
        {
            dist = double.PositiveInfinity;
            Cluster closest = null;
            foreach (var cluster in _clusters)
            {
                double d = Distance(cluster, record);
                if (d < dist)
                {
                    dist = d;
                    closest = cluster;
                }
            }

            return closest;
        }
    }
}
