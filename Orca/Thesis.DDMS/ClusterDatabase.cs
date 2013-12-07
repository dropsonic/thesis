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
        private IList<Cluster> _clusters = new List<Cluster>();
        private double _eps;
        private Func<Record, Record, Weights, double> _distanceFunc;

        private bool IsEmpty
        {
            get { return _clusters.Count == 0; }
        }

        public ClusterDatabase(double eps, Func<Record, Record, Weights, double> distanceFunc)
        {
            Contract.Requires<ArgumentOutOfRangeException>(eps >= 0);
            Contract.Requires<ArgumentNullException>(distanceFunc != null);

            _eps = eps;
        }

        public void AddRecord(Record record)
        {
            if (IsEmpty) // if cluster database empty
                // form input vector into cluster and insert into cluster database
                AddCluster(new Cluster(record));
        }

        private void AddCluster(Cluster cluster)
        {
            if (cluster != null)
                _clusters.Add(cluster);
        }

        /// <summary>
        /// Finds closest cluster from specified record.
        /// </summary>
        private Cluster FindClosest(Record record)
        {

        }
    }
}
