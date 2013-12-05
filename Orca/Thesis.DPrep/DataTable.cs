using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Thesis.Orca.Common;

namespace Thesis.DPrep
{
    class DataTable
    {
        private IDataReader _reader;

        public Weights Weights { get; private set; }

        public DataTable(IDataReader reader)
        {
            Contract.Requires<ArgumentNullException>(reader != null);

            _reader = reader;

            Weights = new Weights(reader.Fields);
        }

        /// <summary>
        /// Converts the data set to a binary file.
        /// </summary>
        /// <returns>Records count.</returns>
        public int ConvertToBinary(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var outfile = new OrcaBinaryWriter(filename, Weights))
            {
                //----------------------
                // write the example to the file
                //
                foreach (var record in _reader)
                    outfile.WriteRecord(record);
            }

            return _reader.Index;
        }
    }
}
