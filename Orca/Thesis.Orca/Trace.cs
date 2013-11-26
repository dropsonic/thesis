using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.Orca
{
    static class Trace
    {
        [Conditional("DEBUG")]
        public static void PrintRecords(IEnumerable<Record> records)
        {
            Console.WriteLine("-----------TRACE-----------");
            IEnumerable<Record> pRecords = records.Count() <= 10 ? records :
                records.Take(10);
            foreach (var record in pRecords)
            {
                Console.Write("#{0}: ", record.Id);
                foreach (var real in record.Real)
                    Console.Write("{0} ", real);
                Console.Write("| ");
                foreach (var discrete in record.Discrete)
                    Console.Write("{0} ", discrete);
                Console.WriteLine();
            }
            Console.WriteLine("-----------END TRACE-----------");
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Console.WriteLine("TRACE: " + message);
        }
    }
}
