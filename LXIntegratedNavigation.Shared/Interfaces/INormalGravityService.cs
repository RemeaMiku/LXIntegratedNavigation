using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.Shared.Interfaces;

public interface INormalGravityService
{
    public double NormalGravityAt(Angle latitude, double altitude);
}
