using MandelbrotWpfFramework.Mandelbrot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MandelbrotWpfFramework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private Mandelbrot.Mandelbrot m = new Mandelbrot.Mandelbrot(10,10);
        private System.Drawing.Color[] _colorset = ColorSet.GetRainbow();

        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += onSizeChanged;
            MandelbrotCanvas.MouseMove += ShowCoords;
            MandelbrotCanvas.MouseLeftButtonDown += MouseMoveCenter;
            MandelbrotCanvas.MouseRightButtonDown += MouseDragZoomingWindow;
            IterationsTarget.PreviewTextInput += IterationsValidator;
            VectorReal.PreviewTextInput += DoubleValidator;
            VectorImaginary.PreviewTextInput += DoubleValidator;
        }

        private void DoubleValidator(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9eE+-]*");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void IterationsValidator(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ShowCoords(object sender, MouseEventArgs e)
        {
            var p = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var c = m.ImageSpaceToZetaSpace((int)p.X, (int)p.Y);
            Title = $"{p.X} - {p.Y} => {c.X} - {c.Y}";
        }
        private void ZoomingWindowRightDragLeave(object sender, MouseEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp -= ZoomingWindowRightButtonUp;
            MandelbrotCanvas.MouseLeave -= ZoomingWindowRightDragLeave;
            MandelbrotCanvas.MouseMove -= ZoomingWindowRightDragMove;
            //ignore halve-complete action
        }
        private void ZoomingWindowRightDragMove(object sender, MouseEventArgs e)
        {
            var _image = new Bitmap(_image_cached);
            _image.SetPixel((int)drag_center.X, (int)drag_center.Y, System.Drawing.Color.Black);

            var blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
            var d_bound = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var diff_x = Math.Abs(d_bound.X - drag_center.X);
            var diff_y = Math.Abs(d_bound.X - drag_center.X) * m.PlaneHeight / (float)m.PlaneWidth;
            
            using (var graphics = Graphics.FromImage(_image))
            {
                graphics.DrawRectangle(blackPen,
                                    (int)(drag_center.X - diff_x), (int)(drag_center.Y - diff_y),
                                    (int)(diff_x *2), (int)(diff_y *2)
                                );
            }
            
            MandelbrotCanvas.Source = BitmapToImageSource(_image);
            _image.Dispose();
        }

        private Bitmap _image_cached;
        private System.Windows.Point drag_center;
        private void ZoomingWindowRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp -= ZoomingWindowRightButtonUp;
            MandelbrotCanvas.MouseLeave -= ZoomingWindowRightDragLeave;
            MandelbrotCanvas.MouseMove -= ZoomingWindowRightDragMove;
            var d_bound = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var diff_x = Math.Abs(d_bound.X - drag_center.X);
            var zoomratio = 2*diff_x / m.PlaneWidth;
            
            m.ChangeCenter(m.ImageSpaceToZetaSpace((int)drag_center.X, (int)drag_center.Y));
            m.ChangeZoom(1/zoomratio);
            UpdateViewCoords();
            _image_cached.Dispose();
            Redraw_Click(null, null);
        }
        private void MouseDragZoomingWindow(object sender, MouseButtonEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp += ZoomingWindowRightButtonUp;
            MandelbrotCanvas.MouseLeave += ZoomingWindowRightDragLeave;
            MandelbrotCanvas.MouseMove += ZoomingWindowRightDragMove;
            _image_cached = DrawBitmap();
            drag_center = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
        }

        private void UpdateViewCoords()
        {
            VectorReal.Text = m.Center.X.ToString(CultureInfo.InvariantCulture);
            VectorImaginary.Text = m.Center.Y.ToString(CultureInfo.InvariantCulture);
        }
        private void MouseMoveCenter(object sender, MouseButtonEventArgs e)
        {
            var p = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            m.ChangeCenter(m.ImageSpaceToZetaSpace((int)p.X, (int)p.Y));
            UpdateViewCoords();
            Redraw_Click(null, null);
        }
        
        private void onSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            MandelbrotCanvas.Width = e.NewSize.Width - 20;
            MandelbrotCanvas.Height = e.NewSize.Height - 130;
            
            m = new Mandelbrot.Mandelbrot(m, (int)MandelbrotCanvas.Width, (int)MandelbrotCanvas.Height,
                                            m.Zoom, m.Center);
        }

        public Bitmap DrawBitmap()
        {
            var _image = new Bitmap(m.PlaneWidth, m.PlaneHeight);
            
            foreach (var zeta in m.Zetas)
            {
                if (!zeta.isDrawn)
                    continue;
                
                if (zeta.HasFinished)
                {
                    //var chosen_color = _colorset[(_colorset.Length - 1) * zeta.IterationsComplete / m.IterationsDone ];
                    //var chosen_color = _colorset[zeta.IterationsComplete % _colorset.Length];
                    var smoothness = Math.Log(Math.Log(zeta.Z_n.LengthSqr(),2));
                    var chosen_color = 
                        _colorset[(zeta.IterationsComplete + _colorset.Length - (int)smoothness ) % _colorset.Length];
                    _image.SetPixel(zeta.P.X, zeta.P.Y, chosen_color);
                }
                else
                    _image.SetPixel(zeta.P.X, zeta.P.Y, ColorCache.GetColor(0, 0, 0));
            }
            return _image;
        }

        private void Redraw_Click(object sender, RoutedEventArgs e)
        {
            m.ChangeCenter(
                VectorFlyWeight.GetVector(
                    double.Parse(VectorReal.Text, CultureInfo.InvariantCulture),
                    double.Parse(VectorImaginary.Text, CultureInfo.InvariantCulture)
                ));
            m.Iterate(int.Parse(IterationsTarget.Text));
            
            var _image = DrawBitmap();
            MandelbrotCanvas.Source = BitmapToImageSource(_image);
            _image.Dispose();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            m = new Mandelbrot.Mandelbrot(m.PlaneWidth, m.PlaneHeight);
            UpdateViewCoords();
            Redraw_Click(null, null);
        }
    }
}