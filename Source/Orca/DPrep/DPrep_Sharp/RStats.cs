using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class RStats
    {
        private char[] _delimiters = { ',', ':', ';' };

        public IList<float> Max { get; set; }
        public IList<float> Min { get; set; }
        public IList<float> Mean { get; set; }
        public IList<float> Std { get; set; }

        public RStats(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException("size");
            Max = new List<float>(size);
            Min = new List<float>(size);
            Mean = new List<float>(size);
            Std = new List<float>(size);
        }

        public void Load(string filename)
        {
            using (var infile = new StreamReader(filename))
            {
                while (!infile.EndOfStream)
                {
                    string line = infile.ReadLine();
                    var tokens = ParserHelper.Tokenize(line, _delimiters);
                    IList<float> v = Std;
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
