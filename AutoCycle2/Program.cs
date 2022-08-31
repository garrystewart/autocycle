using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using IronOcr;
using System.Diagnostics.Metrics;
using System.IO;
using System.Runtime.CompilerServices;
using AutoCycle2.Models;
using System.Text.Json;

namespace AutoCycle2
{
    public class Program
    {
        private readonly static UdpClient _udpClient = new UdpClient(5005);
        private readonly static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

        private const int _bikeLowerLimit = 1;
        private const int _bikeUpperLimit = 16;

        private const int _screenWidth = 1920;
        private const int _screenHeight = 1080;

        private const int _poll = 1000;
        private const int _base = 0;
        private static readonly Regex _digitsWithOptionalNegativeRegex = new Regex("-?\\d");

        private static readonly Dictionary<int, int> _profile = GetProfile(8);

        private static readonly IEnumerable<YouTubeFeed> _youTubeFeeds = new List<YouTubeFeed>()
        {
            new YouTubeFeed
            {
                Id = 1,
                Name= "30 minute Fat Burning Indoor Cycling Workout Alps South Tyrol Lake Tour Garmin 4K Video",
                Uri = new Uri("https://www.youtube.com/watch?v=sOpm6E1lnpc"),
                MinimumGradient = -10,
                MaximumGradient = 12,
                AreaToMonitor = new Rectangle(1740, 1019, 73, 41),
                IsWhiteTextOnBlackBackground = true,
                ConfidenceLevel = 90
            },
            new YouTubeFeed
            {
                Id = 2,
                Name= "45 minute Fat Burning Indoor Cycling Workout Alps South Tyrol Lake Tour Garmin 4K",
                Uri = new Uri("https://www.youtube.com/watch?v=JOWN4WmuItM"),
                AreaToMonitor = new Rectangle(1744, 1012, 92, 44),
                IsWhiteTextOnBlackBackground = true,
                ConfidenceLevel = 90
            },
            new YouTubeFeed
            {
                Id = 3,
                Name= "Fat Burning 40 Minute Sunshine Cycling Motivation Training 4k Ultra HD Video",
                Uri = new Uri("https://www.youtube.com/watch?v=-ybtzixqrq4"),
                AreaToMonitor = new Rectangle(335, 1028, 49, 29),
                IsWhiteTextOnBlackBackground = true,
                ConfidenceLevel = 90
            }
        };

        private const string _fileLocation = @"C:\Users\garry\OneDrive\Desktop\Test";

        static void Main(string[] args)
        {
            Console.WriteLine("AutoCycle2");
            Console.WriteLine("");
            //Console.WriteLine("Do you want to pick from a list of YouTube feeds?");

            TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
            tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
            tesseractConfiguration.WhiteListCharacters = "-0123456789%";

            IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);

            //Rectangle rectangle = new Rectangle(1740, 1019, 73, 41);
            //Rectangle rectangle = new Rectangle(458, 1024, 238, 56); // Neigung

            DirectoryInfo directoryInfo = new DirectoryInfo(_fileLocation);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            YouTubeFeed? youTubeFeed = GetYouTubeFeed(3);

            if (youTubeFeed is null)
            {
                return;
            }

            int count = 0;
            int? previousResult = null;

            while (true)
            {
                Bitmap printScreenBitmap = new Bitmap(_screenWidth, _screenHeight);
                Size size = new Size(printScreenBitmap.Width, printScreenBitmap.Height);
                Graphics graphics = Graphics.FromImage(printScreenBitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, size);
                Bitmap croppedBitmap = printScreenBitmap.Clone(youTubeFeed.AreaToMonitor, printScreenBitmap.PixelFormat);

                using (OcrInput ocrInput = new())
                {
                    ocrInput.AddImage(croppedBitmap);
                    ocrInput.ToGrayScale();

                    if (youTubeFeed.IsWhiteTextOnBlackBackground)
                    {
                        ocrInput.Invert();
                    }

                        ocrInput.SaveAsImages($@"C:\Users\garry\OneDrive\Desktop\Test\{count}", OcrInput.ImageType.BMP);

                    OcrResult ocrResult = ironTesseract.Read(ocrInput);

                    //using (StreamWriter sw = File.AppendText($@"{_fileLocation}\{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss").txt")
                    //{
                    //    sw.WriteLine("This");
                    //    sw.WriteLine("is Extra");
                    //    sw.WriteLine("Text");
                    //}

                    previousResult = ProcessOcrResult(youTubeFeed, ocrResult, previousResult, count);
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

        private static void Send(Json json)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(json));
            _udpClient.Send(bytes, bytes.Length, _ipEndPoint);
        }

