using Celarix.Starfall.Mathematics.Schoolbook;

namespace Celarix.Starfall.Audio;

/// <summary>
/// Turns the table lookups in a schoolbook step into two-note chords: the column (X plus any carry)
/// as one note and the row (Y) an octave up as the other, played together.
/// </summary>
/// <remarks>
/// Note numbers are <see cref="NotePitch"/> indices (middle C = 0). By default the column plays at
/// X + carry (0–9 for a plain lookup, 1–10 with a carried 1, and the running total when adding up a
/// long column) and the row at 12 + Y. Notes above <see cref="MaximumNote"/> drop by octaves.
/// Steps with several lookups play their chords one after another, spread across the step.
/// </remarks>
public sealed class SchoolbookSound
{
    /// <summary>Gets the note for column 0 of the table. Defaults to middle C.</summary>
    public int ColumnBaseNote { get; init; } = 0;

    /// <summary>Gets the note for row 0 of the table. Defaults to the C above middle C.</summary>
    public int RowBaseNote { get; init; } = 12;

    /// <summary>
    /// Gets the highest note to play; higher notes (from big running totals) drop by octaves until
    /// they fit. Defaults to 36, the C three octaves above middle C (about 2093 Hz).
    /// </summary>
    public int MaximumNote { get; init; } = 36;

    /// <summary>Gets the volume of each note; a chord is two notes. Defaults to 0.15.</summary>
    public float Volume { get; init; } = 0.15f;

    /// <summary>Gets how long a note rings when stepping slowly. Defaults to 0.5 seconds.</summary>
    public double RingSeconds { get; init; } = 0.5d;

    /// <summary>Gets the time constant of a ringing note's decay. Defaults to 0.1 seconds.</summary>
    public double DecaySeconds { get; init; } = 0.1d;

    /// <summary>Gets the most time between chords within one slow step. Defaults to 0.09 seconds.</summary>
    public double ChordSpacingSeconds { get; init; } = 0.09d;

    /// <summary>
    /// Gets the step length below which notes stop ringing and crossfade into one another instead,
    /// for the rapid-fire sound of a fast computation. Defaults to 0.15 seconds.
    /// </summary>
    public double FastStepSeconds { get; init; } = 0.15d;

    /// <summary>
    /// Gets a small fixed delay added to fast notes, so a step that ran a little late (between
    /// frames) can still be placed exactly on the beat. Defaults to 25 ms.
    /// </summary>
    public double FastLeadSeconds { get; init; } = 0.025d;

    /// <summary>Gets the two note numbers for a lookup: (column, row).</summary>
    public (int ColumnNote, int RowNote) NotesFor(TableLookup lookup) =>
        (Fold(ColumnBaseNote + lookup.ShiftedX), Fold(RowBaseNote + lookup.Y));

    /// <summary>
    /// Plays a step's chords.
    /// </summary>
    /// <param name="secondsPerStep">How long until the next step when auto-playing, or null when stepping by hand.</param>
    /// <param name="lateBySeconds">How long ago the step was due, when auto-play runs it between frames.</param>
    public void PlayStep(ITonePlayer player, SchoolbookStep step, double? secondsPerStep, double lateBySeconds = 0d)
    {
        var lookups = step.Lookups;
        if (lookups.Count == 0) { return; }

        var fast = secondsPerStep is { } s && s < FastStepSeconds;
        var spacing = secondsPerStep is { } stepSeconds
            ? Math.Min(ChordSpacingSeconds, stepSeconds / lookups.Count)
            : ChordSpacingSeconds;

        for (var k = 0; k < lookups.Count; k++)
        {
            var (columnNote, rowNote) = NotesFor(lookups[k]);
            if (fast)
            {
                // Each note lasts two slots and crossfades with its neighbors, so back-to-back notes
                // glide into one another at a steady volume instead of clicking on and off.
                var delay = Math.Max(0d, FastLeadSeconds + k * spacing - lateBySeconds);
                var shape = ToneShape.Crossfade(spacing);
                player.PlayTone(NotePitch.Frequency(columnNote), 2 * spacing, Volume, delay, shape);
                player.PlayTone(NotePitch.Frequency(rowNote), 2 * spacing, Volume, delay, shape);
            }
            else
            {
                var delay = k * spacing;
                var shape = ToneShape.Ringing(DecaySeconds);
                player.PlayTone(NotePitch.Frequency(columnNote), RingSeconds, Volume, delay, shape);
                player.PlayTone(NotePitch.Frequency(rowNote), RingSeconds, Volume, delay, shape);
            }
        }
    }

    private int Fold(int note)
    {
        while (note > MaximumNote) { note -= 12; }
        return note;
    }
}
