using System;
using System.Collections.Generic;
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
        ClusterDatabase _db;

        public Regime(double eps, IDataReader records, Func<Record, Record, Weights, double> distanceFunc)
        {
            _db = new ClusterDatabase(eps, records.Fields.Weights(), distanceFunc);
            foreach (var record in records)
                _db.AddRecord(record);
            
        }

        public bool Contains(Record record)
        {
            return _db.Contains(record);
        }
    }
}
