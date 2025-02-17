using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CollageApp
{
    internal class DrawingPad : Canvas
    {
        public static uint gridSize = 3; // default size is 3
        public bool GridEnabled = true;
        private static Brush _line_brush_color = Brushes.Black;
        private ObservableCollection<PadImage> _ImageStack = new ObservableCollection<PadImage>();
        private ObservableCollection<Line> _GridLines = new ObservableCollection<Line>();
        private EditingFrame _editingFrame;
        private bool editing = false;
        private bool isDragging = false;
        private Point selected_object_position;
        private PadImage selectedImage;

        public DrawingPad() : base()
        {
            AllowDrop = true;
            SizeChanged += _DrawingPad_SizeChanged;
            _ImageStack.CollectionChanged += ImageStack_CollectionChanged;
            _GridLines.CollectionChanged += _GridLines_CollectionChanged;
            MouseLeftButtonDown += DrawingPad_MouseLeftButtonDown;
            MouseLeftButtonUp += DrawingPad_MouseLeftButtonUp;
            PreviewMouseMove += DrawingPad_PreviewMouseMove;
            //MouseRightButtonDown += DrawingPad_MouseRightButtonDown;
            KeyDown += DrawingPad_KeyDown;
            Focusable = true;
            GridEnabled = true;

            _editingFrame = new EditingFrame();
            DrawGrid();
        }

        public void AddImage(string filepath)
        {
            _ImageStack.Add(new PadImage(filepath));
        }

        private void _DrawingPad_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _GridLines.Clear();
            DrawGrid();

            foreach (var image in _ImageStack)
            {
                image.Width = ActualWidth / gridSize * image.stretch_factor.Item1;
                image.Height = ActualHeight / gridSize * image.stretch_factor.Item2;
            }
        }

        private void ImageStack_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
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
                foreach (PadImage oldImage in e.OldItems)
                {
                    if (Children.Contains(oldImage))
                    {
                        Children.Remove(oldImage);
                    }
                }
            }
        }

        private void _GridLines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Line newLine in e.NewItems)
                {
                    Children.Insert(0, newLine);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                for (uint i = 0; i < gridSize * 2; i++)
                {
                    Children.Remove(Children[0]);
                }
            }
        }

        // handle image dragging
        private void DrawingPad_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // check if escape key is pressed (esc)
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.editing = false;
                e.Handled = true;
                Children.Remove(_editingFrame);
                this.selectedImage = null;
                this.isDragging = false;

                return;
            }
        }

        private void DrawingPad_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // center editing frame frame
            if (e.OriginalSource is Rectangle)
            {
                isDragging = true;
            }

            // clicking on resizing points

            // no frame, image
            else if (e.OriginalSource is PadImage selectedImage)
            {
                BringToFront(selectedImage);
                _editingFrame.AttachToImage(selectedImage);
                this.selectedImage = selectedImage;
                Children.Add(_editingFrame);
                selected_object_position = e.GetPosition(selectedImage);
                editing = true;
                isDragging = true;
                return;
            }

            // clicking anywhere else
            else
            {
                this.editing = false;
                e.Handled = true;
                Children.Remove(_editingFrame);
                this.selectedImage = null;
                this.isDragging = false;
            }
            e.Handled = true;
        }

        private void DrawingPad_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (editing && isDragging)
            {
                // change editing frame location
                var newPos = e.GetPosition(this) - selected_object_position;
                _editingFrame.MoveTo(newPos);

                // change image location
                SetTop(selectedImage, newPos.Y);
                SetLeft(selectedImage, newPos.X);

            }
            e.Handled = true;
        }

        //private void DrawingPad_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (editing)
        //    {
        //        Children.Remove(_editingFrame);
        //        editing = false;
        //    }
        //    e.Handled = true;
        //}

        //private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (e.OriginalSource is PadImage selectedImage)
        //    {
        //        BringToFront(selectedImage);
        //    }
        //    e.Handled = true;
        //}

        private void BringToFront(UIElement element)
        {
            if (Children.Contains(element))
            {
                Children.Remove(element);
                Children.Add(element);
            }
        }

        private void DrawGrid()
        {
            for (uint i = 0; i < gridSize; i++)
            {
                _GridLines.Add(new Line { X1 = 0, Y1 = i * ActualHeight / gridSize, X2 = ActualWidth, Y2 = i * ActualHeight / gridSize, Stroke = _line_brush_color });
                _GridLines.Add(new Line { X1 = i * ActualWidth / gridSize, Y1 = 0, X2 = i * ActualWidth / gridSize, Y2 = ActualHeight, Stroke = _line_brush_color });
            }
        }

        public void ToggleGrid()
        {
            if (!GridEnabled)
            {
                DrawGrid();
            }
            else
            {
                {
                    _GridLines.Clear();
                }
            }
            GridEnabled ^= true;
        }
    }

    internal class EditingFrame : Canvas
    {
        private Rectangle _outline;
        private Rectangle[] _resizeHandles;

        public EditingFrame()
        {
            _outline = new Rectangle
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                Fill = Brushes.Transparent
            };
            Children.Add(_outline);

            _resizeHandles = new Rectangle[8];
            for (int i = 0; i < 8; i++)
            {
                _resizeHandles[i] = new Rectangle
                {
                    Stroke = Brushes.Blue,
                    Fill = Brushes.Blue,
                    Width = 6,
                    Height = 6
                };
                Children.Add(_resizeHandles[i]);
            }
        }

        public void AttachToImage(PadImage image)
        {
            _outline.Width = image.Width;
            _outline.Height = image.Height;
            SetTop(this, Canvas.GetTop(image));
            SetLeft(this, Canvas.GetLeft(image));
            PositionResizeHandles(image.Width, image.Height);
        }

        public void MoveTo(Vector newPosition)
        {
            SetTop(this, newPosition.Y);
            SetLeft(this, newPosition.X);
        }

        private void PositionResizeHandles(double width, double height)
        {
            double halfSize = _resizeHandles[0].Width / 2;
            double[,] positions =
            {
                { 0, 0 }, { width / 2, 0 }, { width, 0 },
                { 0, height / 2 }, { width, height / 2 },
                { 0, height }, { width / 2, height }, { width, height }
            };

            for (int i = 0; i < 8; i++)
            {
                SetLeft(_resizeHandles[i], positions[i, 0] - halfSize);
                SetTop(_resizeHandles[i], positions[i, 1] - halfSize);
            }
        }
    }
}
