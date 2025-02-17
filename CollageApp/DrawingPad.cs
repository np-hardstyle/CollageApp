using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
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
        private int StretchMode = -1;
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
                double relativeX = GetLeft(image) / e.PreviousSize.Width;
                double relativeY = GetTop(image) / e.PreviousSize.Height;
                double relativeWidth = image.Width / e.PreviousSize.Width;
                double relativeHeight = image.Height / e.PreviousSize.Height;

                image.Width = ActualWidth * relativeWidth;
                image.Height = ActualHeight * relativeHeight;
                SetLeft(image, ActualWidth * relativeX);
                SetTop(image, ActualHeight * relativeY);
            }

            // check if image is selected and in editing mode
            if (editing && selectedImage != null)
            {
                _editingFrame.AttachToImage(selectedImage);
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

        // escape editing mode
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
            if (editing && StretchMode == -1)
            {
                // get cell dimentions and floor to get quadrant number then multiply by cell dimensions
                var cursorPos = e.GetPosition(this);
                double gridSizePixelsX = ActualWidth / gridSize;
                double gridSizePixelsY = ActualHeight / gridSize;

                double snappedLeft = Math.Floor(cursorPos.X / gridSizePixelsX) * gridSizePixelsX;
                double snappedTop = Math.Floor(cursorPos.Y / gridSizePixelsY) * gridSizePixelsY;

                SetLeft(selectedImage, snappedLeft);
                SetTop(selectedImage, snappedTop);

                _editingFrame.MoveTo(new Vector(snappedLeft, snappedTop));
            }
            StretchMode = -1;
            e.Handled = true;
        }

        private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // image selected, in editing mode
            if (e.OriginalSource is Rectangle)
            {
                // check if it's just the resizing rectangle or the editing points
                int editing_point = _editingFrame.GetResizingHandle(e.GetPosition(this));
                if (editing_point == -1)
                {
                    isDragging = true;
                }
                else
                {
                    isDragging = false;
                    StretchMode = editing_point;
                    selected_object_position = e.GetPosition(this);
                }
            }

            // image is not selected yet
            else if (e.OriginalSource is PadImage selectedImage)
            {

                // check if already editing
                if (editing)
                {
                    Children.Remove(_editingFrame);
                    _editingFrame.AttachToImage(selectedImage);
                }
                BringToFront(selectedImage);
                _editingFrame.AttachToImage(selectedImage);
                this.selectedImage = selectedImage;
                Children.Add(_editingFrame);
                selected_object_position = e.GetPosition(selectedImage);
                editing = true;
                isDragging = true;
                return;
            }

            // exiting edit mode by clicking anywhere but image or editing frame
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
                Console.WriteLine("here");
                var newPos = e.GetPosition(this) - selected_object_position;
                _editingFrame.MoveTo(newPos);

                // change image location
                SetTop(selectedImage, newPos.Y);
                SetLeft(selectedImage, newPos.X);

            }
            else if (StretchMode != -1)
            {
                // stretch both editing frame and the image
                var newPos = e.GetPosition(this);
                var left = GetLeft(selectedImage);
                var top = GetTop(selectedImage);
                double gridSizePixelsX = ActualWidth / gridSize;
                double gridSizePixelsY = ActualHeight / gridSize;

                // Calculate snapped positions
                double snappedX = Math.Round(newPos.X / gridSizePixelsX) * gridSizePixelsX;
                double snappedY = Math.Round(newPos.Y / gridSizePixelsY) * gridSizePixelsY;

                // Calculate new width and height
                double newWidth = selectedImage.Width;
                double newHeight = selectedImage.Height;

                // Determine which edges are being resized
                bool resizeLeft = (StretchMode == 0 || StretchMode == 3 || StretchMode == 5);
                bool resizeRight = (StretchMode == 2 || StretchMode == 4 || StretchMode == 7);
                bool resizeTop = (StretchMode == 0 || StretchMode == 1 || StretchMode == 2);
                bool resizeBottom = (StretchMode == 5 || StretchMode == 6 || StretchMode == 7);

                if (resizeLeft)
                {
                    newWidth = Math.Round((left + selectedImage.Width - newPos.X) / gridSizePixelsX) * gridSizePixelsX;
                    SetLeft(selectedImage, snappedX);
                }
                else if (resizeRight)
                {
                    newWidth = Math.Round((newPos.X - left) / gridSizePixelsX) * gridSizePixelsX;
                }

                if (resizeTop)
                {
                    newHeight = Math.Round((top + selectedImage.Height - newPos.Y) / gridSizePixelsY) * gridSizePixelsY;
                    SetTop(selectedImage, snappedY);
                }
                else if (resizeBottom)
                {
                    newHeight = Math.Round((newPos.Y - top) / gridSizePixelsY) * gridSizePixelsY;
                }

                // Apply the snapped width and height
                selectedImage.Width = newWidth;
                selectedImage.Height = newHeight;

                // Update the editing frame to match the resized image
                _editingFrame.AttachToImage(selectedImage);
                _editingFrame.AttachToImage(selectedImage);
            }
            e.Handled = true;
        }

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
        private int _size_rresizeHandles = 10;
        private double top = 0;
        private double left = 0;

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
                    Width = _size_rresizeHandles,
                    Height = _size_rresizeHandles
                };
                Children.Add(_resizeHandles[i]);
            }
        }

        public void AttachToImage(PadImage image)
        {
            _outline.Width = image.Width;
            _outline.Height = image.Height;
            this.top = Canvas.GetTop(image);
            this.left = Canvas.GetLeft(image);
            SetTop(this, top);
            SetLeft(this, left);
            PositionResizeHandles(image.Width, image.Height);
        }

        public void MoveTo(Vector newPosition)
        {
            SetTop(this, newPosition.Y);
            SetLeft(this, newPosition.X);
            this.top = newPosition.Y;
            this.left = newPosition.X;

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
        public bool ContainsPoint(Point point)
        {
            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);
            return (point.X >= left && point.X <= left + Width &&
                    point.Y >= top && point.Y <= top + Height);
        }

        public int GetResizingHandle(Point position)
        {
            for (int i = 0; i < _resizeHandles.Length; i++)
            {
                double left = this.left + Canvas.GetLeft(_resizeHandles[i]);
                double top = this.top + Canvas.GetTop(_resizeHandles[i]);
                double right = left + _resizeHandles[i].Width;
                double bottom = top + _resizeHandles[i].Height;

                // Check if the point is within the bounds of the handle
                if (position.X >= left && position.X <= right && position.Y >= top && position.Y <= bottom)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
