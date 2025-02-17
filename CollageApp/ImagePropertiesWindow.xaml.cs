using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CollageApp
{
    /// <summary>
    /// Interaction logic for ImagePropertiesWindow.xaml
    /// </summary>
    public partial class ImagePropertiesWindow : Window
    {
        public ImagePropertiesWindow(List<string> imageProperties)
        {
            InitializeComponent();

            foreach (var property in imageProperties)
            {
                PropertiesListBox.Items.Add(property);
            }
        }

    }
}
