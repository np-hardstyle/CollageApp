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
    /// 
    using System.Collections.Generic;
    using System.Xml.Linq;

    public partial class ImagePropertiesWindow : Window
    {
        public ImagePropertiesWindow(List<PadImage> images)
        {
            InitializeComponent();

            foreach(PadImage img in images)
            {
                PropertiesListBox.Items.Add(img.ToXElement());
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            XElement root = new XElement("DrawingPad");
            foreach (var item in PropertiesListBox.Items)
            {
                root.Add(item);
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Save the XML document to file
                root.Save(saveFileDialog.FileName);
            }

            Close();
        }
    }
}
