using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DDMS
{
    /// <summary>
    /// Represents all regimes of work for the system.
    /// </summary>
    public class SystemModel
    {
        private Func<Record, Record, Weights, double> _distanceFunc;
        private List<Regime> _regimes = new List<Regime>();
        private double _eps;

        public IReadOnlyCollection<Regime> Regimes
        {
            get { return _regimes.AsReadOnly(); }
        }

        public SystemModel(double eps)
            : this(eps, DistanceFunctions.Euclid)
        { }

        public SystemModel(double eps, Func<Record, Record, Weights, double> distanceFunc)
        {
            Contract.Requires<ArgumentOutOfRangeException>(eps >= 0);
            Contract.Requires<ArgumentNullException>(distanceFunc != null);

            _distanceFunc = distanceFunc;
            _eps = eps;
        }

        public void AddRegime(string name, IDataReader records)
        {
            Contract.Requires<ArgumentNullException>(records != null);

            Regime regime = new Regime(name, _eps, records, _distanceFunc);
            _regimes.Add(regime);
        }

        /// <summary>
        /// Detects closest regime to the record.
        /// Returns null, if record is further then epsilon from all regimes.
        /// </summary>
        public Regime DetectRegime(Record record)
        {
            double mind = double.PositiveInfinity;
            Regime closestRegime = null;

            foreach (var regime in _regimes)
            {
                double d = regime.Distance(record);
                if (d < mind)
                {
                    mind = d;
                    closestRegime = regime;
                }
            }

            return mind > _eps ? null : closestRegime;
        }
    }
}
