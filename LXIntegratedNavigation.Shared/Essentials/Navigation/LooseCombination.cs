using System.Collections;
using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;
using LXIntegratedNavigation.Shared.Filters;
using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Essentials.Navigation;

public partial class LooseCombination
{
    public INormalGravityModel GravityModel => InertialNavigation.GravityModel;
    public InertialNavigation InertialNavigation { get; init; }
    public KalmanFilter Filter { get; init; }
    public LooseCombinationOptions Options { get; init; }

    readonly Matrix _q;

    public LooseCombination(INormalGravityModel gravityService, LooseCombinationOptions options)
    {
        InertialNavigation = new(gravityService);
        Options = options;
        Filter = new(new(21), BuildInitP(), new(21, 21));
        _q = Buildq();
    }

    public LooseCombination(InertialNavigation inertialNavigation, LooseCombinationOptions options)
    {
        InertialNavigation = inertialNavigation;
        Options = options;
        Filter = new(new(21), BuildInitP(), new(21, 21));
        _q = Buildq();
    }

    private Matrix BuildInitP()
    => Matrix.FromArrayAsDiagonal
            (
                Pow(Options.StdInitR_n, 2), Pow(Options.StdInitR_e, 2), Pow(Options.StdInitR_d, 2),
                Pow(Options.StdInitV_n, 2), Pow(Options.StdInitV_e, 2), Pow(Options.StdInitV_d, 2),
                Pow(Options.StdInitPhi_n.Radians, 2), Pow(Options.StdInitPhi_e.Radians, 2), Pow(Options.StdInitPhi_d.Radians, 2),
                Pow(Options.StdGyroBias, 2), Pow(Options.StdGyroBias, 2), Pow(Options.StdGyroBias, 2),
                Pow(Options.StdAccBias, 2), Pow(Options.StdAccBias, 2), Pow(Options.StdAccBias, 2),
                Pow(Options.StdGyroScale, 2), Pow(Options.StdGyroScale, 2), Pow(Options.StdGyroScale, 2),
                Pow(Options.StdAccScale, 2), Pow(Options.StdAccScale, 2), Pow(Options.StdAccScale, 2)
            );

    private (NaviPose Pose, ImuData Imu) Update(NaviPose prePose, ImuData preImu, ImuData curImu, GnssData? curGnss = null)
    {
        var curPose = InertialNavigation.Mechanizations(prePose, preImu, curImu, Abs((curImu.TimeStamp - preImu.TimeStamp).TotalSeconds));
        var gravity = GravityModel.NormalGravityAsVectorAt(curPose.Latitude, curPose.H);
        var isZeroVelocity = IsZeroVelocity(curImu.Accelerometer, gravity);
        var Phi_kSub1Tok = BuildPhi(curPose, curImu);
        var I = Matrix.Identity(Phi_kSub1Tok.RowCount);
        if (curGnss is null)
        {
            if (isZeroVelocity)
            {
                var HZR = BuildHZRFromZupt(curPose);
                var (X_k, _) = Filter.Solve(I, Phi_kSub1Tok, HZR);
                curPose = CorrectPose(X_k, curPose);
                curImu = CorrectImuData(X_k, curImu);
                Filter.X[0..9] = new(9);
                Filter.Q = BuildQ(curPose, curImu, Phi_kSub1Tok);
                return (curPose, curImu);
            }
            Filter.Solve(I, Phi_kSub1Tok);
            Filter.Q = BuildQ(curPose, curImu, Phi_kSub1Tok);
            return (curPose, curImu);
        }
        else
        {
            var HZR = BuildHZRFromGnss(curPose, curImu, curGnss);
            var (X_k, _) = Filter.Solve(I, Phi_kSub1Tok, HZR);
            curPose = CorrectPose(X_k, curPose);
            curImu = CorrectImuData(X_k, curImu);
            Filter.X[0..9] = new(9);
            Filter.Q = BuildQ(curPose, curImu, Phi_kSub1Tok);
            return (curPose, curImu);
        }
    }



