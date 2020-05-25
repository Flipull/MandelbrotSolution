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

        public double Zoom { get; set; } = 1;
        
        public Zeta[,] Zetas;
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
            Center = model.Center;
            Zetas = new Zeta[model.PlaneWidth, model.PlaneHeight];
        }

        public Mandelbrot(int width, int height, 
                            double zoom = 1, 
                            Vector2d center = null
            )
        {
            PlaneWidth = width;
            PlaneHeight = height;
            Zoom = zoom;
            Center = (center ?? VectorFlyWeight.GetVector());
            Zetas = new Zeta[width, height];
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
            //calculate viewport so, on zoom 1, shown rectangle is in [(-2,-2)..(2,2)]
            var zoom_ratio = 4 / (double)Math.Min(PlaneWidth, PlaneHeight) / Zoom;
            var o_prime = VectorFlyWeight.GetVector(
                                    Center.X - (PlaneWidth / 2) * zoom_ratio,
                                    Center.Y - (PlaneHeight / 2) * zoom_ratio
                               );
            
            Parallel.For(0, PlaneWidth,
                    new ParallelOptions() { MaxDegreeOfParallelism=3 },
                    i =>
                    {
                        for (var y = 0; y < PlaneHeight; y++)
                        {
                            var result = 
                                new Zeta(
                                    new Point(i, y),
                                    VectorFlyWeight.GetVector(
                                        i * zoom_ratio + o_prime.X, 
                                        y * zoom_ratio + o_prime.Y
                                    ));
                            Zetas[i, y] = result;
                            result.ZSquaredPlusC(iterations);
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
///////////////////////
        public void FloodfillZetaDrawing(System.Windows.Point base_position)
        {
            var history = new List<Point>();
            var processing = new List<Zeta>();
            var st_x = (int) base_position.X;
            var st_y = (int) base_position.Y;
            processing.Add(Zetas[st_x, st_y] );
            var should_be_drawn = !Zetas[st_x, st_y].isDrawn;

            

            while (processing.Count > 0)
            {
                var current_zeta = processing.First();
                var current_p = current_zeta.P;
                processing.Remove(current_zeta);
                
                if (history.Any(h => h == current_p))
                    continue;

                current_zeta.isDrawn = should_be_drawn;
                history.Add(current_p);

                if (current_p.X > 0 &&
                    current_zeta.IterationsComplete == Zetas[current_p.X - 1, current_p.Y].IterationsComplete)
                    processing.Add(Zetas[current_p.X - 1, current_p.Y]);
                if (current_p.X < PlaneWidth - 1 &&
                    current_zeta.IterationsComplete == Zetas[current_p.X + 1, current_p.Y].IterationsComplete)
                    processing.Add(Zetas[current_p.X + 1, current_p.Y]);
                if (current_p.Y > 0 &&
                    current_zeta.IterationsComplete == Zetas[current_p.X, current_p.Y - 1].IterationsComplete)
                    processing.Add(Zetas[current_p.X, current_p.Y - 1]);
                if (current_p.Y < PlaneHeight - 1 &&
                    current_zeta.IterationsComplete == Zetas[current_p.X, current_p.Y + 1].IterationsComplete)
                    processing.Add(Zetas[current_p.X, current_p.Y + 1]);

            }

        }


    }
}
