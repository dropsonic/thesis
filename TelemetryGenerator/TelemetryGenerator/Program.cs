using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Threading;

namespace TelemetryGenerator
{
    class Program
    {
        static int _completed = 0;
        static int _failed = 0;

        static void Main(string[] args)
        {
            string fileName = "iss_cmg.data";
            if (args.Length > 1)
            {
                fileName = args[0];
            }

            Console.CursorVisible = false;
            Console.WriteLine("Starting Firefox...");
            var iss = new ISSDataRetriever(@"http://spacestationlive.nasa.gov/displays/adcoDisplay3.html");
            iss.LineCompleted += iss_LineCompleted;
            iss.LineFailed += iss_LineFailed;
            Console.WriteLine("Done.\n");

            var cancelSource = new CancellationTokenSource();

            Thread thread = new Thread(RetrieveData);
            thread.IsBackground = false;
            thread.Start(new object[] { iss, cancelSource.Token, fileName });

            Console.WriteLine("Retrieving data... Press any key to stop.");
            Console.WriteLine();
            Console.WriteLine("Progress:");
            Console.ReadKey();
            cancelSource.Cancel();
        }

        static void iss_LineCompleted(object sender, EventArgs e)
        {
            Console.SetCursorPosition(0, 6);
            Console.Write("{0} attempts completed", _completed);
            Console.SetCursorPosition(0, 8);
            _completed++;
        }
        static void iss_LineFailed(object sender, EventArgs e)
        {
            Console.SetCursorPosition(0, 7);
            Console.Write("{0} attempts failed", _failed);
            Console.SetCursorPosition(0, 8);
            _failed++;
        }

        static void RetrieveData(object parameters)
        {
            var args = (object[])parameters;
            var iss = (ISSDataRetriever)args[0];
            var token = (CancellationToken)args[1];
            var fileName = (string)args[2];
            iss.GetData(token, fileName);
        }
    }
}
