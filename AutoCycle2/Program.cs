using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using IronOcr;
using System.Diagnostics.Metrics;

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

        private const byte _tolerance = 2;
        private const ushort _poll = 1000;


        private static bool Troubleshooting => true;

        static void Main(string[] args)
        {
            byte? _previousReading = null;

            Console.WriteLine("AutoCycle2");

            TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
            tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
            tesseractConfiguration.WhiteListCharacters = "-0123456789% ";

            IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);
            ironTesseract.Language = OcrLanguage.Financial;

            //Rectangle rectangle = new Rectangle(1740, 1019, 73, 41);
            Rectangle rectangle = new Rectangle(458, 1024, 238, 56);

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

                    if (Troubleshooting)
                    {
                        ocrInput.SaveAsImages($"C:\\Users\\garry\\OneDrive\\Desktop\\Test\\{count}", OcrInput.ImageType.BMP);
                    }

                    OcrResult ocrResult = ironTesseract.Read(ocrInput);

                    Send($"{ocrResult.Text} Confidence: {ocrResult.Confidence}");
                    Thread.Sleep(_poll);
                    continue;

                    if (byte.TryParse(ocrResult.Text, out byte result))
                    {
                        _previousReading = ProcessResult(result, count, _previousReading);
                    }
                    else
                    {
                        Regex regex = new Regex("\\d");
                        Match match = regex.Match(ocrResult.Text);

                        if (match.Success)
                        {
                            if (byte.TryParse(match.Value, out byte regexResult))
                            {
                                ProcessResult(regexResult, count, _previousReading);
                            }
                            else
                            {
                                if (Troubleshooting)
                                {
                                    Send($"{count}: ERROR! Unable to parse {match.Value} (regex) into byte");
                                }
                            }
                        }
                        else
                        {
                            if (Troubleshooting)
                            {
                                Send($"{count}: ERROR! Unable to parse {ocrResult.Text} into byte as no regex match");
                            }
                        }
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

        private static bool IsWithinTolerance(byte result, byte previousReading)
        {
            return true;
            //return (result - previousReading < _tolerance) ? true : false;
        }

        private static byte? ProcessResult(byte result, int count, byte? previousReading)
        {
            if (previousReading is null)
            {
                previousReading = result;
            }

            if (!IsWithinTolerance(result, previousReading.Value))
            {
                if (Troubleshooting)
                {
                    Send($"{count}: ERROR! {result} compared with previous reading of {previousReading} is not within tolerance of {_tolerance}. Aborting...");
                }

                return previousReading;
            }

            if (result < _bikeLowerLimit)
            {
                if (Troubleshooting)
                {
                    Send($"{count}: ERROR! {result} is below bike lower limit. Sending 1...");
                    return result;
                }

                Send(1);

            }
            else if (result > _bikeUpperLimit)
            {
                if (Troubleshooting)
                {
                    Send($"{count}: ERROR! {result} is above bike upper limit. Sending 16...");
                    return result;
                }

                Send(16);
            }
            else
            {
                if (Troubleshooting)
                {
                    Send($"{count}: SUCCESS! {result}");
                    return result;
                }

                Send(result);
            }

            return result;
        }
    }
}