using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.DPrep
{
    class PlainTextReader : IDataReader, IDisposable
    {
        private StreamReader _infile;
        private List<Field> _fields = new List<Field>();

        private char[] _fieldsDelimiters;
        private char[] _recordDelimiters;
        private string _noValueReplacement;

        private float _missingR;
        private int _missingD;
        private float _realWeight;
        private float _discreteWeight;

        public int _realFieldsCount;
        public int _discreteFieldsCount;

        public PlainTextReader(string dataFile, string fieldsFile, 
                               char[] fieldsDelimiters, char[] recordDelimiters,
                               float realWeight = 1.0f, float discreteWeight = 0.4f,
                               string noValueReplacement = "?",
                               float missingR = float.MinValue, int missingD = -1)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(dataFile));
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(fieldsFile));
            Contract.Requires<ArgumentNullException>(fieldsDelimiters != null);
            Contract.Requires<ArgumentNullException>(recordDelimiters != null);
            Contract.Requires<ArgumentException>(fieldsDelimiters.Length > 0);
            Contract.Requires<ArgumentException>(recordDelimiters.Length > 0);

            _missingR = missingR;
            _missingD = missingD;
            _realWeight = realWeight;
            _discreteWeight = discreteWeight;
            _fieldsDelimiters = fieldsDelimiters;
            _recordDelimiters = recordDelimiters;

            LoadFields(fieldsFile);
            _infile = new StreamReader(dataFile);
            Index = 0;
        }


        private string[] Tokenize(string line, char[] delimiters)
        {
            Contract.Requires<ArgumentNullException>(line != null);
            Contract.Requires<ArgumentNullException>(delimiters != null);
            Contract.Requires<ArgumentException>(delimiters.Length > 0, "No delimiters specified.");

            // Strip comments (original; remove comments in this version)
            int index = line.IndexOf('%');
            if (index >= 0)
                line = line.Remove(index);
            // Replace tab characters
            line = line.Replace('\t', ' ');
            // Split string into tokens
            var tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            // Trim whitespaces in tokens
            tokens = tokens.Select(s => s.Trim()).ToArray();

            return tokens;
        }

        private void LoadFields(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = Tokenize(line, _fieldsDelimiters);
                    if (tokens.Length > 0)
                    {
                        Field newField = CreateField(tokens);
                        _fields.Add(newField);
                    }
                }
            }

            _realFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Continuous);
            _discreteFieldsCount = _fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                                      f.Type == Field.FieldType.DiscreteDataDriven);
        }

        private Field CreateField(string[] tokens)
        {
            Contract.Requires<ArgumentNullException>(tokens != null);
            Contract.Requires<ArgumentException>(tokens.Length > 0);

            Field field = new Field();

            field.Weight = float.NaN; // no weight

            int i = 0; // start token
            float weight;
            if (float.TryParse(tokens[0], out weight)) // if weight is defined
            {
                field.Weight = weight;
                i++;
            }

            field.Name = tokens[i++];
            string sType = tokens[i];

            if (tokens.Length == i)
                field.Type = Field.FieldType.IgnoreFeature;
            else
            {
                switch (sType)
                {
                    case "ignore":
                        field.Type = Field.FieldType.IgnoreFeature;
                        break;
                    case "continuous":
                        field.Type = Field.FieldType.Continuous;
                        if (!field.HasWeight)
                            field.Weight = _realWeight;
                        break;
                    case "discrete":
                        field.Type = Field.FieldType.DiscreteDataDriven;
                        field.Values = new List<string>();
                        if (!field.HasWeight)
                            field.Weight = _discreteWeight;
                        break;
                    default:
                        //Discrete type of field: adding all of it's values
                        field.Type = Field.FieldType.Discrete;
                        field.Values = new List<string>(tokens.Length - 1);
                        for (int j = 1; j < tokens.Length; j++)
                            field.Values.Add(tokens[j]);
                        if (!field.HasWeight)
                            field.Weight = _discreteWeight;
                        break;
                }
            }

            return field;
        }

        private Record CreateRecord(string[] tokens)
        {
            Contract.Requires<ArgumentNullException>(tokens != null);

            // check to make sure there are the correct number of tokens
            if (tokens.Length != _fields.Count)
                throw new DataFormatException();

            var real = new float[_realFieldsCount];
            var discrete = new int[_discreteFieldsCount];
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
                                throw new DataFormatException();
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

            return new Record(Index, real, discrete);
        }

        public IEnumerable<Field> Fields
        {
            get { return _fields; }
        }

        public Record ReadRecord()
        {
            if (!EndOfData)
            {
                string line = _infile.ReadLine();
                string[] tokens = Tokenize(line, _recordDelimiters);
                Index++;
                return CreateRecord(tokens);
            }
            else
            {
                return null;
            }
        }

        public void Reset()
        {
            Index = 0;
            _infile.BaseStream.Position = 0;
        }

        public bool EndOfData
        {
            get { return _infile.EndOfStream; }
        }

        public int Index { get; private set; }

        public IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<Record> GetEnumerator()
        {
            Reset();
            while (!EndOfData)
                yield return ReadRecord();
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
 
        ~PlainTextReader()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
