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
    }
}
