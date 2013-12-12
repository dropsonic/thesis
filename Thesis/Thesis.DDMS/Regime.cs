using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    /// <summary>
    /// Regime of the system.
    /// </summary>
    public class Regime
    {
        private string _name;
        private ClusterDatabase _db;

        public IReadOnlyCollection<Cluster> Clusters
        {
            get { return _db.Clusters; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Regime(string name, double eps, IDataReader records, ClusterDistanceMetric distanceFunc, DistanceMetric metric)
        {
            Contract.Requires<ArgumentOutOfRangeException>(eps >= 0);
            Contract.Requires<ArgumentNullException>(records != null);
            Contract.Requires<ArgumentNullException>(distanceFunc != null);
            Contract.Requires<ArgumentNullException>(metric != null);

            _name = name;
            _db = new ClusterDatabase(eps, records.Fields.Weights(), distanceFunc, metric);
            foreach (var record in records)
                _db.AddRecord(record);
        }

        /// <summary>
        /// Determines if record is inside or close enough
        /// to at least one cluster in the regime.
        /// </summary>
        public bool Contains(Record record)
        {
            return _db.Contains(record);
        }

        /// <summary>
        /// Calculates distance from record to the regime. 
        /// </summary>
        public double Distance(Record record)
        {
            return _db.Distance(record);
        }
    }
}
