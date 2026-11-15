using Celarix.Starfall.Mathematics.Schoolbook;

namespace Celarix.Starfall.Audio;

/// <summary>
/// Turns the table lookups in a schoolbook step into two-note chords: the column (X) as one note and
/// the row (Y) an octave up as the other, played together.
/// </summary>
/// <remarks>
/// Note numbers are <see cref="NotePitch"/> indices (middle C = 0). By default X plays at 0–9 (1–10
/// for an addition with a carried 1) and Y at 12–21. Steps with several lookups (long columns, folded
/// multiplication) play their chords one after another, spread across the step.
/// </remarks>
public sealed class SchoolbookSound
{
    /// <summary>Gets the note for column 0 of the table. Defaults to middle C.</summary>
    public int ColumnBaseNote { get; init; } = 0;

    /// <summary>Gets the note for row 0 of the table. Defaults to the C above middle C.</summary>
    public int RowBaseNote { get; init; } = 12;

    /// <summary>Gets the volume of each note; a chord is two notes. Defaults to 0.15.</summary>
    public float Volume { get; init; } = 0.15f;

    /// <summary>Gets how long a note rings when stepping slowly. Defaults to 0.5 seconds.</summary>
    public double RingSeconds { get; init; } = 0.5d;

    /// <summary>Gets the time constant of a ringing note's decay. Defaults to 0.1 seconds.</summary>
    public double DecaySeconds { get; init; } = 0.1d;

    /// <summary>Gets the most time between chords within one slow step. Defaults to 0.09 seconds.</summary>
    public double ChordSpacingSeconds { get; init; } = 0.09d;

    /// <summary>
    /// Gets the step length below which notes stop ringing and simply last until the next note,
    /// for the rapid-fire sound of a fast computation. Defaults to 0.15 seconds.
    /// </summary>
    public double FastStepSeconds { get; init; } = 0.15d;

    /// <summary>Gets the two note numbers for a lookup: (column, row).</summary>
    public (int ColumnNote, int RowNote) NotesFor(TableLookup lookup) =>
        (ColumnBaseNote + lookup.X + (lookup.Operation == TableOperation.Addition && lookup.CarryIn ? 1 : 0),
         RowBaseNote + lookup.Y);

    /// <summary>
    /// Plays a step's chords.
    /// </summary>
    /// <param name="secondsPerStep">How long until the next step when auto-playing, or null when stepping by hand.</param>
    public void PlayStep(ITonePlayer player, SchoolbookStep step, double? secondsPerStep)
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
            var delay = k * spacing;
            if (fast)
            {
                // Back-to-back notes with no decay: each lasts until the next one starts.
                player.PlayTone(NotePitch.Frequency(columnNote), spacing, Volume, delay);
                player.PlayTone(NotePitch.Frequency(rowNote), spacing, Volume, delay);
            }
            else
            {
                player.PlayTone(NotePitch.Frequency(columnNote), RingSeconds, Volume, delay, DecaySeconds);
                player.PlayTone(NotePitch.Frequency(rowNote), RingSeconds, Volume, delay, DecaySeconds);
            }
        }
    }
}
