using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public interface IDataSource
{
    IEnumerable<DataPoint> GetData(DataSourceRequest request);
}
