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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
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
        private Rectangle outline, topcenter, topright, right, bottomright, bottomcenter, bottomleft, left, topleft;
        private bool selected =                                 false;
        private bool editing =                                  false;
        private Point selected_object_position;

        public DrawingPad() : base()
        {
            AllowDrop = true;
            SizeChanged += _DrawingPad_SizeChanged;
            _ImageStack.CollectionChanged += ImageStack_CollectionChanged;
            _GridLines.CollectionChanged += _GridLines_CollectionChanged;
            MouseLeftButtonDown += DrawingPad_MouseLeftButtonDown;
            MouseLeftButtonUp += DrawingPad_MouseLeftButtonUp;
            PreviewMouseMove += DrawingPad_PreviewMouseMove;
            MouseRightButtonDown += DrawingPad_MouseRightButtonDown;
            Focusable = true;

            _Highlight = new Rectangle
            {
                Stroke = Brushes.Red,  // Red border
                StrokeThickness = 3,   // Border thickness
                Fill = Brushes.Transparent, // No fill for the border
                Width = 0,
                Height = 0,
            };

            outline = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                Fill = Brushes.Transparent,
                Width = 0,
                Height = 0,
            };

            topcenter = topright = right = bottomright = bottomcenter = bottomleft = left = topleft = new Rectangle
            {
                Stroke = Brushes.Blue,
                Width = 0,
                Height = 0,
            };

            topcenter.Fill =
                topright.Fill =
                right.Fill =
                bottomright.Fill =
                bottomcenter.Fill =
                bottomleft.Fill =
                left.Fill =
                topleft.Fill =
                Brushes.Blue;

            topcenter.StrokeThickness =
                topright.StrokeThickness =
                right.StrokeThickness =
                bottomright.StrokeThickness =
                bottomcenter.StrokeThickness =
                bottomleft.StrokeThickness =
                left.StrokeThickness =
                topleft.StrokeThickness =
                6;

            DrawGrid();

        }

        //private void DrawingPad_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (editing)
        //    {
        //        editing = false;
        //        Children.RemoveAt(Children.Count - 1);
        //    }
        //    e.Handled = true;
        //}

        private void DrawingPad_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var original_source = e.OriginalSource;

            if (editing)
            {
                editing = false;
                for (int i = 1; i <= 2; i++)
                {
                    Children.RemoveAt(Children.Count - 1);
                }
                e.Handled = true;
                return;
            }

            // check if it's an image
            if (original_source is PadImage)
            {
                PadImage selected_image = (PadImage)original_source;
                var removal_index = Children.IndexOf(selected_image);
                var top = GetTop(Children[removal_index]);
                var left = GetLeft(Children[removal_index]);

                // children
                Children.Remove(selected_image);
                SetTop(selected_image, top);
                SetLeft(selected_image, left);
                Children.Add(selected_image);

                var image_width = selected_image.Width;
                var image_height = selected_image.Height;

                // outline
                this.outline.Width = image_width;
                this.outline.Height = image_height;
                SetTop(outline, top);
                SetLeft(outline, left);
                Children.Add(outline);

                // draw editing points

                var editing_point_size = image_width / 20;

                //topleft
                topleft.Width = editing_point_size;
                topleft.Height = editing_point_size;
                SetTop(topleft, top);
                SetLeft(topleft, left);
                Children.Add(topleft);

                //topright
                //topright.Width = editing_point_size;
                //topright.Height = editing_point_size;
                //SetTop(topright, top);
                //SetLeft(topright, left + image_width);
                //Children.Add(topright);

                //topcenter
                //topcenter.Width = editing_point_size;
                //topcenter.Height = editing_point_size;
                //SetTop(topcenter, top);
                //SetLeft(topcenter, left + image_width / 2);
                //Children.Add(topcenter);

                selected_object_position = e.GetPosition(selected_image);

                editing = true;
                e.Handled = true;
                return;
            }            

        }

        private void DrawingPad_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (selected)
            {
                int stack_size = Children.Count;
                int highlight_position = stack_size - 1;
                int image_location = stack_size - 2;

                var newPos = e.GetPosition(this) - selected_object_position;
                PadImage curr_image = (PadImage)Children[image_location];
                curr_image.X = newPos.X;
                curr_image.Y = newPos.Y;

                SetTop(Children[image_location], newPos.Y);
                SetLeft(Children[image_location], newPos.X);

                SetTop(Children[highlight_position], newPos.Y);
                SetLeft(Children[highlight_position], newPos.X);
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
