using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Thesis.DPrep
{
    internal static class ParserHelper
    {
        internal static string[] Tokenize(string line, char[] delimiters)
        {
            Contract.Requires<ArgumentNullException>(line != null);
            Contract.Requires<ArgumentNullException>(delimiters != null);
            Contract.Requires<ArgumentException>(delimiters.Length > 0, "No delimiters specified.");

            // Strip comments (original; remove comments in this version)
            int index = line.IndexOf('%');
            if (index >= 0)
                line = line.Remove(index);
            // Split string into tokens
            var tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tokens;
        }
    }
}
