using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing
{
    internal enum TripletSplitKind
    {
        AllOnOneRow,
        LastByteWraps,
        LastTwoBytesWrap
    }

    internal enum BinaryDrawingStage
    {
        Initial,
        ShowBytes,
        ShowBoxes,
        ColorBoxes,
        MergeColors,
        BuildPixelRow,
        FillImage
    }
}
