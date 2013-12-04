using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TelemetryGenerator
{
    /// <summary>
    /// Retrieves live data from the ISSLive website (http), parses it and writes to the file.
    /// </summary>
    class ISSDataRetriever
    {
        private string _url;
        private FirefoxDriver _browser;
        private object _syncLock = new object();

        public ISSDataRetriever(string url)
        {
            _url = url;
            _browser = new FirefoxDriver();
        }

        public void GetData(CancellationToken token, string fileName, string separator = ";")
        {
            if (token == null)
                throw new ArgumentNullException("token");
            _browser.Navigate().GoToUrl(_url);
            _browser.FindElementById("EduTableScrollButton").Click();

            bool writeHeader = !File.Exists(fileName);

            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                //Write header
                if (writeHeader)
                    WriteLine(writer, GetFieldNames(), separator);
                else
                    writer.WriteLine("% {0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                string previousKey = String.Empty;
                //Write data
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var record = GetRecord();
                        if (record[0] != previousKey) // if page has been updated
                        {
                            WriteLine(writer, record, separator);
                            previousKey = record[0];
                            OnLineCompleted();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        OnLineFailed();
                    }
                }
            }
        }

        public event EventHandler LineCompleted;
        public event EventHandler LineFailed;

        protected void OnLineCompleted()
        {
            if (LineCompleted != null)
                LineCompleted(this, EventArgs.Empty);
        }

        protected void OnLineFailed()
        {
            if (LineFailed != null)
                LineFailed(this, EventArgs.Empty);
        }

        void WriteLine(TextWriter writer, string[] values, string separator)
        {
            writer.WriteLine(String.Join(separator, values));
        }

        string[] GetRecord()
        {
            return _browser.FindElementByXPath(@"(//div[contains(@class, 'messageContainer')])[3]")
                           .FindElements(By.ClassName("messageData"))
                           .Select(element => element.Text)
                           .ToArray();
        }

        string[] GetFieldNames()
        {
            return _browser.FindElementByClassName("messageContainerHead")
                            .FindElements(By.ClassName("messageNameHead"))
                            .Select(element => element.Text)
                            .ToArray();
        }
    }
}
