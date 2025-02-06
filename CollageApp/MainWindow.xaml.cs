using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CollageApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // setup image stack for user inputted images (empty by default)
            
            // setup grid toggle button
            GridCheckBox.IsChecked = true;
            GridCheckBox.Checked += CheckBox_Checked;
            GridCheckBox.Unchecked += CheckBox_Unchecked;

            // setup file open button
            FileOpenButton.IsEnabled = true;
            FileOpenButton.Click += MenuItem_Click;

            
            // setup canvas
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CollageCanvas.ToggleGrid();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CollageCanvas.ToggleGrid();
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
                CollageCanvas.AddImage(_dialog.FileName);
            }
        }
    }
}