    public IEnumerable<NaviPose> Solve(NaviPose initPose, IEnumerable<ImuData> imuDatas, IEnumerable<GnssData> gnssDatas, GpsTime? initTime = null)
    {
        yield return initPose;
        if (!imuDatas.Any())
            yield break;
        if (initTime is not null)
            imuDatas = imuDatas.Where(d => d.TimeStamp >= initTime);
        var imuList = imuDatas.ToList();
        var gnssList = gnssDatas.ToList();
        var gnssIndex = gnssList.FindIndex(d => d.TimeStamp >= imuList[0].TimeStamp);
        var prePose = initPose;
        var preImu = imuList[0];
        if (gnssIndex == -1)
        {
            for (var imuIndex = 1; imuIndex < imuList.Count;)
            {
                var curImu = imuList[imuIndex];
                (var curPose, curImu) = Update(prePose, preImu, curImu);
                yield return curPose;
                imuIndex++;
                preImu = curImu;
                prePose = curPose;
            }
        }
        for (var imuIndex = 1; imuIndex < imuList.Count;)
        {
            if (gnssIndex < gnssList.Count && Abs((gnssList[gnssIndex].TimeStamp - imuList[imuIndex].TimeStamp).TotalSeconds) <= 0.01)
            {
                var curGnss = gnssList[gnssIndex];
                var curImu = imuList[imuIndex];
                (var curPose, curImu) = Update(prePose, preImu, curImu, curGnss);
                yield return curPose;
                imuIndex++;
                gnssIndex++;
                preImu = curImu;
                prePose = curPose;
                continue;
            }
            else if (gnssIndex < gnssList.Count && gnssList[gnssIndex].TimeStamp < imuList[imuIndex].TimeStamp)
            {
                var curGnss = gnssList[gnssIndex];
                (var curImu, imuList[imuIndex]) = SplitImuData(preImu.TimeStamp, curGnss.TimeStamp, curGnss.TimeStamp, imuList[imuIndex]);
                (var curPose, curImu) = Update(prePose, preImu, curImu, curGnss);
                yield return curPose;
                gnssIndex++;
                preImu = curImu;
                prePose = curPose;
                continue;
            }
            else
            {
                var curImu = imuList[imuIndex];
                (var curPose, curImu) = Update(prePose, preImu, curImu);
                yield return curPose;
                imuIndex++;
                preImu = curImu;
                prePose = curPose;
            }
        }
    }




    private Matrix BuildF(NaviPose pose, ImuData imuData)
    {
        var f = imuData.Accelerometer;
        var omega_ib_b = imuData.Gyroscope;
        var omega_ie_n = BuildOmega_ie_n(pose.Latitude);
        var omega_en_n = BuildOmega_en_n(pose.Latitude, pose.H, pose.NorthVelocity, pose.EastVellocity, Grs80);
        var omega_in_n = omega_ie_n + omega_en_n;
        var g = GravityModel.NormalGravityAt(pose.Latitude, pose.H);
        var v_n = pose.NorthVelocity;
        var v_e = pose.EastVellocity;
        var v_d = pose.DownVelocity;
        var h = pose.H;
        var m = Grs80.M(pose.Latitude);
        var n = Grs80.N(pose.Latitude);
        var mAddh = m + h;
        var nAddh = n + h;
        var tan = Tan(pose.Latitude);
        var cos = Cos(pose.Latitude);
        var sec = Sec(pose.Latitude);
        var v_e_2 = v_e * v_e;
        var mAddh2 = mAddh * mAddh;
        var nAddh2 = nAddh * nAddh;
        var sec2 = sec * sec;
        var sin = Sin(pose.Latitude);
        var v_n_2 = v_n * v_n;
        var C_b_n = pose.Orientation.Matrix;
        var F_rr = new Matrix(new[,]
        {
            { -v_d/mAddh,0,v_n/mAddh },
            { v_e*tan/nAddh,-(v_d+v_n*tan)/nAddh,v_e/nAddh },
            { 0,0,0 }
        });
        var F_vr = new Matrix(new[,]
        {
            { -2*v_e*EarthRotationSpeed*cos/mAddh-v_e_2*sec2/(mAddh*nAddh),0,v_n*v_d/mAddh2-v_e_2*tan/nAddh2 },
            { 2*EarthRotationSpeed*(v_n*cos-v_d*sin)/mAddh+v_n*v_e*sec2/(mAddh*nAddh),0,(v_e*v_d+v_n*v_e*tan)/nAddh2 },
            { 2*EarthRotationSpeed*v_e*sin/mAddh,0,-v_e_2/nAddh2-v_n_2/mAddh2+2*g/(Sqrt(m*n)+h) }
        });
        var F_vv = new Matrix(new[,]
        {
            { v_d/mAddh,-2*(EarthRotationSpeed*sin+v_e*tan/nAddh),v_n/mAddh },
            { 2*EarthRotationSpeed*sin+v_e*tan/nAddh,(v_d+v_n*tan)/nAddh,2*EarthRotationSpeed*cos+v_e/nAddh },
            { -2*v_n/mAddh,-2*(EarthRotationSpeed*cos+v_e/nAddh),0 }
        });
        var F_phir = new Matrix(new[,]
        {
            { -EarthRotationSpeed*sin/mAddh,0,v_e/nAddh2 },
            { 0,0,-v_n/mAddh2},
            { -EarthRotationSpeed*cos/mAddh-v_e*sec2/(mAddh*nAddh),0,-v_e*tan/nAddh2 }
        });
        var F_phiv = new Matrix(new[,]
        {
            { 0,1/nAddh,0 },
            { -1/mAddh,0,0 },
            { 0,-tan/nAddh,0 },
        });
        var O = new Matrix(3, 3);
        var I = Matrix.Identity(3);
        var C_b_nfx = Matrix.FromAxialVector(C_b_n * f);
        var diag_f = Matrix.FromVectorAsDiagonal(f);
        var diag_omega_ib_b = Matrix.FromVectorAsDiagonal(omega_ib_b);
        var omega_in_nx = Matrix.FromAxialVector(omega_in_n);
        return Matrix.FromBlockMatrixArray(new[,]
        {
            { F_rr, I, O, O, O, O, O},
            { F_vr, F_vv, C_b_nfx, O, C_b_n, O, C_b_n* diag_f },
            { F_phir, F_phiv,-omega_in_nx,-C_b_n, O,-C_b_n* diag_omega_ib_b, O },
            { O, O, O,-I/ Options.RelevantTimeGyroBias, O, O, O },
            { O, O, O, O,-I/ Options.RelevantTimeAccBias, O, O},
            { O, O, O, O, O,-I/ Options.RelevantTimeGyroScale, O },
            { O, O, O, O, O, O,-I/ Options.RelevantTimeAccScale }
        });
    }

