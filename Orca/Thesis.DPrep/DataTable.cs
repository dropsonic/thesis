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
    class DataTable : IDisposable
    {
        private IDataReader _reader;
        private IDataWriter _writer;


        public Weights Weights { get; private set; }

        public DataTable(IDataReader reader, IDataWriter writer)
        {
            Contract.Requires<ArgumentNullException>(reader != null);
            Contract.Requires<ArgumentNullException>(writer != null);

            _reader = reader;
            _writer = writer;


            Weights = new Weights();
            Weights.Real = _fields.Where(f => f.Type == Field.FieldType.Continuous).Select(f => f.Weight).ToArray();
            Weights.Discrete = _fields.Where(f => f.Type == Field.FieldType.Discrete)
                                     .Concat(_fields.Where(f => f.Type == Field.FieldType.DiscreteDataDriven))
                                     .Select(f => f.Weight).ToArray();
        }


        /// <summary>
        /// Converts the data set to a binary file.
        /// </summary>
        public void ConvertToBinary(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            using (var outfile = new BinaryOutFile(filename, Weights))
            {
                bool status = true;
                int numRecords = 0;

                //----------------------
                // write the example to the file
                //
                foreach (var record in _reader)
                    outfile.WriteRecord(record);

                //-----------------------------
	            // write header information
	            //
                outfile.WriteHeader(numRecords);

                return numRecords;
            }
        }

        
    }
}
