using LXIntegratedNavigation.Shared.Models;
using LXIntegratedNavigation.Shared.Services;

var imuDatas = new List<ImuData>(AscFileService.ReadImuDatas("D:\\RemeaMiku study\\course in progress\\2023大三实习\\友谊广场0511\\ProcessedData\\cover_Rover\\20230511_cover_imu.ASC"));
imuDatas = imuDatas.DistinctBy(data => data.TimeStamp).ToList();
var initLatitude = FromDegrees(30.5278045116);
var initHeight = 22.119;

var inertialNavigationService = new InertialNavigationService(new Grs80GravityService());
WriteLine(inertialNavigationService.StaticAlignment(initLatitude, initHeight, imuDatas).ToString("F4", null));