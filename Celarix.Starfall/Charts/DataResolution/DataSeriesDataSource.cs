using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class DataSeriesDataSource : IDataSource
{
    private readonly DataSeries _series;

    public DataSeriesDataSource(DataSeries series)
    {
        _series = series;
    }

    public IEnumerable<DataPoint> GetData(DataSourceRequest request)
    {
        throw new NotImplementedException();
    }
}
