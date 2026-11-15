namespace Celarix.Starfall.Audio;

/// <summary>
/// Equal-tempered note numbers: middle C (C4) is 0, each step is one semitone (a factor of the
/// twelfth root of two), 12 is the C an octave up, and negative numbers go below middle C.
/// A above middle C (index 9) is 440 Hz.
/// </summary>
public static class NotePitch
{
    public const double ConcertA = 440d;
    public const int ConcertAIndex = 9;

    public static double Frequency(double noteIndex) => ConcertA * Math.Pow(2d, (noteIndex - ConcertAIndex) / 12d);
}
