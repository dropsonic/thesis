using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    internal static class ParserHelper
    {
        internal static string[] Tokenize(string line, char[] delimiters)
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
    }
}
