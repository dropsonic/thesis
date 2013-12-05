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
    /// Base decorator for all data scaling readers.
    /// </summary>
    public abstract class ScaleDataReader : IDataReader
    {
        private IDataReader _baseReader;
        private int _realFieldsCount;
        private int _discreteFieldsCount;

        protected IDataReader BaseReader 
        { 
            get { return _baseReader; }
        }

        public ScaleDataReader(IDataReader baseReader)
        {
            Contract.Requires<ArgumentNullException>(baseReader != null);

            _baseReader = baseReader;

            _realFieldsCount = Fields.RealCount();
            _discreteFieldsCount = Fields.DiscreteCount();
            
            GetDataProperties();
        }

        /// <summary>
        /// Calculates data properties on DataReader creation.
        /// </summary>
        protected abstract void GetDataProperties();

        protected abstract void ScaleRecord(Record record);

        protected int RealFieldsCount
        {
            get { return _realFieldsCount; }
        }

        protected int DiscreteFieldsCount
        {
            get { return _discreteFieldsCount; }
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
                ScaleRecord(record);
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
