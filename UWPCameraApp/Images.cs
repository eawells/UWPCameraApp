using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace UWPCameraApp
{
    public class Images
    {
        private List<SoftwareBitmap> images;

        public Images()
        {
            images = new List<SoftwareBitmap>();
        }

        public void addImage(SoftwareBitmap image)
        {
            images.Add(image);
        }

        public List<SoftwareBitmap> getImages()
        {
            return images;
        }
    }
}
