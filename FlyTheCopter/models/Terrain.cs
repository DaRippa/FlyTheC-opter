using System;

namespace FlyTheCopter.models
{
    public static class Terrain
    {
        public static double GetUpperPathCoord(double x) =>
            100 + Math.Sin(x / 20) * 10 + Math.Cos(x / 52) * 37 * (Math.Sin(Math.Sqrt(x) / 100) + 0.75);

        public static double GetLowerPathCoord(double x) => 120 + Math.Sin(x / 52) * 40 +
                                                            Math.Cos(x / 37) * 31 *
                                                            (Math.Sin(Math.Sqrt(x) / 100) + 0.75);
    }
}