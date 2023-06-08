using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Essentials.Navigation;

//public record class LooseCombinationOptions(Vector GnssLeverArm, Angle StdInitPhi_n, Angle StdInitPhi_e, Angle StdInitPhi_d, double Arw, double Vrw, double StdAccBias, double StdAccScale, double StdGyroBias, double StdGyroScale, double StdInitR_n, double StdInitR_e, double StdInitR_d, double StdInitV_n, double StdInitV_e, double StdInitV_d, double RelevantTimeAccBias, double RelevantTimeAccScale, double RelevantTimeGyroBias, double RelevantTimeGyroScale);

public record class LooseCombinationOptions
{
    public Vector GnssLeverArm { get; init; }
    public double StdInitR_n { get; init; }
    public double StdInitR_e { get; init; }
    public double StdInitR_d { get; init; }
    public double StdInitV_n { get; init; }
    public double StdInitV_e { get; init; }
    public double StdInitV_d { get; init; }
    public Angle StdInitPhi_n { get; init; }
    public Angle StdInitPhi_e { get; init; }
    public Angle StdInitPhi_d { get; init; }
    public ImuErrorModel ImuErrorModel { get; init; }
    public double Arw => ImuErrorModel.Arw;
    public double Vrw => ImuErrorModel.Vrw;
    public double StdAccBias => ImuErrorModel.StdAccBias;
    public double StdAccScale => ImuErrorModel.StdAccScale;
    public double StdGyroBias => ImuErrorModel.StdGyroBias;
    public double StdGyroScale => ImuErrorModel.StdGyroScale;
    public double CotAccBias => ImuErrorModel.CotAccBias;
    public double CotAccScale => ImuErrorModel.CotAccScale;
    public double CotGyroBias => ImuErrorModel.CotGyroBias;
    public double CotGyroScale => ImuErrorModel.CotGyroScale;
    public LooseCombinationOptions(Vector gnssLeverArm, double stdInitR_n, double stdInitR_e, double stdInitR_d, double stdInitV_n, double stdInitV_e, double stdInitV_d, Angle stdInitPhi_n, Angle stdInitPhi_e, Angle stdInitPhi_d, ImuErrorModel imuErrorModel)
    {
        GnssLeverArm = gnssLeverArm;
        StdInitR_n = stdInitR_n;
        StdInitR_e = stdInitR_e;
        StdInitR_d = stdInitR_d;
        StdInitV_n = stdInitV_n;
        StdInitV_e = stdInitV_e;
        StdInitV_d = stdInitV_d;
        StdInitPhi_n = stdInitPhi_n;
        StdInitPhi_e = stdInitPhi_e;
        StdInitPhi_d = stdInitPhi_d;
        ImuErrorModel = imuErrorModel;
    }

    public LooseCombinationOptions(Vector gnssLeverArm, double[] stdInitR, double[] stdInitV, double[] stdInitPhiDegs, ImuErrorModel imuErrorModel)
    {
        GnssLeverArm = gnssLeverArm;
        StdInitR_n = stdInitR[0];
        StdInitR_e = stdInitR[1];
        StdInitR_d = stdInitR[2];
        StdInitV_n = stdInitV[0];
        StdInitV_e = stdInitV[1];
        StdInitV_d = stdInitV[2];
        StdInitPhi_n = FromDegrees(stdInitPhiDegs[0]);
        StdInitPhi_e = FromDegrees(stdInitPhiDegs[1]);
        StdInitPhi_d = FromDegrees(stdInitPhiDegs[2]);
        ImuErrorModel = imuErrorModel;
    }

    public LooseCombinationOptions(Vector gnssLeverArm, double stdInitR, double stdInitV, Angle stdInitPhi, ImuErrorModel imuErrorModel)
    {
        GnssLeverArm = gnssLeverArm;
        StdInitR_n = stdInitR;
        StdInitR_e = stdInitR;
        StdInitR_d = stdInitR;
        StdInitV_n = stdInitV;
        StdInitV_e = stdInitV;
        StdInitV_d = stdInitV;
        StdInitPhi_n = stdInitPhi;
        StdInitPhi_e = stdInitPhi;
        StdInitPhi_d = stdInitPhi;
        ImuErrorModel = imuErrorModel;
    }
}