﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis
{
    public interface IDataReader: IEnumerable<Record>
    {
        IEnumerable<Field> Fields { get; }

        Record ReadRecord();

        void Reset();

        bool EndOfData { get; }

        int Index { get; }
    }

    public class DataFormatException : Exception
    {

    }
}