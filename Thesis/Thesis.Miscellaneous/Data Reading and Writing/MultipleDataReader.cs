using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    //Костыль, который никто не должен увидеть. Не смотрите сюда, это было написано за час до сдачи диплома.
    /// <summary>
    /// Decorator for multiple readers.
    /// </summary>
    public class MultipleDataReader : IDataReader
    {
        private IEnumerator<IDataReader> _enum;
        private IList<Field> _fields;
        private int _index = 0;

        public MultipleDataReader(IEnumerable<IDataReader> readers)
        {
            Contract.Requires<ArgumentNullException>(readers != null);
            //_readers = readers;
            _enum = readers.GetEnumerator();
            _fields = readers.First().Fields;
        }

        #region IDataReader
        public IList<Field> Fields
        {
            get { return _fields; }
        }

        public Record ReadRecord()
        {
            if (_enum.Current == null ||_enum.Current.EndOfData)
                if (!_enum.MoveNext())
                    return null;
                else
                    _enum.Current.Reset();

            _index++;
            return _enum.Current.ReadRecord();
        }

        public void Reset()
        {
            try
            {
                _enum.Reset();
            }
            catch (System.Exception ex)
            {
            }
            _index = 0;
        }

        public bool EndOfData
        {
            get { return _enum.Current == null; }
        }

        public int Index
        {
            get { return _index; }
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
                }

                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }

        ~MultipleDataReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
