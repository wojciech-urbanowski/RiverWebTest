namespace River.Core;

/// <summary>
/// Wynik obliczeń dla pojedynczego odcinka. Tablice mają długość N+1
/// (punkt 0 = wejście do odcinka, punkt N = wyjście).
/// </summary>
public sealed class SegmentResult
{
    /// <summary>Pozycja wzdłuż odcinka [km].</summary>
    public required double[] X { get; init; }

    /// <summary>Czas przepływu od początku odcinka [d].</summary>
    public required double[] T { get; init; }

    /// <summary>Natężenie przepływu [m³/s].</summary>
    public required double[] Q { get; init; }

    /// <summary>BZT całkowite [gO2/m³].</summary>
    public required double[] B { get; init; }

    /// <summary>BZT5 [gO2/m³].</summary>
    public required double[] B5 { get; init; }

    /// <summary>N-NH3 [gN/m³].</summary>
    public required double[] N { get; init; }

    /// <summary>Stężenie tlenu rozpuszczonego [gO2/m³].</summary>
    public required double[] O { get; init; }
}
