using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Contracts;

namespace Thesis.Orca.Common
{
    /// <summary>
    /// Represents Orca format binary file writer.
    /// </summary>
    public class OrcaBinaryWriter : IDataWriter, IDisposable
    {
        BinaryWriter _outfile;

        bool _headerWritten = false;

        Weights _weights;

        private bool HasWeights { get { return _weights != null; } }

        private int _realFieldsCount;
        private int _discreteFieldsCount;

        private int _count = 0; // number of records

        public OrcaBinaryWriter(string filename, int realFieldsCount, int discreteFieldsCount)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentOutOfRangeException>(realFieldsCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(discreteFieldsCount >= 0);

            _realFieldsCount = realFieldsCount;
            _discreteFieldsCount = discreteFieldsCount;

            _outfile = new BinaryWriter(File.Create(filename));

            WriteHeader();
        }

        public OrcaBinaryWriter(string filename, Weights weights)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(weights != null);
            Contract.Requires<ArgumentException>(weights.Real != null);
            Contract.Requires<ArgumentException>(weights.Discrete != null);

            _outfile = new BinaryWriter(File.Create(filename));

            _realFieldsCount = weights.Real.Length;
            _discreteFieldsCount = weights.Discrete.Length;

            _weights = weights;

            WriteHeader();
        }

        private void WriteHeader()
        {
            //long oldPos = _outfile.BaseStream.Position;

            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write(_count); // number of records
            _outfile.Write(_realFieldsCount);
            _outfile.Write(_discreteFieldsCount);
            _outfile.Write(HasWeights);
            if (HasWeights)
            {
                _outfile.Write(_weights.Real);
                _outfile.Write(_weights.Discrete);
            }

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
 
        ~OrcaBinaryWriter()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
