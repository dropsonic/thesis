using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.BinaryFiles;

namespace Thesis.Orca
{
    class BatchInFile : BinaryInFile
    {
        int _startBatchSize;
        int _batchSize;

        public BatchInFile(string filename, int startBatchSize, int batchSize)
            : base(filename)
        {
            _startBatchSize = startBatchSize;
            _batchSize = batchSize;
        }

        public bool GetNextBatch(out Record[] records)
        {
            int nr = Math.Min(RecordsCount - Index, _batchSize);
            records = new Record[nr];

            for (int i = 0; i < nr; i++)
            {

            }

            throw new NotImplementedException();
        }
    }
}
