using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class BatchDataReader : IDataReader
    {
        IDataReader _baseReader;

        int _batchSize;
        int _lastOffset;

        public int Offset { get; private set; }

        public IList<Record> CurrentBatch { get; private set; }

        public BatchDataReader(IDataReader baseReader, int batchSize)
        {
            Contract.Requires<ArgumentNullException>(baseReader != null);
            Contract.Requires<ArgumentOutOfRangeException>(batchSize > 0);

            _baseReader = baseReader;
            _baseReader.Reset();
            _batchSize = batchSize;
        }

        /// <summary>
        /// Reads next batch of records.
        /// </summary>s
        public bool GetNextBatch()
        {
            CurrentBatch = new List<Record>(_batchSize);

            int nr = 0;
            for (int i = 0; i < _batchSize; i++)
            {
                if (_baseReader.EndOfData)
                    break;
                CurrentBatch.Add(_baseReader.ReadRecord());
                nr++;
            }

            Offset += _lastOffset;
            _lastOffset = nr;

            return nr > 0;
        }

        #region IDataReader
        public IList<Field> Fields
        {
            get { return _baseReader.Fields; }
        }

        public Record ReadRecord()
        {
            return _baseReader.ReadRecord();
        }

        public void Reset()
        {
            _baseReader.Reset();
            Offset = 0;
            _lastOffset = 0;
            CurrentBatch = null;
        }

        public bool EndOfData
        {
            get { return _baseReader.EndOfData; }
        }

        public int Index
        {
            get { return _baseReader.Index; }
        }

        public IEnumerator<Record> GetEnumerator()
        {
            Reset();
            var record = ReadRecord();
            while (record != null)
            {
                yield return record;
                record = ReadRecord();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_Disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    // Managed resources are released here.
                    _baseReader.Dispose();
                }

                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }

        ~BatchDataReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
