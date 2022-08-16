// See https://aka.ms/new-console-template for more information

using IronOcr;
using System.Diagnostics;
using System.Drawing;

Console.WriteLine("Hello, World!");

//string fileName = string.Format(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
//          @"\Screenshot" + "_" +
//          DateTime.Now.ToString("(dd_MMMM_hh_mm_ss_tt)") + ".png");

while (true)
{
    Bitmap bitmap;
    bitmap = new Bitmap(5120, 1440);
    Size size = new Size(bitmap.Width, bitmap.Height);

    Graphics graphics = Graphics.FromImage(bitmap);

    graphics.CopyFromScreen(0, 0, 0, 0, size);

    bitmap.Save(@"C:\Users\garry\OneDrive\Desktop\Bike.png");

    var Ocr = new IronTesseract();
    using (var Input = new OcrInput())
    {
        // a 41% improvement on speed
        var ContentArea = new CropRectangle(4976, 837, 81, 36);
        Input.AddImage(@"C:\Users\garry\OneDrive\Desktop\Bike.png", ContentArea);
        var Result = Ocr.Read(Input);
        Console.WriteLine(Result.Text);
    }

    Thread.Sleep(1000);
}

