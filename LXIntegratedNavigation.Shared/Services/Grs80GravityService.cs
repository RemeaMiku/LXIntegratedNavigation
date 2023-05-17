﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LXIntegratedNavigation.Shared.Interfaces;

namespace LXIntegratedNavigation.Shared.Services;

internal class Grs80GravityService : IGravityService
{
    public double CalculateNormalGravity(Angle latitude, double height)
    {
        var sin = Sin(latitude);
        var sin2 = sin * sin;
        var g0 = 9.7803267715 * (1 + sin2 * (0.0052790414 + 0.0000232718 * sin2));
        return g0 + height * (-3.087691089E-6 + 4.397731E-9 * sin2 + 0.721E-12 * height);
    }
}