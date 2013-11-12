using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class DataTable : IDisposable
    {
        private string _dataFile;
        private string _fieldsFile;
        private float _missingR;
        private int _missingD;

        private char[] _delimiters = { '.', ',', ':', ';' };

        private IList<Field> _fields = new List<Field>();

        private StreamReader _infile;
        private int _example;

        public DataTable(string dataFile, string fieldsFile, float missingR, int missingD)
        {
            _dataFile = dataFile;
            _fieldsFile = fieldsFile;
            _missingR = missingR;
            _missingD = missingD;

            LoadFields(fieldsFile);

            _infile = new StreamReader(dataFile);
            _example = 0;
        }

        private void LoadFields(string filename)
        {
            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = ParserHelper.Tokenize(line, _delimiters);
                    if (tokens.Length > 0)
                    {
                        Field newField = new Field(tokens);
                        _fields.Add(newField);
                    }
                }
            }
        }

        public int RealFieldsCount
        {
            get
            {
                return _fields.Count(f => f.Type == Field.FieldType.Continuous);
            }
        }

        public int DiscreteFieldsCount
        {
            get
            {
                return _fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                          f.Type == Field.FieldType.DiscreteCompiled);
            }
        }

        public void WriteWeightFile(string filename)
        {
            using (var writer = new StreamWriter(filename, false))
            {
                foreach (var field in _fields)
                    if (field.Type != Field.FieldType.IgnoreFeature)
                        writer.WriteLine("{0} {1}", field.Name, field.Type == Field.FieldType.Continuous ? 1.0 : 0.4);
            }
        }

        private int[] GetFields(Field.FieldType type)
        {
            //return _fields
            //TODO: this
            throw new NotImplementedException();
        }

        private bool GetNextRecord(out Record r, out bool valid)
        {
            //TODO: this
            throw new NotImplementedException();
        }

        private bool LoadRecord(string[] tokens, int lineNo, ref Record record)
        {
            //TODO: this
            // check to make sure there are the correct number of tokens
            // if there are an incorrect number ignore the line
            if (tokens.Length != _fields.Count)
                //throw new ArgumentException("Incorrect number of fields");
                // skip to next iteration of loop
                return false;
            //record.Real = //new List<
            throw new NotImplementedException();
        }

        private void ResetFileCounter()
        {
            _infile.BaseStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Converts the data set to a binary file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Number of converted records.</returns>
        public int ConvertToBinary(string filename)
        {
            ResetFileCounter();
            using (var stream = File.Create(filename))
            {
                using (var outfile = new BinaryWriter(stream))
                {
                    bool status = true;
                    int numRecords = 0;
                    int numReal = RealFieldsCount;
                    int numDiscrete = DiscreteFieldsCount;

                    //-------------------------------
                    // just allocating space for the
                    // header information 
                    outfile.Write(numRecords);
                    outfile.Write(numReal);
                    outfile.Write(numDiscrete);

                    //----------------------
                    // write the example to the file
                    //
                    int recordNumber = 1;
                    while (status)
                    {
                        Record R;
                        bool valid;
                        status = GetNextRecord(out R, out valid);
                        if (status && valid)
                        {
                            // write index number
                            outfile.Write(recordNumber);
                            if (numReal > 0)
                                outfile.Write(R.Real.ToArray());
                            if (numDiscrete > 0)
                                outfile.Write(R.Discrete.ToArray());
                            numRecords++;
                            recordNumber++;
                        }
                    }

                    //-----------------------------
	                // go back to the beginning and 
	                // write header information
	                //
                    outfile.Seek(0, SeekOrigin.Begin);
                    outfile.Write(numRecords);
                    outfile.Close();

                    return numRecords;
                }
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
 
        ~DataTable()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
