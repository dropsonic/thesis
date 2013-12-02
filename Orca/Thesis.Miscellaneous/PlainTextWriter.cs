using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Thesis
{
    public class PlainTextWriter : IDataWriter, IDisposable
    {
        private StreamWriter _outfile;

        public PlainTextWriter(string fileName)
        {
            _outfile = new StreamWriter(fileName);
        }

        public void DeleteRecord(int number)
        {
            _outfile
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
                    _outfile.Close();
                }
 
                // Unmanaged resources are released here.
                m_Disposed = true;
            }
        }
 
        ~PlainWriter()    
        {        
            Dispose(false);
        }
        #endregion
    }
}
