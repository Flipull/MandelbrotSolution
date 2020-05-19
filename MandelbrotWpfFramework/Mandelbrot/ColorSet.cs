using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWpfFramework.Mandelbrot
{
    class ColorSet
    {
        public static Color[] GetRainbow()
        {
            var result = new Color[360];
            for (int i = 0; i < 360; i++)
            {
                result[i] = ColorTools.ColorFromHSV(i, 1, 1);
            }
            return result;
        }


    }
}
