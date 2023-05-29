using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXIntegratedNavigation.Shared.Models.Navi;

/// <summary>
/// 空间定向的同一描述
/// </summary>
public record class Orientation
{
    public EulerAngles EulerAngles { get; }
    public Matrix Matrix { get; }
    public Quaternion Quaternion { get; }

    public Orientation(EulerAngles eulerAngles)
    {
        EulerAngles = eulerAngles;
        Matrix = eulerAngles.ToRotationMatrix<double>();
        Quaternion = eulerAngles.ToQuaternion<double>();
    }

    public Orientation(Matrix rotationMatrix)
    {
        if (!rotationMatrix.IsSizeOf(3, 3))
            throw new ArgumentException("", nameof(rotationMatrix));
        Matrix = rotationMatrix;
        EulerAngles = rotationMatrix.ToEulerAngles();
        Quaternion = EulerAngles.ToQuaternion<double>();
    }

    public Orientation(Quaternion rotationQuaternion)
    {
        Quaternion = rotationQuaternion;
        Matrix = rotationQuaternion.ToRotationMatrix();
        EulerAngles = Matrix.ToEulerAngles();
    }
}
