using Celarix.Starfall.Libra;
using Celarix.Starfall.Libra.Expressions;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed record FittedLabel(LibraLayoutResult LibraLayoutResult, SPointF Position);
