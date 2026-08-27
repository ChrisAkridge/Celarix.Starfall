using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Charts.DataResolution;

public sealed class FunctionDataSource : IDataSource
{
    private readonly Func<BigInteger, double> _function;

    public FunctionDataSource(Func<BigInteger, double> function)
    {
        _function = function;
    }

    // TODO STATS PASS TWO!!!
    public IEnumerable<DataPoint> GetData(DataSourceRequest request)
    {
        throw new NotImplementedException();
    }
}
