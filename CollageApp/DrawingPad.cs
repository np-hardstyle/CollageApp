using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CollageApp
{
    internal class DrawingPad : Canvas
    {
        public static uint gridSize =                           3; // default size is 3
        public bool GridEnabled =                               true;
        private static Brush _line_brush_color =                Brushes.Black;
        private ObservableCollection<PadImage> _ImageStack =    new ObservableCollection<PadImage>();
        private ObservableCollection<Line> _GridLines =         new ObservableCollection<Line>();
        private Rectangle _Highlight =                          new Rectangle();
        private bool selected =                                 false;
        private Point selected_object_position;

        public DrawingPad() : base()
        {
            AllowDrop =                         true;
            SizeChanged +=                      _DrawingPad_SizeChanged;
            _ImageStack.CollectionChanged +=    ImageStack_CollectionChanged;
            _GridLines.CollectionChanged +=     _GridLines_CollectionChanged;
            MouseLeftButtonDown +=              DrawingPad_MouseLeftButtonDown;
            MouseLeftButtonUp +=                DrawingPad_MouseLeftButtonUp;
            PreviewMouseMove +=                 DrawingPad_PreviewMouseMove;
            Focusable =                         true;
            
            _Highlight = new Rectangle
            {
                Stroke = Brushes.Red,  // Red border
                StrokeThickness = 3,   // Border thickness
                Fill = Brushes.Transparent, // No fill for the border
                Width = 0,
                Height = 0,
            };

            DrawGrid();

        }

        private void DrawingPad_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (selected)
            {
                var newPos = e.GetPosition(this) - selected_object_position;
                PadImage curr_image = (PadImage)Children[Children.Count - 2];
                curr_image.X = newPos.X;
                curr_image.Y = newPos.Y;

                SetTop(Children[Children.Count - 2], newPos.Y);
                SetLeft(Children[Children.Count - 2], newPos.X);
            }
            e.Handled = true;
        }

        private void DrawingPad_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Children.Count > 0){
                if (Children[Children.Count - 1] is Rectangle)
                {
                    Children.RemoveAt(Children.Count - 1);
                    selected = false;
                }
            }
            e.Handled = true;
        }


        // cursor position is same as image dimensions and canvas dimensions
        private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var original_source = e.OriginalSource; //PadImage object

            // toggle image highlight
            if (original_source is PadImage)
            {
                // select the image and move to the top of the stack
                PadImage selected_image = (PadImage)original_source;
                var removal_index = Children.IndexOf(selected_image);
                var top = GetTop(Children[removal_index]);
                var left = GetLeft(Children[removal_index]);

                Console.WriteLine("topleft: "+top+" "+left);

                Children.RemoveAt(removal_index);
                SetTop(selected_image, top);
                SetLeft(selected_image, left);
                Children.Add(selected_image);

                // highlight
                this._Highlight.Width = selected_image.Width;
                this._Highlight.Height = selected_image.Height;
                SetLeft(_Highlight, left);
                SetTop(_Highlight, top);
                Children.Add(_Highlight);

                // update selected image
                selected = true;
                selected_object_position = e.GetPosition(selected_image);
                Console.WriteLine("Selected Object Position: " + selected_object_position.ToString());

            }
            e.Handled = true;
        }

        private void DrawingPad_ImageDragging(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        public void ToggleGrid()
        {
            if (!GridEnabled)
            {
                DrawGrid();
            }
            else
            {
                _GridLines.Clear();
            }
            GridEnabled ^= true;
        }

        public void AddImage(string filepath)
        {
            _ImageStack.Add(new PadImage(filepath));
        }

        private void _GridLines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Line newLine in e.NewItems)
                {
                    
                        // Add the grid lines first to ensure they are underneath
                        Children.Insert(0, newLine);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                for (uint i = 0; i < gridSize * 2; i++)
                {
                    Children.Remove(Children[0]);
                }

                //foreach (Line newLine in Children.OfType<Line>().ToList())
                //    Children.Remove(newLine);
            }
        }

        private void _DrawingPad_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // redraw grid lines since every grid size will change from window resizing.
            _GridLines.Clear();
            DrawGrid();

            // redraw each image
            foreach (var image in  _ImageStack)
            {
                image.Width = ActualWidth / gridSize * image.stretch_factor.Item1;
                image.Height = ActualHeight / gridSize * image.stretch_factor.Item2;
            }

            _Highlight.Width = ActualWidth / gridSize;
            _Highlight.Height = ActualHeight / gridSize;
        }

        private void ImageStack_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Add new items to the canvas
                foreach (PadImage newImage in e.NewItems)
                {
                    if (!Children.Contains(newImage))
                    {
                        newImage.Width = ActualWidth / gridSize * newImage.stretch_factor.Item1;
                        newImage.Height = ActualHeight / gridSize * newImage.stretch_factor.Item2;
                        SetTop(newImage, newImage.Y);
                        SetLeft(newImage, newImage.X);
                        Children.Add(newImage);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Remove items from the canvas
                foreach (PadImage oldImage in e.OldItems)
                {
                    if (Children.Contains(oldImage))
                    {
                        Children.Remove(oldImage);
                    }
                }
            }
        }

        // initialize background grid, assume that at this point, grid isn't drawn
        private void DrawGrid()
        {
            for (uint i = 0; i < gridSize; i++)
            {
                _GridLines.Add(new Line
                {
                    X1 = 0,
                    Y1 = i * ActualHeight / gridSize,
                    X2 = ActualWidth,
                    Y2 = i * ActualHeight / gridSize,
                    Stroke = _line_brush_color
                });
            }

            // horizontal lines
            for (uint i = 0; i < gridSize; i++)
            {
                _GridLines.Add(new Line
                {
                    X1 = i * ActualWidth / gridSize,
                    Y1 = 0,
                    X2 = i * ActualWidth / gridSize,
                    Y2 = ActualHeight,
                    Stroke = _line_brush_color
                });
            }
        }

    }
}
