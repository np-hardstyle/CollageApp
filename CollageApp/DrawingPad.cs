using System.CodeDom;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CollageApp
{
    internal class DrawingPad : Canvas
    {
        public static uint _gridSize = 3; // default size is 3
        public bool GridEnabled = true;
        private static Brush _line_brush_color = Brushes.Black;
        private ObservableCollection<PadImage> _ImageStack = new ObservableCollection<PadImage>();
        private ObservableCollection<Line> _GridLines = new ObservableCollection<Line>();
        private Rectangle _Highlight = new Rectangle();

        public DrawingPad() : base()
        {
            AllowDrop = true;
            SizeChanged += _DrawingPad_SizeChanged;
            _ImageStack.CollectionChanged += ImageStack_CollectionChanged;
            _GridLines.CollectionChanged += _GridLines_CollectionChanged;
            MouseLeftButtonDown += DrawingPad_MouseLeftButtonDown;
            MouseLeftButtonUp += DrawingPad_MouseLeftButtonUp;
            Focusable = true;
            
            // obj for rectangle
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

        private void DrawingPad_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Children[Children.Count - 1] == _Highlight)
            {
                Children.Remove(_Highlight);
            }
        }

        private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var original_source = e.OriginalSource; //PadImage object
            if (original_source is PadImage)
            {
                if (Children[Children.Count - 1] == _Highlight)
                {
                    Children.RemoveAt(Children.Count - 1);
                }
                PadImage selected_image = (PadImage)original_source;
                this._Highlight.Width = selected_image.Width;
                this._Highlight.Height = selected_image.Height;
                Children.Add(_Highlight);
            }
        }

        public void ToggleGrid()
        {
            if (!GridEnabled)
            {
                DrawGrid();
                GridEnabled = true;
            }
            else
            {
                _GridLines.Clear();
                GridEnabled = false;
            }
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
                for (uint i = 0; i < _gridSize * 2; i++)
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
                        newImage.Width = ActualWidth / _gridSize;
                        newImage.Height = ActualHeight / _gridSize;
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
            // vertical lins
            for (uint i = 0; i < _gridSize; i++)
            {
                _GridLines.Add(new Line
                {
                    X1 = 0,
                    Y1 = i * ActualHeight / _gridSize,
                    X2 = ActualWidth,
                    Y2 = i * ActualHeight / _gridSize,
                    Stroke = _line_brush_color
                });
            }

            // horizontal lines
            for (uint i = 0; i < _gridSize; i++)
            {
                _GridLines.Add(new Line
                {
                    X1 = i * ActualWidth / _gridSize,
                    Y1 = 0,
                    X2 = i * ActualWidth / _gridSize,
                    Y2 = ActualHeight,
                    Stroke = _line_brush_color
                });
            }
        }

    }
}
