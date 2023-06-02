using System.Diagnostics;
using LXIntegratedNavigation.Shared.Filters;
using KalmanFilter = LXIntegratedNavigation.Shared.Filters.KalmanFilter;

namespace LXIntegratedNavigation.Shared.Essentials;

public class GnssInsLooseCombination
{
    internal INormalGravityService GravityService => InertialNavigation._gravityService;
    public InertialNavigation InertialNavigation { get; init; }
    public KalmanFilter Filter { get; init; }
    public GnssInsLooseCombinationOptions Options { get; init; }
    Matrix _q;
    public GnssInsLooseCombination(INormalGravityService gravityService, GnssInsLooseCombinationOptions options)
    {
        InertialNavigation = new(gravityService);
        Options = options;
        Filter = new(new(21), BuildInitP(), new(21, 21));
        _q = Buildq();
    }

    public GnssInsLooseCombination(InertialNavigation inertialNavigation, GnssInsLooseCombinationOptions options)
    {
        InertialNavigation = inertialNavigation;
        Options = options;
        Filter = new(new(21), BuildInitP(), new(21, 21));
        _q = Buildq();
    }
    //TODO:修改为旋转矢量标准差
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

    public static ImuData InterpolateImuData(GpsTime preTime, GpsTime interTime, ImuData imuData)
    {
        imuData.IsVirtual = true;
        imuData.IntervalSeconds = (imuData.TimeStamp - interTime).TotalSeconds;
        return imuData with { TimeStamp = interTime, IntervalSeconds = (interTime - preTime).TotalSeconds };
    }

    private NaviPose Update(NaviPose prePose, ImuData preImu, ImuData curImu, GnssData? curGnss = null)
    {
        var curPose = InertialNavigation.Mechanizations(prePose, preImu, curImu);
        var Phi_kSub1Tok = BuildPhi(curPose, curImu);
        (Matrix, Vector, Matrix)? m = null;
        if (curGnss is null)
        {
        }
        else
        {
            m = BuildMeasurement(curPose, curImu, curGnss);
        }
        var I = Matrix.Identity(Phi_kSub1Tok.RowCount);
        var (X_k, _) = Filter.Solve(I, Phi_kSub1Tok, m);
        var Q_k = BuildQ(curPose, curImu, Phi_kSub1Tok);
        Filter.X = new(21);
        Filter.Q = Q_k;
        CorrectInsState(X_k, curPose);
        CorrectImuData(X_k, curImu);
        return curPose;
    }

    public IEnumerable<NaviPose> Solve(NaviPose initPose, IEnumerable<ImuData> imuDatas, IEnumerable<GnssData> gnssDatas)
    {
        yield return initPose;
        if (!imuDatas.Any())
            yield break;
        var imuList = imuDatas.ToList();
        var gnssList = gnssDatas.ToList();
        var gnssIndex = gnssList.FindIndex(d => d.TimeStamp >= imuList[0].TimeStamp);
        var prePose = initPose;
        var preImu = imuList[0];
        for (int imuIndex = 1; imuIndex < imuList.Count;)
        {
            if (gnssIndex < gnssList.Count && gnssList[gnssIndex].TimeStamp <= imuList[imuIndex].TimeStamp)
            {
                var curGnss = gnssList[gnssIndex];
                (var curImu, imuList[imuIndex]) = SplitImuData(preImu.TimeStamp, curGnss.TimeStamp, curGnss.TimeStamp, imuList[imuIndex]);
                var curPose = Update(prePose, preImu, curImu, curGnss);
                yield return curPose;
                gnssIndex++;
                preImu = curImu;
                prePose = curPose;
            }
            else
            {
                var curImu = imuList[imuIndex];
                var curPose = Update(prePose, preImu, curImu);
                yield return curPose;
                imuIndex++;
                preImu = curImu;
                prePose = curPose;
            }
        }
    }

