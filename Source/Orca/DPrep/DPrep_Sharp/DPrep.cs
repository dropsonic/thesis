using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Random random = new Random(parameters.Seed);
            List<string> files = new List<string>();
            files.Add(parameters.DataFile);

            //-------------------------------------------------------------
            // Create the DataTable (load the Names File)
            //
            DataTable dataTable = new DataTable(parameters.DataFile, 
                                                parameters.NamesFiles, 
                                                parameters.MissingR, 
                                                parameters.MissingD);

            //-------------------------------------------------------------
            // Write weight file
            //
            dataTable.WriteWeightFile(parameters.WeightFile);
            
            // Load the Scale File
            RStats rStats = new RStats(dataTable.RealFieldsCount);
            if (!String.IsNullOrEmpty(parameters.ScaleFile))
            {
                rStats.Load(parameters.ScaleFile);
            }

            string outputName = parameters.TempFileStem + ".out";
            files.Add(outputName);
            int convertedRecords = dataTable.ConvertToBinary(outputName);

            if (convertedRecords == 0)
                throw new Exception("No records converted.");

            if (parameters.Scaling != Parameters.Scale.None)
            {
                BFile bfile = new BFile(outputName, parameters.MissingR, parameters.MissingD);
                string scaleOutputName = parameters.TempFileStem + ".scale";
                files.Add(scaleOutputName);

                if (parameters.Scaling == Parameters.Scale.ZeroToOne)
                {
                    if (String.IsNullOrEmpty(parameters.ScaleFile))
                        bfile.GetMaxMin(rStats.Max, rStats.Min);
                    bfile.ScaleZeroToOne(scaleOutputName, rStats.Max, rStats.Min);
                }
                else if (parameters.Scaling == Parameters.Scale.Std)
                {
                    if (String.IsNullOrEmpty(parameters.ScaleFile))
                        bfile.GetMeanStd(rStats.Mean, rStats.Std);
                    bfile.ScaleStd(scaleOutputName, rStats.Mean, rStats.Std);
                }
            }
        }
    }
}
