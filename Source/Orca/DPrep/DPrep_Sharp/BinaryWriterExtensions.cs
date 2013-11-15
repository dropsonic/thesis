using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.DPrep
{
    internal static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, float[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null);

            byte[] binaryData = new byte[data.Length * sizeof(float)];
            System.Buffer.BlockCopy(data, 0, binaryData, 0, binaryData.Length);
            writer.Write(binaryData);
        }

        public static void Write(this BinaryWriter writer, int[] data)
        {
            Contract.Requires<ArgumentNullException>(data != null);

            byte[] binaryData = new byte[data.Length * sizeof(int)];
            System.Buffer.BlockCopy(data, 0, binaryData, 0, binaryData.Length);
            writer.Write(binaryData);
        }
    }
}
