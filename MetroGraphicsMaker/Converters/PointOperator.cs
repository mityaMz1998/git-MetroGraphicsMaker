using System;
using System.Windows;

// need add to refereces WindowsBase.dll assembly

namespace Converters
{
    public static class PointOperator
    {
        public static Point mul(this Point point, Double factor)
        {
            point.X *= factor;
            point.Y *= factor;
            return point;
        }

        public static Point div(this Point point, Double divisor)
        {
            point.X /= divisor;
            point.Y /= divisor;
            return point;
        }
    }
}