        private static YouTubeFeed? GetYouTubeFeed(int id)
        {
            return _youTubeFeeds.SingleOrDefault(y => y.Id == id);
        }

        private static string GetConfidence(OcrResult ocrResult)
        {
            return ocrResult.Confidence.ToString("0.00");
        }

        private static int? ProcessOcrResult(YouTubeFeed youTubeFeed, OcrResult ocrResult, int? previousResult, int count)
        {
            if (ocrResult.Confidence >= youTubeFeed.ConfidenceLevel && OcrResultHasDigits(ocrResult))
            {
                int result = ExtractDigits(ocrResult);

                return CheckBikeLimits(result, ocrResult);                
            }
            else
            {
                if (OcrResultHasDigits(ocrResult))
                {
                    int result = ExtractDigits(ocrResult);
                    int tolerance = 2;

                    if (result - previousResult < tolerance)
                    {
                        return CheckBikeLimits(result, ocrResult, true);
                    }
                    else
                    {
                        Send(new Json
                        {
                            Confidence = GetConfidence(ocrResult),
                            OcrResultText = ocrResult.Text,
                            WithinTolerance = false
                        });

                        return null;
                    }
                }
                else
                {
                    Send(new Json
                    {
                        Confidence = GetConfidence(ocrResult),
                        OcrResultText = ocrResult.Text,
                    });

                    return null;
                }
            }
        }

        private static int CheckBikeLimits(int result, OcrResult ocrResult, bool? withinTolerance = null)
        {
            result = GetResistance(result);

            if (result < _bikeLowerLimit)
            {
                Send(new Json
                {
                    Confidence = GetConfidence(ocrResult),
                    OcrResultText = ocrResult.Text,
                    Result = 0 + _base,
                    WithinTolerance = withinTolerance
                });

                return result;
            }
            else if (result > _bikeUpperLimit)
            {
                Send(new Json
                {
                    Confidence = GetConfidence(ocrResult),
                    OcrResultText = ocrResult.Text,
                    Result = 16,
                    WithinTolerance = withinTolerance
                });

                return result;
            }
            else
            {
                Send(new Json
                {
                    Confidence = GetConfidence(ocrResult),
                    OcrResultText = ocrResult.Text,
                    Result = result + _base,
                    WithinTolerance = withinTolerance
                });

                return result;
            }
        }

        private static int ExtractDigits(OcrResult ocrResult)
        {
            return Convert.ToInt16(_digitsWithOptionalNegativeRegex.Match(ocrResult.Text).Value);
        }

        private static bool OcrResultHasDigits(OcrResult ocrResult)
        {
            return _digitsWithOptionalNegativeRegex.IsMatch(ocrResult.Text);
        }

        private static int GetResistance(int result)
        {
            return _profile[result];
        }

        private static Dictionary<int, int> GetProfile(int resistance)
        {
            Dictionary<int, int> profile = new Dictionary<int, int>();
            int gradient = 0;
            int resistanceParam = resistance;

            profile.Add(gradient, resistance);

            for (int i = 0; i < 90; i++)
            {
                gradient++;
                resistance--;
                profile.Add(gradient, IsWithinBikeLowerLimit(resistance));
            }

            gradient = 0;
            resistance = resistanceParam;

            for (int i = 0; i < 90; i++)
            {
                gradient--;
                resistance++;
                profile.Add(gradient, IsWithinBikeUpperLimit(resistance));
            }

            return profile;
        }

        private static int IsWithinBikeUpperLimit(int resistance)
        {
            return (resistance <= _bikeUpperLimit) ? resistance : 16;
        }

        private static int IsWithinBikeLowerLimit(int resistance)
        {
            return (resistance >= _bikeLowerLimit) ? resistance : 1;
        }
    }
}