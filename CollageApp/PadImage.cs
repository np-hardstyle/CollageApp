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
        private Rectangle _borderRectangle;

        //public
        public double _x;
        public double _y; // top left corner location
        public double _width;
        public double _height; // adjustable width and height
        public double _border_color; // highlight color
        public uint _affinity; // stack location (z-index in canvas graph)

        //constructor
        public PadImage(string filepath, double height = -1.0, double width = -1.0, double x = -0.1, double y = -0.1) : base()
        {
            this.Source = new BitmapImage(new Uri(filepath));
            this.Width = width > 0 ? width : _default_pixel;
            this.Height = height > 0 ? height : _default_pixel;
            this._x = x > 0 ? x : 0;
            this._y = y > 0 ? y : 0;
            this.MouseLeftButtonDown += image_poiner_dropped;
            StretchDirection = StretchDirection.Both;
            Stretch = Stretch.Fill;

        }

        // public members
        public void resize_image(double in_width, double in_height)
        {
            this._width = in_width;
            this._height = in_height;
        }

        public void relocate_image(double in_x, double in_y)
        {
            this._x = in_x;
            this._y = in_y;
        }

        // reference -> https://stackoverflow.com/questions/54423232/how-to-drag-and-drop-images-inside-canvas-in-wpf-using-c-sharp
        // press mouse event == select image on the canvas
        private void image_pointer_pressed(object sender, RoutedEventArgs e)
        {
            if (!_is_selected)
            {
                SelectImage();
            }
        }

        private void SelectImage()
        {
            this._is_selected = true;
        }

        private void image_poiner_dropped(object sender, RoutedEventArgs e)
        {

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
