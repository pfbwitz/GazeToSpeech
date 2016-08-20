using System.IO;
using Android.Content.Res;
using Android.Graphics;
using VocalEyes.Common.Interface;
using VocalEyes.Droid.Implementation;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageHelper))]
namespace VocalEyes.Droid.Implementation
{
    public class ImageHelper : IImageHelper
    {
        public byte[] Get(string image)
        {
            var b = BitmapFactory.DecodeResource(
                Forms.Context.Resources,
                Forms.Context.Resources.GetIdentifier(image.Replace(".png", string.Empty)
                    .Replace(".jpg", string.Empty).Replace(".jpeg", string.Empty), "drawable", Forms.Context.PackageName)
                );
            using (var stream = new MemoryStream())
            {
                b.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }
    }
}