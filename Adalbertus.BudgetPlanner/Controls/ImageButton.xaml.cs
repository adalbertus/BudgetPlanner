using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;

namespace Adalbertus.BudgetPlanner.Controls
{
    /// <summary>
    /// Interaction logic for ImageButton.xaml
    /// </summary>
    public partial class ImageButton : Button
    {
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(default(ImageSource), OnImageSourceChanged));
        private static void OnImageSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        public string ImageUrl
        {
            get { return (string)GetValue(ImageUrlProperty); }
            set { SetValue(ImageUrlProperty, value); }
        }

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof(string), typeof(ImageButton), new UIPropertyMetadata(string.Empty, OnImageUrlChanged));

        private static void OnImageUrlChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var imageButton = o as ImageButton;
            if (imageButton != null)
            {
                var uriString = string.Format("pack://application:,,,/{0};component/{1}", Assembly.GetEntryAssembly().GetName().Name, imageButton.ImageUrl);
                var uri = new Uri(uriString);
                imageButton.ImageSource = new BitmapImage(uri);
            }
        }

        public int ImageWidth
        {
            get { return (int)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(int), typeof(ImageButton), new UIPropertyMetadata(16));

        public int ImageHeight
        {
            get { return (int)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(int), typeof(ImageButton), new UIPropertyMetadata(16));

        public ImageButton()
        {
            InitializeComponent();

        }
    }
}
