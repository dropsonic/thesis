﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    static class StringHelper
    {
        public static string[] Tokenize(string line, char[] delimiters)
        {
            Contract.Requires<ArgumentNullException>(line != null);
            Contract.Requires<ArgumentNullException>(delimiters != null);
            Contract.Requires<ArgumentException>(delimiters.Length > 0, "No delimiters specified.");

            // Strip comments (original; remove comments in this version)
            int index = line.IndexOf('%');
            if (index >= 0)
                line = line.Remove(index);
            // Replace tab characters
            line = line.Replace('\t', ' ');
            // Split string into tokens
            var tokens = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            // Trim whitespaces in tokens
            tokens = tokens.Select(s => s.Trim()).ToArray();

            return tokens;
        }
    }
}
