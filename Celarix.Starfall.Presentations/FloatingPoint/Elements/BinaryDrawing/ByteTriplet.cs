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

        private int _rowOf0;
        private int _columnOf0;

        public ByteTriplet(byte byte0, byte byte1, byte byte2, int rowOfByte0, int columnOfByte0)
        {
            _0 = byte0;
            _1 = byte1;
            _2 = byte2;
            _rowOf0 = rowOfByte0;
            _columnOf0 = columnOfByte0;
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

        public (int row, int column) GetPosition(int index, int rowWidth)
        {
            if (index == 0)
            {
                return (_rowOf0, _columnOf0);

            }
            else if (index == 1)
            {
                int newColumn = _columnOf0 + 1;
                int newRow = _rowOf0;
                if (newColumn >= rowWidth)
                {
                    newColumn = 0;
                    newRow++;
                }
                return (newRow, newColumn);
            }
            else if (index == 2)
            {
                int newColumn = _columnOf0 + 2;
                int newRow = _rowOf0;
                if (newColumn >= rowWidth)
                {
                    newColumn -= rowWidth;
                    newRow++;
                }
                return (newRow, newColumn);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0, 1, or 2.");
            }
        }
    }
}
