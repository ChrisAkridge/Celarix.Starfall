using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal sealed class ByteTriplet
    {
        private byte _0;
        private byte _1;
        private byte _2;

        private int _row;
        private int _column;

        public TripletSplitKind SplitKind { get; set; }

        public ByteTriplet(byte byte0, byte byte1, byte byte2, int row, int column)
        {
            _0 = byte0;
            _1 = byte1;
            _2 = byte2;
            _row = row;
            _column = column;
        }

        public byte GetByte(int index)
        {
            return index switch
            {
                0 => _0,
                1 => _1,
                2 => _2,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0, 1, or 2.")
            };
        }
    }
}
