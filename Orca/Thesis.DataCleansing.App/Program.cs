using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca;

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

            if (args.Length < 2)
            {
                Console.WriteLine("Wrong args.");
                Environment.Exit(0);
            }

            string dataFile = args[0];
            string fieldsFile = args[1];
            string binFile = String.Concat(dataFile, ".bin");
            string shuffleFile = String.Concat(binFile, ".shuffle");

            try
            {
                IRecordParser<string> parser = new PlainTextParser();

                using (IDataReader reader = new PlainTextReader(dataFile, fieldsFile, parser)) // read input data   
                using (IDataReader binReader = new BinaryDataReader(reader, binFile))
                using (IDataReader scaleReader = new MinmaxScaleDataReader(binReader))
                {
                    scaleReader.Shuffle(shuffleFile);

                    using (IDataReader cases = new BinaryDataReader(shuffleFile))
                    using (IDataReader references = new BinaryDataReader(shuffleFile))
                    {
                        var orca = new OrcaAD(DistanceFunctions.Euclid, neighborsCount: 10);
                        var outliers = orca.Run(cases, references, true);

                        IAnomaliesFilter filter = new GaussianFilter();
                        //IAnomaliesFilter filter = new DifferenceFilter(0.05);
                        var anomalies = filter.Filter(outliers);

                        Console.WriteLine("Anomalies:");
                        foreach (var anomaly in anomalies)
                            Console.WriteLine("  Id = {0}, Score = {1}", anomaly.Id, anomaly.Score);

                        Console.WriteLine();
                        Console.ReadKey();

                        using (IDataReader cleanReader = new CleanDataReader(binReader, anomalies))
                        {
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
                    }
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
            finally
            {
                File.Delete(shuffleFile);
                File.Delete(binFile);
            }
        }
    }
}
