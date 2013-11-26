using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
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

            Parameters parameters = new Parameters(args[0], args[1], args[2]);
            if (args.Contains("-norand"))
                parameters.Randomize = false;

            DPrep dprep = new DPrep(parameters);
            dprep.Run();
            Console.WriteLine("Done!");
        }
    }
}
