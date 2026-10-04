namespace Celarix.Starfall.Atria;

/// <summary>
/// One slide in a <see cref="PresentationDefinition"/>: its name and how to build it. The slide
/// isn't constructed until it's shown, so a harness can list slides cheaply.
/// </summary>
/// <param name="DisplayName">The human-readable name shown in slide lists.</param>
/// <param name="Factory">Builds a fresh instance of the slide. The viewport size is available as
/// <see cref="AtriaRuntime.ViewportSize"/>.</param>
public sealed record SlideDefinition(string DisplayName, Func<AtriaRuntime, AtriaSlide> Factory)
{
    /// <summary>
    /// Gets a short description of what the slide shows.
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// A presentation (deck): its name, where it's grouped, and its ordered slides.
/// </summary>
public sealed class PresentationDefinition
{
    public required string Name { get; init; }

    /// <summary>
    /// Gets the folder-like group a harness lists this presentation under, such as "Talks".
    /// </summary>
    public string? Folder { get; init; }

    public string? Description { get; init; }

    public required IReadOnlyList<SlideDefinition> Slides { get; init; }
}
