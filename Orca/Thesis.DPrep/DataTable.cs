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

            Weights = new Weights();
            Weights.Real = _reader.Fields.Where(f => f.Type == Field.FieldType.Continuous).Select(f => f.Weight).ToArray();
            Weights.Discrete = _reader.Fields.Where(f => f.Type == Field.FieldType.Discrete)
                                     .Concat(_reader.Fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven))
                                     .Select(f => f.Weight).ToArray();
        }

        /// <summary>
        /// Converts the data set to a binary file.
        /// </summary>
        /// <returns>Records count.</returns>
        public int ConvertToBinary(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var outfile = new BinaryOutFile(filename, Weights))
            {
                //----------------------
                // write the example to the file
                //
                foreach (var record in _reader)
                    if (record != null)
                        outfile.WriteRecord(record);

                //-----------------------------
	            // write header information
	            //
                outfile.WriteHeader(_reader.Index);
            }

            return _reader.Index;
        }
    }
}
