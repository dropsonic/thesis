using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Scales real values to the [0..1] range.
    /// </summary>
    public class MinmaxScaleDataReader : ScaleDataReader
    {
        float[] _min;
        float[] _range;

        protected override void GetDataProperties()
        {
            // initialize vectors 
            _min = new float[RealFieldsCount];
            float[] max = new float[RealFieldsCount];
            _range = new float[RealFieldsCount];
            for (int i = 0; i < RealFieldsCount; i++)
            {
                _min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            foreach (var record in this)
            {
                for (int i = 0; i < RealFieldsCount; i++)
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
            for (int i = 0; i < RealFieldsCount; i++)
                _range[i] = max[i] - _min[i];
        }

        protected override void ScaleRecord(Record record)
        {
            for (int i = 0; i < RealFieldsCount; i++)
                record.Real[i] = (record.Real[i] - _min[i]) / _range[i];
        }
    }
}
