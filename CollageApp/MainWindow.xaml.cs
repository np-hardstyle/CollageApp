﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
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

            // filetab font
            FileTab.FontWeight = FontWeights.Medium;

            // setup grid toggle button
            GridCheckBox.IsChecked = true;
            GridCheckBox.Checked += CheckBox_Checked;
            GridCheckBox.Unchecked += CheckBox_Unchecked;
            GridCheckBox.FontWeight = FontWeights.Medium;

            // setup file open button
            FileOpenButton.IsEnabled = true;
            FileOpenButton.Click += MenuItem_Click;
            FileOpenButton.FontWeight = FontWeights.Medium;

            // properties
            Properties.Click += Properties_Click;
            Properties.Background = null;
            Properties.BorderBrush = null;
            Properties.FontWeight = FontWeights.Medium;

            // window size change event
            //SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeFontSize(this, e.NewSize.Width * 0.015);
        }

        private void ChangeFontSize(DependencyObject parent, double newFontSize)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is Control control)
                {
                    control.FontSize = newFontSize;
                }
                ChangeFontSize(child, newFontSize);
            }
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            CollageCanvas.ShowAllImageProperties();
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
                //ImageDrawing temp = new ImageDrawing();
                //temp.ImageSource = new BitmapImage(new Uri(_dialog.FileName));
                //temp.Rect = new Rect(0, 0, 100, 100);
                //CollageCanvas.Children.Add(temp);
            }
        }
    }
}
