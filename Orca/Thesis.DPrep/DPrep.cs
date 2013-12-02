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

        private IDataReader _reader;
        private IDataWriter _writer;

        public DPrep(IDataReader reader, IDataWriter writer, Parameters parameters)
        {
            Contract.Requires<ArgumentNullException>(reader != null);
            Contract.Requires<ArgumentNullException>(writer != null);

            _reader = reader;
            _writer = writer;
            Parameters = parameters;
        }

        /// <returns>Number of converted records.</returns>
        public void Run()
        {
            Contract.Requires<ArgumentNullException>(Parameters != null);

            Random random = new Random(Parameters.Seed);
            List<string> files = new List<string>();

            //-------------------------------------------------------------
            // Create the DataTable (load the Names File)
            //
            DataTable dataTable = new DataTable(_reader,
                                                _writer);
            
            //-------------------------------------------------------------
            // Convert Data set to binary format
            //
            string outputName = Parameters.TempFileStem + ".out";
            files.Add(outputName);
            dataTable.ConvertToBinary(outputName);

            //-------------------------------------------------------------
            // Scale data set 
            //
            if (Parameters.Scaling != Parameters.Scale.None)
            {
                ScaleFile scaleFile = new ScaleFile(outputName, dataTable.Weights, 
                    Parameters.MissingR, Parameters.MissingD);
                string scaleOutputName = Parameters.TempFileStem + ".scale";
                files.Add(scaleOutputName);

                if (Parameters.Scaling == Parameters.Scale.ZeroToOne)
                {
                    scaleFile.GetMaxMin(rStats.Max, rStats.Min);
                    scaleFile.ScaleZeroToOne(scaleOutputName, rStats.Max, rStats.Min);
                }
                else if (Parameters.Scaling == Parameters.Scale.Std)
                {
                    scaleFile.GetMeanStd(rStats.Mean, rStats.Std);
                    scaleFile.ScaleStd(scaleOutputName, rStats.Mean, rStats.Std);
                }

                scaleFile.Dispose();
            }

            //-------------------------------------------------------------
            // Randomize data set 
            //
            if (Parameters.Randomize)
            {
                ShuffleFile bScale = new ShuffleFile(files.Last(), dataTable.Weights,
                    Parameters.MissingR, Parameters.MissingD);
                string randOutputFile = Parameters.TempFileStem + ".rand";
                files.Add(randOutputFile);
                bScale.MultiShuffle(randOutputFile, Parameters.Iterations, Parameters.RandFiles, Parameters.Seed);
                bScale.Dispose();
            }

            //-------------------------------------------------------------
            // copy data from last temp file to writer
            //
            CopyData(files.Last());

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
        }

        private void CopyData(string outFile)
        {
            _writer.WriteHeader(_reader.Fields);
            using (var infile = new BinaryInFile(outFile))
            {
                foreach (var record in infile)
                    _writer.WriteRecord(record);
            }
        }
    }
}
