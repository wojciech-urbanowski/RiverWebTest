namespace River.Core;

public enum PollutionSource
{
    None = 0,
    Point = 1,
    Linear = 2
}

/// <summary>
/// Parametry pojedynczego odcinka rzeki. Odpowiednik <c>FramePart</c> w wersji WinForms.
/// </summary>
public sealed record SegmentInput
{
    /// <summary>Długość odcinka [km].</summary>
    public double L { get; init; } = 10.0;

    /// <summary>Natężenie przepływu [m³/s].</summary>
    public double Q { get; init; } = 30.0;

    /// <summary>Prędkość przepływu [km/h].</summary>
    public double V { get; init; } = 0.5;

    /// <summary>Temperatura [°C].</summary>
    public double T { get; init; } = 20.0;

    /// <summary>Liczba kroków obliczeniowych w odcinku.</summary>
    public int N { get; init; } = 10;

    /// <summary>Czy to odcinek początkowy (warunki początkowe brane z <see cref="InitB"/>/<see cref="InitN"/>/<see cref="InitDeficit"/>).</summary>
    public bool IsInitial { get; init; } = true;

    /// <summary>Warunek początkowy: BZT5 [gO2/m³].</summary>
    public double InitB { get; init; } = 30.0;

    /// <summary>Warunek początkowy: N-NH3 [gN/m³].</summary>
    public double InitN { get; init; } = 5.0;

    /// <summary>Warunek początkowy: deficyt tlenu [gO2/m³].</summary>
    public double InitDeficit { get; init; } = 2.0;

    /// <summary>Rodzaj źródła zanieczyszczeń wzdłuż odcinka.</summary>
    public PollutionSource Source { get; init; } = PollutionSource.None;

    /// <summary>Przepływ źródła [m³/s] (oryginał trzyma w m³/s po podzieleniu wartości z UI [m³/d] przez 86400).</summary>
    public double SrcQ { get; init; } = 0.0;

    /// <summary>BZT5 źródła [gO2/m³].</summary>
    public double SrcB { get; init; } = 30.0;

    /// <summary>N-NH3 źródła [gN/m³].</summary>
    public double SrcN { get; init; } = 5.0;

    /// <summary>Stężenie tlenu w źródle [gO2/m³] (w oryginale `vSO`, mylnie nazwane „deficyt" w UI).</summary>
    public double SrcO { get; init; } = 2.0;

    /// <summary>
    /// Numer odcinka docelowego względem bieżącego: 0 = ujście (brak połączenia),
    /// 1 = następny odcinek, 2 = przeskok o jeden, itd.
    /// Odpowiada <c>vOUT</c> = <c>comboBoxOut.SelectedIndex</c>.
    /// </summary>
    public int OutletTo { get; init; } = 0;

    /// <summary>Czy odcinek ma być rysowany na profilu wykresu.</summary>
    public bool Plot { get; init; } = true;
}
