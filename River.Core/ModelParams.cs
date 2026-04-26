namespace River.Core;

/// <summary>
/// Globalne parametry modelu kinetycznego (zakładka „parametry" w lewym górnym panelu).
/// Wartości w temperaturze odniesienia 20°C — kompensacja temperaturowa stosowana per odcinek.
/// </summary>
public sealed record ModelParams(
    double K1 = 0.5,    // 1/d  – stała szybkości biochemicznego rozkładu zanieczyszczeń organicznych
    double K2 = 0.3,    // 1/d  – współczynnik reaeracji
    double K3 = 0.25,   // 1/d  – stała szybkości usuwania substancji organicznych na drodze sedymentacji
    double K4 = 0.3,    // 1/d  – stała szybkości nitryfikacji
    double RB = 1.0,    // g/m3/d – współczynnik zapotrzebowania tlenu przez osady denne
    double RAP = 1.0    // g/m3/d – szybkość dostarczania tlenu przez mikroorganizmy (rAP-rAR)
);
