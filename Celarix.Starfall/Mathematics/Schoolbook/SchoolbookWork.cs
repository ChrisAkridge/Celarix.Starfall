using System.Numerics;

namespace Celarix.Starfall.Mathematics.Schoolbook;

/// <summary>
/// What a cell in a schoolbook layout holds, which decides how it is drawn.
/// </summary>
public enum SchoolbookCellKind
{
    /// <summary>A digit of one of the numbers in the problem.</summary>
    Operand,

    /// <summary>The operator sign (+, −, ×).</summary>
    Operator,

    /// <summary>A small carried digit written above a column.</summary>
    Carry,

    /// <summary>A small rewritten digit after borrowing.</summary>
    Borrow,

    /// <summary>A digit of a partial product in long multiplication.</summary>
    Partial,

    /// <summary>A placeholder zero that shifts a partial product left.</summary>
    Placeholder,

    /// <summary>A digit of the final answer.</summary>
    Result
}

/// <summary>
/// How long multiplication is written out.
/// </summary>
public enum MultiplicationLayout
{
    /// <summary>
    /// Every digit of the multiplier times every digit of the multiplicand, each product on its own
    /// shifted row (n × m rows), then all of them added up.
    /// </summary>
    DigitPairs,

    /// <summary>
    /// One row per multiplier digit, carrying within the row as it goes, then the rows added up.
    /// </summary>
    RowCarries,

    /// <summary>
    /// The partial products folded away: just the factors and the answer, worked one column at a
    /// time by summing the digit products that land in each column. Good for very long numbers.
    /// </summary>
    Folded
}

/// <summary>
/// The height of a row: normal rows hold the problem and answer, small rows hold carries and borrows.
/// </summary>
public enum SchoolbookRowKind
{
    Normal,
    Small
}

/// <summary>
/// One piece of text on the schoolbook grid. Columns count from the left, and column 0 holds the operator.
/// </summary>
public sealed record SchoolbookCell(int Row, int Column, string Text, SchoolbookCellKind Kind, bool InitiallyVisible);

/// <summary>
/// A horizontal line drawn under <see cref="AfterRow"/>, spanning the given columns.
/// </summary>
public sealed record SchoolbookRule(int AfterRow, int FirstColumn, int LastColumn);

/// <summary>
/// One step of working the problem by hand. Cell lists hold indices into <see cref="SchoolbookWork.Cells"/>.
/// </summary>
public sealed class SchoolbookStep
{
    /// <summary>Gets what happens in this step, in words, e.g. "Ones column: 8 + 4 = 12. Write 2, carry 1."</summary>
    public required string Description { get; init; }

    /// <summary>Gets the cells read in this step.</summary>
    public IReadOnlyList<int> Highlighted { get; init; } = [];

    /// <summary>Gets the cells written in this step.</summary>
    public IReadOnlyList<int> Revealed { get; init; } = [];

    /// <summary>Gets the cells erased in this step (e.g. carries from the previous partial product).</summary>
    public IReadOnlyList<int> Hidden { get; init; } = [];

    /// <summary>Gets the cells crossed out in this step (e.g. a digit that was borrowed from).</summary>
    public IReadOnlyList<int> StruckOut { get; init; } = [];

    /// <summary>
    /// Gets the value to sound for this step, from 0 to <see cref="SchoolbookWork.ToneCount"/> − 1,
    /// or null for a silent step.
    /// </summary>
    public int? Tone { get; init; }
}

/// <summary>
/// What a schoolbook layout looks like after a given step.
/// </summary>
public sealed class SchoolbookState
{
    internal SchoolbookState(bool[] visible, bool[] struckOut, HashSet<int> highlighted, HashSet<int> written)
    {
        Visible = visible;
        StruckOut = struckOut;
        Highlighted = highlighted;
        Written = written;
    }

    public IReadOnlyList<bool> Visible { get; }
    public IReadOnlyList<bool> StruckOut { get; }

    /// <summary>Gets the cells the current step reads.</summary>
    public IReadOnlySet<int> Highlighted { get; }

    /// <summary>Gets the cells the current step wrote.</summary>
    public IReadOnlySet<int> Written { get; }
}

