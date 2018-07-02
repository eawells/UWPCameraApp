using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPCameraApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Images _images;
        public MainPage()
        {
            _images = new Images();
            this.InitializeComponent();
        }

        private async void TakeAPhoto_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Camera), _images);
           
        }

        private async void addImageToGrid()
        {
            _images.getImages().ForEach(async image =>
            {
                var backupImage = image;
                if (backupImage.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    backupImage.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    backupImage = SoftwareBitmap.Convert(backupImage, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                Image imageChild = new Image();
                var sbs = new SoftwareBitmapSource();
                await sbs.SetBitmapAsync(backupImage);

                imageChild.Source = sbs;
                imageChild.Margin = new Thickness(20);
                imageChild.MaxWidth = 300;
                ImageGrid.Children.Add(imageChild);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _images = (Images) e.Parameter;
            addImageToGrid();
        }
    }
}
