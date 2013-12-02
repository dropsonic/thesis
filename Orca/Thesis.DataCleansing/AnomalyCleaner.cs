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

        /// <param name="analizedPart">Part of input data to be loaded in memory and analized.</param>
        public AnomalyCleaner(IDataReader input, IDataWriter output, IAnomaliesFilter filter)
        {
            Contract.Requires<ArgumentNullException>(input != null);
            Contract.Requires<ArgumentNullException>(output != null);
            Contract.Requires<ArgumentNullException>(filter != null);

            _filter = filter;
            _input = input;
            _output = output;
        }

        private IEnumerable<Outlier> FindOutliers()
        {
            string orcaFile = "input.bin";
            var dprepParams = new DPrep.Parameters();
            var dprep = new DPrep.DPrep(_input, orcaFile, dprepParams);
            int count = dprep.Run();

            var orcaParams = new Orca.Parameters(orcaFile) 
            { 
                BatchSize = 10000, 
                NumOutliers = count,
                ScoreFunction = Orca.ScoreFunctions.Average
            };

            var orca = new Orca.Orca(orcaParams);
            return orca.Run();
        }

        public void Clean()
        {
            var outliers = FindOutliers();
            var anomaliesId = _filter.Filter(outliers).Select(o => o.Record.Id);
            foreach (var anomaly in anomaliesId)
                _output.DeleteRecord(anomaly);
        }
    }
}
