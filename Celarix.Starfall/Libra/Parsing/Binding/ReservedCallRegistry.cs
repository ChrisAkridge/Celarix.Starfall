using System.Diagnostics.CodeAnalysis;

namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal static class ReservedCallRegistry
    {
        private static readonly IReadOnlyDictionary<string, IReservedCallBinder> _binders =
            new Dictionary<string, IReservedCallBinder>(StringComparer.Ordinal)
            {
                [";frac"] = new FractionReservedCallBinder(),
                [";catEm"] = new CatEmReservedCallBinder()
            };

        public static bool TryGetBinder(string name,
            [NotNullWhen(true)] out IReservedCallBinder? binder)
        {
            return _binders.TryGetValue(name, out binder);
        }
    }
}
