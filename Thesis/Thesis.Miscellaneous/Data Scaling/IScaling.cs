using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public interface IScaling
    {
        void Scale(Record record);
        void Unscale(Record record);

        float[] Scale(float[] real);
        float[] Unscale(float[] real);
    }
}
