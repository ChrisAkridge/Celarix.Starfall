using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Charts.Models;

public sealed class InfoPanelProviderProperties<TData, TKey> : ChartPropertyBase
{
    private Func<TData, ChartText> _formatData;
    private Func<TData, ChartText>? _formatDataAlternate;
    private Func<TKey, ChartText> _formatKey;
    private Func<TKey, ChartText>? _formatKeyAlternate;

    public Func<TData, ChartText> FormatData
    {
        get => _formatData;
        set => SetProperty(value, _formatData, v => _formatData = v);
    }

    public Func<TData, ChartText>? FormatDataAlternate
    {
        get => _formatDataAlternate;
        set => SetProperty(value, _formatDataAlternate, v => _formatDataAlternate = v);
    }

    public Func<TKey, ChartText> FormatKey
    {
        get => _formatKey;
        set => SetProperty(value, _formatKey, v => _formatKey = v);
    }

    public Func<TKey, ChartText>? FormatKeyAlternate
    {
        get => _formatKeyAlternate;
        set => SetProperty(value, _formatKeyAlternate, v => _formatKeyAlternate = v);
    }

    public InfoPanelProviderProperties(Func<TData, ChartText> formatData,
        Func<TData, ChartText>? formatDataAlternate,
        Func<TKey, ChartText> formatKey,
        Func<TKey, ChartText>? formatKeyAlternate)
    {
        _formatData = formatData;
        _formatDataAlternate = formatDataAlternate;
        _formatKey = formatKey;
        _formatKeyAlternate = formatKeyAlternate;

        if (!Valid(out var ex))
        {
            throw ex;
        }
    }

    protected override bool Valid([System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out Exception? ex)
    {
        if (_formatData is null)
        {
            ex = new ArgumentNullException(nameof(FormatData));
            return false;
        }
        if (_formatKey is null)
        {
            ex = new ArgumentNullException(nameof(FormatKey));
            return false;
        }
        ex = null;
        return true;
    }
}
