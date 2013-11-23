using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca.Common
{
    /// <summary>
    /// Represents Orca format binary file reader.
    /// </summary>
    public class BinaryInFile : IDisposable
    {
        BinaryReader _infile;

        public int Index { get; set; }

        public int RecordsCount { get; private set; }
        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public Weights Weights { get; private set; }

        private int HeaderSize
        {
            get
            {
                return 3 * sizeof(int) + RealFieldsCount * sizeof(float) + DiscreteFieldsCount * sizeof(int);
            }
        }

        public BinaryInFile(string filename)
        {
            _infile = new BinaryReader(File.OpenRead(filename));
            Weights = new Weights();
            ReadHeader();
        }

        private void ReadHeader()
        {
            long oldPos = _infile.BaseStream.Position;

            _infile.BaseStream.Position = 0;
            RecordsCount = _infile.ReadInt32();
            RealFieldsCount = _infile.ReadInt32();
            DiscreteFieldsCount = _infile.ReadInt32();

            Weights.Real = _infile.ReadFloatArray(RealFieldsCount);
            Weights.Discrete = _infile.ReadFloatArray(DiscreteFieldsCount);

            _infile.BaseStream.Position = oldPos;
        }

        public void SeekPosition(int pos)
        {
            Contract.Requires<ArgumentOutOfRangeException>(pos >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(pos < RecordsCount);

            long filepos = HeaderSize + pos * RealFieldsCount * sizeof(float) + pos * DiscreteFieldsCount * sizeof(int);
            _infile.BaseStream.Position = filepos;
            Index = pos;
        }

        public Record GetNextRecord()
        {
            // WARNING does not check to make sure read command succeeded.
            var id = _infile.ReadInt32();
            var real = _infile.ReadFloatArray(RealFieldsCount);
            var discrete = _infile.ReadIntArray(DiscreteFieldsCount);

            Index++;

            return new Record(id, real, discrete);
        }

        public Record GetRecord(int pos)
        {
            SeekPosition(pos);
            return GetNextRecord();
        }

        /// <summary>
        /// Executes action for each record in file.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<Record> action)
        {
            SeekPosition(0);
            while (Index < RecordsCount)
            {
                action(GetNextRecord());
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
 
        ~BinaryInFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
