using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Wrong args.");
                Environment.Exit(0);
            }

            Parameters parameters = new Parameters(args[0]);
            int numOutliers;
            if (args.Length > 1 && int.TryParse(args[1], out numOutliers))
                parameters.NumOutliers = numOutliers;

            Orca orca = new Orca(parameters);
            var results = orca.Run();
            Console.WriteLine("Done!");
            Console.WriteLine();

            Console.WriteLine("Results:");
            foreach (var result in results)
            {
                Console.WriteLine("  Record #{0}: score = {1}", result.Index, result.Score);
            }

            Console.ReadKey();
        }
    }
}
