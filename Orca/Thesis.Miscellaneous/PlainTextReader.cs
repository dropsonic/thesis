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
    public class PlainTextReader : IDataReader, IDisposable
    {
        private IRecordParser<string> _parser;

        private StreamReader _infile;
        private List<Field> _fields = new List<Field>();

        private char[] _fieldsDelimiters;

        private float _realWeight;
        private float _discreteWeight;

        public int _realFieldsCount;
        public int _discreteFieldsCount;

        public PlainTextReader(string dataFile, string fieldsFile, IRecordParser<string> parser,
                               float realWeight = 1.0f, float discreteWeight = 0.4f,
                               string noValueReplacement = "?",
                               float missingR = float.MinValue, int missingD = -1)
            : this(dataFile, fieldsFile, parser,
                   new char[] { '.', ',', ':', ';' }, realWeight, discreteWeight)
        { }

        public PlainTextReader(string dataFile, string fieldsFile, IRecordParser<string> parser, 
                               char[] fieldsDelimiters,
                               float realWeight = 1.0f, float discreteWeight = 0.4f)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(dataFile));
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(fieldsFile));
            Contract.Requires<ArgumentNullException>(parser != null);
            Contract.Requires<ArgumentNullException>(fieldsDelimiters != null);
            Contract.Requires<ArgumentException>(fieldsDelimiters.Length > 0);

            _parser = parser;

            _realWeight = realWeight;
            _discreteWeight = discreteWeight;
            _fieldsDelimiters = fieldsDelimiters;

            LoadFields(fieldsFile);
            _infile = new StreamReader(dataFile);
            Index = 0;
        }


        private void LoadFields(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = StringHelper.Tokenize(line, _fieldsDelimiters);
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

        #region IDataReader
        public IList<Field> Fields
        {
            get { return _fields.AsReadOnly(); }
        }

        public Record ReadRecord()
        {
            if (!EndOfData)
            {
                string line = _infile.ReadLine();
                var record = _parser.Parse(line, _fields);
                if (record == null) // if comment
                    return ReadRecord(); // go to next line
                else
                {
                    Index++;
                    record.Id = Index;
                    return record;
                }
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<Record> GetEnumerator()
        {
            Reset();
            var record = ReadRecord();
            while (record != null)
            {
                yield return record;
                record = ReadRecord();
            }
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
 
        ~PlainTextReader()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
