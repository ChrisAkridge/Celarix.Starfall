using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public abstract class ChartPropertyBase
{
    public event EventHandler? PropertiesChanged;
    public bool RaiseEventOnChanged { get; set; } = true;

    protected void SetProperty<T>(T newValue, T currentValue, Action<T> setter)
    {
        setter(newValue);
        if (!Valid(out var ex))
        {
            setter(currentValue);
            throw ex;
        }

        if (!EqualityComparer<T>.Default.Equals(newValue, currentValue)
            && RaiseEventOnChanged)
        {
            PropertiesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetProperties(Action multipropertySetter)
    {
        var oldRaiseEventOnChanged = RaiseEventOnChanged;

        try
        {
            RaiseEventOnChanged = false;
            multipropertySetter();
        }
        finally
        {
            RaiseEventOnChanged = oldRaiseEventOnChanged;
            OnPropertiesChanged();
        }
    }

    protected virtual bool Valid([NotNullWhen(false)] out Exception? ex)
    {
        ex = null;
        return true;
    }

    protected virtual void OnPropertiesChanged()
    {
        PropertiesChanged?.Invoke(this, EventArgs.Empty);
    }
}
