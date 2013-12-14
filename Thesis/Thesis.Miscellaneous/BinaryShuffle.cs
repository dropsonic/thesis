using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    class BinaryShuffle
    {
        private IDataReader _sourceReader;
        private IDataReader _shuffleReader;

        private string _shuffleFile;

        private int _iterations;
        private int _randFilesCount;
        private Random _rand;

        /// <param name="sourceReader">Reader for input data.</param>
        /// <param name="shuffleFile">Name of shuffle binary file.</param>
        /// <param name="iterations">Number of shuffle iterations.</param>
        /// <param name="randFilesCount">Number of random part-files.</param>
        public BinaryShuffle(IDataReader sourceReader, string shuffleFile, 
                             int iterations = 5, int randFilesCount = 10)
        {
            Contract.Requires<ArgumentNullException>(sourceReader != null);
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(shuffleFile));
            Contract.Requires<ArgumentOutOfRangeException>(iterations >= 1);
            Contract.Requires<ArgumentOutOfRangeException>(randFilesCount >= 1);

            _sourceReader = sourceReader;
            _shuffleFile = shuffleFile;
            _iterations = iterations;
            _randFilesCount = randFilesCount;

            _rand = new Random((int)DateTime.Now.Ticks);

            MultiShuffle();
        }


        private void SetFileReader(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            // if infile_ already points to a file, close it
            if (_shuffleReader != null)
                _shuffleReader.Dispose();

            _shuffleReader = new BinaryDataReader(filename);
        }

        private void ResetFileReader()
        {
            if (_shuffleReader != null)
                _shuffleReader.Dispose();
        }

        public void MultiShuffle()
        {
            Shuffle(_sourceReader, _shuffleFile);
            _shuffleReader = new BinaryDataReader(_shuffleFile);
            
            for (int i = 1; i < _iterations; i++)
            {
                Shuffle(_shuffleReader, _shuffleFile);
                SetFileReader(_shuffleFile);
            }

            ResetFileReader();
        }

        private void Shuffle(IDataReader sourceReader, string filename)
        {
            //-------------------------
            // set up tmp file names
            //
            string[] tmpFileNames = new string[_randFilesCount];
            for (int i = 0; i < _randFilesCount; i++)
                tmpFileNames[i] = filename + ".tmp." + i.ToString();


            //-------------------------------
            // open files for writing
            //
            IDataWriter[] tmpFilesOut = new BinaryDataWriter[_randFilesCount];
            try
            {
                for (int i = 0; i < tmpFileNames.Length; i++)
                    tmpFilesOut[i] = new BinaryDataWriter(tmpFileNames[i], _sourceReader.Fields);

                //--------------------------------
                // read in data file and randomly shuffle examples to
                // temporary files
                //
                foreach (var rec in _sourceReader)
                {
                    int index = _rand.Next(tmpFilesOut.Length);
                    tmpFilesOut[index].WriteRecord(rec);
                }
            }
            finally
            {
                // close temporary files
                for (int i = 0; i < tmpFilesOut.Length; i++)
                    if (tmpFilesOut[i] != null)
                        tmpFilesOut[i].Dispose();
            }

            //-------------------------------
            // open tmpfiles for reading 
            //

            IDataReader[] tmpFilesIn = new BinaryDataReader[_randFilesCount];
            try
            {
                for (int i = 0; i < tmpFilesIn.Length; i++)
                    tmpFilesIn[i] = new BinaryDataReader(tmpFileNames[i]);

                //-----------------------------------
                // open final destination file
                //

                ResetFileReader(); // closes original file

                using (IDataWriter outfile = new BinaryDataWriter(filename, _sourceReader.Fields))
                {
                    //--------------------------------------
                    // concatenate tmp files in random order
                    //
                    int[] order = new int[_randFilesCount];
                    for (int i = 0; i < _randFilesCount; i++)
                        order[i] = i;

                    // The modern version of the Fisher–Yates shuffle (the Knuth shuffle)
                    for (int i = order.Length - 1; i >= 0; i--)
                    {
                        int j = _rand.Next(i + 1);
                        int temp = order[i];
                        order[i] = order[j];
                        order[j] = temp;
                    }

                    for (int i = 0; i < order.Length; i++)
                    {
                        IDataReader infile = tmpFilesIn[order[i]];
                        foreach (var rec in infile)
                            outfile.WriteRecord(rec);
                    }
                }
            }
            finally
            {
                // close temporary files
                for (int i = 0; i < tmpFilesIn.Length; i++)
                    if (tmpFilesIn[i] != null)
                        tmpFilesIn[i].Dispose();
            }

            //-------------------------------
            // delete tmpfiles 
            //
            foreach (var fileName in tmpFileNames)
                File.Delete(fileName);

            ResetFileReader();
        }
    }
}
