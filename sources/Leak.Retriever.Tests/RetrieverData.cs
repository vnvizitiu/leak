﻿using System.Linq;
using Leak.Common;

namespace Leak.Retriever.Tests
{
    public class RetrieverData
    {
        private readonly byte[] bytes;

        public RetrieverData(int size)
        {
            bytes = Bytes.Random(size);
        }

        public int Pieces
        {
            get { return (bytes.Length - 1) / 16384 + 1; }
        }

        public byte[] ToBytes()
        {
            return bytes;
        }

        public byte[] this[int index]
        {
            get { return bytes.Skip(index * 16384).Take(16384).ToArray(); }
        }
    }
}