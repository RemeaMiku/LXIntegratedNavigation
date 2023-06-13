using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LXIntegratedNavigation.Shared.Models;
using NaviSharp;
using Syncfusion.Windows.PropertyGrid;

namespace LXIntegratedNavigation.WPF.ViewModels;

//[PropertyGrid(NestedPropertyDisplayMode = NestedPropertyDisplayMode.Show, PropertyName = $"{nameof(TimeSpan)},{nameof(Ecef)},{nameof(EulerAngles)}")]
public class NaviPoseViewModel : ObservableObject
{
    private readonly NaviPose _pose;
    [Category("时间")]
    [DisplayName("从开始时刻的时长")]
    [Display(Description = "从开始时刻的时长")]
    [ReadOnly(true)]
    public TimeSpan TimeSpan { get; init; }
    [Category("时间")]
    [DisplayName("GPS周")]
    [Display(Description = "GPS系统时间的周数分量")]
    public ushort GpsWeek => _pose.TimeStamp.Week;
    [Category("时间")]
    [DisplayName("GPS周内秒")]
    [Display(Description = "GPS系统时间的周内秒分量")]
    public double GpsSecond => _pose.TimeStamp.Sow;
    [Category("时间")]
    [DisplayName("UTC")]
    [Display(Description = "协调世界时")]
    public UtcTime Utc => _pose.TimeStamp.Utc;
    [Category("位置")]
    [DisplayName("纬度")]
    [Display(Description = "大地纬度(°)")]
    public double Lat => _pose.Latitude.Degrees;
    [Category("位置")]
    [DisplayName("经度")]
    [Display(Description = "大地经度(°)")]
    public double Lon => _pose.Longitude.Degrees;
    [Category("位置")]
    [DisplayName("大地高")]
    [Display(Description = "地面点与参考椭球面的距离")]
    public double H => _pose.H;
    [Category("速度")]
    [DisplayName("东向速度")]
    [Display(Description = "导航坐标系下的东向速度分量")]
    public double V_e => _pose.EastVellocity;
    [Category("速度")]
    [DisplayName("北向速度")]
    [Display(Description = "导航坐标系下的北向速度分量")]
    public double V_n => _pose.NorthVelocity;
    [Category("位置")]
    [DisplayName("东向距离")]
    [Display(Description = "导航坐标系下与开始位置间的东向坐标之差")]
    public float R_e { get; init; }
    [Category("位置")]
    [DisplayName("北向距离")]
    [Display(Description = "导航坐标系下与开始位置间的北向坐标之差")]
    public float R_n { get; init; }
    [Category("位置")]
    [DisplayName("垂向距离")]
    [Display(Description = "导航坐标系下与开始位置间的垂向坐标之差")]
    public float R_u { get; init; }
    [Category("速度")]
    [DisplayName("天向速度")]
    [Display(Description = "导航坐标系下的天向速度分量")]
    public double V_u => -_pose.DownVelocity;
    [Category("速度")]
    [DisplayName("地向速度")]
    [Display(Description = "导航坐标系下的地向速度分量")]
    public double V_d => _pose.DownVelocity;
    //[Category("姿态")]
    //[DisplayName("欧拉角")]
    //public EulerAngles EulerAngles => _pose.EulerAngles;
    [Category("姿态")]
    [DisplayName("航向角")]
    [Display(Description = "导航坐标系下的航向角，范围是(-180°,180°]")]
    public double Yaw => _pose.EulerAngles.Yaw.Degrees;
    [Category("姿态")]
    [DisplayName("俯仰角")]
    [Display(Description = "导航坐标系下的俯仰角，范围是[-90°,90°]")]
    public double Pitch => _pose.EulerAngles.Pitch.Degrees;
    [Category("姿态")]
    [DisplayName("横滚角")]
    [Display(Description = "导航坐标系下的横滚，范围是(-90°,90°]")]
    public double Roll => _pose.EulerAngles.Roll.Degrees;

