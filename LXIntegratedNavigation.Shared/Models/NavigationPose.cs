using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.Shared.Models;

public record class NavigationPose
{
    public GpsTime TimeStamp { get; set; }
    public EulerAngle EulerAngle { get; set; }
    public GeodeticCoord Location { get; set; }
}
