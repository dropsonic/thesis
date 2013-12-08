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
        private char[] _recordDelimiters;
        private string _noValueReplacement;

        public PlainTextParser(char[] recordDelimiters,
                               string noValueReplacement = "?")
        {
            Contract.Requires<ArgumentNullException>(recordDelimiters != null);
            Contract.Requires<ArgumentException>(recordDelimiters.Length > 0);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(noValueReplacement));

            _recordDelimiters = recordDelimiters;
            _noValueReplacement = noValueReplacement;
        }

        public PlainTextParser(string noValueReplacement = "?")
            : this(new char[] { ',', ';' }, noValueReplacement)
        { }

        /// <summary>
        /// Parses string and returns record. Returns null if string is a comment.
        /// </summary>
        /// <exception cref="Thesis.DataFormatException"/>
        public Record Parse(string input, IList<Field> fields)
        {
            Contract.Requires<ArgumentNullException>(fields != null);

            var tokens = StringHelper.Tokenize(input, _recordDelimiters);

            if (tokens.Length == 0) // if comment
                return null;

            // check to make sure there are the correct number of tokens
            if (tokens.Length != fields.Count)
                throw new DataFormatException("Wrong number of tokens.");

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
                            real[iReal++] = float.NaN; break;
                        case Field.FieldType.Discrete:
                        case Field.FieldType.DiscreteDataDriven:
                            discrete[iDiscrete++] = -1; break;
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
                                throw new DataFormatException(String.Format(
                                    "Discrete value '{0}' for field '{1}' doesn't exist.", tokens[i], fields[i]));
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

        public Record TryParse(string input, IList<Field> fields)
        {
            Contract.Requires<ArgumentNullException>(fields != null);

            try
            {
                return Parse(input, fields);
            }
            catch (DataFormatException)
            {
                return null;
            }
        }
    }
}
