using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.DPrep
{
    class ShuffleFile : IDisposable
    {
        IDataReader _infile;

        Weights _weights;

        string _dataFile;

        public ShuffleFile(string dataFile)
        {
            Contract.Requires(!String.IsNullOrEmpty(dataFile));

            _dataFile = dataFile;

            SetFileReader(dataFile);
        }

        private void SetFileReader(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            // if infile_ already points to a file, close it
            if (_infile != null)
                _infile.Dispose();

            _infile = new BinaryDataReader(filename);
        }

        private void ResetFileReader()
        {
            if (_infile != null)
                _infile.Dispose();
        }
        

        public void MultiShuffle(string destFile, int iterations, int tmpFiles, int seed)
        {
            Contract.Requires<ArgumentOutOfRangeException>(iterations > 0);
            Contract.Requires<ArgumentOutOfRangeException>(tmpFiles > 0);

            Random rand = new Random(seed);
            for (int i = 0; i < iterations; i++)
            {
                Shuffle(destFile, tmpFiles, rand);
                SetFileReader(destFile);
            }
            ResetFileReader();
        }

        private void Shuffle(string filename, int nTmpFiles, Random rand)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentOutOfRangeException>(nTmpFiles > 0);
            Contract.Requires<ArgumentNullException>(rand != null);

            //-------------------------
            // set up tmp file names
            //
            string[] tmpFileNames = new string[nTmpFiles];
            for (int i = 0; i < nTmpFiles; i++)
                tmpFileNames[i] = filename + ".tmp." + i.ToString();


            //-------------------------------
            // open files for writing
            //
            IDataWriter[] tmpFilesOut = new BinaryDataWriter[nTmpFiles];
            try
            {
                for (int i = 0; i < tmpFileNames.Length; i++)
                    tmpFilesOut[i] = new BinaryDataWriter(tmpFileNames[i], _infile.Fields);

                //--------------------------------
                // read in data file and randomly shuffle examples to
                // temporary files
                //
                foreach (var rec in _infile)
                {
                    int index = rand.Next(tmpFilesOut.Length);
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

            IDataReader[] tmpFilesIn = new BinaryDataReader[nTmpFiles];
            try
            {
                for (int i = 0; i < tmpFilesIn.Length; i++)
                    tmpFilesIn[i] = new BinaryDataReader(tmpFileNames[i]);

                //-----------------------------------
                // open final destination file
                //

                ResetFileReader(); // closes original file

                using (IDataWriter outfile = new BinaryDataWriter(filename, _infile.Fields))
                {
                    //--------------------------------------
                    // concatenate tmp files in random order
                    //
                    int[] order = new int[nTmpFiles];
                    for (int i = 0; i < nTmpFiles; i++)
                        order[i] = i;

                    // The modern version of the Fisher–Yates shuffle (the Knuth shuffle)
                    for (int i = order.Length-1; i >= 0; i--)
                    {
                        int j = rand.Next(i + 1);
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
        }


        
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
 
        private bool m_Disposed = false;
 
        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                // Managed resources are released here.
                    _infile.Dispose();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~ShuffleFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