    private static Matrix BuildG(NaviPose pose)
    {
        var C_b_n = pose.Orientation.Matrix;
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        return Matrix.FromBlockMatrixArray(new[,]
        {
            {O,O,O,O,O,O},
            {C_b_n,O,O,O,O,O },
            {O,C_b_n,O,O,O,O },
            {O,O,I,O,O,O},
            {O,O,O,I,O,O, },
            {O,O,O,O,I,O },
            {O,O,O,O,O,I }
        });
    }

    private Matrix BuildH_r(NaviPose pose)
    {
        var C_b_nlx = Matrix.FromAxialVector(pose.Orientation.Matrix * Options.GnssLeverArm);
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var H_r = Matrix.FromBlockMatrixArray(new[,]
        {
            { I,O,C_b_nlx,O,O,O,O }
        });
        return H_r;
    }

    private Matrix BuildH_v(NaviPose pose, ImuData imuData)
    {
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var omega_ib_b = imuData.Gyroscope;
        var omega_ie_n = BuildOmega_ie_n(pose.Latitude);
        var omega_en_n = BuildOmega_en_n(pose.Latitude, pose.H, pose.NorthVelocity, pose.EastVellocity, Grs80);
        var omega_in_n = omega_ie_n + omega_en_n;
        var C_b_n = pose.Orientation.Matrix;
        var omega_in_nx = Matrix.FromAxialVector(omega_in_n);
        var lx = Matrix.FromAxialVector(Options.GnssLeverArm);
        var diag_omega_ib_b = Matrix.FromVectorAsDiagonal(omega_ib_b);
        var C_b_nlx = Matrix.FromAxialVector(C_b_n * Options.GnssLeverArm);
        var H_v3 = -omega_in_nx * C_b_nlx - C_b_n * Matrix.FromAxialVector(Options.GnssLeverArm.OuterProduct(omega_ib_b));
        var H_v6 = -C_b_n * lx * diag_omega_ib_b;
        var H_v = Matrix.FromBlockMatrixArray(new[,]
        {
            {O,I,H_v3,-C_b_nlx,O,H_v6,O }
        });
        return H_v;
    }

    private Matrix BuildPhi(NaviPose pose, ImuData imuData)
    {
        var F_kSub1 = BuildF(pose, imuData);
        var I = Matrix.Identity(F_kSub1.RowCount);
        var Phi_kSub1Tok = I + F_kSub1 * imuData.IntervalSeconds;
        return Phi_kSub1Tok;
    }