    private Matrix BuildF(NaviPose insState, ImuData imuObs)
    {
        var f = imuObs.Accelerometer;
        var omega_ib_b = imuObs.Gyroscope;
        var omega_ie_n = BuildOmega_ie_n(insState.Latitude);
        var omega_en_n = BuildOmega_en_n(insState.Latitude, insState.H, insState.NorthVelocity, insState.EastVellocity, Grs80);
        var omega_in_n = omega_ie_n + omega_en_n;
        var g = new Grs80NormalGravityService().NormalGravityAt(insState.Latitude, insState.H);
        var v_n = insState.NorthVelocity;
        var v_e = insState.EastVellocity;
        var v_d = insState.DownVelocity;
        var h = insState.H;
        var m = Grs80.M(insState.Latitude);
        var n = Grs80.N(insState.Latitude);
        var mAddh = m + h;
        var nAddh = n + h;
        var tan = Tan(insState.Latitude);
        var cos = Cos(insState.Latitude);
        var sec = Sec(insState.Latitude);
        var v_e_2 = v_e * v_e;
        var mAddh2 = mAddh * mAddh;
        var nAddh2 = nAddh * nAddh;
        var sec2 = sec * sec;
        var sin = Sin(insState.Latitude);
        var v_n_2 = v_n * v_n;
        var C_b_n = insState.Orientation.Matrix;
        var F_rr = new Matrix(new double[,]
        {
            { -v_d/mAddh,0,v_n/mAddh },
            { v_e*tan/nAddh,-(v_d+v_n*tan)/nAddh,v_e/nAddh },
            { 0,0,0 }
        });
        var F_vr = new Matrix(new double[,]
        {
            { -2*v_e*EarthRotationSpeed*cos/mAddh-v_e_2*sec2/(mAddh*nAddh),0,v_n*v_d/mAddh2-v_e_2*tan/nAddh2 },
            { 2*EarthRotationSpeed*(v_n*cos-v_d*sin)/mAddh+v_n*v_e*sec2/(mAddh*nAddh),0,(v_e*v_d+v_n*v_e*tan)/nAddh2 },
            { 2*EarthRotationSpeed*v_e*sin/mAddh,0,-v_e_2/nAddh2-v_n_2/mAddh2+2*g/(Sqrt(m*n)+h) }
        });
        var F_vv = new Matrix(new double[,]
        {
            { v_d/mAddh,-2*(EarthRotationSpeed*sin+v_e*tan/nAddh),v_n/mAddh },
            { 2*EarthRotationSpeed*sin+v_e*tan/nAddh,(v_d+v_n*tan)/nAddh,2*EarthRotationSpeed*cos+v_e/nAddh },
            { -2*v_n/mAddh,-2*(EarthRotationSpeed*cos+v_e/nAddh),0 }
        });
        var F_phir = new Matrix(new double[,]
        {
            { -EarthRotationSpeed*sin/mAddh,0,v_e/nAddh2 },
            { 0,0,-v_n/mAddh2},
            { -EarthRotationSpeed*cos/mAddh-v_e*sec2/(mAddh*nAddh),0,-v_e*tan/nAddh2 }
        });
        var F_phiv = new Matrix(new double[,]
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
        return Matrix<double>.FromBlockMatrixArray(new Matrix<double>[,]
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

    private static Matrix BuildG(NaviPose insState)
    {
        var C_b_n = insState.Orientation.Matrix;
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        return Matrix.FromBlockMatrixArray(new Matrix[,]
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

    private Matrix BuildH_r(NaviPose insState)
    {
        var C_b_nlx = Matrix<double>.FromAxialVector(insState.Orientation.Matrix * Options.GnssLeverArm);
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var H_r = Matrix.FromBlockMatrixArray(new Matrix[,]
        {
            { I,O,C_b_nlx,O,O,O,O }
        });
        return H_r;
    }

    private Matrix BuildH_v(NaviPose insState, ImuData imuObs)
    {
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var omega_ib_b = imuObs.Gyroscope;
        var omega_ie_n = BuildOmega_ie_n(insState.Latitude);
        var omega_en_n = BuildOmega_en_n(insState.Latitude, insState.H, insState.NorthVelocity, insState.EastVellocity, Grs80);
        var omega_in_n = omega_ie_n + omega_en_n;
        var C_b_n = insState.Orientation.Matrix;
        var omega_in_nx = Matrix.FromAxialVector(omega_in_n);
        var lx = Matrix<double>.FromAxialVector(Options.GnssLeverArm);
        var diag_omega_ib_b = Matrix.FromVectorAsDiagonal(omega_ib_b);
        var C_b_nlx = Matrix<double>.FromAxialVector(C_b_n * Options.GnssLeverArm);
        var H_v3 = -omega_in_nx * C_b_nlx - C_b_n * Matrix<double>.FromAxialVector(Options.GnssLeverArm.OuterProduct(omega_ib_b));
        var H_v6 = -C_b_n * lx * diag_omega_ib_b;
        var H_v = Matrix.FromBlockMatrixArray(new Matrix[,]
        {
            {O,I,H_v3,-C_b_nlx,O,H_v6,O }
        });
        return H_v;
    }

    private Matrix BuildPhi(NaviPose insState, ImuData imuObs)
    {
        var F_kSub1 = BuildF(insState, imuObs);
        var I = Matrix.Identity(F_kSub1.RowCount);
        var Phi_kSub1Tok = I + F_kSub1 * imuObs.IntervalSeconds;
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

    private Matrix BuildQ(NaviPose insState, ImuData imuObs, Matrix Phi)
    {
        var G_k = BuildG(insState);
        var q_k = Buildq();
        var G_kq_kGt_k = G_k * q_k * G_k.Transpose();
        var Q_k = 0.5 * imuObs.IntervalSeconds * (Phi * G_kq_kGt_k * Phi.Transpose() + G_kq_kGt_k);
        return Q_k;
    }

    private static Matrix BuildR_r(GnssData gnssState)
    {
        return Matrix.FromArrayAsDiagonal(Pow(gnssState.StdR_n, 2), Pow(gnssState.StdR_e, 2), Pow(gnssState.StdR_d, 2));
    }

    private static Matrix BuildR_v(GnssData gnssState) => Matrix.FromArrayAsDiagonal(Pow(gnssState.StdV_n, 2), Pow(gnssState.StdV_e, 2), Pow(gnssState.StdV_d, 2));

    private (Matrix H, Vector Z, Matrix R) BuildMeasurement(NaviPose insState, ImuData imuObs, GnssData gnssState)
    {
        var H_v = BuildH_v(insState, imuObs);
        var Z_v = BuildZ_v(insState, gnssState);
        var R_v = BuildR_v(gnssState);
        var H_r = BuildH_r(insState);
        var Z_r = BuildZ_r(insState, gnssState);
        var R_r = BuildR_r(gnssState);
        var H = H_r.Combine(H_v, MatrixCombinationMode.Vertical);
        var Z = Z_r.Combine(Z_v);
        var R = R_r.Combine(R_v, MatrixCombinationMode.Diagonal);
        return (H, Z, R);
    }

    private Vector BuildZ_r(NaviPose insState, GnssData gnssState)
    {
        var r_m = Grs80.M(insState.Latitude);
        var r_n = Grs80.N(insState.Latitude);
        var D_R = Matrix.FromArrayAsDiagonal(r_m + insState.H, (r_n + insState.H) * Cos(insState.Latitude), -1);
        var z_r = D_R * insState.Location.ToVector() + insState.Orientation.Matrix * Options.GnssLeverArm - D_R * gnssState.Location.ToVector();
        return z_r;
    }

    private static Vector BuildZ_v(NaviPose insState, GnssData gnssState) => insState.Velocity - gnssState.Velocity;

    private static void CorrectImuData(Vector stateVector, ImuData imuObs)
    {
        var b_g = stateVector.SubVector(9, 3);
        var b_a = stateVector.SubVector(12, 3);
        var s_g = stateVector.SubVector(15, 3);
        var s_a = stateVector.SubVector(18, 3);
        var S_g = Matrix.FromVectorAsDiagonal(s_g);
        var S_a = Matrix.FromVectorAsDiagonal(s_a);
        var correctedAcclrm = imuObs.Accelerometer - b_a - S_a * imuObs.Accelerometer;
        var correctedGyro = imuObs.Gyroscope - b_g - S_g * imuObs.Gyroscope;
        imuObs.Accelerometer = correctedAcclrm;
        imuObs.Gyroscope = correctedGyro;
    }

    private static void CorrectInsState(Vector stateVector, NaviPose insState)
    {
        var dr = stateVector.SubVector(3);
        var r_M = Grs80.M(insState.Latitude);
        var r_N = Grs80.N(insState.Latitude);
        var D_R_inv = Matrix.FromArrayAsDiagonal(1 / (r_M + insState.H), 1 / ((r_N + insState.H) * Cos(insState.Latitude)), -1);
        var dc = D_R_inv * dr;
        var correctedCoord = GeodeticCoord.FromVector(insState.Location.ToVector() - dc);
        var dv = stateVector.SubVector(3, 3);
        var correctedVelocity = insState.Velocity - dv;
        var dphi = stateVector.SubVector(6, 3);
        var C_p_n = Matrix.Identity(3) - Matrix.FromAxialVector(dphi);
        var correctedAttitudeMatrix = C_p_n.Inverse() * insState.Orientation.Matrix;
        var correctedAttitudeQuaternion = correctedAttitudeMatrix.ToEulerAngles().ToQuaternion<double>();
        insState.Location = correctedCoord;
        insState.Velocity = correctedVelocity;
        insState.Orientation = new(correctedAttitudeQuaternion);
    }
}
