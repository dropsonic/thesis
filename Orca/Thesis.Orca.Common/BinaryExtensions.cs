using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Orca.Common
{
    internal static class BinaryExtensions
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

        public static float[] ReadFloatArray(this BinaryReader reader, int count)
        {
            Contract.Requires<ArgumentOutOfRangeException>(count > 0);

            float[] result = new float[count];
            byte[] binaryData = reader.ReadBytes(sizeof(float) * count);
            Buffer.BlockCopy(binaryData, 0, result, 0, binaryData.Length);

            return result;
        }

        public static int[] ReadIntArray(this BinaryReader reader, int count)
        {
            Contract.Requires<ArgumentOutOfRangeException>(count > 0);

            int[] result = new int[count];
            byte[] binaryData = reader.ReadBytes(sizeof(int) * count);
            Buffer.BlockCopy(binaryData, 0, result, 0, binaryData.Length);

            return result;
        }
    }
}
