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
    enum Tools { TransformView, RegionSelector, Brush};
    public partial class MainWindow : Window
    {
        private void OnSelectTransformViewTool(object sender, RoutedEventArgs e)
        {
            if (MandelbrotCanvas != null)
                SetListeners(Tools.TransformView);
        }
        private void OnSelectRegionTool(object sender, RoutedEventArgs e)
        {
            if (MandelbrotCanvas != null)
                SetListeners(Tools.RegionSelector);
        }
        private void SetListeners(Tools tool) 
        {
            UnsetAlllisteners();
            switch (tool)
            {
                case Tools.TransformView:
                    SetTransformViewListeners();
                    break;
                case Tools.RegionSelector:
                    SetRegionSelectorListeners();
                    break;
                case Tools.Brush:
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        private void UnsetAlllisteners()
        {
            MandelbrotCanvas.MouseLeftButtonDown -= MouseTransformViewMoveCenter;
            MandelbrotCanvas.MouseRightButtonDown -= MouseTransformViewWindow;
            MandelbrotCanvas.MouseLeftButtonDown -= MouseSelectRegionToFlip;
            MandelbrotCanvas.MouseRightButtonDown -= MouseSelectIterationLimit;
        }

        private void SetTransformViewListeners()
        {
            MandelbrotCanvas.MouseLeftButtonDown += MouseTransformViewMoveCenter;
            MandelbrotCanvas.MouseRightButtonDown += MouseTransformViewWindow;
        }
        private void SetRegionSelectorListeners()
        {
            MandelbrotCanvas.MouseLeftButtonDown += MouseSelectRegionToFlip;
            MandelbrotCanvas.MouseRightButtonDown += MouseSelectIterationLimit;
        }

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

            IterationsTarget.PreviewTextInput += IterationsValidator;
            Zoom.PreviewTextInput += DoubleValidator;
            VectorReal.PreviewTextInput += DoubleValidator;
            VectorImaginary.PreviewTextInput += DoubleValidator;
            SetTransformViewListeners();
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

        //SelectImageRegions
        private void MouseSelectIterationLimit(object sender, MouseButtonEventArgs e)
        {
            var p = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var selected_zeta = m.Zetas[(int)p.X, (int)p.Y];
            var selected_iter = selected_zeta.IterationsComplete;
            foreach (var z in m.Zetas)
            {
                z.isDrawn = (z.IterationsComplete >= selected_iter);
            }

            var _image = DrawBitmap();
            MandelbrotCanvas.Source = BitmapToImageSource(_image);
            _image.Dispose();
        }
        private void MouseSelectRegionToFlip(object sender, MouseButtonEventArgs e)
        {
            var p = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            m.FloodfillZetaDrawing(p);
            var _image = DrawBitmap();
            MandelbrotCanvas.Source = BitmapToImageSource(_image);
            _image.Dispose();
        }
        //--SelectImageRegions

        //TransformView
        private void TransformViewWindowCancel(object sender, MouseEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp -= TransformViewWindowComplete;
            MandelbrotCanvas.MouseLeave -= TransformViewWindowCancel;
            MandelbrotCanvas.MouseMove -= TransformViewWindowDrag;
            //ignore halve-complete action
        }
        private void TransformViewWindowDrag(object sender, MouseEventArgs e)
        {
            var _image = new Bitmap(_image_cached);
            _image.SetPixel((int)drag_center.X, (int)drag_center.Y, System.Drawing.Color.Black);

            var blackPen = new System.Drawing.Pen(System.Drawing.Color.DimGray, 1);
            var redPen = new System.Drawing.Pen(System.Drawing.Color.AliceBlue, 1);
            var d_bound = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var diff_x = Math.Abs(d_bound.X - drag_center.X);
            var diff_y = Math.Abs(d_bound.X - drag_center.X) * m.PlaneHeight / (float)m.PlaneWidth;
            
            using (var graphics = Graphics.FromImage(_image))
            {
                if (d_bound.X > drag_center.X)
                    graphics.DrawRectangle(blackPen,
                                        (int)(drag_center.X - diff_x), (int)(drag_center.Y - diff_y),
                                        (int)(diff_x *2), (int)(diff_y *2)
                                    );
                else
                    graphics.DrawRectangle(redPen,
                                        (int)(drag_center.X - diff_x), (int)(drag_center.Y - diff_y),
                                        (int)(diff_x * 2), (int)(diff_y * 2)
                                    );
            }

            MandelbrotCanvas.Source = BitmapToImageSource(_image);
            _image.Dispose();
        }

        private Bitmap _image_cached;
        private System.Windows.Point drag_center;
        private void TransformViewWindowComplete(object sender, MouseButtonEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp -= TransformViewWindowComplete;
            MandelbrotCanvas.MouseLeave -= TransformViewWindowCancel;
            MandelbrotCanvas.MouseMove -= TransformViewWindowDrag;
            var d_bound = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            var diff_x = Math.Abs(d_bound.X - drag_center.X);
            var zoomratio = 2*diff_x / m.PlaneWidth;
            
            m.ChangeCenter(m.ImageSpaceToZetaSpace((int)drag_center.X, (int)drag_center.Y));
            if (d_bound.X > drag_center.X)
                m.ChangeZoom(1 / zoomratio);
            else 
                m.ChangeZoom(zoomratio);

            UpdateViewCoords();
            _image_cached.Dispose();
            Redraw_Click(null, null);
        }
        private void MouseTransformViewWindow(object sender, MouseButtonEventArgs e)
        {
            MandelbrotCanvas.MouseRightButtonUp += TransformViewWindowComplete;
            MandelbrotCanvas.MouseLeave += TransformViewWindowCancel;
            MandelbrotCanvas.MouseMove += TransformViewWindowDrag;
            _image_cached = DrawBitmap();
            drag_center = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
        }

        private void MouseTransformViewMoveCenter(object sender, MouseButtonEventArgs e)
        {
            var p = this.TranslatePoint(e.GetPosition(this), MandelbrotCanvas);
            m.ChangeCenter(m.ImageSpaceToZetaSpace((int)p.X, (int)p.Y));
            UpdateViewCoords();
            Redraw_Click(null, null);
        }
        //--TransformView




        private void UpdateViewCoords()
        {
            VectorReal.Text = m.Center.X.ToString(CultureInfo.InvariantCulture);
            VectorImaginary.Text = m.Center.Y.ToString(CultureInfo.InvariantCulture);
            Zoom.Text = m.Zoom.ToString(CultureInfo.InvariantCulture);
        }
        private void onSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            MandelbrotCanvas.Width = e.NewSize.Width - 20;
            MandelbrotCanvas.Height = e.NewSize.Height - 130;
            
            m = new Mandelbrot.Mandelbrot((int)MandelbrotCanvas.Width, (int)MandelbrotCanvas.Height,
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
            m.Zoom = double.Parse(Zoom.Text, CultureInfo.InvariantCulture);
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