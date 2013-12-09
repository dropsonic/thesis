using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DataCleansing
{
    /// <summary>
    /// Filters anomalies which score (reduced by min score) is greater then threshold.
    /// </summary>
    public class ThresholdFilter : IAnomaliesFilter
    {
        private double _threshold;

        public ThresholdFilter(double threshold)
        {
            Contract.Requires<ArgumentOutOfRangeException>(threshold >= 0);

            _threshold = threshold;
        }

        public IEnumerable<Outlier> Filter(IEnumerable<Outlier> outliers)
        {
            double min = outliers.Min().Score;
            foreach (var outlier in outliers)
            {
                if (outlier.Score - min > _threshold)
                    yield return outlier;
            }
        }
    }
}
