using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWpfFramework.Mandelbrot
{
    class Zeta
    {
        public readonly Point P;
        public readonly Vector2d Z;
        public bool HasFinished { get; private set; } = false;
        public int IterationsComplete { get; private set; } = 0;

        public Vector2d Z_n { get; set; } = VectorFlyWeight.GetVector();
        public bool isDrawn { get; set; } = true;
        
        public Zeta(Point p, Vector2d z)
        {
            P = p;
            Z = z;
        }
        public void ZSquaredPlusC(int iterations)
        {
            while (Z_n.LengthSqr() < 4 && IterationsComplete < iterations)
            {
                var oldx = Z_n.X;
                Z_n.X = Z_n.X * Z_n.X - Z_n.Y * Z_n.Y + Z.X;
                Z_n.Y = 2 * oldx * Z_n.Y + Z.Y;
                IterationsComplete++;
            }
            if (Z_n.LengthSqr() >= 4)
                HasFinished = true;
        }

        public void Dispose()
        {
            VectorFlyWeight.RecycleVector(Z);
            VectorFlyWeight.RecycleVector(Z_n);
        }
    }
}
