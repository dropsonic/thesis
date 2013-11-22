using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Thesis.DPrep
{
    public class DPrep
    {
        Parameters _parameters;

        public DPrep(Parameters parameters)
        {
            _parameters = parameters;
        }

        public static void Run(Parameters parameters)
        {
            Contract.Requires<ArgumentNullException>(parameters != null);

            Random random = new Random(parameters.Seed);
            List<string> files = new List<string>();
            files.Add(parameters.DataFile);

            //-------------------------------------------------------------
            // Create the DataTable (load the Names File)
            //
            DataTable dataTable = new DataTable(parameters.DataFile, 
                                                parameters.NamesFiles, 
                                                parameters.MissingR, 
                                                parameters.MissingD,
                                                parameters.RealWeight,
                                                parameters.DiscreteWeight);

            //-------------------------------------------------------------
            // Load the Scale File 
            //
            RStats rStats = new RStats(dataTable.RealFieldsCount);
            if (!String.IsNullOrEmpty(parameters.ScaleFile))
            {
                rStats.Load(parameters.ScaleFile);
            }

            //-------------------------------------------------------------
            // Convert Data set to binary format
            //
            string outputName = parameters.TempFileStem + ".out";
            files.Add(outputName);
            int convertedRecords = dataTable.ConvertToBinary(outputName);

            if (convertedRecords == 0)
                throw new Exception("No records converted.");

            dataTable.Dispose();

            //-------------------------------------------------------------
            // Scale data set 
            //
            if (parameters.Scaling != Parameters.Scale.None)
            {
                BFile bFile = new BFile(outputName, parameters.MissingR, parameters.MissingD);
                string scaleOutputName = parameters.TempFileStem + ".scale";
                files.Add(scaleOutputName);

                if (parameters.Scaling == Parameters.Scale.ZeroToOne)
                {
                    if (String.IsNullOrEmpty(parameters.ScaleFile))
                        bFile.GetMaxMin(rStats.Max, rStats.Min);
                    bFile.ScaleZeroToOne(scaleOutputName, rStats.Max, rStats.Min);
                }
                else if (parameters.Scaling == Parameters.Scale.Std)
                {
                    if (String.IsNullOrEmpty(parameters.ScaleFile))
                        bFile.GetMeanStd(rStats.Mean, rStats.Std);
                    bFile.ScaleStd(scaleOutputName, rStats.Mean, rStats.Std);
                }

                bFile.Dispose();
            }

            //-------------------------------------------------------------
            // Randomize data set 
            //
            if (parameters.Randomize)
            {
                BFile bScale = new BFile(files.Last(), parameters.MissingR, parameters.MissingD);
                string randOutputFile = parameters.TempFileStem + ".rand";
                files.Add(randOutputFile);
                bScale.MultiShuffle(randOutputFile, parameters.Iterations, parameters.RandFiles, parameters.Seed);
                bScale.Dispose();
            }

            //-------------------------------------------------------------
            // rename last temporary file to destination file
            //
            if (File.Exists(parameters.DestinationFile))
                File.Delete(parameters.DestinationFile);
            File.Move(files.Last(), parameters.DestinationFile);

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
    }
}
