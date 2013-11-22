using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    class ScaleFile : IDisposable
    {
        BinaryInFile _infile;

        IEnumerable<Field> _fields;

        float _missingR;
        int _missingD;

        public ScaleFile(string dataFile, IEnumerable<Field> fields, float missingR, int missingD)
        {
            Contract.Requires(!String.IsNullOrEmpty(dataFile));
            Contract.Requires<ArgumentNullException>(fields != null);

            _infile = new BinaryInFile(dataFile);
            _fields = fields;
            _missingR = missingR;
            _missingD = missingD;
        }

        /// <summary>
        /// Makes a pass through the data and calculates the max and min values
        /// for real variables.
        /// </summary>
        //public void GetMaxMin(ref IList<float> max, ref IList<float> min)
        public void GetMaxMin(float[] max, float[] min)
        {
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
                float[] range = new float[_infile.RealFieldsCount];

                for (int i = 0; i < _infile.RealFieldsCount; i++)
                {
                    range[i] = max[i] - min[i];
                };

                //--------------------------------
                // read in file and scale it
                //
                float[] Rscale = new float[_infile.RealFieldsCount];
                _infile.ForEach((id, R, D) =>
                    {
                        for (int i = 0; i < _infile.RealFieldsCount; i++)
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

                        outfile.WriteRecord(id, Rscale, D);
                    });

                outfile.WriteHeader(_infile.Records);
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
            using (var outfile = new BinaryOutFile(filename, _fields))
            {
                //--------------------------------
                // read in file and scale it
                //
                float[] Rscale = new float[_infile.RealFieldsCount];
                _infile.ForEach((id, R, D) =>
                {
                    for (int i = 0; i < _infile.RealFieldsCount; i++)
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

                    outfile.WriteRecord(id, Rscale, D);
                });

                outfile.WriteHeader(_infile.Records);
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
