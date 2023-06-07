using LXIntegratedNavigation.Shared.Essentials.Navigation;
using LXIntegratedNavigation.Shared.Essentials.NormalGravityModel;
using LXIntegratedNavigation.Shared.Models;


var imuDatasPath = "D:\\RemeaMiku study\\course in progress\\2023大三实习\\友谊广场0511\\ProcessedData\\wide_Rover\\20230511_wide_imu.ASC";
var gnssDatasPath = "D:\\onedrive\\文档\\Tencent Files\\1597638582\\FileRecv\\wide.pos";
var imuDatas = ReadImuDatas(imuDatasPath, TimeSpan.FromSeconds(0.01));
var gnssDatas = ReadGnssDatas(gnssDatasPath);
imuDatas = imuDatas.DistinctBy(data => data.TimeStamp);
var initLocation = new GeodeticCoord(FromDegrees(30.5278108404), FromDegrees(114.3557126448), 22.312);
var options = new LooseCombinationOptions
    (
        StdInitR_n: 0.009,
        StdInitR_e: 0.008,
        StdInitR_d: 0.022,
        StdInitV_n: 0,
        StdInitV_e: 0,
        StdInitV_d: 0,
        StdInitPhi_n: FromDegrees(0),
        StdInitPhi_e: FromDegrees(0),
        StdInitPhi_d: FromDegrees(0),
        GnssLeverArm: new(0.235, 0.1, 0.89),
        Arw: 0.2 * RadiansPerDegree / 60,
        Vrw: 0.4 / 60,
        StdAccBias: 400E-5,
        StdAccScale: 1000E-6,
        StdGyroBias: 24 * RadiansPerDegree / 3600,
        StdGyroScale: 1000E-6,
        RelevantTimeAccBias: 3600,
        RelevantTimeAccScale: 3600,
        RelevantTimeGyroBias: 3600,
        RelevantTimeGyroScale: 3600
    );
var lc = new LooseCombination(new Grs80NormalGravityModel(), options);
const int samplingRate = 100;
var staticEpochNum = 5 * 60 * samplingRate;
var staticImuDatas = imuDatas.Take(staticEpochNum);
var initOriention = lc.InertialNavigation.StaticAlignment(initLocation, staticImuDatas);
var dynamicImuDatas = imuDatas.Skip(staticEpochNum);
var initImuData = dynamicImuDatas.First();
var initPose = new NaviPose(dynamicImuDatas.First().TimeStamp, initLocation, new(3), initOriention);
var lcRes = lc.Solve(initPose, dynamicImuDatas, gnssDatas);
WritePoses(GetPathAtDesktop($"InsResult_{DateTime.Now:yyMMddHHmmss}.csv"), lcRes);
WriteLine("Done");
