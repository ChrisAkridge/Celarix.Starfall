namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed record PropertyEntry(
        string Key,
        string Value,
        TextSpan PropertySpan,
        TextSpan KeySpan,
        TextSpan ValueSpan);
}
