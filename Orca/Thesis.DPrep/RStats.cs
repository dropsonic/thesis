using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Thesis.DPrep
{
    class RStats
    {
        private char[] _delimiters = { ',', ':', ';', ' '};

        public float[] Max { get; set; }
        public float[] Min { get; set; }
        public float[] Mean { get; set; }
        public float[] Std { get; set; }

        public RStats(int size)
        {
            Contract.Requires<ArgumentOutOfRangeException>(size >= 0);

            Max = new float[size];
            Min = new float[size];
            Mean = new float[size];
            Std = new float[size];
        }

        public void Load(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = ParserHelper.Tokenize(line, _delimiters);
                    float[] v = Std;
                    if (tokens.Length > 0)
                    {
                        switch (tokens[0])
                        {
                            case "max":
                                v = Max; break;
                            case "min":
                                v = Min; break;
                            case "mean":
                                v = Mean; break;
                            case "std":
                                v = Std; break;
                            default:
                                throw new FormatException("Unknown statistic");
                        }
                    }
                    
                    // get numbers
                    for (int i = 1; i < tokens.Length; i++)
                        v[i - 1] = float.Parse(tokens[i]);
                }
            }
        }
    }
}
