using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DataCleansing
{
    public class DifferenceFilter : IAnomaliesFilter
    {
        private double _delta;

        public DifferenceFilter(double delta)
        {
            Contract.Requires<ArgumentOutOfRangeException>(delta >= 0);

            _delta = delta;
        }

        public IEnumerable<Outlier> Filter(IEnumerable<Outlier> outliers)
        {
            Contract.Requires<ArgumentNullException>(outliers != null);

            var o = outliers.ToArray();
            if (o.Length < 2)
                return Enumerable.Empty<Outlier>();

            for (int i = o.Length - 1; i > 0; i--)
            {
                double diff = o[i-1].Score - o[i].Score;
                if (diff > _delta)
                    return o.Take(i);
            }

            return Enumerable.Empty<Outlier>();
        }
    }
}
