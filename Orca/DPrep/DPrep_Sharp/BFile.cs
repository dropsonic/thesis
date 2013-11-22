using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class BFile : IDisposable
    {
        BinaryInFile _infile;

        string _dataFile;

        float _missingR;
        int _missingD;

        public BFile(string filename, float missingR, int missingD)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));

            _dataFile = filename;
            _missingR = missingR;
            _missingD = missingD;

            SetFileReader(filename);
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

        /// <summary>
        /// Makes a pass through the data and calculates the max and min values
        /// for real variables.
        /// </summary>
        //public void GetMaxMin(ref IList<float> max, ref IList<float> min)
        public void GetMaxMin(float[] max, float[] min)
        {
            Contract.Requires(max.Length == _infile.RealFieldsCount);
            Contract.Requires(min.Length == _infile.RealFieldsCount);

            // initialize max and min vectors 
            for (int i = 0; i < _infile.RealFieldsCount; i++)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            // process rest of examples
            _infile.ForEach((id, R, D) =>
                {
                    for (int i = 0; i < _infile.RealFieldsCount; i++)
                    {
                        if (R[i] != _missingR)
                        {
                            if (R[i] < min[i])
                            {
                                min[i] = R[i];
                            }
                            else if (R[i] > max[i])
                            {
                                max[i] = R[i];
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// Makes a pass through the data and calculates the mean and standard
        /// deviation for real variables.
        /// </summary>
        public void GetMeanStd(float[] mean, float[] std)
        {
            Contract.Requires(mean.Length == _infile.RealFieldsCount);
            Contract.Requires(std.Length == _infile.RealFieldsCount);

            var sumv = new double[_infile.RealFieldsCount];
            var sumsqv = new double[_infile.RealFieldsCount];
            var num = new int[_infile.RealFieldsCount];

            _infile.ForEach((id, R, D) =>
                {
                    for (int i = 0; i < _infile.RealFieldsCount; i++)
                    {
                        if (R[i] != _missingR)
                        {
                            double r = ((double)R[i]);
                            sumv[i] += r;
                            sumsqv[i] += r * r;
                            num[i]++;
                        }
                    }

                    for (int i = 0; i < _infile.RealFieldsCount; i++)
                    {
                        if (num[i] > 1)
                        {
                            double meanValue = sumv[i] / num[i];
                            mean[i] = ((float)meanValue);

                            double stdValue = Math.Sqrt((sumsqv[i] - sumv[i] * sumv[i] / num[i]) / (num[i] - 1));
                            std[i] = ((float)stdValue);
                        }
                        else
                        {
                            mean[i] = 0;
                            std[i] = 0;
                        }
                        // error checkin
                        // check mean /std are not NaN
                    }
                });
        }

        /// <summary>
        /// Scales real values to the range [0,1].
        /// </summary>
        public void ScaleZeroToOne(string filename, float[] max, float[] min)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(max != null);
            Contract.Requires<ArgumentNullException>(min != null);

            //-------------------------------
	        // open file for writing
	        //
            using (var outfile = new BinaryOutFile(filename, _fields))
            {
	            // write header information
                outfile.Write(_records);
                outfile.Write(_realFieldsCount);
                outfile.Write(_discreteFieldsCount);

	            float[] range = new float[_realFieldsCount];

	            for (int i = 0; i < _realFieldsCount; i++) {
		            range[i] = max[i] - min[i];
	            };

	            //--------------------------------
	            // read in file and scale it
	            //
	            SeekPosition(0);

	            float[] Rscale = new float[_realFieldsCount];
                while (_index < _records)
                {
                    int id;
                    float[] R;
                    int[] D;
                    GetNext(out id, out R, out D);

                    for (int i = 0; i < _realFieldsCount; i++)
                    {
                        if (R[i] == _missingR)
                        {
                            Rscale[i] = _missingR;
                        }
                        else if (range[i] != 0)
                        {
                            Rscale[i] = (R[i] - min[i]) / range[i];
                        }
                        else
                        {
                            Rscale[i] = 0;
                        }
                    }

                    outfile.Write(id);
                    outfile.Write(Rscale);
                    outfile.Write(D);
                }

                outfile.Close();
	        }
        }

        /// <summary>
        /// Scales real values to standard deviations from the mean.
        /// </summary>
        public void ScaleStd(string filename, float[] mean, float[] std)
        {
            Contract.Requires(!String.IsNullOrEmpty(filename));
            Contract.Requires<ArgumentNullException>(mean != null);
            Contract.Requires<ArgumentNullException>(std != null);

            //-------------------------------
            // open file for writing
            //
            using (var outstream = File.Create(filename))
            using (BinaryWriter outfile = new BinaryWriter(outstream))
            {
                // write header information
                outfile.Write(_records);
                outfile.Write(_realFieldsCount);
                outfile.Write(_discreteFieldsCount);

                //--------------------------------
                // read in file and scale it
                //
                SeekPosition(0);

                float[] Rscale = new float[_realFieldsCount];
                while (_index < _records)
                {
                    int id;
                    float[] R;
                    int[] D;
                    GetNext(out id, out R, out D);

                    for (int i = 0; i < _realFieldsCount; i++)
                    {
                        if (R[i] == _missingR)
                        {
                            Rscale[i] = _missingR;
                        }
                        else if (std[i] != 0)
                        {
                            Rscale[i] = (R[i] - mean[i]) / std[i];
                        }
                        else
                        {
                            Rscale[i] = 0;
                        }
                    }

                    outfile.Write(id);
                    outfile.Write(Rscale);
                    outfile.Write(D);
                }

                outfile.Close();
            }
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

        private void Shuffle(string file, int blockSize, int nTmpFiles, Random rand)
        {
            Contract.Requires(!String.IsNullOrEmpty(file));
            Contract.Requires<ArgumentOutOfRangeException>(blockSize > 0);
            Contract.Requires<ArgumentOutOfRangeException>(nTmpFiles > 0);
            Contract.Requires<ArgumentNullException>(rand != null);

            //-------------------------
            // set up tmp file names
            //
            string[] tmpFileNames = new string[nTmpFiles];
            for (int i = 0; i < nTmpFiles; i++)
                tmpFileNames[i] = file + ".tmp." + i.ToString();


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
                SeekPosition(0);

                while (_index < _records)
                {
                    int id;
                    float[] R;
                    int[] D;
                    GetNext(out id, out R, out D);
                    int index = rand.Next(tmpFilesOut.Length);

                    tmpFilesOut[index].Write(id);
                    tmpFilesOut[index].Write(R);
                    tmpFilesOut[index].Write(D);
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
            
            BinaryReader[] tmpFilesIn = new BinaryReader[nTmpFiles];
            try
            {
                for (int i = 0; i < tmpFilesIn.Length; i++)
                    tmpFilesIn[i] = new BinaryReader(File.OpenRead(tmpFileNames[i]));

                //-----------------------------------
                // open final destination file
                //

                ResetFileReader(); // closes original file

                using (var outstream = File.Create(file))
                using (var outfile = new BinaryWriter(outstream))
                {
                    outfile.Write(_records);
                    outfile.Write(_realFieldsCount);
                    outfile.Write(_discreteFieldsCount);

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

                    outfile.Close();
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
 
        ~BFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
