using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Wrong args.");
                Environment.Exit(0);
            }

            Parameters parameters = new Parameters(args[0], args[1], args[2]);
            DPrep.Run(parameters);
            Console.WriteLine("Done!");
        }
    }
}
