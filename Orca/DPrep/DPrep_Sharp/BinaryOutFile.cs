using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Thesis.DPrep
{
    /// <summary>
    /// Represents Orca format binary file writer.
    /// </summary>
    class BinaryOutFile : IDisposable
    {
        BinaryWriter _outfile;

        int _index;
        IEnumerable<Field> _fields;
        bool _headerWritten = false;

        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public BinaryOutFile(string filename, IEnumerable<Field> fields)
        {
            _outfile = new BinaryWriter(File.Create(filename));
            _fields = fields;
            RealFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Continuous);
            DiscreteFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                                      f.Type == Field.FieldType.DiscreteDataDriven);
            WriteHeader();
        }

        private void WriteHeader()
        {
            long oldPos = _outfile.BaseStream.Position;

            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write((int)0); // number of records
            _outfile.Write(RealFieldsCount);
            _outfile.Write(DiscreteFieldsCount);
            WriteFieldsWeight(_fields.Where(f => f.Type == Field.FieldType.Continuous));
            WriteFieldsWeight(_fields.Where(f => f.Type == Field.FieldType.Discrete));
            WriteFieldsWeight(_fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven));

            _outfile.BaseStream.Position = oldPos;
            _headerWritten = true;
        }

        public void WriteHeader(int numRecords)
        {
            long oldPos = _outfile.BaseStream.Position;
            _outfile.Seek(0, SeekOrigin.Begin);
            _outfile.Write(numRecords);
            _outfile.BaseStream.Position = oldPos;
        }

        private void WriteFieldsWeight(IEnumerable<Field> fields)
        {
            foreach (var field in fields)
            {
                _outfile.Write(field.Name);
                _outfile.Write(field.Weight);
            }
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
