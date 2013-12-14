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
        private ClusterDistanceMetric _distance;
        private DistanceMetric _metric;

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

        public ClusterDatabase(double eps, Weights weights, ClusterDistanceMetric distanceFunc, DistanceMetric metric)
        {
            Contract.Requires<ArgumentOutOfRangeException>(eps >= 0);
            Contract.Requires<ArgumentNullException>(weights != null);
            Contract.Requires<ArgumentNullException>(distanceFunc != null);
            Contract.Requires<ArgumentNullException>(metric != null);

            _eps = eps;
            _weights = weights;
            _distance = distanceFunc;
            _metric = metric;
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
        /// Finds closest cluster from specified record.
        /// </summary>
        private Cluster FindClosest(Record record, out double dist)
        {
            dist = double.PositiveInfinity;
            Cluster closest = null;
            foreach (var cluster in _clusters)
            {
                double d = _distance(cluster, record, _weights, _metric);
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
