using System.Drawing;

namespace BitmapTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Bitmap bitmap = new Bitmap(@"C:\Users\garry\OneDrive\Desktop\TestBitmap.bmp");
            Graphics graphics = Graphics.FromImage(bitmap);
        }
    }
}