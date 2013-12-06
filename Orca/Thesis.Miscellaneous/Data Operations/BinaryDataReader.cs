using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    /// <summary>
    /// Represents binary record format file reader.
    /// </summary>
    public class BinaryDataReader : IDataReader
    {
        BinaryReader _infile;

        public int Index { get; set; }

        public int RecordsCount { get; private set; }
        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        private long _dataOffset; // 0 + size of header

        IList<Field> _fields;

        public BinaryDataReader(string filename)
        {
            InitReader(filename);
        }

        /// <summary>
        /// Copies all record from source to binary file using BinaryDataWriter
        /// and opens data reader on that file.
        /// </summary>
        /// <param name="source">Source data reader.</param>
        /// <param name="filename">New binary file name.</param>
        public BinaryDataReader(IDataReader source, string filename)
        {
            var writer = new BinaryDataWriter(source, filename);
            writer.Dispose();
            InitReader(filename);
        }

        /// <summary>
        /// Initializes reader.
        /// </summary>
        /// <param name="filename">Input data file name.</param>
        private void InitReader(string filename)
        {
            _infile = new BinaryReader(File.OpenRead(filename));
            _fields = new List<Field>();
            ReadHeader();
            SeekPosition(0);
        }

        private void ReadHeader()
        {
            long oldPos = _infile.BaseStream.Position;

            _infile.BaseStream.Position = 0;
            RecordsCount = _infile.ReadInt32();
            RealFieldsCount = _infile.ReadInt32();
            DiscreteFieldsCount = _infile.ReadInt32();
            
            int fieldsCount = _infile.ReadInt32();
            for (int i = 0; i < fieldsCount; i++)
                _fields.Add(ReadField());

            _dataOffset = _infile.BaseStream.Position;
            _infile.BaseStream.Position = oldPos;
        }

        private Field ReadField()
        {
            string name = _infile.ReadString();
            Field.FieldType type = (Field.FieldType)_infile.ReadInt32();
            float weight = _infile.ReadSingle();

            bool hasValues = _infile.ReadBoolean();
            List<string> values = null;
            if (hasValues)
            {
                int valuesCount = _infile.ReadInt32();
                values = new List<string>();
                for (int i = 0; i < valuesCount; i++)
                    values.Add(_infile.ReadString());
            }

            return new Field() { Name = name, Type = type, Weight = weight, Values = values };
        }

        private void SeekPosition(int pos)
        {
            Contract.Requires<ArgumentOutOfRangeException>(pos >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(pos <= RecordsCount);

            long filepos = _dataOffset + pos * 
                (sizeof(int) + RealFieldsCount * sizeof(float) + DiscreteFieldsCount * sizeof(int));
            
            _infile.BaseStream.Position = filepos;
            Index = pos;
        }

        #region IDataReader
        public IList<Field> Fields
        {
            get { return _fields; }
        }

        public Record ReadRecord()
        {
            if (EndOfData)
                return null;

            var id = _infile.ReadInt32();
            var real = _infile.ReadFloatArray(RealFieldsCount);
            var discrete = _infile.ReadIntArray(DiscreteFieldsCount);

            Index++;

            return new Record(id, real, discrete);
        }

        public void Reset()
        {
            SeekPosition(0);
        }

        public bool EndOfData
        {
            get { return Index == RecordsCount; }
        }

        public IEnumerator<Record> GetEnumerator()
        {
            Reset();
            while (!EndOfData)
                yield return ReadRecord();
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
                    _infile.Close();
                }

                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }

        ~BinaryDataReader()
        {
            Dispose(false);
        }
        #endregion
    }
}
