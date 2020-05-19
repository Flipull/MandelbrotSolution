using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MandelbrotWpfFramework.Mandelbrot
{
    class Mandelbrot
    {
        public int PlaneWidth { get; private set; }
        public int PlaneHeight { get; private set; }

        public double Zoom { get; private set; } = 1;
        
        public List<Zeta> Zetas = new List<Zeta>();
        public Vector2d Center { get; set; } = VectorFlyWeight.GetVector();

        public int IterationsDone { get; private set; }

        public Mandelbrot(int width, int height) 
        {
            PlaneWidth = width;
            PlaneHeight = height;
        }
        public Mandelbrot(Mandelbrot model)
        {
            PlaneWidth = model.PlaneWidth;
            PlaneHeight = model.PlaneHeight;
            Zoom = model.Zoom;
            Zetas.AddRange(model.Zetas);
            Center = model.Center;
        }

        public Mandelbrot(Mandelbrot model, 
                            int width, int height, 
                            double zoom = 1, 
                            Vector2d center = null
            )
        {
            PlaneWidth = width;
            PlaneHeight = height;
            Zoom = zoom;
            Center = (center ?? VectorFlyWeight.GetVector());
            Zetas.AddRange(model.Zetas);
        }

        public Vector2d ImageSpaceToZetaSpace(int x, int y)
        {
            var zoom_ratio = 4 / (double)Math.Min(PlaneWidth, PlaneHeight) / Zoom;
            return VectorFlyWeight.GetVector(
                    Center.X - (PlaneWidth/2-x) * zoom_ratio,
                    Center.Y - (PlaneHeight/2-y) * zoom_ratio
                );
        }

        public void Iterate(int iterations = 10)
        {
            Zetas = new List<Zeta>();
            //calculate viewport so, on zoom 1, shown rectangle is in [(-2,-2)..(2,2)]
            var zoom_ratio = 4 / (double)Math.Min(PlaneWidth, PlaneHeight) / Zoom;
            var o_prime = VectorFlyWeight.GetVector(
                                    Center.X - (PlaneWidth / 2) * zoom_ratio,
                                    Center.Y - (PlaneHeight / 2) * zoom_ratio
                               );
            
            Parallel.For(0, PlaneWidth,
                    i =>
                    {
                        List<Zeta> results = new List<Zeta>();
                        for (var y = 0; y < PlaneHeight; y++)
                        {
                            var result = 
                                new Zeta(
                                    new Point(i, y),
                                    VectorFlyWeight.GetVector(
                                        i * zoom_ratio + o_prime.X, 
                                        y * zoom_ratio + o_prime.Y
                                    ));
                            result.ZSquaredPlusC(iterations);
                            results.Add(result);
                        }
                        lock (Zetas)
                        {
                            Zetas.AddRange(results);
                        }
                    });
            IterationsDone = iterations;
        }

        public void ChangeZoom(double scale)
        {
            Zoom *= scale;
        }
        public void ChangeCenter(Vector2d vec)
        {
            VectorFlyWeight.RecycleVector(Center);
            Center = vec;
        }



    }
}