    private CartesianCoord Ecef => _pose.Location.ToCart(EarthEllipsoid.Grs80);
    [Category("位置")]
    [DisplayName("ECEF-X")]
    public double X => Ecef.X;
    [Category("位置")]
    [DisplayName("ECEF-Y")]
    public double Y => Ecef.Y;
    [Category("位置")]
    [DisplayName("ECEF-Z")]
    public double Z => Ecef.Z;
    [Category("速度")]
    [DisplayName("速度大小")]
    public double V => _pose.Velocity.Norm();

    public NaviPoseViewModel(NaviPose pose, NaviPose initPose)
    {
        _pose = pose;
        R_e = (float)(pose.L - initPose.L) * 6371000;
        R_n = (float)(pose.B - initPose.B) * 6371000;
        R_u = (float)(pose.H - initPose.H);
        TimeSpan = pose.TimeStamp - initPose.TimeStamp;
    }

    public static ObservableCollection<NaviPoseViewModel> FromNaviPoses(List<NaviPose>?
        naviPoses)
    {
        if (naviPoses is null)
            return new();
        var res = new ObservableCollection<NaviPoseViewModel>();
        for (int i = 0; i < naviPoses.Count; i++)
        {
            res.Add(new(naviPoses[i], naviPoses[0]));
        }
        return res;
    }

    //    public object? GetPropertyByTitle(string title)
    //    {
    //        return GetPropertyValue(this, ItemToPropertyNamePairs[title].Name);
    //    }

    //    // 定义一个递归的方法
    //    public static object? GetPropertyValue(object? obj, string propertyName)
    //    {
    //#if DEBUG
    //        // 如果对象为空或属性名为空，抛出异常
    //        if (obj == null) throw new ArgumentNullException(nameof(obj));
    //        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
    //#endif
    //        // 如果属性名包含点号
    //        if (propertyName.Contains('.'))
    //        {
    //            // 分割成两部分
    //            var parts = propertyName.Split(new char[] { '.' }, 2);

    //            // 获取第一级属性
    //            var property = obj?.GetType().GetProperty(parts[0]);

    //            // 获取第一级属性的值
    //            var value = property?.GetValue(obj);

    //            // 对值调用递归方法，传入剩余的属性名
    //            return GetPropertyValue(value, parts[1]);
    //        }
    //        else
    //        {
    //            // 获取最后一级属性
    //            var property = obj?.GetType().GetProperty(propertyName);

    //            // 获取最后一级属性的值
    //            var value = property?.GetValue(obj);

    //            // 返回值
    //            return value;
    //        }
    //    }

    public static Dictionary<string, (string Name, string Format)> ItemToPropertyNamePairs { get; } = new()
    {
        {"GPS周",(nameof(GpsWeek),string.Empty) },
        {"GPS秒",(nameof(GpsSecond),"F4") },
        {"UTC",(nameof(Utc), "yyyyMMdd_HH:mm:ss.ffff") },
        {"纬度B(°)",(nameof(Lat), "F8") },
        {"经度L(°)",(nameof(Lon), "F8") },
        {"大地高H(m)",(nameof(H), "F4") },
        {"ECEF-X(m)",  (nameof(X), "F4") },
        {"ECEF-Y(m)",(nameof(Y), "F4") },
        {"ECEF-Z(m)",(nameof(Z), "F4") },
        {"东向距离RE(m)",(nameof(R_e), "F4")},
        {"北向距离RN(m)",  (nameof(R_n), "F4")},
        {"垂向距离RU(m)",(nameof(R_u), "F4")},
        {"北向速度VN(m/s)",(nameof(V_n), "F4") },
        {"东向速度VE(m/s)",(nameof(V_e), "F4") },
        {"垂向速度VD(m/s)",(nameof(V_d), "F4") },
        {"垂向速度VU(m/s)",(nameof(V_u), "F4") },
        {"航向Yaw(°)",(nameof(Yaw), "F4") },
        {"俯仰Pitch(°)",(nameof(Pitch), "F4") },
        {"横滚Roll(°)",(nameof(Roll), "F4") },
    };

}