/// <summary>
/// A problem worked out the way it is taught in school: the numbers stacked in columns, a rule,
/// and the answer filled in one column at a time, with carries, borrows and partial products.
/// Build one with <see cref="Addition"/>, <see cref="Subtraction"/> or <see cref="Multiplication"/>.
/// </summary>
public sealed class SchoolbookWork
{
    internal SchoolbookWork(string title, int numberBase, BigInteger result, int toneCount,
        List<SchoolbookRowKind> rows, int columnCount, List<SchoolbookCell> cells,
        List<SchoolbookRule> rules, List<SchoolbookStep> steps)
    {
        Title = title;
        NumberBase = numberBase;
        Result = result;
        ToneCount = toneCount;
        Rows = rows;
        ColumnCount = columnCount;
        Cells = cells;
        Rules = rules;
        Steps = steps;
    }

    /// <summary>Gets the problem as one line, e.g. "4738 + 2594".</summary>
    public string Title { get; }
    public int NumberBase { get; }
    public BigInteger Result { get; }

    /// <summary>Gets how many distinct values <see cref="SchoolbookStep.Tone"/> can take.</summary>
    public int ToneCount { get; }

    public IReadOnlyList<SchoolbookRowKind> Rows { get; }
    public int ColumnCount { get; }
    public IReadOnlyList<SchoolbookCell> Cells { get; }
    public IReadOnlyList<SchoolbookRule> Rules { get; }
    public IReadOnlyList<SchoolbookStep> Steps { get; }

    /// <summary>
    /// Gets the layout after <paramref name="stepIndex"/> steps have been applied, counting from 0.
    /// −1 shows only the problem.
    /// </summary>
    public SchoolbookState GetState(int stepIndex)
    {
        if (stepIndex < -1 || stepIndex >= Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(stepIndex));
        }

        var visible = Cells.Select(c => c.InitiallyVisible).ToArray();
        var struck = new bool[Cells.Count];
        for (var i = 0; i <= stepIndex; i++)
        {
            var step = Steps[i];
            foreach (var cell in step.Hidden) { visible[cell] = false; }
            foreach (var cell in step.Revealed) { visible[cell] = true; }
            foreach (var cell in step.StruckOut) { struck[cell] = true; }
        }

