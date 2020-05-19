using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWpfFramework.Mandelbrot
{
    class VectorFlyWeight
    {
        private static List<Vector2d> Recycled = new List<Vector2d>();

        public static Vector2d GetVector(double x, double y)
        {
            var v = GetVector();
            v.X = x;
            v.Y = y;
            return v;
        }

        public static Vector2d GetVector()
        {
            if (Recycled.Count > 0)
            {
                var v = Recycled.First();
                Recycled.Remove(v);
                return v;
            } else
            {
                return new Vector2d();
            }
            
        }

        public static void RecycleVector(Vector2d vec)
        {
            vec.X = vec.Y = 0;
            Recycled.Add(vec);
        }

    }
}
