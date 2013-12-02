using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPrep = Thesis.DPrep;
using Orca = Thesis.Orca;

namespace Thesis.DataCleansing
{
    public class AnomalyCleaner
    {
        IAnomaliesFilter _filter;
        IDataReader _input;
        IDataWriter _output;
        float _outliersPart;

        /// <param name="analizedPart">Part of input data to be loaded in memory and analized.</param>
        public AnomalyCleaner(IDataReader input, IDataWriter output, IAnomaliesFilter filter, float analizedPart = 0.05f)
        {
            Contract.Requires<ArgumentNullException>(input != null);
            Contract.Requires<ArgumentNullException>(output != null);
            Contract.Requires<ArgumentNullException>(filter != null);

            _filter = filter;
            _input = input;
            _output = output;
            _outliersPart = outliersPart;
        }

        private IList<Outlier> FindOutliers()
        {
            string orcaFile = "input.bin";
            var dprepParams = new DPrep.Parameters();
            var dprep = new DPrep.DPrep(_input, orcaFile, dprepParams);
            int count = dprep.Run();

            var orcaParams = new Orca.Parameters(orcaFile) 
            { 
                BatchSize = 10000, 
                NumOutliers = (int)Math.Round(count * _outliersPart), 
                ScoreFunction = Orca.ScoreFunctions.Average
            };
        }

        public void Clean()
        {
            //var anomalies = _filter.Filter(
        }
    }
}
