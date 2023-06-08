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
        stdInitR_n: 0.009,
        stdInitR_e: 0.008,
        stdInitR_d: 0.022,
        stdInitV_n: 0,
        stdInitV_e: 0,
        stdInitV_d: 0,
        stdInitPhi_n: FromDegrees(0),
        stdInitPhi_e: FromDegrees(0),
        stdInitPhi_d: FromDegrees(0),
        gnssLeverArm: new(0.235, 0.1, 0.89),
        imuErrorModel: new(
        arw: 0.2 * RadiansPerDegree / 60,
        vrw: 0.4 / 60,
        stdAccBias: 400E-5,
        stdAccScale: 1000E-6,
        stdGyroBias: 24 * RadiansPerDegree / 3600,
        stdGyroScale: 1000E-6,
        cotAccBias: 3600,
        cotAccScale: 3600,
        cotGyroBias: 3600,
        cotGyroScale: 3600)
    );
var lc = new LooseCombination(new Grs80NormalGravityModel(), options);
const int samplingRate = 100;
var staticEpochNum = 5 * 60 * samplingRate;
var staticImuDatas = imuDatas.Take(staticEpochNum);
var initOriention = lc.InertialNavigation.StaticAlignment(initLocation, staticImuDatas);
var dynamicImuDatas = imuDatas.Skip(staticEpochNum);
var initImuData = dynamicImuDatas.First();
var initPose = new NaviPose(dynamicImuDatas.First().TimeStamp, initLocation, new(3), initOriention);
var lcRes = lc.Solve(initPose, dynamicImuDatas, gnssDatas).ToList();
//WritePoses(GetPathAtDesktop($"InsResult_{DateTime.Now:yyMMddHHmmss}.csv"), lcRes);
WriteLine("Done");
