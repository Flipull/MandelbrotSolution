using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWpfFramework.Mandelbrot
{
    class Vector2d
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double LengthSqr()
        {
            return X * X + Y * Y;
        }
    }
}
