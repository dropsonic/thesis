using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class Weights
    {
        public float[] Real { get; set; }
        public float[] Discrete { get; set; }

        public static Weights Identity(int realFieldsCount, int discreteFieldsCount)
        {
            Contract.Requires<ArgumentOutOfRangeException>(realFieldsCount >= 0);
            Contract.Requires<ArgumentOutOfRangeException>(discreteFieldsCount >= 0);

            float[] real = Enumerable.Repeat(1f, realFieldsCount).ToArray();
            float[] discrete = Enumerable.Repeat(1f, discreteFieldsCount).ToArray();
            
            return new Weights()
            {
                Real = real,
                Discrete = discrete
            };
        }
    }
}
