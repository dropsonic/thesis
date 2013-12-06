using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    class ClusterDatabase
    {
        private IList<Cluster> _cluster = new List<Cluster>();
        private double _eps;

        public ClusterDatabase(double eps)
        {
            _eps = eps;
        }

        public void AddRecord(Record record)
        {

        }
    }
}
