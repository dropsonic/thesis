using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.Orca
{
    class BatchOrcaBinaryReader : OrcaBinaryReader
    {
        int _batchSize;
        int _lastOffset;

        public int Offset { get; private set; }

        public Record[] CurrentBatch { get; private set; }

        public BatchOrcaBinaryReader(string filename, int batchSize)
            : base(filename)
        {
            _batchSize = batchSize;
            Offset = 0;
        }

        /// <summary>
        /// Reads next batch of records.
        /// </summary>s
        public bool GetNextBatch()
        {
            int nr = Math.Min(RecordsCount - Index, _batchSize);
            CurrentBatch = new Record[nr];

            for (int i = 0; i < nr; i++)
                CurrentBatch[i] = ReadRecord();

            Offset += _lastOffset;
            _lastOffset = nr;

            return nr > 0;
        }
    }
}
