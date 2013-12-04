using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep.App
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
                Parameters parameters = new Parameters();
                if (args.Contains("-norand"))
                    parameters.Randomize = false;

                IRecordParser<string> parser = new PlainTextParser();
                IDataReader textReader = new PlainTextReader(args[0], args[1], parser);
                DPrep dprep = new DPrep(textReader, args[2], parameters);
                Console.WriteLine("Converting data...");
                dprep.Run();
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
