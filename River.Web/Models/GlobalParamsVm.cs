using River.Core;

namespace River.Web.Models;

/// <summary>Mutowalny ViewModel dla globalnych parametrów modelu — wygodny do dwukierunkowego bindowania w MudBlazor.</summary>
public sealed class GlobalParamsVm
{
    public double K1  { get; set; } = 0.5;
    public double K2  { get; set; } = 0.3;
    public double K3  { get; set; } = 0.25;
    public double K4  { get; set; } = 0.3;
    public double RB  { get; set; } = 1.0;
    public double RAP { get; set; } = 1.0;

    public ModelParams ToRecord() => new(K1, K2, K3, K4, RB, RAP);
}
