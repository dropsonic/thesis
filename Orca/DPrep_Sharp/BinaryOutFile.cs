using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Contracts;

namespace Thesis.DPrep
{
    /// <summary>
    /// Represents Orca format binary file writer.
    /// </summary>
    class BinaryOutFile : IDisposable
    {
        BinaryWriter _outfile;

        int _index;
        bool _headerWritten = false;

        float[] _realWeights;
        float[] _discreteWeights;

        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public BinaryOutFile(string filename, float[] realWeights, float[] discreteWeights)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(realWeights != null);
            Contract.Requires<ArgumentNullException>(discreteWeights != null);

            _outfile = new BinaryWriter(File.Create(filename));

            _realWeights = realWeights;
            _discreteWeights = discreteWeights;
            RealFieldsCount = realWeights.Length;
            DiscreteFieldsCount = discreteWeights.Length;

            WriteHeader();
        }

        private void WriteHeader()
        {
            //long oldPos = _outfile.BaseStream.Position;

            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write((int)0); // number of records
            _outfile.Write(RealFieldsCount);
            _outfile.Write(DiscreteFieldsCount);
            _outfile.Write(_realWeights);
            _outfile.Write(_discreteWeights);

            //_outfile.BaseStream.Position = oldPos;
            _headerWritten = true;
        }

        public void WriteHeader(int numRecords)
        {
            if (!_headerWritten)
                WriteHeader();

            long oldPos = _outfile.BaseStream.Position;
            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write(numRecords);
            _outfile.BaseStream.Position = oldPos;
        }

        public void WriteRecord(int id, float[] real, int[] discrete)
        {
            if (!_headerWritten)
                WriteHeader();

            _outfile.Write(id);
            if (RealFieldsCount > 0)
                _outfile.Write(real);
            if (DiscreteFieldsCount > 0)
                _outfile.Write(discrete);
        }

        public void Write(byte[] data)
        {
            if (!_headerWritten)
                WriteHeader();

            _outfile.Write(data);
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
                    _outfile.Close();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~BinaryOutFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
