using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public interface IDataWriter : IDisposable
    {
        void WriteRecord(Record record);
        int Count { get; }
    }
}
