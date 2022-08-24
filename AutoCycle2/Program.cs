using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using IronOcr;
using System.Diagnostics.Metrics;
using System.IO;
using System.Runtime.CompilerServices;

namespace AutoCycle2
{
    public class Program
    {
        private readonly static UdpClient _udpClient = new UdpClient(5005);
        private readonly static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

        private const byte _bikeLowerLimit = 1;
        private const byte _bikeUpperLimit = 16;

        private const ushort _screenWidth = 1920;
        private const ushort _screenHeight = 1080;

        private const ushort _poll = 1000;
        private const byte _confidence = 90;
        private const short _base = 3;
        private static readonly Regex _digitsWithOptionalNegativeRegex = new Regex("-?\\d");

        private readonly IEnumerable<YouTubeFeed> _youTubeFeeds = new List<YouTubeFeed>()
        {
            new YouTubeFeed
            {
                Name= "30 minute Fat Burning Indoor Cycling Workout Alps South Tyrol Lake Tour Garmin 4K Video",
                Uri = new Uri("https://www.youtube.com/watch?v=sOpm6E1lnpc")
            }
        };

        private class YouTubeFeed
        {
            public string Name { get; set; }
            public Uri Uri { get; set; }
        }

        private const string _fileLocation = @"C:\Users\garry\OneDrive\Desktop\Test";

        private static bool Troubleshooting => false;

        static void Main(string[] args)
        {
            Console.WriteLine("AutoCycle2");
            Console.WriteLine("");
            //Console.WriteLine("Do you want to pick from a list of YouTube feeds?");

            TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
            tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
            tesseractConfiguration.WhiteListCharacters = "-0123456789%";

            IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);
            //ironTesseract.Language = OcrLanguage.Financial;

            //Rectangle rectangle = new Rectangle(1740, 1019, 73, 41);
            Rectangle rectangle = new Rectangle(458, 1024, 238, 56); // Neigung

            int count = 0;

            while (true)
            {
                Bitmap printScreenBitmap = new Bitmap(_screenWidth, _screenHeight);
                Size size = new Size(printScreenBitmap.Width, printScreenBitmap.Height);
                Graphics graphics = Graphics.FromImage(printScreenBitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, size);
                Bitmap croppedBitmap = printScreenBitmap.Clone(rectangle, printScreenBitmap.PixelFormat);

                using (OcrInput ocrInput = new())
                {
                    ocrInput.AddImage(croppedBitmap);
                    ocrInput.ToGrayScale();
                    //ocrInput.DeNoise();

                    if (Troubleshooting)
                    {
                        ocrInput.SaveAsImages($@"C:\Users\garry\OneDrive\Desktop\Test\{count}", OcrInput.ImageType.BMP);
                    }

                    OcrResult ocrResult = ironTesseract.Read(ocrInput);

                    //using (StreamWriter sw = File.AppendText($@"{_fileLocation}\{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss").txt")
                    //{
                    //    sw.WriteLine("This");
                    //    sw.WriteLine("is Extra");
                    //    sw.WriteLine("Text");
                    //}

                    string resultString = ocrResult.Text.Replace("%", "");

                    //Send($"{ocrResult.Text} Confidence: {ocrResult.Confidence}");
                    //Thread.Sleep(_poll);
                    //continue;

                    if (ocrResult.Confidence < _confidence)
                    {
                        Thread.Sleep(_poll);
                        continue;
                    }

                    if (short.TryParse(resultString, out short result))
                    {
                        ProcessResult((short)(result + _base), count);
                    }
                }

                count++;

                Thread.Sleep(_poll);
            }
        }

        private static void Send(int number)
        {
            Send(number.ToString());
        }

        private static void Send(string message)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            _udpClient.Send(bytes, bytes.Length, _ipEndPoint);
        }

        private static short? ProcessOcrResult(OcrResult ocrResult, short previousOcrResult, int count)
        {
            if (ocrResult.Confidence >= _confidence && OcrResultHasDigits(ocrResult))
            {
                short result = ExtractDigits(ocrResult);

                if (result < _bikeLowerLimit)
                {
                    if (Troubleshooting)
                    {
                        Send($"{count}: ERROR! {result} is below bike lower limit. Sending 1...");
                    }

                    Send(1);
                    return result;
                }
                else if (result > _bikeUpperLimit)
                {
                    if (Troubleshooting)
                    {
                        Send($"{count}: ERROR! {result} is above bike upper limit. Sending 16...");
                    }

                    Send(16);
                    return result;
                }
                else
                {
                    if (Troubleshooting)
                    {
                        Send($"{count}: SUCCESS! {result}");
                    }

                    Send(result);
                    return result;
                }
            }
            else
            {
                if (OcrResultHasDigits(ocrResult))
                {
                    short result = ExtractDigits(ocrResult);
                    byte tolerance = 2;

                    if (result - previousOcrResult < tolerance)
                    {
                        if (Troubleshooting)
                        {
                            Send($"{count}: SUCCESS! {result} (within tolerance)");
                        }

                        Send(result);
                        return result;
                    }
                    else
                    {
                        if (Troubleshooting)
                        {
                            Send($"{count}: ERROR! No confidence and outwith tolerance");
                        }

                        return null;
                    }
                }
                else
                {
                    if (Troubleshooting)
                    {
                        Send($"{count}: ERROR! No confidence and no digits in result");
                    }

                    return null;
                }
            }
        }

        private static short ExtractDigits(OcrResult ocrResult)
        {
            return Convert.ToInt16(_digitsWithOptionalNegativeRegex.Match(ocrResult.Text).Value);
        }

        private static bool OcrResultHasDigits(OcrResult ocrResult)
        {
            return _digitsWithOptionalNegativeRegex.IsMatch(ocrResult.Text);
        }

        private static void ProcessResult(short result, int count)
        {
            if (result < _bikeLowerLimit)
            {
                if (Troubleshooting)
                {
                    Send($"{count}: ERROR! {result} is below bike lower limit. Sending 1...");
                    return;
                }

                Send(1);

            }
            else if (result > _bikeUpperLimit)
            {
                if (Troubleshooting)
                {
                    Send($"{count}: ERROR! {result} is above bike upper limit. Sending 16...");
                    return;
                }

                Send(16);
            }
            else
            {
                if (Troubleshooting)
                {
                    Send($"{count}: SUCCESS! {result}");
                    return;
                }

                Send(result);
            }
        }
    }
}