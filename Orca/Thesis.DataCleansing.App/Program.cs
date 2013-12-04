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
