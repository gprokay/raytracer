using System.Drawing;
using System.Drawing.Imaging;

namespace RayTracer.Lib
{
    public class BitmapWriter
    {
        public Bitmap WriteToBitmap(Bitmap bitmap, Color[] result, int width, int height)
        {
            if (bitmap == null || bitmap.Width != width && bitmap.Height != height)
            {
                bitmap = new Bitmap(width, height);
            }

            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (var i = 0; i < result.Length; ++i)
                {
                    var x = i % width;
                    var y = i / width;
                    var color = result[i];
                    var offset = x * 3 + y * stride;
                    ptr[offset] = color.B;
                    ptr[offset + 1] = color.G;
                    ptr[offset + 2] = color.R;
                }
            }
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
