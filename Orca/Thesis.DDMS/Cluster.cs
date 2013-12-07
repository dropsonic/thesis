using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    class ClusterBoundary
    {
        public float[] Real { get; set; }
        public HashSet<int>[] Discrete { get; set; }

        public ClusterBoundary(Record init)
        {
            Real = (float[])init.Real.Clone();
            int discreteFieldsCount = init.Discrete.Length;
            Discrete = new HashSet<int>[discreteFieldsCount];
            for (int i = 0; i < discreteFieldsCount; i++)
            {
                Discrete[i] = new HashSet<int>();
                Discrete[i].Add(init.Discrete[i]);
            }
        }
    }

    class Cluster
    {
        public ClusterBoundary UpperBound { get; set; }
        public ClusterBoundary LowerBound { get; set; }


        public Cluster(Record init)
        {
            LowerBound = new ClusterBoundary(init);
            UpperBound = new ClusterBoundary(init);
        }

        public bool Contains(Record record)
        {
            int realFieldsCount = UpperBound.Real.Length;
            for (int i = 0; i < realFieldsCount; i++)
            {
                if (record.Real[i] > UpperBound.Real[i])
                    return false;
                if (record.Real[i] < LowerBound.Real[i])
                    return false;
            }


            return true;
        }
    }
}
