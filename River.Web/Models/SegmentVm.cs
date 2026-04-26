using River.Core;

namespace River.Web.Models;

/// <summary>
/// Mutowalny ViewModel pojedynczego odcinka. Pole <see cref="SrcQDay"/> jest w m³/d
/// (jak w UI oryginału) — konwersja do m³/s przy budowie <see cref="SegmentInput"/>.
/// </summary>
public sealed class SegmentVm
{
    public double L { get; set; } = 10.0;             // km
    public double Q { get; set; } = 30.0;             // m³/s
    public double V { get; set; } = 0.5;              // km/h
    public double T { get; set; } = 20.0;             // °C
    public int N { get; set; } = 10;

    public bool IsInitial { get; set; } = true;
    public double InitB { get; set; } = 30.0;         // BZT5 gO2/m³
    public double InitN { get; set; } = 5.0;          // N-NH3 gN/m³
    public double InitDeficit { get; set; } = 2.0;    // gO2/m³

    public PollutionSource Source { get; set; } = PollutionSource.None;
    public double SrcQDay { get; set; } = 100.0;      // m³/d (UI używa m³/d)
    public double SrcB { get; set; } = 30.0;
    public double SrcN { get; set; } = 5.0;
    public double SrcO { get; set; } = 2.0;

    public int OutletTo { get; set; } = 0;
    public bool Plot { get; set; } = true;

    public SegmentInput ToRecord() => new()
    {
        L = L, Q = Q, V = V, T = T, N = N,
        IsInitial = IsInitial,
        InitB = InitB, InitN = InitN, InitDeficit = InitDeficit,
        Source = Source,
        SrcQ = SrcQDay / 86400.0,
        SrcB = SrcB, SrcN = SrcN, SrcO = SrcO,
        OutletTo = OutletTo, Plot = Plot
    };
}
