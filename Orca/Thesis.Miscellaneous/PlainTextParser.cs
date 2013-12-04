using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class PlainTextParser : IRecordParser<string>
    {
        private const string _defaultNoValueReplacement = "?";
        private const float _defaultMissingR = float.MinValue;
        private const int _defaultMissingD = -1;

        private char[] _recordDelimiters;
        private string _noValueReplacement;
        private float _missingR;
        private int _missingD;

        public PlainTextParser(char[] recordDelimiters,
                               string noValueReplacement = _defaultNoValueReplacement,
                               float missingR = _defaultMissingR, 
                               int missingD = _defaultMissingD)
        {
            Contract.Requires<ArgumentNullException>(recordDelimiters != null);
            Contract.Requires<ArgumentException>(recordDelimiters.Length > 0);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(noValueReplacement));

            _recordDelimiters = recordDelimiters;
            _noValueReplacement = noValueReplacement;
            _missingR = missingR;
            _missingD = missingD;
        }

        public PlainTextParser(string noValueReplacement = _defaultNoValueReplacement,
                               float missingR = _defaultMissingR,
                               int missingD = _defaultMissingD)
            : this(new char[] { ',', ';' }, 
                   noValueReplacement, missingR, missingD)
        { }

        public Record Parse(string input, IList<Field> fields)
        {
            Contract.Requires<ArgumentNullException>(fields != null);

            var tokens = StringHelper.Tokenize(input, _recordDelimiters);

            // check to make sure there are the correct number of tokens
            if (tokens.Length != fields.Count)
                return null;

            int realFieldsCount = fields.Count(f => f.Type == Field.FieldType.Continuous);
            int discreteFieldsCount = fields.Count(f => f.Type == Field.FieldType.Discrete ||
                                                      f.Type == Field.FieldType.DiscreteDataDriven);

            var real = new float[realFieldsCount];
            var discrete = new int[discreteFieldsCount];
            int iReal = 0;
            int iDiscrete = 0;


            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Type == Field.FieldType.IgnoreFeature)
                    continue;
                if (tokens[i] == _noValueReplacement)
                {
                    switch (fields[i].Type)
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
                    switch (fields[i].Type)
                    {
                        case Field.FieldType.Continuous:
                            real[iReal++] = float.Parse(tokens[i]); break;
                        case Field.FieldType.Discrete:
                            int value = fields[i].Values.IndexOf(tokens[i]);
                            if (value != -1)
                                discrete[iDiscrete++] = value;
                            else
                                throw new DataFormatException();
                            break;
                        case Field.FieldType.DiscreteDataDriven:
                            int valuec = fields[i].Values.IndexOf(tokens[i]);
                            if (valuec != -1)
                                discrete[iDiscrete++] = valuec;
                            else
                            {
                                // Add new value to the field description.
                                fields[i].Values.Add(tokens[i]);
                                discrete[iDiscrete++] = fields[i].Values.Count - 1;
                            }
                            break;
                    }
                }
            }

            return new Record(0, real, discrete);
        }
    }
}
