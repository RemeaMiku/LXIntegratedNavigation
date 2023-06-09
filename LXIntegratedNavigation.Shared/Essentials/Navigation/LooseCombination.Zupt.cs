﻿using LXIntegratedNavigation.Shared.Models;

namespace LXIntegratedNavigation.Shared.Essentials.Navigation;


public partial class LooseCombination
{
    readonly Queue<Vector> _accelerationWindow = new();
    const double _accelerationThreshold = 0.1;
    const double _windowSize = 50;

#pragma warning disable IDE1006 // 命名样式
    static readonly Matrix _H_v = BuildH_v();
#pragma warning restore IDE1006 // 命名样式
#pragma warning disable IDE1006 // 命名样式
    static readonly Matrix _R_v = BuildR_v();
#pragma warning restore IDE1006 // 命名样式

    private bool IsZeroVelocity(Vector accelerometer, Vector normalGravity)
    {
        var acceleration = accelerometer + normalGravity;
        _accelerationWindow.Enqueue(acceleration);
        if (_accelerationWindow.Count <= _windowSize)
            return false;
        _accelerationWindow.Dequeue();
        var averageAcceleration = _accelerationWindow.Average(a => a.Norm());
        return averageAcceleration <= _accelerationThreshold;
    }

    private static Matrix BuildH_v()
    {
        var I = Matrix.Identity(3);
        var O = new Matrix(3, 3);
        var H_v = Matrix.FromBlockMatrixArray(new[,]
        {
            {O,I,O,O,O,O,O }
        });
        return H_v;
    }

    private static Matrix BuildR_v() =>
        Matrix.FromArrayAsDiagonal(0.01, 0.01, 0.01);

    private static Vector BuildZ_v(NaviPose pose)
        => new(pose.NorthVelocity, pose.EastVellocity, pose.DownVelocity);

    private static (Matrix H, Vector Z, Matrix R) BuildHZRFromZupt(NaviPose pose)
    => (_H_v, BuildZ_v(pose), _R_v);
}
