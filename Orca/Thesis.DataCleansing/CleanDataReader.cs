using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DataCleansing
{
    /// <summary>
    /// Decorator for IDataReader: filters all specified anomalies.
    /// </summary>
    public class CleanDataReader : IDataReader
    {
        private IDataReader _baseReader;
        private HashSet<int> _anomalies;

        public CleanDataReader(IDataReader baseReader, IEnumerable<Outlier> anomalies)
        {
            Contract.Requires<ArgumentNullException>(baseReader != null);
            Contract.Requires<ArgumentNullException>(anomalies != null);

            _baseReader = baseReader;
            _anomalies = new HashSet<int>(anomalies.Select(a => a.Id));
        }

        public IList<Field> Fields
        {
            get { return _baseReader.Fields; }
        }

        public Record ReadRecord()
        {
            var record = _baseReader.ReadRecord();
            if (EndOfData)
                return null;

            if (_anomalies.Contains(record.Id)) // if this record is anomaly
                return ReadRecord();            // go to next record
            else
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
    }
}
