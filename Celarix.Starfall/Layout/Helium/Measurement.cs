using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium
{
    public readonly struct Measurement
    {
        public required decimal Value { get; init; }
        public required Unit Unit { get; init; }

        public Measurement(decimal value, Unit unit)
        {
            Value = value;
            Unit = unit;
        }
    }
}
