using IronOcr;
using System.Drawing;

namespace AutoCycle_Recorder
{
    internal class Program
    {
        private const int _screenWidth = 1920;
        private const int _screenHeight = 1080;

        private const int _poll = 1000;
        private const bool _isWhiteTextOnBlackBackground = true;
        private static Rectangle _areaToMonitor = new Rectangle(1740, 1019, 73, 41);

        static void Main(string[] args)
        {
            TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
            tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
            tesseractConfiguration.WhiteListCharacters = "-0123456789%";

            IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);

            using (StreamWriter streamWriter = File.AppendText("C:\\Users\\garry\\OneDrive\\Desktop\\TestFile.txt"))
            {
                while (true)
                {
                    Bitmap printScreenBitmap = new Bitmap(_screenWidth, _screenHeight);
                    Size size = new Size(printScreenBitmap.Width, printScreenBitmap.Height);
                    Graphics graphics = Graphics.FromImage(printScreenBitmap);
                    graphics.CopyFromScreen(0, 0, 0, 0, size);
                    Bitmap croppedBitmap = printScreenBitmap.Clone(_areaToMonitor, printScreenBitmap.PixelFormat);

                    using (OcrInput ocrInput = new())
                    {
                        ocrInput.AddImage(croppedBitmap);
                        ocrInput.ToGrayScale();

                        if (_isWhiteTextOnBlackBackground)
                        {
                            ocrInput.Invert();
                        }

                        OcrResult ocrResult = ironTesseract.Read(ocrInput);

                        streamWriter.WriteLine(ocrResult.Text);
                        streamWriter.Flush();
                    }

                    Thread.Sleep(_poll);
                }
            }
        }
    }
}