using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Parameters parameters = new Parameters(args[0]);
                int numOutliers;
                if (args.Length > 1 && int.TryParse(args[1], out numOutliers))
                    parameters.NumOutliers = numOutliers;

                if (args.Contains("-kth"))
                    parameters.ScoreF = Parameters.DistanceType.KthNeighbor;

                int batchIndex = Array.IndexOf(args, "-b", 1);
                if (batchIndex > 0 && args.Length > batchIndex + 1)
                {
                    int batchSize;
                    if (int.TryParse(args[batchIndex + 1], out batchSize))
                        parameters.BatchSize = batchSize;
                }


                Orca orca = new Orca(parameters);
                Console.WriteLine("Processing data...");
                var results = orca.Run();
                Console.WriteLine("Done!");
                Console.WriteLine();

                Console.WriteLine("Results:");
                int i = 1;
                foreach (var result in results)
                {
                    Console.WriteLine("  {0}) Record #{1}: score = {2}", i++, result.Record.Id, result.Score);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}. Please contact the developer.", ex.Message);
            }
        }
    }
}
