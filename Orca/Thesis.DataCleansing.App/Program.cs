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
                string outputFile = args[2];

                if (File.Exists(outputFile))
                    File.Delete(outputFile);
                File.Copy(dataFile, outputFile);

                var aDetector = new AnomaliesDetector(new PlainTextReader(args[0], args[1]), new GaussianFilter());
                var anomalies = aDetector.FindAnomalies().ToList();
                anomalies.Sort();

                int recId = 0;
                int a = 0;
                using (var reader = new StreamReader(dataFile))
                using (var writer = new StreamWriter(outputFile))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.Length > 0 && line[0] != '%')
                        {
                            recId++;
                            if (a < anomalies.Count - 1 && recId == anomalies[a])
                            {
                                line = String.Concat("% ", line);
                                a++;
                            }
                        }
                        writer.WriteLine(line);
                    }
                }

                Console.WriteLine("Done!");
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
