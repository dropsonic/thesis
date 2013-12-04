using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class Weights
    {
        public float[] Real { get; set; }
        public float[] Discrete { get; set; }

        public Weights() { }

        public Weights(IEnumerable<Field> fields)
        {
            Real = fields.Where(f => f.Type == Field.FieldType.Continuous).Select(f => f.Weight).ToArray();
            Discrete = fields.Where(f => f.Type == Field.FieldType.Discrete)
                                     .Concat(fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven))
                                     .Select(f => f.Weight).ToArray();
        }
    }
}
