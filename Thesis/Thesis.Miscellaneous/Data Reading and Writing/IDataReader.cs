using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public interface IDataReader: IEnumerable<Record>, IDisposable
    {
        IList<Field> Fields { get; }

        Record ReadRecord();

        void Reset();

        bool EndOfData { get; }

        int Index { get; }
    }
}
