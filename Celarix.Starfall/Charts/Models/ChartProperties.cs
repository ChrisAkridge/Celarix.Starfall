using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public abstract class ChartProperties
{
    public event EventHandler? PropertiesChanged;

    protected void SetProperty<T>(T newValue, T currentValue, Action<T> setter)
    {
        setter(newValue);
        if (!Valid(out var ex))
        {
            setter(currentValue);
            throw ex;
        }
        
        if (!EqualityComparer<T>.Default.Equals(newValue, currentValue))
        {
            PropertiesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual bool Valid([NotNullWhen(false)] out Exception? ex)
    {
        ex = null;
        return true;
    }
}
