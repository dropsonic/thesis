using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public interface IRecordParser<T>
    {
        Record Parse(T input, IList<Field> fields);
    }
}