        var current = stepIndex >= 0 ? Steps[stepIndex] : null;
        return new SchoolbookState(visible, struck,
            [.. current?.Highlighted ?? []],
            [.. current?.Revealed ?? []]);
    }

    /// <summary>
    /// Long addition of two or more non-negative numbers, carrying from right to left.
    /// </summary>
    public static SchoolbookWork Addition(IReadOnlyList<BigInteger> addends, int numberBase = 10)
    {
        if (addends.Count < 2) { throw new ArgumentException("Addition needs at least two addends.", nameof(addends)); }
        Digits.ThrowIfInvalid(numberBase);
        if (addends.Any(a => a.Sign < 0)) { throw new ArgumentException("Addends must not be negative.", nameof(addends)); }

        var sum = addends.Aggregate(BigInteger.Zero, (a, b) => a + b);
        var digits = addends.Select(a => Digits.Of(a, numberBase)).ToList();
        var width = Math.Max(Digits.Of(sum, numberBase).Count, digits.Max(d => d.Count)) + 1;
        var b = new Builder(numberBase, width);

        var carryRow = b.AddRow(SchoolbookRowKind.Small);
        var operandRows = new List<int>();
        var operandCells = new List<int[]>();
        foreach (var number in digits)
        {
            var row = b.AddRow(SchoolbookRowKind.Normal);
            operandRows.Add(row);
            operandCells.Add(b.AddNumber(row, number, SchoolbookCellKind.Operand, visible: true));
        }
        b.AddCell(operandRows[^1], 0, "+", SchoolbookCellKind.Operator, visible: true);
        b.AddRule(operandRows[^1]);
        var resultRow = b.AddRow(SchoolbookRowKind.Normal);

        b.AddColumns(digits, operandCells, carryRow, resultRow, Digits.Of(sum, numberBase).Count, SchoolbookCellKind.Result, firstStepHides: []);

        var title = string.Join(" + ", addends.Select(a => Digits.Format(a, numberBase)));
        return b.Build(title, sum, toneCount: numberBase);
    }

    /// <summary>
    /// Long addition of two non-negative numbers.
    /// </summary>
    public static SchoolbookWork Addition(BigInteger augend, BigInteger addend, int numberBase = 10) =>
        Addition([augend, addend], numberBase);

    /// <summary>
    /// Long subtraction, borrowing from the left (and across zeros) when a column's top digit is too small.
    /// The minuend must be at least as large as the subtrahend, and neither may be negative.
    /// </summary>
    public static SchoolbookWork Subtraction(BigInteger minuend, BigInteger subtrahend, int numberBase = 10)
    {
        Digits.ThrowIfInvalid(numberBase);
        if (subtrahend.Sign < 0) { throw new ArgumentException("The subtrahend must not be negative.", nameof(subtrahend)); }
        if (minuend < subtrahend) { throw new ArgumentException("The minuend must be at least as large as the subtrahend.", nameof(minuend)); }

        var top = Digits.Of(minuend, numberBase);
        var bottom = Digits.Of(subtrahend, numberBase);
        var difference = minuend - subtrahend;
        var differenceLength = Digits.Of(difference, numberBase).Count;
        var width = top.Count + 1;

        // Work the borrows out first so we know how many borrow rows to make room for.
        var current = top.ToArray();
        var rewrites = new List<int>[top.Count];
        for (var i = 0; i < rewrites.Length; i++) { rewrites[i] = []; }
        var borrows = new List<(int Place, int Lender, int[] Rewritten)>();
        for (var place = 0; place < top.Count; place++)
        {
            var below = place < bottom.Count ? bottom[place] : 0;
            if (current[place] >= below) { continue; }

            var lender = place + 1;
            while (current[lender] == 0) { lender++; }
            var rewritten = new List<int>();
            for (var p = lender; p >= place; p--)
            {
                current[p] = p == lender ? current[p] - 1
                    : p == place ? current[p] + numberBase
                    : numberBase - 1;
                rewrites[p].Add(current[p]);
                rewritten.Add(p);
            }
            borrows.Add((place, lender, [.. rewritten]));
        }

        var borrowRowCount = rewrites.Max(r => r.Count);
        var b = new Builder(numberBase, width);
        var borrowRows = new int[borrowRowCount];
        for (var level = borrowRowCount - 1; level >= 0; level--)
        {
            borrowRows[level] = b.AddRow(SchoolbookRowKind.Small);
        }
        var topRow = b.AddRow(SchoolbookRowKind.Normal);
        var topCells = b.AddNumber(topRow, top, SchoolbookCellKind.Operand, visible: true);
        var bottomRow = b.AddRow(SchoolbookRowKind.Normal);
        var bottomCells = b.AddNumber(bottomRow, bottom, SchoolbookCellKind.Operand, visible: true);
        b.AddCell(bottomRow, 0, "−", SchoolbookCellKind.Operator, visible: true);
        b.AddRule(bottomRow);
        var resultRow = b.AddRow(SchoolbookRowKind.Normal);

        // The cell currently holding each place's value: the minuend digit, or its latest rewrite.
        var holder = (int[])topCells.Clone();
        var rewriteLevel = new int[top.Count];
        var nextBorrow = 0;
        for (var place = 0; place < top.Count; place++)
        {
            var below = place < bottom.Count ? bottom[place] : 0;
            var belowCell = place < bottom.Count ? bottomCells[place] : (int?)null;
            var highlighted = belowCell is { } bc ? new[] { holder[place], bc } : [holder[place]];

            if (nextBorrow < borrows.Count && borrows[nextBorrow].Place == place)
            {
                var borrow = borrows[nextBorrow++];
                var revealed = new List<int>();
                var struck = new List<int>();
                foreach (var p in borrow.Rewritten)
                {
                    var level = rewriteLevel[p]++;
                    var cell = b.AddCell(borrowRows[level], b.Column(p), Digits.Format(rewrites[p][level], numberBase),
                        SchoolbookCellKind.Borrow, visible: false);
                    struck.Add(holder[p]);
                    revealed.Add(cell);
                    holder[p] = cell;
                }

                var before = b.Cells[struck[^1]].Text;
                var across = borrow.Lender - place > 1 ? $", borrowing across the zeros in between" : "";
                b.AddStep(new SchoolbookStep
                {
                    Description = $"{Places.ColumnTitle(place, numberBase)}: {before} is less than {Digits.Char(below)}, "
                        + $"so borrow 1 from {Places.Column(borrow.Lender, numberBase)}{across}. "
                        + $"{before} becomes {b.Cells[holder[place]].Text}.",
                    Highlighted = highlighted,
                    Revealed = revealed,
                    StruckOut = struck,
                    Tone = current[place] - below
                });
                highlighted = belowCell is { } bc2 ? [holder[place], bc2] : [holder[place]];
            }

            var digit = current[place] - below;
            var writes = place < differenceLength || place == 0;
            var resultCells = new List<int>();
            if (writes)
            {
                resultCells.Add(b.AddCell(resultRow, b.Column(place), Digits.Char(digit), SchoolbookCellKind.Result, visible: false));
            }

            var topText = b.Cells[holder[place]].Text;
            b.AddStep(new SchoolbookStep
            {
                Description = $"{Places.ColumnTitle(place, numberBase)}: {topText} − {Digits.Char(below)} = {Digits.Char(digit)}."
                    + (writes ? $" Write {Digits.Char(digit)}." : " That's a leading zero, so leave it blank."),
                Highlighted = highlighted,
                Revealed = resultCells,
                Tone = digit
            });
        }

        var title = $"{Digits.Format(minuend, numberBase)} − {Digits.Format(subtrahend, numberBase)}";
        return b.Build(title, difference, toneCount: numberBase);
    }

    /// <summary>
    /// Long multiplication of two non-negative numbers, laid out in the given style.
    /// </summary>
    public static SchoolbookWork Multiplication(BigInteger multiplicand, BigInteger multiplier, int numberBase = 10,
        MultiplicationLayout layout = MultiplicationLayout.DigitPairs)
    {
        Digits.ThrowIfInvalid(numberBase);
        if (multiplicand.Sign < 0 || multiplier.Sign < 0) { throw new ArgumentException("Factors must not be negative."); }

        return layout switch
        {
            MultiplicationLayout.DigitPairs => MultiplyByDigitPairs(multiplicand, multiplier, numberBase),
            MultiplicationLayout.RowCarries => MultiplyWithRowCarries(multiplicand, multiplier, numberBase),
            MultiplicationLayout.Folded => MultiplyFolded(multiplicand, multiplier, numberBase),
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };
    }

    /// <summary>
    /// One partial product per pair of digits (n × m rows, each shifted with placeholder zeros), then the sum.
    /// </summary>
    private static SchoolbookWork MultiplyByDigitPairs(BigInteger multiplicand, BigInteger multiplier, int numberBase)
    {
        var top = Digits.Of(multiplicand, numberBase);
        var bottom = Digits.Of(multiplier, numberBase);
        var product = multiplicand * multiplier;
        var width = Math.Max(Digits.Of(product, numberBase).Count, Math.Max(top.Count, bottom.Count)) + 1;
        var b = new Builder(numberBase, width);

        var topRow = b.AddRow(SchoolbookRowKind.Normal);
        var topCells = b.AddNumber(topRow, top, SchoolbookCellKind.Operand, visible: true);
        var bottomRow = b.AddRow(SchoolbookRowKind.Normal);
        var bottomCells = b.AddNumber(bottomRow, bottom, SchoolbookCellKind.Operand, visible: true);
        b.AddCell(bottomRow, 0, "×", SchoolbookCellKind.Operator, visible: true);
        b.AddRule(bottomRow);

        var pairCount = top.Count * bottom.Count;
        if (pairCount == 1)
        {
            // One digit times one digit: the only partial product is the answer.
            var resultRow = b.AddRow(SchoolbookRowKind.Normal);
            var value = top[0] * bottom[0];
            var resultCells = b.AddNumber(resultRow, Digits.Of(value, numberBase), SchoolbookCellKind.Result, visible: false);
            b.AddStep(new SchoolbookStep
            {
                Description = $"{Digits.Char(top[0])} × {Digits.Char(bottom[0])} = {Digits.Format(value, numberBase)}.",
                Highlighted = [topCells[0], bottomCells[0]],
                Revealed = resultCells,
                Tone = value % numberBase
            });
            return b.Build($"{Digits.Format(multiplicand, numberBase)} × {Digits.Format(multiplier, numberBase)}", product, numberBase);
        }

        var sumCarryRow = b.AddRow(SchoolbookRowKind.Small);
        var partialDigits = new List<IReadOnlyList<int>>();
        var partialCells = new List<int[]>();
        var lastPartialRow = -1;
        for (var j = 0; j < bottom.Count; j++)
        {
            for (var i = 0; i < top.Count; i++)
            {
                var row = b.AddRow(SchoolbookRowKind.Normal);
                lastPartialRow = row;
                var value = top[i] * bottom[j];
                var shift = i + j;
                var digits = new List<int>();
                var cells = new List<int>();
                for (var p = 0; p < shift; p++)
                {
                    digits.Add(0);
                    cells.Add(b.AddCell(row, b.Column(p), "0", SchoolbookCellKind.Placeholder, visible: false));
                }
                foreach (var digit in Digits.Of(value, numberBase))
                {
                    cells.Add(b.AddCell(row, b.Column(digits.Count), Digits.Char(digit), SchoolbookCellKind.Partial, visible: false));
                    digits.Add(digit);
                }
                partialDigits.Add(digits);
                partialCells.Add([.. cells]);

                var zeros = shift switch { 0 => "", 1 => ", shifted one place", _ => $", shifted {shift} places" };
                b.AddStep(new SchoolbookStep
                {
                    Description = $"{Digits.Char(top[i])} × {Digits.Char(bottom[j])} = {Digits.Format(value, numberBase)}{zeros}.",
                    Highlighted = [topCells[i], bottomCells[j]],
                    Revealed = cells,
                    Tone = value % numberBase
                });
            }
        }

        var plus = b.AddCell(lastPartialRow, 0, "+", SchoolbookCellKind.Operator, visible: false);
        b.AddRule(lastPartialRow);
        var sumRow = b.AddRow(SchoolbookRowKind.Normal);
        b.AddColumns(partialDigits, partialCells, sumCarryRow, sumRow, Digits.Of(product, numberBase).Count, SchoolbookCellKind.Result,
            firstStepHides: [], firstStepReveals: [plus], announceSum: true);

        return b.Build($"{Digits.Format(multiplicand, numberBase)} × {Digits.Format(multiplier, numberBase)}", product, numberBase);
    }

    /// <summary>
    /// Only the factors and the answer. Each step works one column of the answer: the sum of every
    /// digit product that lands in that column, plus the carry.
    /// </summary>
    private static SchoolbookWork MultiplyFolded(BigInteger multiplicand, BigInteger multiplier, int numberBase)
    {
        var top = Digits.Of(multiplicand, numberBase);
        var bottom = Digits.Of(multiplier, numberBase);
        var product = multiplicand * multiplier;
        var productLength = Digits.Of(product, numberBase).Count;
        var width = Math.Max(productLength, Math.Max(top.Count, bottom.Count)) + 1;
        var b = new Builder(numberBase, width);

        var topRow = b.AddRow(SchoolbookRowKind.Normal);
        var topCells = b.AddNumber(topRow, top, SchoolbookCellKind.Operand, visible: true);
        var bottomRow = b.AddRow(SchoolbookRowKind.Normal);
        var bottomCells = b.AddNumber(bottomRow, bottom, SchoolbookCellKind.Operand, visible: true);
        b.AddCell(bottomRow, 0, "×", SchoolbookCellKind.Operator, visible: true);
        b.AddRule(bottomRow);
        var carryRow = b.AddRow(SchoolbookRowKind.Small);
        var resultRow = b.AddRow(SchoolbookRowKind.Normal);

        var lastPlace = top.Count + bottom.Count - 2;
        BigInteger carry = 0;
        int? carryCell = null;
        for (var place = 0; place <= lastPlace; place++)
        {
            var highlighted = new HashSet<int>();
            var products = new List<string>();
            BigInteger total = carry;
            for (var i = Math.Max(0, place - bottom.Count + 1); i <= Math.Min(place, top.Count - 1); i++)
            {
                var j = place - i;
                total += top[i] * bottom[j];
                products.Add($"{Digits.Char(top[i])}×{Digits.Char(bottom[j])}");
                highlighted.Add(topCells[i]);
                highlighted.Add(bottomCells[j]);
            }
            if (carryCell is { } cc) { highlighted.Add(cc); }

            var terms = products.Count <= 4
                ? string.Join(" + ", products)
                : $"{products.Count} digit products";
            var expression = terms + (carry > 0 ? $" + {Digits.Format(carry, numberBase)} carried" : "");
            var revealed = new List<int>();
            string description;
            if (place == lastPlace)
            {
                var totalDigits = Digits.Of(total, numberBase);
                for (var k = 0; k < totalDigits.Count; k++)
                {
                    if (place + k > 0 && place + k >= productLength) { continue; }
                    revealed.Add(b.AddCell(resultRow, b.Column(place + k), Digits.Char(totalDigits[k]), SchoolbookCellKind.Result, visible: false));
                }
                description = $"{Places.ColumnTitle(place, numberBase)}: {expression} = {Digits.Format(total, numberBase)}. "
                    + $"Write {Digits.Format(total, numberBase)}.";
                carryCell = null;
            }
            else
            {
                var digit = (int)(total % numberBase);
                var nextCarry = total / numberBase;
                revealed.Add(b.AddCell(resultRow, b.Column(place), Digits.Char(digit), SchoolbookCellKind.Result, visible: false));
                carryCell = null;
                if (nextCarry > 0)
                {
                    var newCarry = b.AddCell(carryRow, b.Column(place + 1), Digits.Format(nextCarry, numberBase), SchoolbookCellKind.Carry, visible: false);
                    revealed.Add(newCarry);
                    carryCell = newCarry;
                }
                description = $"{Places.ColumnTitle(place, numberBase)}: {expression} = {Digits.Format(total, numberBase)}. "
                    + $"Write {Digits.Char(digit)}" + (nextCarry > 0 ? $", carry {Digits.Format(nextCarry, numberBase)}." : ".");
                carry = nextCarry;
            }

            b.AddStep(new SchoolbookStep
            {
                Description = description,
                Highlighted = [.. highlighted],
                Revealed = revealed,
                Tone = (int)(total % numberBase)
            });
        }

        return b.Build($"{Digits.Format(multiplicand, numberBase)} × {Digits.Format(multiplier, numberBase)}", product, numberBase);
    }

    /// <summary>
    /// Long multiplication: one partial product per digit of the multiplier (shifted left with
    /// placeholder zeros), then the partial products added up. A one-digit multiplier skips the addition.
    /// </summary>
    private static SchoolbookWork MultiplyWithRowCarries(BigInteger multiplicand, BigInteger multiplier, int numberBase)
    {
        var top = Digits.Of(multiplicand, numberBase);
        var bottom = Digits.Of(multiplier, numberBase);
        var product = multiplicand * multiplier;
        var width = Math.Max(Digits.Of(product, numberBase).Count, Math.Max(top.Count, bottom.Count)) + 1;
        var b = new Builder(numberBase, width);

        var carryRow = b.AddRow(SchoolbookRowKind.Small);
        var topRow = b.AddRow(SchoolbookRowKind.Normal);
        var topCells = b.AddNumber(topRow, top, SchoolbookCellKind.Operand, visible: true);
        var bottomRow = b.AddRow(SchoolbookRowKind.Normal);
        var bottomCells = b.AddNumber(bottomRow, bottom, SchoolbookCellKind.Operand, visible: true);
        b.AddCell(bottomRow, 0, "×", SchoolbookCellKind.Operator, visible: true);
        b.AddRule(bottomRow);

        var singleDigit = bottom.Count == 1;
        var sumCarryRow = singleDigit ? -1 : b.AddRow(SchoolbookRowKind.Small);
        var partialRows = new int[bottom.Count];
        for (var j = 0; j < bottom.Count; j++) { partialRows[j] = b.AddRow(SchoolbookRowKind.Normal); }
        var partialKind = singleDigit ? SchoolbookCellKind.Result : SchoolbookCellKind.Partial;

        var partialDigits = new List<IReadOnlyList<int>>();
        var partialCells = new List<int[]>();
        var leftoverCarries = new List<int>();
        for (var j = 0; j < bottom.Count; j++)
        {
            var row = partialRows[j];
            var multiplierDigit = bottom[j];
            var digitsOfPartial = new List<int>();
            var cellsOfPartial = new List<int>();
            var hide = new List<int>(leftoverCarries);
            leftoverCarries.Clear();

            if (j > 0)
            {
                var placeholders = new List<int>();
                for (var p = 0; p < j; p++)
                {
                    placeholders.Add(b.AddCell(row, b.Column(p), "0", SchoolbookCellKind.Placeholder, visible: false));
                    digitsOfPartial.Add(0);
                }
                cellsOfPartial.AddRange(placeholders);
                b.AddStep(new SchoolbookStep
                {
                    Description = $"Next multiplier digit: {Digits.Char(multiplierDigit)}, from {Places.Column(j, numberBase)}. Shift left: "
                        + $"write {(j == 1 ? "a placeholder zero" : $"{j} placeholder zeros")} first.",
                    Highlighted = [bottomCells[j]],
                    Revealed = placeholders,
                    Hidden = hide,
                    Tone = 0
                });
                hide = [];
            }

            var carry = 0;
            int? carryCell = null;
            for (var i = 0; i < top.Count; i++)
            {
                var value = top[i] * multiplierDigit + carry;
                var highlighted = new List<int> { topCells[i], bottomCells[j] };
                if (carryCell is { } cc) { highlighted.Add(cc); }

                var revealed = new List<int>();
                string description;
                var times = $"{Digits.Char(top[i])} × {Digits.Char(multiplierDigit)}";
                var plusCarry = carry > 0 ? $" + {Digits.Char(carry)} carried" : "";
                if (i == top.Count - 1)
                {
                    // The last digit writes everything it has; nothing is left to carry into.
                    var valueDigits = Digits.Of(value, numberBase);
                    for (var k = 0; k < valueDigits.Count; k++)
                    {
                        revealed.Add(b.AddCell(row, b.Column(i + j + k), Digits.Char(valueDigits[k]), partialKind, visible: false));
                        digitsOfPartial.Add(valueDigits[k]);
                    }
                    cellsOfPartial.AddRange(revealed);
                    description = $"{times}{plusCarry} = {Digits.Format(value, numberBase)}. Write {Digits.Format(value, numberBase)}.";
                    carryCell = null;
                }
                else
                {
                    var digit = value % numberBase;
                    var nextCarry = value / numberBase;
                    var written = b.AddCell(row, b.Column(i + j), Digits.Char(digit), partialKind, visible: false);
                    revealed.Add(written);
                    digitsOfPartial.Add(digit);
                    cellsOfPartial.Add(written);
                    carryCell = null;
                    if (nextCarry > 0)
                    {
                        // Multiplication carries go above the multiplicand digit they'll be added to.
                        var newCarry = b.AddCell(carryRow, b.Column(i + 1), Digits.Char(nextCarry), SchoolbookCellKind.Carry, visible: false);
                        revealed.Add(newCarry);
                        leftoverCarries.Add(newCarry);
                        carryCell = newCarry;
                    }
                    description = $"{times}{plusCarry} = {Digits.Format(value, numberBase)}. Write {Digits.Char(digit)}"
                        + (nextCarry > 0 ? $", carry {Digits.Char(nextCarry)}." : ".");
                    carry = nextCarry;
                }

                b.AddStep(new SchoolbookStep
                {
                    Description = description,
                    Highlighted = highlighted,
                    Revealed = revealed,
                    Hidden = hide,
                    Tone = (int)(value % numberBase)
                });
                hide = [];
            }

            partialDigits.Add(digitsOfPartial);
            partialCells.Add([.. cellsOfPartial]);
        }

        if (!singleDigit)
        {
            b.AddCell(partialRows[^1], 0, "+", SchoolbookCellKind.Operator, visible: false);
            b.AddRule(partialRows[^1]);
            var resultRow = b.AddRow(SchoolbookRowKind.Normal);
            b.AddColumns(partialDigits, partialCells, sumCarryRow, resultRow, Digits.Of(product, numberBase).Count, SchoolbookCellKind.Result,
                firstStepHides: leftoverCarries, firstStepReveals: [b.Cells.Count - 1], announceSum: true);
        }

        var title = $"{Digits.Format(multiplicand, numberBase)} × {Digits.Format(multiplier, numberBase)}";
        return b.Build(title, product, toneCount: numberBase);
    }

    private sealed class Builder(int numberBase, int width)
    {
        private readonly List<SchoolbookRowKind> _rows = [];
        private readonly List<SchoolbookRule> _rules = [];
        private readonly List<SchoolbookStep> _steps = [];

        public List<SchoolbookCell> Cells { get; } = [];

        public int Column(int place) => width - 1 - place;

        public int AddRow(SchoolbookRowKind kind)
        {
            _rows.Add(kind);
            return _rows.Count - 1;
        }

        public int AddCell(int row, int column, string text, SchoolbookCellKind kind, bool visible)
        {
            Cells.Add(new SchoolbookCell(row, column, text, kind, visible));
            return Cells.Count - 1;
        }

        /// <summary>Adds a number's digits to a row and returns their cells, least significant first.</summary>
        public int[] AddNumber(int row, IReadOnlyList<int> digits, SchoolbookCellKind kind, bool visible) =>
            [.. digits.Select((d, place) => AddCell(row, Column(place), Digits.Char(d), kind, visible))];

        public void AddRule(int afterRow) => _rules.Add(new SchoolbookRule(afterRow, 0, width - 1));

        public void AddStep(SchoolbookStep step) => _steps.Add(step);

        /// <summary>
        /// Adds stacked numbers column by column, writing carries in <paramref name="carryRow"/>.
        /// </summary>
        public void AddColumns(IReadOnlyList<IReadOnlyList<int>> numbers, IReadOnlyList<int[]> numberCells,
            int carryRow, int resultRow, int resultLength, SchoolbookCellKind resultKind, IReadOnlyList<int> firstStepHides,
            IReadOnlyList<int>? firstStepReveals = null, bool announceSum = false)
        {
            var columns = numbers.Max(n => n.Count);
            var carry = 0;
            int? carryCell = null;
            var hide = firstStepHides;
            var extraReveals = firstStepReveals ?? [];
            for (var place = 0; place < columns; place++)
            {
                var highlighted = new List<int>();
                var terms = new List<int>();
                for (var n = 0; n < numbers.Count; n++)
                {
                    if (place < numbers[n].Count)
                    {
                        terms.Add(numbers[n][place]);
                        highlighted.Add(numberCells[n][place]);
                    }
                }
                if (carryCell is { } cc) { highlighted.Add(cc); }

                var total = terms.Sum() + carry;
                var revealed = new List<int>(extraReveals);
                var expression = string.Join(" + ", terms.Select(Digits.Char))
                    + (carry > 0 ? $" + {Digits.Format(carry, numberBase)} carried" : "");
                string description;
                if (place == columns - 1)
                {
                    var totalDigits = Digits.Of(total, numberBase);
                    for (var k = 0; k < totalDigits.Count; k++)
                    {
                        // Skip leading zeros (e.g. 0 × 55, whose partial products are 0 and 00).
                        if (place + k > 0 && place + k >= resultLength) { continue; }
                        revealed.Add(AddCell(resultRow, Column(place + k), Digits.Char(totalDigits[k]), resultKind, visible: false));
                    }
                    description = $"{Places.ColumnTitle(place, numberBase)}: {expression} = {Digits.Format(total, numberBase)}. "
                        + $"Write {Digits.Format(total, numberBase)}.";
                    carryCell = null;
                }
                else
                {
                    var digit = total % numberBase;
                    var nextCarry = total / numberBase;
                    revealed.Add(AddCell(resultRow, Column(place), Digits.Char(digit), resultKind, visible: false));
                    carryCell = null;
                    if (nextCarry > 0)
                    {
                        var newCarry = AddCell(carryRow, Column(place + 1), Digits.Format(nextCarry, numberBase), SchoolbookCellKind.Carry, visible: false);
                        revealed.Add(newCarry);
                        carryCell = newCarry;
                    }
                    description = $"{Places.ColumnTitle(place, numberBase)}: {expression} = {Digits.Format(total, numberBase)}. "
                        + $"Write {Digits.Char(digit)}" + (nextCarry > 0 ? $", carry {Digits.Format(nextCarry, numberBase)}." : ".");
                    carry = nextCarry;
                }

                AddStep(new SchoolbookStep
                {
                    Description = place == 0 && announceSum ? "Now add the partial products. " + description : description,
                    Highlighted = highlighted,
                    Revealed = revealed,
                    Hidden = hide,
                    Tone = (int)(total % numberBase)
                });
                hide = [];
                extraReveals = [];
            }
        }

        public SchoolbookWork Build(string title, BigInteger result, int toneCount) =>
            new(title, numberBase, result, toneCount, _rows, width, Cells, _rules, _steps);
    }
}

