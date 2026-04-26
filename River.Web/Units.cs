namespace River.Web;

/// <summary>
/// Unit strings with proper typography: superscripts, subscripts, and non-breaking spacing.
/// Uses Unicode characters for proper rendering across all contexts (Blazor, Plotly, etc.)
/// </summary>
public static class Units
{
    // Rate constants
    public const string PerDay = "1/d";
    public const string PerDayPerM3 = "g/m³/d";

    // Hydraulic parameters
    public const string Kilometer = "km";
    public const string CubicMeterPerSecond = "m³/s";
    public const string CubicMeterPerDay = "m³/d";
    public const string KilometerPerHour = "km/h";
    public const string Celsius = "°C";

    // Water quality parameters
    public const string GramO2PerM3 = "g O₂/m³";
    public const string GramNPerM3 = "g N/m³";

    // Axis and legend labels
    public const string DistanceAxis = "L [km]";
    public const string ConcentrationAxis = "stężenie";
    public const string O2Legend = "O₂";
    public const string BZT5Legend = "BZT₅";
    public const string NAmmoniaLegend = "N-NH₃";
}
