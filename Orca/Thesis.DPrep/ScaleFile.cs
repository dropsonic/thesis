using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Orca.Common;

namespace Thesis.DPrep
{
    class ScaleFile : IDisposable
    {
        BinaryInFile _infile;

        Weights _weights;

        float _missingR;
        int _missingD;

        public ScaleFile(string dataFile, Weights weights, float missingR, int missingD)
        {
            Contract.Requires(!String.IsNullOrEmpty(dataFile));
            Contract.Requires<ArgumentNullException>(weights != null);

            _infile = new BinaryInFile(dataFile);

            _weights = weights;
            _missingR = missingR;
            _missingD = missingD;
        }

        /// <summary>
        /// Makes a pass through the data and calculates the max and min values
        /// for real variables.
        /// </summary>
        //public void GetMaxMin(ref IList<float> max, ref IList<float> min)
        public void GetMaxMin(out float[] maxr, out float[] minr)
        {
            var max = new float[_infile.RealFieldsCount];
            var min = new float[_infile.RealFieldsCount];

            // initialize max and min vectors 
            for (int i = 0; i < _infile.RealFieldsCount; i++)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            // process rest of examples
            _infile.ForEach((rec) =>
            {
                for (int i = 0; i < _infile.RealFieldsCount; i++)
                {
                    if (rec.Real[i] != _missingR)
                    {
                        if (rec.Real[i] < min[i])
                        {
                            min[i] = rec.Real[i];
                        }
                        else if (rec.Real[i] > max[i])
                        {
                            max[i] = rec.Real[i];
                        }
                    }
                }
            });

            maxr = max;
            minr = min;
        }

        /// <summary>
        /// Makes a pass through the data and calculates the mean and standard
        /// deviation for real variables.
        /// </summary>
        public void GetMeanStd(out float[] meanr, out float[] stdr)
        {
            var mean = new float[_infile.RealFieldsCount];
            var std = new float[_infile.RealFieldsCount];

            var sumv = new double[_infile.RealFieldsCount];
            var sumsqv = new double[_infile.RealFieldsCount];
            var num = new int[_infile.RealFieldsCount];

            _infile.ForEach((rec) =>
            {
                for (int i = 0; i < _infile.RealFieldsCount; i++)
                {
                    if (rec.Real[i] != _missingR)
                    {
                        double r = ((double)rec.Real[i]);
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

            meanr = mean;
            stdr = std;
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
            using (var outfile = new BinaryOutFile(filename, _weights))
            {
                float[] range = new float[_infile.RealFieldsCount];

                for (int i = 0; i < _infile.RealFieldsCount; i++)
                {
                    range[i] = max[i] - min[i];
                };

                //--------------------------------
                // read in file and scale it
                //
                float[] Rscale = new float[_infile.RealFieldsCount];
                _infile.ForEach((rec) =>
                    {
                        for (int i = 0; i < _infile.RealFieldsCount; i++)
                        {
                            if (rec.Real[i] == _missingR)
                            {
                                Rscale[i] = _missingR;
                            }
                            else if (range[i] != 0)
                            {
                                Rscale[i] = (rec.Real[i] - min[i]) / range[i];
                            }
                            else
                            {
                                Rscale[i] = 0;
                            }
                        }

                        outfile.WriteRecord(new Record(rec.Id, Rscale, rec.Discrete));
                    });

                outfile.WriteHeader(_infile.RecordsCount);
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
            using (var outfile = new BinaryOutFile(filename, _weights))
            {
                //--------------------------------
                // read in file and scale it
                //
                float[] Rscale = new float[_infile.RealFieldsCount];
                _infile.ForEach((rec) =>
                {
                    for (int i = 0; i < _infile.RealFieldsCount; i++)
                    {
                        if (rec.Real[i] == _missingR)
                        {
                            Rscale[i] = _missingR;
                        }
                        else if (std[i] != 0)
                        {
                            Rscale[i] = (rec.Real[i] - mean[i]) / std[i];
                        }
                        else
                        {
                            Rscale[i] = 0;
                        }
                    }

                    outfile.WriteRecord(new Record(rec.Id, Rscale, rec.Discrete));
                });

                outfile.WriteHeader(_infile.RecordsCount);
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
                    _infile.Dispose();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~ScaleFile()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
