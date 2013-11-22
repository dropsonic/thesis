using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    /// <summary>
    /// Represents Orca format binary file reader.
    /// </summary>
    class BinaryInFile : IDisposable
    {
        BinaryReader _infile;

        int _index;
        Dictionary<string, float> _weights;

        public int Records { get; private set; }
        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public IReadOnlyDictionary<string, float> Weights
        {
            get { return _weights; }
        }

        public BinaryInFile(string filename)
        {
            _infile = new BinaryReader(File.OpenRead(filename));
            _weights = new Dictionary<string, float>();
            ReadHeader();
        }

        private void ReadHeader()
        {
            long oldPos = _infile.BaseStream.Position;

            _infile.BaseStream.Position = 0;
            Records = _infile.ReadInt32();
            RealFieldsCount = _infile.ReadInt32();
            DiscreteFieldsCount = _infile.ReadInt32();

            int fieldsCount = RealFieldsCount + DiscreteFieldsCount;
            for (int i = 0; i < fieldsCount; i++)
            {
                
            }

            _infile.BaseStream.Position = oldPos;
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
 
        ~BinaryInFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
