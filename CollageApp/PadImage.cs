using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

//TOOO : https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/painting-with-images-drawings-and-visuals?view=netframeworkdesktop-4.8


namespace CollageApp
{
    internal class PadImage : Image
    {
        //private
        static private double _default_pixel = 100;
        private bool _is_selected = true;
        private bool _is_dragging = false;
        private Point initialPointerPosition;
        private Point initialImagePosition;

        //public
        public double X;
        public double Y; // top left corner location
        public double _width;
        public double _height; // adjustable width and height
        public double _border_color; // highlight color
        public uint _affinity; // stack location (z-index in canvas graph)
        public (double, double) stretch_factor;
        public uint quadrant;
        public Rectangle _borderRectangle;

        //constructor
        public PadImage(string filepath, double height = -1.0, double width = -1.0, double x = -0.1, double y = -0.1, (double, double) stretch_factor = default) : base()
        {
            this.Source = new BitmapImage(new Uri(filepath));
            this.Width = width > 0 ? width : _default_pixel;
            this.Height = height > 0 ? height : _default_pixel;
            this.X = x > 0 ? x : 0;
            this.Y = y > 0 ? y : 0;
            this.stretch_factor.Item1 = stretch_factor.Item1 > 0 ? stretch_factor.Item1 : 1;
            this.stretch_factor.Item2 = stretch_factor.Item2 > 0 ? stretch_factor.Item2 : 1;
            this.quadrant = 1;

            this.MouseLeftButtonDown += image_poiner_dropped;
            StretchDirection = StretchDirection.Both;
            Stretch = Stretch.Fill;
            _borderRectangle = new Rectangle
            {
                Stroke = Brushes.Red,  // Red border
                StrokeThickness = 3,   // Border thickness
                Fill = Brushes.Transparent, // No fill for the border
                Width = 0,
                Height = 0,
            };
        }

        public void relocate_image(double inX, double inY)
        {
            this.X = inX;
            this.Y = inY;
        }

        // reference -> https://stackoverflow.com/questions/54423232/how-to-drag-and-drop-images-inside-canvas-in-wpf-using-c-sharp
        // press mouse event == select image on the canvas
        private void image_pointer_pressed(object sender, RoutedEventArgs e)
        {
            this._is_selected = true;
            this._borderRectangle.Width = this._width;
            this._borderRectangle.Height = this._height;
        }

        private void image_poiner_dropped(object sender, RoutedEventArgs e)
        {
            this._is_selected &= this._is_selected;
            this._borderRectangle.Width = 0;
            this._borderRectangle.Height = 0;
        }

        // affinity swap between two images in canvas
        public static void swap_affinity(PadImage prey, PadImage hunter)
        {
            hunter._affinity = hunter._affinity ^ prey._affinity;
            prey._affinity = prey._affinity ^ hunter._affinity;
            hunter._affinity = hunter._affinity ^ prey._affinity;
        }
    }
}
