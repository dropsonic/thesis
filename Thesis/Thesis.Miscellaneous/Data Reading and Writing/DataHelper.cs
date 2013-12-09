using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public static class DataHelper
    {
        public static void CopyTo(this IDataReader reader, IDataWriter destination)
        {
            foreach (var record in reader)
                destination.WriteRecord(record);
        }

        public static void Shuffle(this IDataReader reader, string outputFile,
                                   int iterations = 5, int randFilesCount = 10)
        {
            Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(outputFile));
            Contract.Requires<ArgumentOutOfRangeException>(iterations >= 1);
            Contract.Requires<ArgumentOutOfRangeException>(randFilesCount >= 1);

            BinaryShuffle shuffle = new BinaryShuffle(reader, outputFile, iterations, randFilesCount);
            shuffle.MultiShuffle();
        }
    }
}
