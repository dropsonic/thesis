using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.Orca
{
    class BatchInFile : BinaryInFile
    {
        int _batchSize;
        int _offset;
        int _lastOffset;

        public BatchInFile(string filename, int batchSize)
            : base(filename)
        {
            _batchSize = batchSize;
            _offset = 0;
            //_lastOffset = _batchSize;
        }

        /// <summary>
        /// Reads next batch of records.
        /// </summary>s
        public bool GetNextBatch(out Record[] records)
        {
            int nr = Math.Min(RecordsCount - Index, _batchSize);
            records = new Record[nr];

            for (int i = 0; i < nr; i++)
                records[i] = GetNextRecord();

            _offset += _lastOffset;
            _lastOffset = nr;

            return nr > 0;
        }
    }
}
