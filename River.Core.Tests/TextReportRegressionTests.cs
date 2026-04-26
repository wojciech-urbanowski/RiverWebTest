using System.Globalization;
using River.Core;

namespace River.Core.Tests;

/// <summary>
/// Testy regresyjne — porównują wynik <see cref="TextReport.Build"/> z fixture'ami
/// wygenerowanymi przez oryginalny program WinForms (zakładka „Tekst").
/// Kultura pl-PL żeby pasowały przecinki dziesiętne.
/// </summary>
public class TextReportRegressionTests
{
    private static readonly CultureInfo PlPl = new("pl-PL");

    [Fact]
    public void EmptySegmentList_MatchesFixture()
    {
        var globals = new ModelParams();
        var segments = Array.Empty<SegmentInput>();

        var results = RiverModel.Compute(globals, segments);
        var actual = TextReport.Build(globals, segments, results, PlPl);

        var expected = ReadFixture("empty.txt");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleDefaultSegment_MatchesFixture()
    {
        var globals = new ModelParams();
        var segments = new[] { new SegmentInput() };

        var results = RiverModel.Compute(globals, segments);
        var actual = TextReport.Build(globals, segments, results, PlPl);

        var expected = ReadFixture("single_default_segment.txt");
        Assert.Equal(expected, actual);
    }

    private static string ReadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
        // Oryginalny RichTextBox używa "\n" — fixture zapisany z CRLF (Windows)
        // normalizujemy do "\n" żeby porównanie było stabilne.
        return File.ReadAllText(path).Replace("\r\n", "\n");
    }
}