    private Matrix Buildq()
    {
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var sigma2_gb = Options.StdGyroBias * Options.StdGyroBias;
        var sigma2_ab = Options.StdAccBias * Options.StdAccBias;
        var sigma2_gs = Options.StdGyroScale * Options.StdGyroScale;
        var sigam2_as = Options.StdAccScale * Options.StdAccScale;
        return Matrix.FromBlockMatrixArray(new[,]
        {
            { Options.Vrw* Options.Vrw* I, O, O, O, O, O},
            { O, Options.Arw* Options.Arw* I, O, O, O, O },
            { O, O,2* sigma2_gb* I/ Options.RelevantTimeGyroBias, O, O, O },
            { O, O, O,2* sigma2_ab* I/ Options.RelevantTimeAccBias, O, O },
            { O, O, O, O,2* sigma2_gs* I/ Options.RelevantTimeGyroScale, O },
            { O, O, O, O, O,2* sigam2_as* I/ Options.RelevantTimeAccScale }
        });
    }

    private Matrix BuildQ(NaviPose pose, ImuData imuData, Matrix Phi)
    {
        var G_k = BuildG(pose);
        var q_k = _q;
        var G_kq_kGt_k = G_k * q_k * G_k.Transpose();
        var Q_k = 0.5 * imuData.IntervalSeconds * (Phi * G_kq_kGt_k * Phi.Transpose() + G_kq_kGt_k);
        return Q_k;
    }

    private static Matrix BuildR_r(GnssData gnssData)

        => Matrix.FromArrayAsDiagonal(Pow(gnssData.StdR_n, 2), Pow(gnssData.StdR_e, 2), Pow(gnssData.StdR_d, 2));


    private static Matrix BuildR_v(GnssData gnssData)
        => Matrix.FromArrayAsDiagonal(Pow(gnssData.StdV_n, 2), Pow(gnssData.StdV_e, 2), Pow(gnssData.StdV_d, 2));

    private (Matrix H, Vector Z, Matrix R) BuildHZRFromGnss(NaviPose pose, ImuData imuData, GnssData gnssData)
    {
        var H_v = BuildH_v(pose, imuData);
        var Z_v = BuildZ_v(pose, gnssData);
        var R_v = BuildR_v(gnssData);
        var H_r = BuildH_r(pose);
        var Z_r = BuildZ_r(pose, gnssData);
        var R_r = BuildR_r(gnssData);
        var H = H_r.Combine(H_v, MatrixCombinationMode.Vertical);
        var Z = Z_r.Combine(Z_v);
        var R = R_r.Combine(R_v, MatrixCombinationMode.Diagonal);
        return (H, Z, R);
    }



    private Vector BuildZ_r(NaviPose pose, GnssData gnssData)
    {
        var r_m = Grs80.M(pose.Latitude);
        var r_n = Grs80.N(pose.Latitude);
        var D_R = Matrix.FromArrayAsDiagonal(r_m + pose.H, (r_n + pose.H) * Cos(pose.Latitude), -1);
        var z_r = D_R * pose.Location.ToVector() + pose.Orientation.Matrix * Options.GnssLeverArm - D_R * gnssData.Location.ToVector();
        return z_r;
    }

    private static Vector BuildZ_v(NaviPose pose, GnssData gnssData) => pose.Velocity - gnssData.Velocity;

    private static ImuData CorrectImuData(Vector X, ImuData imuData)
    {
        var b_g = X[9..12];
        var b_a = X[12..15];
        var s_g = X[15..18];
        var s_a = X[18..];
        var S_g = Matrix.FromVectorAsDiagonal(s_g);
        var S_a = Matrix.FromVectorAsDiagonal(s_a);
        var newAcc = imuData.Accelerometer - b_a - S_a * imuData.Accelerometer;
        var newGyro = imuData.Gyroscope - b_g - S_g * imuData.Gyroscope;
        return imuData with { Accelerometer = newAcc, Gyroscope = newGyro, IsVirtual = true };
    }

    private static NaviPose CorrectPose(Vector X, NaviPose pose)
    {
        var dr = X[..3];
        var r_M = Grs80.M(pose.Latitude);
        var r_N = Grs80.N(pose.Latitude);
        var D_R_inv = Matrix.FromArrayAsDiagonal(1 / (r_M + pose.H), 1 / ((r_N + pose.H) * Cos(pose.Latitude)), -1);
        var dc = D_R_inv * dr;
        var newLocation = GeodeticCoord.FromVector(pose.Location.ToVector() - dc);
        var dv = X[3..6];
        var newVelocity = pose.Velocity - dv;
        var dphi = X[6..9];
        var C_p_n = Matrix.Identity(3) - Matrix.FromAxialVector(dphi);
        var newMatrix = C_p_n.Inverse() * pose.Orientation.Matrix;
        return pose with { Location = newLocation, Velocity = newVelocity, Orientation = new(newMatrix) };
    }
}
