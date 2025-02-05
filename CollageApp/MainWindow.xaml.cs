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
        private ObservableCollection<Image> _images = new ObservableCollection<Image>();

        public MainWindow()
        {
            InitializeComponent();
            GridCheckBox.IsChecked = true;
            //this._images.CollectionChanged += _images_CollectionChanged;
            DrawGrid();
        }

        //private void _images_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    // snap images
        //    for (int i = 0; i < this._images.Count; i++)
        //    {
        //        // calculate image position with grid definition
        //        this.AddChild()
        //    }
        //}

        private void Get_Next_Slot()
        {
            int quadrant_number = this._images.Count - 1;
            double cellWidth = CollageCanvas.ActualWidth / this._gridSize;
            double cellHeight = CollageCanvas.ActualHeight / this._gridSize;
            double x = cellWidth * quadrant_number, y = cellHeight * quadrant_number;


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

                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(_dialog.FileName)),
                    Width = CollageCanvas.ActualWidth / this._gridSize,
                    Height = CollageCanvas.ActualHeight / this._gridSize
                };

                this._images.Add(img);
            }
        }
    }
}
