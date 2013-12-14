using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public class DataFormatException : FormatException
    {
        public DataFormatException()
            : base() { }

        public DataFormatException(string message)
            : base(message) { }

        public DataFormatException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
