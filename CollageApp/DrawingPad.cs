using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System;
using System.Collections.Generic;

namespace CollageApp
{
    internal class DrawingPad : Canvas
    {
        #pragma warning disable IDE0044
        public static uint gridSize = 3; // default size is 3
        public bool GridEnabled = true;
        private static Brush _lineBrushColor = Brushes.Black;
        private ObservableCollection<PadImage> _imageStack = new ObservableCollection<PadImage>();
        private ObservableCollection<Line> _gridLines = new ObservableCollection<Line>();
        private EditingFrame _editingFrame;
        private bool _editing = false;
        private bool _isDragging = false;
        private int _stretchMode = -1;
        private Point _selectedObjectPosition;
        private PadImage _selectedImage;
        #pragma warning restore IDE0044

        public DrawingPad() : base()
        {
            AllowDrop = true;
            SizeChanged += _DrawingPad_SizeChanged;
            _imageStack.CollectionChanged += ImageStack_CollectionChanged;
            _gridLines.CollectionChanged += _GridLines_CollectionChanged;
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
            _imageStack.Add(new PadImage(filepath));
        }

        public void ShowAllImageProperties()
        {
            List<string> imageProperties = new List<string>();

            foreach (var image in _imageStack)
            {
                double left = Canvas.GetLeft(image);
                double top = Canvas.GetTop(image);
                double width = image.Width;
                double height = image.Height;

                string propertyText = $"{image.filepath} at X: {left}, Y: {top} | Width: {width}, Height: {height}";
                imageProperties.Add(propertyText);
            }

            ImagePropertiesWindow propertiesWindow = new ImagePropertiesWindow(imageProperties);
            propertiesWindow.Show();
        }


        private void _DrawingPad_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _gridLines.Clear();
            DrawGrid();

            foreach (var image in _imageStack)
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
            if (_editing && _selectedImage != null)
            {
                _editingFrame.AttachToImage(_selectedImage);
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
                this._editing = false;
                e.Handled = true;
                Children.Remove(_editingFrame);
                this._selectedImage = null;
                this._isDragging = false;

                return;
            }
            else if (e.Key == System.Windows.Input.Key.Delete)
            {
                if (this._editing)
                {
                    _imageStack.Remove(_selectedImage);
                    _selectedImage = null;
                    _editing = _isDragging = false;
                    Children.Remove(_editingFrame);
                }
            }
        }

