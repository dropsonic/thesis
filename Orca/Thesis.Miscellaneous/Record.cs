using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    [DebuggerDisplay("Id = {Id}")]
    public class Record : ICloneable
    {
        public int Id { get; set; }
        public float[] Real { get; set; }
        public int[] Discrete { get; set; }

        public Record() { }

        public Record(int id, float[] real, int[] discrete)
        {
            Id = id;
            Real = real;
            Discrete = discrete;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Record Clone()
        {
            return new Record(Id, (float[])Real.Clone(), (int[])Discrete.Clone());
        }
    }
}
