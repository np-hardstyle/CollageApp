using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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
using Microsoft.Win32;

namespace CollageApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _gridSize = 5; // Default N value
        private ObservableCollection<CanvasCollageImage> _ImageStack = new ObservableCollection<CanvasCollageImage>();

        public MainWindow()
        {
            InitializeComponent();

            // setup image stack for user inputted images (empty by default)
            this._ImageStack.CollectionChanged += _image_stack_CollectionChanged;
            
            // setup grid toggle button
            GridCheckBox.IsChecked = true;
            GridCheckBox.Checked += CheckBox_Checked;
            GridCheckBox.Unchecked += CheckBox_Unchecked;

            // setup file open button
            FileOpenButton.IsEnabled = true;
            FileOpenButton.Click += MenuItem_Click;

            // setup canvas
            CollageCanvas.AllowDrop = true;
            CollageCanvas.SizeChanged += CollageCanvas_SizeChanged;
            DrawGrid();
        }

        private void _image_stack_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Add new items to the canvas
                foreach (CanvasCollageImage newImage in e.NewItems)
                {
                    if (!CollageCanvas.Children.Contains(newImage))
                    {
                        CollageCanvas.Children.Add(newImage);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Remove items from the canvas
                foreach (CanvasCollageImage oldImage in e.OldItems)
                {
                    if (CollageCanvas.Children.Contains(oldImage))
                    {
                        CollageCanvas.Children.Remove(oldImage);
                    }
                }
            }
        }

        private (double, double) Get_Next_Slot()
        {
            int quadrant_number = this._ImageStack.Count - 1;
            double cellWidth = CollageCanvas.ActualWidth / this._gridSize;
            double cellHeight = CollageCanvas.ActualHeight / this._gridSize;
            return (cellWidth * quadrant_number, cellHeight * quadrant_number);

        }

        private void CollageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGrid();
        }

        private void DrawGrid()
        {
            CollageCanvas.Children.Clear();

            double cellWidth = CollageCanvas.ActualWidth / this._gridSize;
            double cellHeight = CollageCanvas.ActualHeight / this._gridSize;

            for (int i = 1; i < _gridSize; i++)
            {
                // Horizontal lines
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = i * cellHeight,
                    X2 = CollageCanvas.ActualWidth,
                    Y2 = i * cellHeight,
                    Stroke = Brushes.Black
                };
                CollageCanvas.Children.Add(horizontalLine);

                // Vertical lines
                Line verticalLine = new Line
                {
                    X1 = i * cellWidth,
                    Y1 = 0,
                    X2 = i * cellWidth,
                    Y2 = CollageCanvas.ActualHeight,
                    Stroke = Brushes.Black
                };
                CollageCanvas.Children.Add(verticalLine);
            }
        }

        // Event for the window resizing to redraw the grid
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGrid();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CollageCanvas.Children.Clear();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog _dialog = new OpenFileDialog
            {
                Title = "Select an image",
                Filter = "Valid Image Files| *.jpg; *.jpeg; *.png;",
                CheckFileExists = true,
            };

            if (_dialog.ShowDialog() == true){

                CanvasCollageImage img = new CanvasCollageImage(_dialog.FileName);
                img.relocate_image(0, 0);
                img.resize_image(CollageCanvas.ActualWidth / this._gridSize, CollageCanvas.ActualHeight / this._gridSize);
                this._ImageStack.Add(img);
            }
        }
    }
}
