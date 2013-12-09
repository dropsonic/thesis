using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Scales every record on reading with defined scaling method.
    /// </summary>
    public class ScaleDataReader : IDataReader
    {
        private IDataReader _baseReader;
        private IScaling _scaling;

        public ScaleDataReader(IDataReader baseReader, IScaling scaling)
        {
            Contract.Requires<ArgumentNullException>(baseReader != null);
            Contract.Requires<ArgumentNullException>(scaling != null);

            _baseReader = baseReader;
            _scaling = scaling;
        }

        #region IDataReader
        public IList<Field> Fields
        {
            get { return _baseReader.Fields; }
        }

        public Record ReadRecord()
        {
            var record = _baseReader.ReadRecord();
            if (record != null)
                _scaling.Scale(record);
            return record;
        }

        public void Reset()
        {
            _baseReader.Reset();
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

        ~ScaleDataReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
