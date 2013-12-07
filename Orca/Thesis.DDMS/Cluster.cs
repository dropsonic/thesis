using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    class Cluster
    {
        public Record UpperBound { get; set; }
        public Record LowerBound { get; set; }

        public Cluster(Record init)
        {
            UpperBound = init.Clone();
            LowerBound = init.Clone();
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
