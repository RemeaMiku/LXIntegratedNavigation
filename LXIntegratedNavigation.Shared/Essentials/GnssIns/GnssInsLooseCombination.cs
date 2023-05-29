using LXIntegratedNavigation.Shared.Essentials.Ins;
using LXIntegratedNavigation.Shared.Filters;
using LXIntegratedNavigation.Shared.Models.Data;
using LXIntegratedNavigation.Shared.Models.Navi;

namespace LXIntegratedNavigation.Shared.Essentials.GnssIns;

public class GnssInsLooseCombination
{
    private readonly INormalGravityService _gravityService;
    public InertialNavigation InertialNavigation { get; }
    public KalmanFilter KalmanFilter { get; init; }
    public GnssInsLooseCombinationOptions Options { get; init; }
    public GnssInsLooseCombination(INormalGravityService gravityService, KalmanFilter kalmanFilter, GnssInsLooseCombinationOptions options, InertialNavigation? inertialNavigation = null)
    {
        _gravityService = gravityService;
        InertialNavigation = inertialNavigation ?? new(_gravityService);
        KalmanFilter = kalmanFilter;
        Options = options;
    }

    private ImuData InterpolateImuData(ImuData previousData, ImuData nextData, GpsTime currentTime)
    {
        var deltaTime = nextData.TimeStamp - previousData.TimeStamp;
        var coefficient = (currentTime - previousData.TimeStamp) / deltaTime;
        var currentAcc = previousData.Accelerometer + coefficient * (nextData.Accelerometer - previousData.Accelerometer);
        var currentGyro = previousData.Gyroscope + coefficient * (nextData.Gyroscope - previousData.Gyroscope);
        return new(currentTime, currentAcc, currentGyro, true);
    }

    private NaviPose UpDate(NaviPose prePose, ImuData preImu, ImuData curImu, GnssData? curGnss = null, double? intervalSeconds = null)
    {
        var dt = intervalSeconds ?? (curImu.TimeStamp - preImu.TimeStamp).TotalSeconds;
        var curPose = InertialNavigation.Mechanizations(prePose, preImu, curImu, dt);
        var Phi_kSub1Tok = BuildMatrixPhi(prePose, curImu, dt);
    }

    private (Matrix H, Vector Z, Matrix R) BuildMeasurement(NaviPose pose, ImuData imuData, GnssData gnssData)
    {

    }

    private Matrix BuildMatrixH_v(NaviPose pose, ImuData imuData)
    {
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var omega_ib_b = imuData.Gyroscope;
        var omega_ie_n = BuildOmega_ie_n(pose.Latitude);
        var omega_en_n = BuildOmega_en_n(pose.Latitude, pose.Altitude, pose.NorthVelocity, pose.EastVellocity, _gravityService.Ellipsoid);
        var omega_in_n = omega_ie_n + omega_en_n;
        var C_b_n = pose.EulerAngles.ToRotationMatrix<double>();
        var omega_in_nx = Matrix.FromAxialVector(omega_in_n);
        var lx = Matrix<double>.FromAxialVector(Storage.Config.L_Gnss);
        var diag_omega_ib_b = Matrix.FromVectorAsDiagonal(omega_ib_b);
        var C_b_nlx = Matrix<double>.FromAxialVector(C_b_n * Storage.Config.L_Gnss);
        var H_v3 = -omega_in_nx * C_b_nlx - C_b_n * Matrix<double>.FromAxialVector(Storage.Config.L_Gnss.OuterProduct(omega_ib_b));
        var H_v6 = -C_b_n * lx * diag_omega_ib_b;
        var H_v = Matrix.FromBlockMatrixArray(new Matrix[,]
        {
            {O,I,H_v3,-C_b_nlx,O,H_v6,O }
        });
        return H_v;
    }


    private Matrix BuildMatrixPhi(NaviPose pose, ImuData imuData, double dt)
    {
        var F_kSub1 = BuildMatrixF(pose, imuData);
        var I = Matrix.Identity(F_kSub1.RowCount);
        var Phi_kSub1Tok = I + F_kSub1 * dt;
        return Phi_kSub1Tok;
    }

    private Matrix BuildMatrixF(NaviPose pose, ImuData imuData)
    {
        var f = imuData.Accelerometer;
        var omega_ib_b = imuData.Gyroscope;
        var v_n = pose.NorthVelocity;
        var v_e = pose.EastVellocity;
        var v_d = pose.GroundVelocity;
        var h = pose.Altitude;
        var omega_ie_n = BuildOmega_ie_n(pose.Latitude);
        var omega_en_n = BuildOmega_en_n(pose.Latitude, h, v_n, v_e, _gravityService.Ellipsoid);
        var omega_in_n = omega_ie_n + omega_en_n;
        var g = _gravityService.NormalGravityAt(pose.Latitude, pose.Altitude);
        var m = _gravityService.Ellipsoid.M(pose.Latitude);
        var n = _gravityService.Ellipsoid.N(pose.Latitude);
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
        var C_b_n = pose.EulerAngles.ToRotationMatrix<double>();
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
            { O, O, O,-I/ Options.Tgb, O, O, O },
            { O, O, O, O,-I/ Options.Tab, O, O},
            { O, O, O, O, O,-I/ Options.Tgs, O },
            { O, O, O, O, O, O,-I/ Options.Tas }
        });
    }
}
