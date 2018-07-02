using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWPCameraApp
{
    public sealed partial class Camera : Page
    {
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool isPreviewing;
        private MediaCapture mediaCapture;
        private Images _images;

        public Camera()
        {
            InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                ShowMessageToUser("The app was denied access to the camera");
                return;
            }

            try
            {
                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (FileLoadException)
            {
                //  mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender,
            MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            switch (args.Status)
            {
                case MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable:
                    ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
                    break;
                case MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable when !isPreviewing:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () => { await StartPreviewAsync(); });
                    break;
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing) await mediaCapture.StopPreviewAsync();

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    displayRequest?.RequestRelease();

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType != typeof(MainPage)) return;
            var deferral = e.SuspendingOperation.GetDeferral();
            await CleanupCameraAsync();
            deferral.Complete();
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            ClosePage();
        }

        private async void ClosePage()
        {
            Frame.GoBack();
            await CleanupCameraAsync();
        }

        private async void BeginPreview()
        {
            await StartPreviewAsync();
            SnapShot.IsEnabled = true;
        }

        private async void SnapShot_Click(object sender, RoutedEventArgs e)
        {
            var lowLagCapture =
                await mediaCapture.PrepareLowLagPhotoCaptureAsync(
                    ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));

            var capturedPhoto = await lowLagCapture.CaptureAsync();
            var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;
            _images.addImage(softwareBitmap);
            await lowLagCapture.FinishAsync();
            ClosePage();
        }

        private void ShowMessageToUser(string message)
        {
            Debug.WriteLine(message);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _images = (Images)e.Parameter;
            BeginPreview();
        }
    }
}