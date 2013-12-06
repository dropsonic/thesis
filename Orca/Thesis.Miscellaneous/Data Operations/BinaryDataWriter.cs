using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Contracts;

namespace Thesis
{
    /// <summary>
    /// Represents Orca format binary file writer.
    /// </summary>
    public class BinaryDataWriter : IDataWriter
    {
        BinaryWriter _outfile;

        bool _headerWritten = false;

        IList<Field> _fields;

        private int _realFieldsCount;
        private int _discreteFieldsCount;

        private int _count = 0; // number of records


        public BinaryDataWriter(string filename, IList<Field> fields)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(fields != null);

            _outfile = new BinaryWriter(File.Create(filename));
            
            _fields = fields;
            _realFieldsCount = fields.RealCount();
            _discreteFieldsCount = fields.DiscreteCount();

            WriteHeader();
        }


        /// <summary>
        /// Creates new BinaryDataWriter and copies all records from IDataReader source.
        /// </summary>
        public BinaryDataWriter(IDataReader source, string filename)
            : this(filename, source.Fields)
        {
            foreach (var record in source)
                WriteRecord(record);

            WriteHeader(Count);
        }

        private void WriteHeader()
        {
            //long oldPos = _outfile.BaseStream.Position;

            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write(_count); // number of records
            _outfile.Write(_realFieldsCount);
            _outfile.Write(_discreteFieldsCount);

            _outfile.Write(_fields.Count);
            foreach (var field in _fields)
                WriteField(field);

            //_outfile.BaseStream.Position = oldPos;
            _headerWritten = true;
        }

        private void WriteHeader(int numRecords)
        {
            if (!_headerWritten)
                WriteHeader();

            long oldPos = _outfile.BaseStream.Position;
            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write(numRecords);
            _outfile.BaseStream.Position = oldPos;
        }

        private void WriteField(Field field)
        {
            _outfile.Write(field.Name);
            _outfile.Write((int)field.Type);
            _outfile.Write(field.Weight);

            bool hasValues = field.Values != null;
            _outfile.Write(hasValues);
            if (hasValues)
            {
                _outfile.Write(field.Values.Count);
                foreach (var value in field.Values)
                    _outfile.Write(value);
            }
        }

        public void WriteRecord(Record record)
        {
            if (record == null)
                return;

            if (record.Real.Length != _realFieldsCount ||
                record.Discrete.Length != _discreteFieldsCount)
                throw new ArgumentException("Wrong number of values in record.");

            if (!_headerWritten)
                WriteHeader();

            _outfile.Write(record.Id);
            if (_realFieldsCount > 0)
                _outfile.Write(record.Real);
            if (_discreteFieldsCount > 0)
                _outfile.Write(record.Discrete);

            _count++;
        }

        public int Count
        {
            get { return _count; }
        }

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
                    WriteHeader(_count);
                    _outfile.Close();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~BinaryDataWriter()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
