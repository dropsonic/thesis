using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    public delegate double ClusterDistanceMetric(Cluster c, Record x, Weights weights, DistanceMetric metric);
}
