using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SchetsEditor
{
    public static class ImageExtensions
    {
        public static byte[] ImageToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
