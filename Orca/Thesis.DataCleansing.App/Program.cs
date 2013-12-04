using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DataCleansing.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Contract.ContractFailed += (s, e) =>
            {
                Console.WriteLine("Something went wrong. Please contact the developer.");
            };

            if (args.Length < 3)
            {
                Console.WriteLine("Wrong args.");
                Environment.Exit(0);
            }

            try
            {
                string dataFile = args[0];
                string fieldsFile = args[1];
                string outputFile = args[2];

                IRecordParser<string> parser = new PlainTextParser();
                
                IDataReader reader = new PlainTextReader(dataFile, fieldsFile, parser);
                var dprep = new DPrep.DPrep();
                dprep.Run(reader, outputFile);

                var orca = new Orca.Orca();
                var outliers = orca.Run(outputFile, true);

                IAnomaliesFilter filter = new GaussianFilter();
                //IAnomaliesFilter filter = new DifferenceFilter(0.05);
                var anomalies = filter.Filter(outliers);

                Console.WriteLine("Anomalies:");
                foreach (var anomaly in anomalies)
                    Console.WriteLine("  Id = {0}, Score = {1}", anomaly.Id, anomaly.Score);

                Console.WriteLine();

                IDataReader cleanReader = new CleanDataReader(reader, anomalies);

                Console.WriteLine("Results:");
                foreach (var record in cleanReader)
                {
                    StringBuilder s = new StringBuilder("  ");
                    s.Append("Id = ")
                     .Append(record.Id)
                     .Append(" | ")
                     .Append(String.Join(" ", record.Real))
                     .Append(" | ")
                     .Append(String.Join(" ", record.Discrete));
                    
                    Console.WriteLine(s.ToString());
                }
            }
            catch (DataFormatException)
            {
                Console.WriteLine("Incorrect input data format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0} Please contact the developer.", ex.Message);
            }
        }
    }
}
