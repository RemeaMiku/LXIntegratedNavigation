using System.Runtime.Serialization;
using LXIntegratedNavigation.Shared.Essentials;
using LXIntegratedNavigation.Shared.Models;

var imuDatasPath = "D:\\RemeaMiku study\\course in progress\\2023大三实习\\友谊广场0511\\ProcessedData\\wide_Rover\\20230511_wide_imu.ASC";
var gnssDatasPath = "D:\\onedrive\\文档\\Tencent Files\\1597638582\\FileRecv\\wide.pos";
var imuDatas = ReadImuDatas(imuDatasPath, TimeSpan.FromSeconds(0.01));
var gnssDatas = ReadGnssDatas(gnssDatasPath);
imuDatas = imuDatas.DistinctBy(data => data.TimeStamp);
var initLocation = new GeodeticCoord(FromDegrees(30.5278108404), FromDegrees(114.3557126448), 22.312);
var options = new GnssInsLooseCombinationOptions
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
var lc = new GnssInsLooseCombination(new Grs80NormalGravityService(), options);
const int samplingRate = 100;
var staticEpochNum = 5 * 60 * samplingRate;
var staticImuDatas = imuDatas.Take(staticEpochNum);
var initOriention = lc.InertialNavigation.StaticAlignment(initLocation, staticImuDatas);
var dynamicImuDatas = imuDatas.Skip(staticEpochNum);
var initImuData = dynamicImuDatas.First();
//gnssDatas = gnssDatas.Where(data => data.TimeStamp >= initImuData.TimeStamp);
var initPose = new NaviPose(dynamicImuDatas.First().TimeStamp, initLocation, new(3), initOriention);
var lcRes = lc.Solve(initPose, dynamicImuDatas, gnssDatas);

//var ins = new InertialNavigation(grs80GravityModel, Grs80);

//var staticImuDatas = imuDatas.Where(data => data.TimeStamp < new GpsTime(2261, 360077.750));
//var initEularAngle = ins.StaticAlignment(initCoord, staticImuDatas);
//var dynamicImuDatas = imuDatas.Skip(staticImuDatas.Count());
//var initPose = new NavigationPose(dynamicImuDatas.First().TimeStamp, initCoord, new(3), initEularAngle);
//var insResult = ins.Solve(initPose, dynamicImuDatas, 0.01);
//foreach (var pose in lcRes)
//    WriteLine(pose);
WritePoses(GetPathAtDesktop($"{Path.GetFileNameWithoutExtension(imuDatasPath)}InsResult_{DateTime.Now:yyMMddHHmmss}.csv"), lcRes);
WriteLine("Done");