        private void DrawingPad_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = false;
            if (_editing && _stretchMode == -1)
            {
                // get cell dimentions and floor to get quadrant number then multiply by cell dimensions
                var cursorPos = e.GetPosition(this);
                double gridSizePixelsX = ActualWidth / gridSize;
                double gridSizePixelsY = ActualHeight / gridSize;

                double snappedLeft = Math.Floor(cursorPos.X / gridSizePixelsX) * gridSizePixelsX;
                double snappedTop = Math.Floor(cursorPos.Y / gridSizePixelsY) * gridSizePixelsY;

                SetLeft(_selectedImage, snappedLeft);
                SetTop(_selectedImage, snappedTop);

                _editingFrame.MoveTo(new Vector(snappedLeft, snappedTop));
            }
            _stretchMode = -1;
            e.Handled = true;
        }

        private void DrawingPad_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // image selected, in editing mode
            if (e.OriginalSource is Rectangle)
            {
                // check if it's just the resizing rectangle or the editing points
                int editing_point = _editingFrame.GetResizingHandle(e.GetPosition(this));
                _isDragging = (editing_point == -1);
                _stretchMode = _isDragging ? -1 : editing_point;

                // if it's dragging, don't overwrite the image's location
                _selectedObjectPosition = _isDragging ? e.GetPosition(_selectedImage) : e.GetPosition(this);
            }

            // image is not selected yet
            else if (e.OriginalSource is PadImage selectedImage)
            {

                // check if already editing
                if (_editing)
                {
                    Children.Remove(_editingFrame);
                }

                _editingFrame.AttachToImage(selectedImage);
                BringToFront(selectedImage);

                this._selectedImage = selectedImage;
                _selectedObjectPosition = e.GetPosition(selectedImage);
                _editing = _isDragging = true;
               
                Children.Add(_editingFrame);

                return;
            }

            // exiting edit mode by clicking anywhere but image or editing frame
            else
            {
                _editing = _isDragging = false;
                _selectedImage = null;
                Children.Remove(_editingFrame);
            }
            e.Handled = true;
        }

        private void DrawingPad_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (_editing && _isDragging)
            {
                // change editing frame location
                var newPos = e.GetPosition(this) - _selectedObjectPosition;
                _editingFrame.MoveTo(newPos);

                // change image location
                SetTop(_selectedImage, newPos.Y);
                SetLeft(_selectedImage, newPos.X);

            }
            else if (_stretchMode != -1)
            {
                // stretch both editing frame and the image
                var newPos = e.GetPosition(this);
                var left = GetLeft(_selectedImage);
                var top = GetTop(_selectedImage);
                double gridSizePixelsX = ActualWidth / gridSize;
                double gridSizePixelsY = ActualHeight / gridSize;

                // Calculate snapped positions
                double snappedX = Math.Round(newPos.X / gridSizePixelsX) * gridSizePixelsX;
                double snappedY = Math.Round(newPos.Y / gridSizePixelsY) * gridSizePixelsY;

                // Calculate new width and height
                double newWidth = _selectedImage.Width;
                double newHeight = _selectedImage.Height;

                // Determine which edges are being resized
                bool resizeLeft = (_stretchMode == 0 || _stretchMode == 3 || _stretchMode == 5);
                bool resizeRight = (_stretchMode == 2 || _stretchMode == 4 || _stretchMode == 7);
                bool resizeTop = (_stretchMode == 0 || _stretchMode == 1 || _stretchMode == 2);
                bool resizeBottom = (_stretchMode == 5 || _stretchMode == 6 || _stretchMode == 7);

                if (resizeLeft)
                {
                    newWidth = Math.Round((left + _selectedImage.Width - newPos.X) / gridSizePixelsX) * gridSizePixelsX;
                    SetLeft(_selectedImage, snappedX);
                }
                else if (resizeRight)
                {
                    newWidth = Math.Round((newPos.X - left) / gridSizePixelsX) * gridSizePixelsX;
                }

                if (resizeTop)
                {
                    newHeight = Math.Round((top + _selectedImage.Height - newPos.Y) / gridSizePixelsY) * gridSizePixelsY;
                    SetTop(_selectedImage, snappedY);
                }
                else if (resizeBottom)
                {
                    newHeight = Math.Round((newPos.Y - top) / gridSizePixelsY) * gridSizePixelsY;
                }

                // Apply the snapped width and height
                _selectedImage.Height = newHeight < gridSizePixelsY? gridSizePixelsY : newHeight;
                _selectedImage.Width = newWidth < gridSizePixelsX ? gridSizePixelsX : newWidth;

                // Update the editing frame to match the resized image
                _editingFrame.AttachToImage(_selectedImage);
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
                _gridLines.Add(new Line { X1 = 0, Y1 = i * ActualHeight / gridSize, X2 = ActualWidth, Y2 = i * ActualHeight / gridSize, Stroke = _lineBrushColor });
                _gridLines.Add(new Line { X1 = i * ActualWidth / gridSize, Y1 = 0, X2 = i * ActualWidth / gridSize, Y2 = ActualHeight, Stroke = _lineBrushColor });
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
                    _gridLines.Clear();
                }
            }
            GridEnabled ^= true;
        }
    }

    internal class EditingFrame : Canvas
    {
        private Rectangle _outline;
        private Rectangle[] _resizeHandles;
        private int _sizeResizeHandles = 20;
        private double _top = 0;
        private double _left = 0;

        public EditingFrame()
        {
            _outline = new Rectangle
            {
                Stroke = Brushes.Red,
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
                    Width = _sizeResizeHandles,
                    Height = _sizeResizeHandles
                };
                Children.Add(_resizeHandles[i]);
            }
        }

        public void AttachToImage(PadImage image)
        {
            _outline.Width = image.Width;
            _outline.Height = image.Height;
            this._top = Canvas.GetTop(image);
            this._left = Canvas.GetLeft(image);
            SetTop(this, _top);
            SetLeft(this, _left);
            PositionResizeHandles(image.Width, image.Height);
        }

        public void MoveTo(Vector newPosition)
        {
            SetTop(this, newPosition.Y);
            SetLeft(this, newPosition.X);
            this._top = newPosition.Y;
            this._left = newPosition.X;

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
                double left = this._left + Canvas.GetLeft(_resizeHandles[i]);
                double top = this._top + Canvas.GetTop(_resizeHandles[i]);
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
