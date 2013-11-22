using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        public int Index { get; set; }

        public int Records { get; private set; }
        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public float[] RealWeights { get; private set; }
        public int[] DiscreteWeights { get; private set; }

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
            ReadHeader();
        }

        private void ReadHeader()
        {
            long oldPos = _infile.BaseStream.Position;

            _infile.BaseStream.Position = 0;
            Records = _infile.ReadInt32();
            RealFieldsCount = _infile.ReadInt32();
            DiscreteFieldsCount = _infile.ReadInt32();

            RealWeights = _infile.ReadFloatArray(RealFieldsCount);
            DiscreteWeights = _infile.ReadIntArray(DiscreteFieldsCount);

            _infile.BaseStream.Position = oldPos;
        }

        public void SeekPosition(int pos)
        {
            Contract.Requires<ArgumentOutOfRangeException>(pos >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(pos < Records);

            long filepos = HeaderSize + pos * RealFieldsCount * sizeof(float) + pos * DiscreteFieldsCount * sizeof(int);
            _infile.BaseStream.Position = filepos;
            Index = pos;
        }

        public void GetNext(out int id, out float[] real, out int[] discrete)
        {
            // WARNING does not check to make sure read command succeeded.
            id = _infile.ReadInt32();
            real = _infile.ReadFloatArray(RealFieldsCount);
            discrete = _infile.ReadIntArray(DiscreteFieldsCount);

            Index++;
        }

        /// <summary>
        /// Executes action for each record in file.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<int, float[], int[]> action)
        {
            SeekPosition(0);
            while (Index < Records)
            {
                int id;
                float[] real;
                int[] discrete;
                GetNext(out id, out real, out discrete);
                action(id, real, discrete);
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
