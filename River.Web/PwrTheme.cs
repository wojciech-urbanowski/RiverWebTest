using MudBlazor;

namespace River.Web;

/// <summary>
/// Motyw MudBlazor oparty o barwy logotypu Politechniki Wrocławskiej.
/// Kolory wyciągnięte z oficjalnego pliku <c>logoPWr_PL_poziom_kolor_v1.png</c>.
/// </summary>
public static class PwrTheme
{
    /// <summary>Czerwień PWr (tarcza herbu).</summary>
    public const string Red = "#b12009";

    /// <summary>Kremowa barwa orła z herbu.</summary>
    public const string Cream = "#f8d0a0";

    /// <summary>Ciemniejszy odcień czerwieni do hover/dark.</summary>
    public const string RedDark = "#8a1907";

    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Red,
            PrimaryDarken = RedDark,
            Secondary = Cream,
            AppbarBackground = "#ffffff",
            AppbarText = "#000000",
            Background = "#fafafa",
            DrawerBackground = "#ffffff",
        }
    };
}
