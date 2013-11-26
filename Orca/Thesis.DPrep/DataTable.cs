using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Thesis.Orca.Common;

namespace Thesis.DPrep
{
    class DataTable : IDisposable
    {
        private string _dataFile;
        private string _fieldsFile;
        private float _missingR;
        private int _missingD;
        private float _realWeight;
        private float _discreteWeight;

        private readonly char[] _fieldsDelimiters = { '.', ',', ':', ';', ' ' };
        private readonly char[] _recordDelimiters = { ',', ':', ';', ' ' };
        private const string _noValueReplacement = "?";

        private List<Field> _fields = new List<Field>();

        public Weights Weights { get; private set; }

        private StreamReader _infile;

        public int RealFieldsCount { get; private set; }
        public int DiscreteFieldsCount { get; private set; }

        public DataTable(string dataFile, string fieldsFile, float missingR, int missingD, float realWeight, float discreteWeight)
        {
            Contract.Requires(!String.IsNullOrEmpty(dataFile));
            Contract.Requires(!String.IsNullOrEmpty(fieldsFile));

            _dataFile = dataFile;
            _fieldsFile = fieldsFile;
            _missingR = missingR;
            _missingD = missingD;
            _realWeight = realWeight;
            _discreteWeight = discreteWeight;

            Weights = new Weights();

            LoadFields(fieldsFile);

            _infile = new StreamReader(dataFile);
        }

        private void LoadFields(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = ParserHelper.Tokenize(line, _fieldsDelimiters);
                    if (tokens.Length > 0)
                    {
                        Field newField = new Field(tokens, _realWeight, _discreteWeight);
                        _fields.Add(newField);
                    }
                }
            }

            RealFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Continuous);
            DiscreteFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Discrete || 
                                                      f.Type == Field.FieldType.DiscreteDataDriven);

            Weights.Real = _fields.Where(f => f.Type == Field.FieldType.Continuous).Select(f => f.Weight).ToArray();
            Weights.Discrete = _fields.Where(f => f.Type == Field.FieldType.Discrete)
                                     .Concat(_fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven))
                                     .Select(f => f.Weight).ToArray();
        }

        /// <param name="valid">true, if the record was correctly loaded; false, if the record had errors and was ignored</param>
        /// <returns>true, if able to retrieve the next record; false, if unable to get the next record</returns>
        private bool GetNextRecord(out float[] real, out int[] discrete, out bool valid)
        {
            if (!_infile.EndOfStream)
            {
                string line = _infile.ReadLine();
                string[] tokens = ParserHelper.Tokenize(line, _recordDelimiters);
                valid = LoadRecord(tokens, out real, out discrete);
                return true;
            }
            else
            {
                valid = false;
                real = null;
                discrete = null;
                return false;
            }
        }

        private bool LoadRecord(string[] tokens, out float[] real, out int[] discrete)
        {
            Contract.Requires<ArgumentNullException>(tokens != null);

            // check to make sure there are the correct number of tokens
            // if there are an incorrect number ignore the line
            if (tokens.Length != _fields.Count)
            {
                // skip to next iteration of loop
                real = null;
                discrete = null;
                return false;
            }

            real = new float[RealFieldsCount];
            discrete = new int[DiscreteFieldsCount];
            int iReal = 0;
            int iDiscrete = 0;


            for (int i = 0; i < _fields.Count; i++)
            {
                if (_fields[i].Type == Field.FieldType.IgnoreFeature)
                    continue;
                if (tokens[i] == _noValueReplacement)
                {
                    switch (_fields[i].Type)
                    {
                        case Field.FieldType.Continuous:
                            real[iReal++] = _missingR; break;
                        case Field.FieldType.Discrete:
                        case Field.FieldType.DiscreteDataDriven:
                            discrete[iDiscrete++] = _missingD; break;
                    }
                }
                else
                {
                    switch (_fields[i].Type)
                    {
                        case Field.FieldType.Continuous:
                            real[iReal++] = float.Parse(tokens[i]); break;
                        case Field.FieldType.Discrete:
                            int value = _fields[i].Values.IndexOf(tokens[i]);
                            if (value != -1)
                                discrete[iDiscrete++] = value;
                            else
                                return false;
                            break;
                        case Field.FieldType.DiscreteDataDriven:
                            int valuec = _fields[i].Values.IndexOf(tokens[i]);
                            if (valuec != -1)
                                discrete[iDiscrete++] = valuec;
                            else
                            {
                                // Add new value to the field description.
                                _fields[i].Values.Add(tokens[i]);
                                discrete[iDiscrete++] = _fields[i].Values.Count - 1;
                            }
                            break;
                    }
                }
            }

            return true;
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
            Contract.Requires(!String.IsNullOrEmpty(filename));

            ResetFileCounter();

            using (var outfile = new BinaryOutFile(filename, Weights))
            {
                bool status = true;
                int numRecords = 0;

                //----------------------
                // write the example to the file
                //
                int recordNumber = 1;
                while (status)
                {
                    float[] real;
                    int[] discrete;
                    bool valid;
                    status = GetNextRecord(out real, out discrete, out valid);
                    if (status && valid)
                    {
                        var record = new Record(recordNumber, real, discrete);
                        outfile.WriteRecord(record);
                        numRecords++;
                        recordNumber++;
                    }
                }

                //-----------------------------
	            // write header information
	            //
                outfile.WriteHeader(numRecords);

                return numRecords;
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
