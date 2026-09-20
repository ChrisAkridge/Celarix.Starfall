using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public abstract class ChartPropertyBase
{
    private List<Action>? _undoLog;
    private bool _atomicUpdateFailed;
    public event EventHandler? PropertiesChanged;
    public bool RaiseEventOnChanged { get; set; } = true;
    protected bool IsAtomicUpdateInProgress => _undoLog is not null;

    protected void SetProperty<T>(T newValue, T currentValue, Action<T> setter)
    {
        if (_undoLog is not null)
        {
            if (EqualityComparer<T>.Default.Equals(newValue, currentValue)) return;
            _undoLog.Add(() => setter(currentValue));
            setter(newValue);
            return;
        }

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

    public void UpdatePropertiesAtomic(Action propertyUpdater)
    {
        ArgumentNullException.ThrowIfNull(propertyUpdater);
        if (_undoLog is not null)
        {
            _atomicUpdateFailed = true;
            throw new InvalidOperationException("Atomic property updates cannot be nested or reentered.");
        }

        _undoLog = [];
        _atomicUpdateFailed = false;

        try
        {
            propertyUpdater();
            if (_atomicUpdateFailed)
            {
                throw new InvalidOperationException("A nested or reentrant atomic property update was attempted.");
            }
            if (!Valid(out var ex))
            {
                throw ex;
            }

            var changed = _undoLog.Count > 0;
            _undoLog = null;
            if (changed && RaiseEventOnChanged)
            {
                OnPropertiesChanged();
            }
        }
        catch
        {
            for (var i = _undoLog!.Count - 1; i >= 0; i--)
            {
                _undoLog[i]();
            }
            _undoLog = null;
            throw;
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
