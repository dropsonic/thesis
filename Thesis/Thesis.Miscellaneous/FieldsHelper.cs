using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public static class FieldsHelper
    {
        public static int RealCount(this IEnumerable<Field> fields)
        {
            return fields.Count(f => f.Type == Field.FieldType.Continuous);
        }

        public static int DiscreteCount(this IEnumerable<Field> fields)
        {
            return fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                     f.Type == Field.FieldType.DiscreteDataDriven);
        }

        public static Weights Weights(this IEnumerable<Field> fields)
        {
            var real = fields.Where(f => f.Type == Field.FieldType.Continuous).Select(f => f.Weight).ToArray();
            var discrete = fields.Where(f => f.Type == Field.FieldType.Discrete)
                                     .Concat(fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven))
                                     .Select(f => f.Weight).ToArray();
            return new Weights() { Real = real, Discrete = discrete };
        }
    }
}
