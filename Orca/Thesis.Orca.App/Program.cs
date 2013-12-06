﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Thesis.Orca.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Contract.ContractFailed += (s, e) =>
            {
                Console.WriteLine("Something went wrong. Please contact the developer.");
            };

            if (args.Length < 1)
            {
                Console.WriteLine("Wrong args.");
                Environment.Exit(0);
            }

            try
            {
                Parameters parameters = new Parameters();
                int numOutliers;
                if (args.Length > 1 && int.TryParse(args[1], out numOutliers))
                    parameters.NumOutliers = numOutliers;

                parameters.ScoreFunction = args.Contains("-kth") ? ScoreFunctions.KthNeighbor 
                    : (args.Contains("-sum") ? ScoreFunctions.Sum : ScoreFunctions.Average);

                int batchIndex = Array.IndexOf(args, "-b", 1);
                if (batchIndex > 0 && args.Length > batchIndex + 1)
                {
                    int batchSize;
                    if (int.TryParse(args[batchIndex + 1], out batchSize))
                        parameters.BatchSize = batchSize;
                }

                int nIndex = Array.IndexOf(args, "-n", 1);
                if (nIndex > 0 && args.Length > nIndex + 1)
                {
                    int nCount;
                    if (int.TryParse(args[nIndex + 1], out nCount))
                        parameters.NumOutliers = nCount;
                }

                int kIndex = Array.IndexOf(args, "-k", 1);
                if (kIndex > 0 && args.Length > kIndex + 1)
                {
                    int kCount;
                    if (int.TryParse(args[kIndex + 1], out kCount))
                        parameters.NeighborsCount = kCount;
                }

                int outIndex = Array.IndexOf(args, "-o", 1);
                bool writeToFile = false;
                string outFilename = String.Empty;
                if (outIndex > 0 && args.Length > outIndex + 1)
                {
                    writeToFile = true;
                    outFilename = args[outIndex + 1];
                }


                Orca orca = new Orca(parameters);
                Console.WriteLine("Processing data...");
                var results = orca.Run(args[0]);
                Console.WriteLine("Done!");
                Console.WriteLine();

                if (writeToFile)
                {
                    using (var writer = new StreamWriter(outFilename, false))
                    {
                        foreach (var result in results)
                        {
                            writer.WriteLine("{0}, {1}", result.Id, result.Score);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Results:");
                    int i = 1;
                    foreach (var result in results)
                    {
                        Console.WriteLine("  {0}) Record #{1}: score = {2}", i++, result.Id, result.Score);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}. Please contact the developer.", ex.Message);
            }
        }
    }
}
