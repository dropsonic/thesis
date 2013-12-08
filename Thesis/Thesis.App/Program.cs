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
        static string GetBinName(string name)
        {
            return String.Concat(name, ".bin");
        }

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
            
            string fieldsFile = args[0];
            string dataFile = args[1];
            string shuffleFile = String.Concat(dataFile, ".shuffle");

            try
            {
                foreach (string filename in args)
                    if (!File.Exists(filename))
                        throw new ApplicationException("File not found.");

                IRecordParser<string> parser = new PlainTextParser();

                using (IDataReader reader = new PlainTextReader(dataFile, fieldsFile, parser))      // read input data   
                using (IDataReader binReader = new BinaryDataReader(reader, GetBinName(dataFile)))  // convert input data to binary format
                {
                    IScaling scaling = new MinmaxScaling(binReader);

                    Console.WriteLine("Enter epsilon:");
                    double eps;
                    while (!double.TryParse(Console.ReadLine(), out eps))
                        Console.WriteLine("Wrong format. Please enter epsilon again.");

                    var model = new SystemModel(eps);

                    for (int i = 2; i < args.Length; i++)
                    {
                        string fileName = args[i];
                        string regimeName = Path.GetFileNameWithoutExtension(fileName);

                        using (IDataReader tempReader = new PlainTextReader(fileName, fieldsFile, parser))
                        using (IDataReader tempBinReader = new BinaryDataReader(tempReader, GetBinName(fileName)))
                        {
                            using (IDataReader tempScaleReader = new ScaleDataReader(tempBinReader, scaling))
                            {
                                model.AddRegime(regimeName, tempScaleReader);
                            }
                        }
                    }

                    using (ScaleDataReader scaleReader = new ScaleDataReader(binReader, scaling)) // scale input data
                    {
                        scaleReader.Shuffle(shuffleFile);
                        IEnumerable<Outlier> outliers = Enumerable.Empty<Outlier>();

                        using (IDataReader cases = new BinaryDataReader(shuffleFile))
                        using (IDataReader references = new BinaryDataReader(shuffleFile))
                        {
                            var orca = new OrcaAD(DistanceFunctions.Euclid, neighborsCount: 10);
                            outliers = orca.Run(cases, references, true);
                        }

                        File.Delete(shuffleFile);

                        IAnomaliesFilter filter = new GaussianFilter();
                        //IAnomaliesFilter filter = new DifferenceFilter(0.05);
                        var anomalies = filter.Filter(outliers);

                        Console.WriteLine("Anomalies:");
                        foreach (var anomaly in anomalies)
                            Console.WriteLine("  Id = {0}, Score = {1}", anomaly.Id, anomaly.Score);

                        Console.WriteLine();

                        using (IDataReader cleanReader = new CleanDataReader(scaleReader, anomalies)) // clean input data from anomalies
                        {
                            model.AddRegime("Nominal", cleanReader);

                            Console.WriteLine("Knowledge base has been created. {0} regime(s) total:", model.Regimes.Count);
                            foreach (var regime in model.Regimes)
                            {
                                Console.WriteLine("\n***** {0} *****", regime.Name);
                                Console.WriteLine("{0} cluster(s) in regime.", regime.Clusters.Count);
                                int i = 0;
                                foreach (var cluster in regime.Clusters)
                                {
                                    Console.WriteLine("  --------------------------");
                                    Console.WriteLine("  Cluster #{0}:", ++i);
                                    Console.WriteLine("  Lower bound: {0}", String.Join(" | ", scaling.Unscale(cluster.LowerBound)));
                                    Console.WriteLine("  Upper bound: {0}", String.Join(" | ", scaling.Unscale(cluster.UpperBound)));
                                    Console.WriteLine("  Appropriate discrete values: {0}", String.Join(" | ", cluster.DiscreteValues.Select(f => String.Join("; ", f))));
                                }
                                Console.WriteLine("  --------------------------");
                                Console.WriteLine("******************", regime.Name);
                            }

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

                                scaling.Scale(record);
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
        }
    }
}
