using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class DataTable
    {
        private string _dataFile;
        private string _fieldsFile;
        private float _missingR;
        private int _missingD;

        private char[] _delimiters = { '.', ',', ':', ';' };

        private IList<Field> _fields = new List<Field>();

        public DataTable(string dataFile, string fieldsFile, float missingR, int missingD)
        {
            _dataFile = dataFile;
            _fieldsFile = fieldsFile;
            _missingR = missingR;
            _missingD = missingD;

            LoadFields(fieldsFile);
        }

        private void LoadFields(string filename)
        {
            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = Tokenize(line, _delimiters);
                    if (tokens.Length > 0)
                    {
                        Field newField = new Field(tokens);
                        _fields.Add(newField);
                    }
                }
            }
        }

        private string[] Tokenize(string line, char[] delimiters)
        {
            if (delimiters == null)
                throw new ArgumentNullException("delimiters");
            if (delimiters.Length == 0)
                throw new ArgumentException("No delimiters specified.");
            // Strip comments (original; remove comments in this version)
            line = line.Remove(line.IndexOf('%'));
            // Split string into tokens
            var tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }

        private int RealFieldsCount
        {
            get
            {
                return _fields.Count(f => f.Type == Field.FieldType.Continuous);
            }
        }

        private int DiscreteFieldsCount
        {
            get
            {
                return _fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                          f.Type == Field.FieldType.DiscreteCompiled);
            }
        }

        private void WriteWeightFile(string filename)
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
            throw new NotImplementedException();
        }

        private bool LoadRecord(string[] tokens, int lineNo, ref Record record)
        {
            // check to make sure there are the correct number of tokens
            // if there are an incorrect number ignore the line
            if (tokens.Length != _fields.Count)
                //throw new ArgumentException("Incorrect number of fields");
                // skip to next iteration of loop
                return false;
            //record.Real = //new List<
            throw new NotImplementedException();
        }
    }
}
