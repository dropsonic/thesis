using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca;

namespace Thesis.DataCleansing
{
    /// <summary>
    /// Filters anomalies which score is greater then M+3σ.
    /// </summary>
    public class GaussianFilter : IAnomaliesFilter
    {
        public IEnumerable<Outlier> Filter(IEnumerable<Outlier> outliers)
        {
            Contract.Requires<ArgumentNullException>(outliers != null);

            var anomalies = new List<Outlier>();
            double cutoff = GetCutoff(outliers);
            foreach (var outlier in outliers)
            {
                if (outlier.Score > cutoff)
                    anomalies.Add(outlier);
            }
            return anomalies;
        }

        private IDictionary<double, double> GetFrequencies(IList<Outlier> outliers)
        {
            return outliers.GroupBy(o => o.Score)
                           .ToDictionary(gr => gr.Key,
                                         gr => (double)gr.Count() / (double)outliers.Count);
        }

        private double GetCutoff(IEnumerable<Outlier> outliers)
        {
            var freq = GetFrequencies(outliers.ToList());
            double mean, disp;
            GetStatValues(freq, out mean, out disp);
            double cutoff = mean + 3 * Math.Sqrt(disp);
            return cutoff;
        }

        /// <summary>
        /// Calculates mean value and dispersion by one pass through frequencies dictionary.
        /// </summary>
        private void GetStatValues(IDictionary<double, double> freq, out double mean, out double dispersion)
        {
            mean = 0;
            dispersion = 0;
            foreach (var f in freq)
            {
                double x = f.Key * f.Value;
                mean += x;
                dispersion += x * f.Key;
            }

            dispersion -= mean * mean;
        }

        // Slow
        //private double Mean(IDictionary<double, double> freq)
        //{
        //    return freq.Select(v => v.Key * v.Value).Sum();
        //}

        //private double Dispersion(IDictionary<double, double> freq, double mean)
        //{
        //    return freq.Select(v => v.Key * v.Key * v.Value).Sum()
        //           - Math.Pow(Mean(freq), 2);
        //}
    }
}
