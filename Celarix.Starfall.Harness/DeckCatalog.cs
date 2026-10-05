using Celarix.Starfall.Atria;
using Celarix.Starfall.Decks.FloatingPoint;
using Celarix.Starfall.Decks.Playground;

namespace Celarix.Starfall.Harness;

/// <summary>
/// The presentations the harness can open. Add new decks here.
/// </summary>
internal static class DeckCatalog
{
    public static IReadOnlyList<PresentationDefinition> All { get; } =
    [
        FloatingPointPresentation.Create(),
        PlaygroundPresentation.Create()
    ];
}
