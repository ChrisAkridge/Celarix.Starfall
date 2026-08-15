namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed record PropertyBindingResult(
        LibraBuildContext Context,
        TextSpan? FenceTypeSpan);
}
