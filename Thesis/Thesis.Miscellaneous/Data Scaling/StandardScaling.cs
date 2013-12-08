using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Standardize real values (scales to mean and standard deviations).
    /// </summary>
    class StandardScaling
    {
        private int _realFieldsCount;

        private float[] _mean;
        private float[] _std;

        private IDataReader _data;

        public StandardScaling(IDataReader data)
        {
            _data = data;
            _realFieldsCount = data.Fields.RealCount();

            GetDataProperties();
        }

        /// <summary>
        /// Calculates data properties (min and range).
        /// </summary>
        private void GetDataProperties()
        {
            // initialize vectors 
            _mean = new float[_realFieldsCount];
            _std = new float[_realFieldsCount];
            double[] sumv = new double[_realFieldsCount];
            double[] sumsqv = new double[_realFieldsCount];
            int[] num = new int[_realFieldsCount];

            foreach (var record in _data)
            {
                for (int i = 0; i < _realFieldsCount; i++)
                {
                    if (!float.IsNaN(record.Real[i]))
                    {
                        double r = ((double)record.Real[i]);
                        sumv[i] += r;
                        sumsqv[i] += r * r;
                        num[i]++;
                    }
                }

                for (int i = 0; i < _realFieldsCount; i++)
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

            _data.Reset();
        }

        public void Scale(Record record)
        {
            for (int i = 0; i < _realFieldsCount; i++)
                record.Real[i] = _std[i] == 0 ? 0 : (record.Real[i] - _mean[i]) / _std[i];
        }

        public void Unscale(Record record)
        {
            for (int i = 0; i < _realFieldsCount; i++)
                record.Real[i] = record.Real[i] * _std[i] + _mean[i];
        }

        public float[] Scale(float[] real)
        {
            Contract.Requires<ArgumentNullException>(real != null);
            Contract.Assert(real.Length == _realFieldsCount);

            float[] result = new float[_realFieldsCount];
            for (int i = 0; i < _realFieldsCount; i++)
                result[i] = _std[i] == 0 ? 0 : (real[i] - _mean[i]) / _std[i];

            return result;
        }

        public float[] Unscale(float[] real)
        {
            Contract.Requires<ArgumentNullException>(real != null);
            Contract.Assert(real.Length == _realFieldsCount);

            float[] result = new float[_realFieldsCount];
            for (int i = 0; i < _realFieldsCount; i++)
                result[i] = real[i] * _std[i] + _mean[i];

            return result;
        }
    }
}
