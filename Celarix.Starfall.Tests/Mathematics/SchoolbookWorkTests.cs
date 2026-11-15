using System.Numerics;
using Celarix.Starfall.Audio;
using Celarix.Starfall.Mathematics.Schoolbook;

namespace Celarix.Starfall.Tests.Mathematics;

public sealed class SchoolbookWorkTests
{
    [Theory]
    [InlineData(4738, 2594, 10)]
    [InlineData(999, 1, 10)]
    [InlineData(0, 0, 10)]
    [InlineData(0b1011, 0b0111, 2)]
    [InlineData(0xBEEF, 0xF00D, 16)]
    public void Addition_ResultRowSpellsTheSum(int a, int b, int numberBase)
    {
        var work = SchoolbookWork.Addition(a, b, numberBase);
        Assert.Equal(Format(a + b, numberBase), ResultText(work));
    }

    [Fact]
    public void Addition_WritesCarriesAboveTheNextColumn()
    {
        var work = SchoolbookWork.Addition(58, 64);
        var carries = work.Cells.Where(c => c.Kind == SchoolbookCellKind.Carry).ToList();

        Assert.Single(carries); // 8 + 4 = 12 carries; 5 + 6 + 1 = 12 is the last column and writes "12".
        Assert.Equal("1", carries[0].Text);
        Assert.Equal(work.ColumnCount - 2, carries[0].Column);
        Assert.Equal("Write 2, carry 1.", work.Steps[0].Description[^"Write 2, carry 1.".Length..]);
    }

    [Fact]
    public void Addition_SupportsMoreThanTwoAddends()
    {
        var work = SchoolbookWork.Addition([99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99]);
        Assert.Equal("1089", ResultText(work));
    }

    [Theory]
    [InlineData(5003, 1867, 10)]
    [InlineData(112, 29, 10)]
    [InlineData(100, 99, 10)]
    [InlineData(42, 42, 10)]
    [InlineData(0b10000, 0b1, 2)]
    public void Subtraction_ResultRowSpellsTheDifference(int a, int b, int numberBase)
    {
        var work = SchoolbookWork.Subtraction(a, b, numberBase);
        Assert.Equal(Format(a - b, numberBase), ResultText(work));
    }

    [Fact]
    public void Subtraction_BorrowsAcrossZeros()
    {
        var work = SchoolbookWork.Subtraction(5003, 1867);
        var finalState = work.GetState(work.Steps.Count - 1);
        var borrowTexts = work.Cells.Where(c => c.Kind == SchoolbookCellKind.Borrow).Select(c => c.Text).ToList();

        // 5 → 4, both zeros → 9, and 3 → 13 in the first borrow.
        Assert.Equal(["4", "9", "9", "13"], borrowTexts.Take(4));
        var struckOperands = work.Cells
            .Select((cell, index) => (cell, index))
            .Where(x => x.cell.Kind == SchoolbookCellKind.Operand && finalState.StruckOut[x.index])
            .Select(x => x.cell.Text);
        Assert.Equal(["3", "0", "0", "5"], struckOperands); // Cells run least significant first.
    }

    [Fact]
    public void Subtraction_StacksSecondBorrowAboveTheFirst()
    {
        // 112 − 29: the tens digit is lent to the ones (1 → 0), then borrows itself (0 → 10).
        var work = SchoolbookWork.Subtraction(112, 29);
        Assert.Equal(2, work.Rows.Count(r => r == SchoolbookRowKind.Small));
        Assert.Contains(work.Cells, c => c.Kind == SchoolbookCellKind.Borrow && c.Text == "10");
    }

    [Fact]
    public void Subtraction_RejectsNegativeResults()
    {
        Assert.Throws<ArgumentException>(() => SchoolbookWork.Subtraction(3, 5));
    }

    public static TheoryData<int, int, int, MultiplicationLayout> MultiplicationCases()
    {
        var data = new TheoryData<int, int, int, MultiplicationLayout>();
        foreach (var layout in Enum.GetValues<MultiplicationLayout>())
        {
            data.Add(487, 36, 10, layout);
            data.Add(105, 203, 10, layout);
            data.Add(9, 9, 10, layout);
            data.Add(1234, 7, 10, layout);
            data.Add(7, 1234, 10, layout);
            data.Add(0, 55, 10, layout);
            data.Add(99999, 99999, 10, layout);
            data.Add(0b1101, 0b101, 2, layout);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(MultiplicationCases))]
    public void Multiplication_ResultRowSpellsTheProduct(int a, int b, int numberBase, MultiplicationLayout layout)
    {
        var work = SchoolbookWork.Multiplication(a, b, numberBase, layout);
        Assert.Equal(Format((long)a * b, numberBase), ResultText(work));
        Assert.All(work.Steps, s => Assert.InRange(s.Tone ?? 0, 0, work.ToneCount - 1));
    }

