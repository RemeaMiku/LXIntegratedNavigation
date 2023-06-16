namespace LXIntegratedNavigation.Shared.Models;

public record class ImuErrorModel
{
    public string? ImuName { get; set; }
    public double Arw { get; set; }
    public double Vrw { get; set; }
    public double StdAccBias { get; set; }
    public double StdAccScale { get; set; }
    public double StdGyroBias { get; set; }
    public double StdGyroScale { get; set; }
    public double CotAccBias { get; set; }
    public double CotAccScale { get; set; }
    public double CotGyroBias { get; set; }
    public double CotGyroScale { get; set; }


    public ImuErrorModel(double arw, double vrw, double stdAccBias, double stdAccScale, double stdGyroBias, double stdGyroScale, double cotAccBias, double cotAccScale, double cotGyroBias, double cotGyroScale, string? imuName = null)
    {
        ImuName = imuName;
        Arw = arw;
        Vrw = vrw;
        StdAccBias = stdAccBias;
        StdAccScale = stdAccScale;
        StdGyroBias = stdGyroBias;
        StdGyroScale = stdGyroScale;
        CotAccBias = cotAccBias;
        CotAccScale = cotAccScale;
        CotGyroBias = cotGyroBias;
        CotGyroScale = cotGyroScale;
    }
}