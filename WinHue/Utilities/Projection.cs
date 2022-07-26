﻿using System;
using System.Numerics;

namespace WinHue.Utilities
{
    public static class Projection
    {
        public static (double Radius, double Azimuth) GetAzimuthalEquidistant(Vector3 v)
        {
            double az = Math.Atan2(v.Y, v.X);
            double r = Math.Acos(Math.Clamp(v.Z / v.Length(), -1, 1));
            return (r, az);
        }

        public static (double Radius, double Azimuth) GetAzimuthalEqualArea(Vector3 v)
        {
            double az = Math.Atan2(v.Y, v.X);
            double r = (v - new Vector3(0, 0, 1)).Length();
            return (r, az);
        }
    }
}
