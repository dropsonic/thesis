using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class BFile : IDisposable
    {
        BinaryReader _infile;
        int _example;

        string _dataFile;

        int _index;

        int _records;
        int _realFieldsCount;
        int _discreteFieldsCount;

        float _missingR;
        int _missingD;

        public BFile(string filename, float missingR, int missingD)
        {
            _dataFile = filename;
            _missingR = missingR;
            _missingD = missingD;

            SetFileReader(filename);
            ReadHeader();
        }

        private void SetFileReader(string filename)
        {
            // if infile_ already points to a file, close it
            if (_infile != null)
                _infile.Close();

            _infile = new BinaryReader(File.OpenRead(filename));
            _index = 0;
        }

        private void ReadHeader()
        {
            _records = _infile.ReadInt32();
            _realFieldsCount = _infile.ReadInt32();
            _discreteFieldsCount = _infile.ReadInt32();
        }

        public int RecordsCount
        {
            get
            {
                return _records;
            }
        }

        public int RealFieldsCount
        {
            get
            {
                return _realFieldsCount;
            }
        }

        public int DiscreteFieldsCount
        {
            get
            {
                return _discreteFieldsCount;
            }
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
                    _infile.Close();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~BFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
