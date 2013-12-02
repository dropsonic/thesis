using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca;

namespace Thesis.DataCleansing
{
    public class AnomalyCleaner
    {
        IAnomaliesFilter _filter;

        public AnomalyCleaner(IAnomaliesFilter filter)
        {
            _filter = filter;
        }
    }
}