    [Fact]
    public void Multiplication_DigitPairsWritesOneShiftedRowPerPair()
    {
        var work = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.DigitPairs);
        var partialRows = work.Cells
            .Where(c => c.Kind is SchoolbookCellKind.Partial or SchoolbookCellKind.Placeholder)
            .GroupBy(c => c.Row)
            .Select(g => string.Concat(g.OrderBy(c => c.Column).Select(c => c.Text)))
            .ToList();

        // 7×6, 8×6, 4×6, then 7×3, 8×3, 4×3, each shifted by its digits' places.
        Assert.Equal(["42", "480", "2400", "210", "2400", "12000"], partialRows);
    }

    [Fact]
    public void Multiplication_FoldedHasNoPartialRowsAndOneStepPerColumn()
    {
        var work = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.Folded);

        Assert.DoesNotContain(work.Cells, c => c.Kind is SchoolbookCellKind.Partial or SchoolbookCellKind.Placeholder);
        Assert.Equal(3 + 2 - 1, work.Steps.Count);
        Assert.StartsWith("The tens column: 7×3 + 8×6 + 4 carried = 73.", work.Steps[1].Description);
    }

    [Fact]
    public void Multiplication_RowCarriesShiftsPartialProductsWithPlaceholders()
    {
        var work = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.RowCarries);
        var partialRows = work.Cells
            .Where(c => c.Kind is SchoolbookCellKind.Partial or SchoolbookCellKind.Placeholder)
            .GroupBy(c => c.Row)
            .Select(g => string.Concat(g.OrderBy(c => c.Column).Select(c => c.Text)))
            .ToList();

        Assert.Equal(["2922", "14610"], partialRows);
    }

    [Fact]
    public void Multiplication_ClearsCarriesBetweenPartialProducts()
    {
        var work = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.RowCarries);
        var finalState = work.GetState(work.Steps.Count - 1);
        var visibleMultiplicationCarries = work.Cells
            .Select((cell, index) => (cell, index))
            .Count(x => x.cell.Kind == SchoolbookCellKind.Carry && x.cell.Row == 0 && finalState.Visible[x.index]);

        Assert.Equal(0, visibleMultiplicationCarries);
    }

    [Fact]
    public void GetState_BeforeAnyStepShowsOnlyTheProblem()
    {
        var work = SchoolbookWork.Addition(12, 34);
        var state = work.GetState(-1);

        for (var i = 0; i < work.Cells.Count; i++)
        {
            Assert.Equal(work.Cells[i].InitiallyVisible, state.Visible[i]);
        }
        Assert.Empty(state.Highlighted);
    }

    [Fact]
    public void Steps_HaveTonesInRange()
    {
        foreach (var work in new[]
        {
            SchoolbookWork.Addition(4738, 2594),
            SchoolbookWork.Subtraction(5003, 1867),
            SchoolbookWork.Multiplication(487, 36),
        })
        {
            Assert.All(work.Steps, s => Assert.InRange(s.Tone ?? 0, 0, work.ToneCount - 1));
        }
    }

    [Theory]
    [InlineData(PitchScale.Linear, 5, 660d)]
    [InlineData(PitchScale.Exponential, 5, 379.47d)]
    public void PitchMapper_MapsTheMiddleIndex(PitchScale scale, int index, double expected)
    {
        var mapper = new PitchMapper { Scale = scale };
        Assert.Equal(120d, mapper.FrequencyFor(0, 11), 2);
        Assert.Equal(expected, mapper.FrequencyFor(index, 11), 2);
        Assert.Equal(1200d, mapper.FrequencyFor(10, 11), 2);
    }

    [Fact]
    public void ToneSynth_FadesInAndOut()
    {
        var samples = ToneSynth.RenderSine(440d, 0.1d, volume: 0.5f);
        Assert.Equal(4410, samples.Length);
        Assert.Equal(0f, samples[0]);
        Assert.Equal(0f, samples[^1]);
        Assert.True(samples.Max() <= 0.5f);
        Assert.True(samples.Max() > 0.45f);
    }

    [Fact]
    public void Addition_LooksUpEachColumnWithCarry()
    {
        var work = SchoolbookWork.Addition(58, 64);

        Assert.Equal([TableLookup.Add(8, 4)], work.Steps[0].Lookups);
        Assert.Equal([TableLookup.Add(5, 6, carryIn: true)], work.Steps[1].Lookups);
        Assert.Equal(12, work.Steps[1].Lookups[0].Value);
    }

    [Fact]
    public void Subtraction_LooksUpTheAdditionTableBackwards()
    {
        var work = SchoolbookWork.Subtraction(52, 18);
        var lookups = work.Steps.SelectMany(s => s.Lookups).ToList();

        // 12 − 8 = 4 is 8 + 4 = 12; then 4 − 1 = 3 is 1 + 3 = 4.
        Assert.Equal([TableLookup.Add(8, 4), TableLookup.Add(1, 3)], lookups);
    }

    [Fact]
    public void Multiplication_LooksUpEachDigitPair()
    {
        var pairs = SchoolbookWork.Multiplication(487, 36);
        Assert.Equal(TableLookup.Multiply(7, 6), pairs.Steps[0].Lookups.Single());

        var folded = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.Folded);
        Assert.Equal([TableLookup.Multiply(7, 3), TableLookup.Multiply(8, 6)], folded.Steps[1].Lookups);
    }

    [Theory]
    [InlineData(9, 440d)]
    [InlineData(0, 261.63d)]
    [InlineData(12, 523.25d)]
    [InlineData(-3, 220d)]
    public void NotePitch_MiddleCIsZeroAndAIs440(int index, double expected)
    {
        Assert.Equal(expected, NotePitch.Frequency(index), 2);
    }

    [Fact]
    public void SchoolbookSound_PlaysColumnAndRowTogether()
    {
        var player = new RecordingTonePlayer();
        var step = SchoolbookWork.Addition(58, 64).Steps[1]; // 5 + 6 with a carried 1

        new SchoolbookSound().PlayStep(player, step, secondsPerStep: null);

        Assert.Equal(2, player.Tones.Count);
        Assert.Equal(NotePitch.Frequency(6), player.Tones[0].Frequency, 6); // 5, shifted to 6 by the carry
        Assert.Equal(NotePitch.Frequency(18), player.Tones[1].Frequency, 6); // 12 + 6
        Assert.All(player.Tones, t => Assert.Equal(0d, t.Delay));
        Assert.All(player.Tones, t => Assert.NotNull(t.Decay));
    }

    [Fact]
    public void SchoolbookSound_FastStepsDoNotDecay()
    {
        var player = new RecordingTonePlayer();
        var step = SchoolbookWork.Multiplication(487, 36, layout: MultiplicationLayout.Folded).Steps[1];

        new SchoolbookSound().PlayStep(player, step, secondsPerStep: 0.04d);

        Assert.Equal(4, player.Tones.Count);
        Assert.All(player.Tones, t => Assert.Null(t.Decay));
        Assert.Equal(0.02d, player.Tones[2].Delay, 6);
        Assert.Equal(0.02d, player.Tones[2].Duration, 6);
    }

    private sealed class RecordingTonePlayer : ITonePlayer
    {
        public List<(double Frequency, double Duration, double Delay, double? Decay)> Tones { get; } = [];

        public void PlayTone(double frequencyHz, double durationSeconds, float volume = 0.3f,
            double delaySeconds = 0d, double? decaySeconds = null) =>
            Tones.Add((frequencyHz, durationSeconds, delaySeconds, decaySeconds));
    }

    private static string ResultText(SchoolbookWork work) => string.Concat(work.Cells
        .Where(c => c.Kind == SchoolbookCellKind.Result)
        .OrderBy(c => c.Column)
        .Select(c => c.Text));

    private static string Format(BigInteger value, int numberBase)
    {
        const string alphabet = "0123456789ABCDEF";
        if (value.IsZero) { return "0"; }
        var text = "";
        while (value > 0) { text = alphabet[(int)(value % numberBase)] + text; value /= numberBase; }
        return text;
    }
}
