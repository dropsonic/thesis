﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.DataCleansing;
using Thesis.DDMS;
using Thesis.Orca;

namespace Thesis.App
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
                using (IDataReader binReader = new BinaryDataReader(reader, binFile))          // convert input data to binary format
                using (ScaleDataReader scaleReader = new MinmaxScaleDataReader(binReader))     // scale input data
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

                        using (IDataReader cleanReader = new CleanDataReader(scaleReader, anomalies)) // clean input data from anomalies
                        {
                            Console.WriteLine("Enter epsilon:");
                            double eps;
                            while (!double.TryParse(Console.ReadLine(), out eps))
                                Console.WriteLine("Wrong format. Please enter epsilon again.");

                            var model = new SystemModel(eps);
                            model.AddRegime("Nominal", cleanReader);
                            var nominal = model.Regimes.First();

                            Console.WriteLine("Knowledge base has been created. {0} cluster(s) total:", nominal.Clusters.Count);
                            int i = 0;
                            foreach (var cluster in nominal.Clusters)
                            {
                                Console.WriteLine("  --------------------------");
                                Console.WriteLine("  Cluster #{0}:", ++i);
                                Console.WriteLine("  Lower bound: {0}", String.Join(" | ", cluster.LowerBound));
                                Console.WriteLine("  Upper bound: {0}", String.Join(" | ", cluster.UpperBound));
                                Console.WriteLine("  Appropriate discrete values: {0}", String.Join(" | ", cluster.DiscreteValues.Select(f => String.Join("; ", f))));
                            }
                            Console.WriteLine("  --------------------------");

                            Console.WriteLine("\nEnter record, or press enter to quit.");

                            string line = String.Empty;
                            do 
                            {
                                line = Console.ReadLine();
                                if (String.IsNullOrEmpty(line)) break;

                                var record = parser.Parse(line, cleanReader.Fields);
                                if (record == null)
                                {
                                    Console.WriteLine("Wrong record format. Please enter record again.");
                                    continue;
                                }

                                scaleReader.ScaleRecord(record);
                                Regime currentRegime = model.DetectRegime(record);
                                if (currentRegime == null)
                                    Console.WriteLine("Anomaly behavior detected.\n");
                                else
                                    Console.WriteLine("Current regime: {0}\n", currentRegime.Name);
                            } while (true);
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
