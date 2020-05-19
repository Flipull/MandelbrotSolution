using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWpfFramework.Mandelbrot
{
    
    class ColorCache
    {
        private static Dictionary<int, Color> Cache = new Dictionary<int, Color>();

        private static int GetHash(Color color)
        {
            return color.R << 16 + color.G << 8 + color.B;
        }
        private static int GetHash(byte r, byte g, byte b)
        {
            return r << 16 + g << 8 + b;
        }
        public static Color GetColor(byte r, byte g, byte b)
        {
            var hash = GetHash(r, g, b);
            Color newcolor;
            lock (Cache)
            {
                if (Cache.ContainsKey(hash))
                return Cache[hash];

                newcolor = Color.FromArgb(r, g, b);
                Cache.Add(hash, newcolor);
            }
            return newcolor;
        }
    }
}
