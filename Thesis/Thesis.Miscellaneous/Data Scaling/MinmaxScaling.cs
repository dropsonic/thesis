using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Scales values by min and max value (from zero to one).
    /// </summary>
    public class MinmaxScaling : IScaling
    {
        private int _realFieldsCount;

        private float[] _min;
        private float[] _range;

        private IDataReader _data;

        public MinmaxScaling(IDataReader data)
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
            _min = new float[_realFieldsCount];
            float[] max = new float[_realFieldsCount];
            _range = new float[_realFieldsCount];
            for (int i = 0; i < _realFieldsCount; i++)
            {
                _min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            foreach (var record in _data)
            {
                for (int i = 0; i < _realFieldsCount; i++)
                {
                    float value = record.Real[i];
                    if (!float.IsNaN(value))
                    {
                        if (value < _min[i])
                            _min[i] = value;
                        else if (value > max[i])
                            max[i] = value;
                    }
                }
            }

            // calculate range
            for (int i = 0; i < _realFieldsCount; i++)
                _range[i] = max[i] - _min[i];

            _data.Reset();
        }

        public void Scale(Record record)
        {
            for (int i = 0; i < _realFieldsCount; i++)
                record.Real[i] = _range[i] == 0 ? 0 : (record.Real[i] - _min[i]) / _range[i];
        }

        public void Unscale(Record record)
        {
            for (int i = 0; i < _realFieldsCount; i++)
                record.Real[i] = record.Real[i] * _range[i] + _min[i];
        }

        public float[] Scale(float[] real)
        {
            Contract.Requires<ArgumentNullException>(real != null);
            Contract.Assert(real.Length == _realFieldsCount);

            float[] result = new float[_realFieldsCount];
            for (int i = 0; i < _realFieldsCount; i++)
                result[i] = _range[i] == 0 ? 0 : (real[i] - _min[i]) / _range[i];

            return result;
        }

        public float[] Unscale(float[] real)
        {
            Contract.Requires<ArgumentNullException>(real != null);
            Contract.Assert(real.Length == _realFieldsCount);

            float[] result = new float[_realFieldsCount];
            for (int i = 0; i < _realFieldsCount; i++)
                result[i] = real[i] * _range[i] + _min[i];

            return result;
        }
    }
}
