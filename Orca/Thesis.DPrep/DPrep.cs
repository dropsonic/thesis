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
    public class DPrep
    {
        public Parameters Parameters { get; set; }

        public DPrep()
        {
            Parameters = new Parameters();
        }

        public DPrep(Parameters parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null);

            Parameters = parameters;
        }

        /// <summary>
        /// Converts the data set to a binary file.
        /// </summary>
        /// <returns>Records count.</returns>
        private int ConvertToBinary(IDataReader reader, string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(reader != null);

            using (var outfile = new OrcaBinaryWriter(filename, reader.Fields))
            {
                //----------------------
                // write the example to the file
                //
                foreach (var record in reader)
                    outfile.WriteRecord(record);
            }

            return reader.Index;
        }

        /// <returns>Number of converted records.</returns>
        public int Run(IDataReader reader, string destFile)
        {
            Contract.Requires<ArgumentNullException>(reader != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(destFile));

            Random random = new Random(Parameters.Seed);
            List<string> files = new List<string>();

            //-------------------------------------------------------------
            // Convert Data set to binary format
            //
            string outputName = "tmp.out";
            files.Add(outputName);
            int convertedRecords = ConvertToBinary(reader, outputName);

            //-------------------------------------------------------------
            // Randomize data set 
            //
            if (Parameters.Randomize)
            {
                ShuffleFile bScale = new ShuffleFile(files.Last());
                string randOutputFile = "out.rand";
                files.Add(randOutputFile);
                bScale.MultiShuffle(randOutputFile, Parameters.Iterations, Parameters.RandFiles, Parameters.Seed);
                bScale.Dispose();
            }

            //-------------------------------------------------------------
            // rename last temporary file to destination file
            //
            if (File.Exists(destFile))
                File.Delete(destFile);
            File.Move(files.Last(), destFile);

            //-------------------------------------------------------------
            // clean temporary files 
            //
            for (int i = 1; i < files.Count; i++)
            {
                try
                {
                    File.Delete(files[i]);
                }
                catch
                { }
            }

            return convertedRecords;
        }
    }
}
