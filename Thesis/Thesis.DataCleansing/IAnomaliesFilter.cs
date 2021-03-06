﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca;

namespace Thesis.DataCleansing
{
    public interface IAnomaliesFilter
    {
        /// <summary>
        /// Return anomalies in all outliers.
        /// </summary>
        IEnumerable<Outlier> Filter(IEnumerable<Outlier> outliers);
    }
}
