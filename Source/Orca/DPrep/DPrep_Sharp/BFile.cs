﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class BFile : IDisposable
    {
        BinaryReader _infile;
        int _example;

        string _dataFile;

        int _index;

        int _records;
        int _realFieldsCount;
        int _discreteFieldsCount;

        float _missingR;
        int _missingD;

        public BFile(string filename, float missingR, int missingD)
        {
            _dataFile = filename;
            _missingR = missingR;
            _missingD = missingD;

            SetFileReader(filename);
            ReadHeader();
        }

        private void SetFileReader(string filename)
        {
            // if infile_ already points to a file, close it
            if (_infile != null)
                _infile.Close();

            _infile = new BinaryReader(File.OpenRead(filename));
            _index = 0;
        }

        private void ReadHeader()
        {
            _records = _infile.ReadInt32();
            _realFieldsCount = _infile.ReadInt32();
            _discreteFieldsCount = _infile.ReadInt32();
        }

        private void SeekPosition(int pos)
        {
            if (pos < 0 || pos > _records)
                throw new ArgumentOutOfRangeException("pos");

            long filepos = 3 * sizeof(int) + pos * sizeof(float) + pos * sizeof(int);
            _infile.BaseStream.Seek(pos, SeekOrigin.Begin);
            _index = pos;
        }

        private void GetNext(out int id, out float[] real, out int[] discrete)
        {
            // WARNING does not check to make sure read command succeeded.
            id = _infile.ReadInt32();
            real = new float[_realFieldsCount];
            discrete = new int[_discreteFieldsCount];
            byte[] temp = _infile.ReadBytes(sizeof(float) * _realFieldsCount);
            Buffer.BlockCopy(temp, 0, real, 0, temp.Length);
            temp = _infile.ReadBytes(sizeof(int) * _discreteFieldsCount);
            Buffer.BlockCopy(temp, 0, discrete, 0, temp.Length);
            
            _index++;
        }

        /// <summary>
        /// Makes a pass through the data and calculates the max and min values
        /// for real variables.
        /// </summary>
        //public void GetMaxMin(ref IList<float> max, ref IList<float> min)
        public void GetMaxMin(float[] max, float[] min)
        {
            SeekPosition(0);

            //if (max == null)
            //    max = new List<float>(_realFieldsCount);
            //if (min == null)
            //    min = new List<float>(_realFieldsCount);

            //var R = new List<float>(RealFieldsCount);
            //var D = new List<int>(DiscreteFieldsCount);

            // initialize max and min vectors 
            for (int i = 0; i < _realFieldsCount; i++)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            // process rest of examples
            while (_index < _records)
            {
                int id;
                float[] R;
                int[] D;
                GetNext(out id, out R, out D);

                for (int i = 0; i < _realFieldsCount; i++)
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
                };
            }
        }

        /// <summary>
        /// Makes a pass through the data and calculates the mean and standard
        /// deviation for real variables.
        /// </summary>
        public void GetMeanStd(float[] mean, float[] std)
        {
            SeekPosition(0);
            var sumv = new double[_realFieldsCount];
            var sumsqv = new double[_realFieldsCount];
            var num = new int[_realFieldsCount];

            while (_index < _records)
            {
                int id;
                float[] R;
                int[] D;
                GetNext(out id, out R, out D);

                for (int i = 0; i < _realFieldsCount; i++)
                {
                    if (R[i] != _missingR)
                    {
                        double r = ((double)R[i]);
                        sumv[i] += r;
                        sumsqv[i] += r * r;
                        num[i]++;
                    }
                }

                for (int i = 0; i < _realFieldsCount; i++)
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
            }
        }

        /// <summary>
        /// Scales real values to the range [0,1].
        /// </summary>
        public void ScaleZeroToOne(string filename, float[] max, float[] min)
        {
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
            if (tmpFiles <= 0)
                throw new ArgumentOutOfRangeException("tmpfiles");

            Random rand = new Random(seed);
            for (int i = 0; i < iterations; i++)
            {
                Shuffle(destFile, 10000, tmpFiles, rand);
                SetFileReader(destFile);
            }
        }

        private void Shuffle(string file, int blockSize, int nTmpFiles, Random rand)
        {
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
                    for (int i = order.Length; i > 0; i--)
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
        }

        public int RecordsCount
        {
            get
            {
                return _records;
            }
        }

        public int RealFieldsCount
        {
            get
            {
                return _realFieldsCount;
            }
        }

        public int DiscreteFieldsCount
        {
            get
            {
                return _discreteFieldsCount;
            }
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
                    _infile.Close();
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