/// <summary>
/// Digit helpers for bases 2 through 16.
/// </summary>
internal static class Digits
{
    private const string Alphabet = "0123456789ABCDEF";

    public static void ThrowIfInvalid(int numberBase)
    {
        if (numberBase < 2 || numberBase > Alphabet.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(numberBase), "The base must be between 2 and 16.");
        }
    }

    public static string Char(int digit) => Alphabet[digit].ToString();

    /// <summary>Gets a number's digits, least significant first. Zero has one digit.</summary>
    public static List<int> Of(BigInteger value, int numberBase)
    {
        var digits = new List<int>();
        do
        {
            digits.Add((int)(value % numberBase));
            value /= numberBase;
        } while (value > 0);
        return digits;
    }

    public static string Format(BigInteger value, int numberBase)
    {
        var digits = Of(value, numberBase);
        digits.Reverse();
        return string.Concat(digits.Select(Char));
    }
}

/// <summary>
/// Names for place values: "the ones column" and so on in base 10, positional names otherwise.
/// </summary>
internal static class Places
{
    private static readonly string[] DecimalNames =
        ["ones", "tens", "hundreds", "thousands", "ten-thousands", "hundred-thousands", "millions"];

    /// <summary>E.g. "the tens column", or "column 2 from the right".</summary>
    public static string Column(int place, int numberBase) =>
        numberBase == 10 && place < DecimalNames.Length
            ? $"the {DecimalNames[place]} column"
            : place == 0 ? "the rightmost column" : $"column {place + 1} from the right";

    /// <summary>E.g. "The tens column", for the start of a sentence.</summary>
    public static string ColumnTitle(int place, int numberBase)
    {
        var name = Column(place, numberBase);
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
