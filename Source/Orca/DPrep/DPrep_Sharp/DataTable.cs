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
            var infile = new StreamReader(filename);
            while (!infile.EndOfStream)
            {
                string line = infile.ReadLine();
                var tokens = Tokenize(line, _delimiters);
                if (tokens.Count > 0)
                {
                    Field newField = new Field(tokens);
                    _fields.Add(newField);
                }
            }
        }

        private IList<string> Tokenize(string line, char[] delimiters)
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
    }
}
