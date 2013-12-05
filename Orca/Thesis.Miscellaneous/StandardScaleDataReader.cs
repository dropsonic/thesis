using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Standardize real values (scales to mean and standard deviations).
    /// </summary>
    public class StandardScaleDataReader : ScaleDataReader
    {
        float[] _mean;
        float[] _std;

        public StandardScaleDataReader(IDataReader baseReader)
            : base(baseReader) { }

        protected override void GetDataProperties()
        {
            // initialize vectors 
            _mean = new float[RealFieldsCount];
            _std = new float[RealFieldsCount];
            double[] sumv = new double[RealFieldsCount];
            double[] sumsqv = new double[RealFieldsCount];
            int[] num = new int[RealFieldsCount];

            foreach (var record in this)
            {
                for (int i = 0; i < RealFieldsCount; i++)
                {
                    if (!float.IsNaN(record.Real[i]))
                    {
                        double r = ((double)record.Real[i]);
                        sumv[i] += r;
                        sumsqv[i] += r * r;
                        num[i]++;
                    }
                }

                for (int i = 0; i < RealFieldsCount; i++)
                {
                    if (num[i] > 1)
                    {
                        double meanValue = sumv[i] / num[i];
                        _mean[i] = double.IsNaN(meanValue) ? 0 : ((float)meanValue);

                        double stdValue = Math.Sqrt((sumsqv[i] - sumv[i] * sumv[i] / num[i]) / (num[i] - 1));
                        _std[i] = double.IsNaN(stdValue) ? 0 : ((float)stdValue);
                    }
                    else
                    {
                        _mean[i] = 0;
                        _std[i] = 0;
                    }
                }
            }
        }

        protected override void ScaleRecord(Record record)
        {
            for (int i = 0; i < RealFieldsCount; i++)
                record.Real[i] = _std[i] == 0 ? 0 : (record.Real[i] - _mean[i]) / _std[i];
        }
    }
}
