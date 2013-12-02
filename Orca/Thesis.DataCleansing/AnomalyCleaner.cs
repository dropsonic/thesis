using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPrep = Thesis.DPrep;
using Orca = Thesis.Orca;

namespace Thesis.DataCleansing
{
    public class AnomaliesDetector
    {
        IAnomaliesFilter _filter;
        IDataReader _input;

        public AnomaliesDetector(IDataReader input, IAnomaliesFilter filter)
        {
            Contract.Requires<ArgumentNullException>(input != null);
            Contract.Requires<ArgumentNullException>(filter != null);

            _filter = filter;
            _input = input;
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
            var outliers = orca.Run();

            File.Delete(orcaFile);

            return outliers;
        }

        public IEnumerable<int> FindAnomalies()
        {
            var outliers = FindOutliers();
            return _filter.Filter(outliers).Select(o => o.Id);
        }
    }
}
