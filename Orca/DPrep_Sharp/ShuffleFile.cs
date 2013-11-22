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
        BinaryInFile _infile;

        float[] _realWeights;
        float[] _discreteWeights;

        string _dataFile;

        float _missingR;
        int _missingD;

        public ShuffleFile(string dataFile, float[] realWeights, float[] discreteWeights, float missingR, int missingD)
        {
            Contract.Requires(!String.IsNullOrEmpty(dataFile));
            Contract.Requires<ArgumentNullException>(realWeights != null);
            Contract.Requires<ArgumentNullException>(discreteWeights != null);

            _realWeights = realWeights;
            _discreteWeights = discreteWeights;
            _dataFile = dataFile;
            _missingR = missingR;
            _missingD = missingD;

            SetFileReader(dataFile);
        }

        private void SetFileReader(string filename)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            // if infile_ already points to a file, close it
            if (_infile != null)
                _infile.Dispose();

            _infile = new BinaryInFile(filename);
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
                Shuffle(destFile, 10000, tmpFiles, rand);
                SetFileReader(destFile);
            }
            ResetFileReader();
        }

        private void Shuffle(string filename, int blockSize, int nTmpFiles, Random rand)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentOutOfRangeException>(blockSize > 0);
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
            BinaryWriter[] tmpFilesOut = new BinaryWriter[nTmpFiles];
            try
            {
                for (int i = 0; i < tmpFileNames.Length; i++)
                    tmpFilesOut[i] = new BinaryWriter(File.Create(tmpFileNames[i]));

                //--------------------------------
                // read in data file and randomly shuffle examples to
                // temporary files
                //
                _infile.ForEach((rec) =>
                    {
                        int index = rand.Next(tmpFilesOut.Length);

                        tmpFilesOut[index].Write(rec.Id);
                        tmpFilesOut[index].Write(rec.Real);
                        tmpFilesOut[index].Write(rec.Discrete);
                    });
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
            
            BinaryReader[] tmpFilesIn = new BinaryReader[nTmpFiles];
            try
            {
                for (int i = 0; i < tmpFilesIn.Length; i++)
                    tmpFilesIn[i] = new BinaryReader(File.OpenRead(tmpFileNames[i]));

                //-----------------------------------
                // open final destination file
                //

                ResetFileReader(); // closes original file

                using (var outfile = new BinaryOutFile(filename, _realWeights, _discreteWeights))
                {
                    outfile.WriteHeader(_infile.RecordsCount);

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
                        int count = blockSize;
                        BinaryReader infile = tmpFilesIn[order[i]];
                        while (count == blockSize)
                        {
                            byte[] temp = infile.ReadBytes(blockSize);
                            count = temp.Length;
                            outfile.Write(temp);
                        }
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
