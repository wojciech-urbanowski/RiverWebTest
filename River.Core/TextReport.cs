using System.Globalization;
using System.Text;

namespace River.Core;

/// <summary>
/// Generuje tekstowy raport identyczny z zawartością zakładki „Tekst" w wersji WinForms.
/// Domyślnie używa <see cref="CultureInfo.InvariantCulture"/> (kropka jako separator dziesiętny).
/// Aby odtworzyć dokładnie wynik oryginału (przecinek), przekaż <c>new CultureInfo("pl-PL")</c>.
/// </summary>
public static class TextReport
{
    public static string Build(
        ModelParams globals,
        IReadOnlyList<SegmentInput> segments,
        IReadOnlyList<SegmentResult> results,
        CultureInfo? culture = null)
    {
        var c = culture ?? CultureInfo.InvariantCulture;
        var sb = new StringBuilder();

        sb.Append("Parametry modelu\n");
        sb.Append($"k1; {Fmt(globals.K1, c)}; 1/d\n");
        sb.Append($"k2; {Fmt(globals.K2, c)}; 1/d\n");
        sb.Append($"k3; {Fmt(globals.K3, c)}; 1/d\n");
        sb.Append($"k4; {Fmt(globals.K4, c)}; 1/d\n");
        sb.Append($"rB; {Fmt(globals.RB, c)}; g/m3/d\n");
        sb.Append($"rAP-rAR; {Fmt(globals.RAP, c)}; g/m2/d\n");
        sb.Append('\n');

        for (int i = 0; i < segments.Count; i++)
        {
            WriteSegmentHeader(sb, segments[i], i + 1, c);
            sb.Append('\n');
        }

        sb.Append("Odc.; L; t; Q; BZT; BZT5; N-NH3; O2\n");
        sb.Append("-; km; d; m3/s; gO2/m3; gO2/m3; gN-NH3/m3; gO2/m3\n");
        for (int i = 0; i < segments.Count; i++)
        {
            WriteSegmentValues(sb, results[i], i + 1, c);
        }

        return sb.ToString();
    }

    private static void WriteSegmentHeader(StringBuilder sb, SegmentInput s, int oneBasedNo, CultureInfo c)
    {
        sb.Append($"Odcinek; {oneBasedNo}\n");
        sb.Append("Parametry\n");
        sb.Append($"Długość; {Fmt(s.L, c)}; km\n");
        sb.Append($"Natężenie przepływu; {Fmt(s.Q, c)}; m3/s\n");
        sb.Append($"Prędkość przepływu; {Fmt(s.V, c)}; km/h\n");
        sb.Append($"Temperatura; {Fmt(s.T, c)}; C\n");
        sb.Append($"Ilość odcinków; {s.N}; -\n");
        sb.Append($"Odcinek początkowy; {(s.IsInitial ? "tak" : "nie")}\n");

        if (s.IsInitial)
        {
            sb.Append("Warunki początkowe:\n");
            sb.Append($"BZT5; {Fmt(s.InitB, c)}; gO2/m3\n");
            sb.Append($"N-NH3; {Fmt(s.InitN, c)}; gN/m3\n");
            sb.Append($"deficyt O2; {Fmt(s.InitDeficit, c)}; gO2/m3\n");
        }

        sb.Append("Odpływ do; ");
        if (s.OutletTo <= 0)
            sb.Append("ujście\n");
        else
            sb.Append($"{oneBasedNo + s.OutletTo}\n");

        switch (s.Source)
        {
            case PollutionSource.None:  sb.Append("Źródło zanieczyszczeń; brak\n"); break;
            case PollutionSource.Point: sb.Append("Źródło zanieczyszczeń; punktowe\n"); break;
            case PollutionSource.Linear: sb.Append("Źródło zanieczyszczeń; liniowe\n"); break;
        }
        if (s.Source != PollutionSource.None)
        {
            sb.Append($"Q; {Fmt(s.SrcQ * 86400.0, c)}; m3/d\n");
            sb.Append($"BZT5; {Fmt(s.SrcB, c)}; gO2/m3\n");
            sb.Append($"N-NH3; {Fmt(s.SrcN, c)}; gN/m3\n");
            sb.Append($"O2; {Fmt(s.SrcO, c)}; gO2/m3\n");
        }
    }

    private static void WriteSegmentValues(StringBuilder sb, SegmentResult r, int oneBasedNo, CultureInfo c)
    {
        for (int i = 0; i < r.X.Length; i++)
        {
            sb.Append($"{oneBasedNo}; ");
            sb.Append($"{Fmt(r.X[i], c)}; ");
            sb.Append($"{Fmt(r.T[i], c)}; ");
            sb.Append($"{Fmt(r.Q[i], c)}; ");
            sb.Append($"{Fmt(r.B[i], c)}; ");
            sb.Append($"{Fmt(r.B5[i], c)}; ");
            sb.Append($"{Fmt(r.N[i], c)}; ");
            sb.Append($"{Fmt(r.O[i], c)}\n");
        }
    }

    // Oryginał (WinForms .NET Framework) używa double.ToString() z domyślnym formatem "G15"
    // w bieżącej kulturze — np. 0,0833333333333333 (15 cyfr znaczących).
    // .NET Core+ ma domyślnie shortest round-trip ("0.08333333333333333"), więc wymuszamy G15
    // żeby pasowało 1:1 do fixture'ów wygenerowanych z oryginalnego programu.
    private static string Fmt(double v, CultureInfo c) => v.ToString("G15", c);
}
