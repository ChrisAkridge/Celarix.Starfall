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

    [Theory]
    [InlineData(487, 36, 10)]
    [InlineData(105, 203, 10)]
    [InlineData(9, 9, 10)]
    [InlineData(1234, 7, 10)]
    [InlineData(0, 55, 10)]
    [InlineData(0b1101, 0b101, 2)]
    public void Multiplication_ResultRowSpellsTheProduct(int a, int b, int numberBase)
    {
        var work = SchoolbookWork.Multiplication(a, b, numberBase);
        Assert.Equal(Format(a * b, numberBase), ResultText(work));
    }

    [Fact]
    public void Multiplication_ShiftsPartialProductsWithPlaceholders()
    {
        var work = SchoolbookWork.Multiplication(487, 36);
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
        var work = SchoolbookWork.Multiplication(487, 36);
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
