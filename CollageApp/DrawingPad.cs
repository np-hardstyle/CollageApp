using System.CodeDom;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        public DrawingPad() : base()
        {
            AllowDrop = true;
            SizeChanged += DrawingPad_SizeChanged;
            _ImageStack.CollectionChanged += ImageStack_CollectionChanged;
            _GridLines.CollectionChanged += _GridLines_CollectionChanged;
            DrawGrid();

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
                    if (!Children.Contains(newLine))
                    {
                        // Add the grid lines first to ensure they are underneath
                        Children.Insert(0, newLine);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Line oldLine in e.OldItems)
                {
                    if (Children.Contains(oldLine))
                    {
                        Children.Remove(oldLine);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (Line newLine in Children.OfType<Line>().ToList())
                    Children.Remove(newLine);
            }
        }

        private void DrawingPad_SizeChanged(object sender, SizeChangedEventArgs e)
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
