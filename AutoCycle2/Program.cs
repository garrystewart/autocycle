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
using System.Diagnostics;

namespace AutoCycle2
{
    public class Program
    {
        private readonly static UdpClient _udpClient = new UdpClient(5005);
        private readonly static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

        private static int _bikeLowerLimit;
        private static int _bikeUpperLimit;
        private static int _youTubeFeedId;

        private const int _screenWidth = 1920;
        private const int _screenHeight = 1080;

        private const int _poll = 1000;
        private const int _tolerance = 2;
        private const int _confidenceLevel = 90;
        private static readonly Regex _digitsWithOptionalNegativeRegex = new Regex("-?\\d");

        private static Dictionary<int, int> _profile;
        private static int _toleranceCount = 0;

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
            },
            new YouTubeFeed
            {
                Id = 4,
                Name= "Ultimate 45 minute MTB Fat Burning Cycling Workout Alps 🚵‍♂️😎 Dolomiti Italy Garmin 4K",
                Uri = new Uri("https://www.youtube.com/watch?v=uWQoMwxd_AM"),
                AreaToMonitor = new Rectangle(1738, 1024, 101, 56),
                IsWhiteTextOnBlackBackground = true,
                ConfidenceLevel = 90
            },
            new YouTubeFeed
            {
                Id = 5,
                Name= "30 Minute Cycling Workout Brasa Canyon Italy Ultra HD Video Garmin",
                Uri = new Uri("https://www.youtube.com/watch?v=x47SSiQea_M"),
                AreaToMonitor = new Rectangle(318, 994, 87, 40),
                IsWhiteTextOnBlackBackground = true,
                ConfidenceLevel = 90
            }
        };

        private const string _fileLocation = @"C:\Users\garry\OneDrive\Desktop\Test";

        static void Main(string[] args)
        {
            Console.WriteLine("AutoCycle2");
            Console.WriteLine();
            Console.WriteLine("What is the bike's lower limit?");
            _bikeLowerLimit = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("What is the bike's upper limit?");
            _bikeUpperLimit = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("What do you want to set as the base resistance?");
            var baseResistance = Console.ReadLine();
            Console.WriteLine("Which YouTube feed do you want to use?");
            _youTubeFeedId = Convert.ToInt32(Console.ReadLine());
            _profile = GetProfile(Convert.ToInt32(baseResistance));
            Console.WriteLine();
            //Console.WriteLine("Opening video...");

            YouTubeFeed? youTubeFeed = GetYouTubeFeed(_youTubeFeedId);

            //Process.Start($"C:\\Users\\garry\\Downloads\\{youTubeFeed.Name}-(2160p30).mp4");

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



            if (youTubeFeed is null)
            {
                return;
            }

            int count = 0;
            int? previousOcrResult = null;

            Console.WriteLine("Beginning OCR scanning...");

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

                    previousOcrResult = ProcessOcrResult(ocrResult, previousOcrResult, count);
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

        private static int? ProcessOcrResult(OcrResult ocrResult, int? previousOcrResult, int count)
        {
            // don't go by confidence initially as it's rubbish at being confident with negative numbers

            // if the tolerance count goes above 5, reset previousOcrResult back to null and let confidence levels take over
            if (_toleranceCount == 5)
            {
                previousOcrResult = null;
                _toleranceCount = 0;
            }

            // previousOcrResult starts off as null, to initialise it we are looking for something over 90% to set as a benchmark
            if (previousOcrResult is null && ocrResult.Confidence >= _confidenceLevel && OcrResultHasDigits(ocrResult))
            {
                previousOcrResult = ExtractDigits(ocrResult);
            }

            if (OcrResultHasDigits(ocrResult))
            {
                // we've found digits, now to check if they are within tolerance

                int result = ExtractDigits(ocrResult);

                if (previousOcrResult.HasValue && IsWithinTolerance(result, previousOcrResult.Value))
                {
                    int resultToSend = GetResistance(result);

                    Send(new Json
                    {
                        Count = count,
                        Result = resultToSend,
                        Confidence = GetConfidence(ocrResult),
                        OcrResultText = ocrResult.Text,
                        WithinTolerance = true
                    });

                    return result;
                }
                else
                {
                    Send(new Json
                    {
                        Count = count,
                        Confidence = GetConfidence(ocrResult),
                        OcrResultText = ocrResult.Text,
                        WithinTolerance = false
                    });

                    _toleranceCount++;

                    return previousOcrResult;
                }
            }
            else
            {
                Send(new Json
                {
                    Count = count,
                    Confidence = GetConfidence(ocrResult),
                    OcrResultText = ocrResult.Text,
                    NoDigits = true
                });

                _toleranceCount++;

                return previousOcrResult;
            }



            //// if there is no previous result to compare against, fall back to confidence

            //if (previousOcrResult is null)
            //{
            //    if (ocrResult.Confidence >= youTubeFeed.ConfidenceLevel && OcrResultHasDigits(ocrResult))
            //    {
            //        int result = ExtractDigits(ocrResult);

            //        CheckBikeLimits(result, ocrResult, count);

            //        return result;
            //    }
            //    else
            //    {
            //        Send(new Json
            //        {
            //            Count = count,
            //            Confidence = GetConfidence(ocrResult),
            //            OcrResultText = ocrResult.Text,
            //        });

            //        return null;
            //    }
            //}
            //else
            //{
            //    if (OcrResultHasDigits(ocrResult))
            //    {
            //        int result = ExtractDigits(ocrResult);

            //        Console.WriteLine($"[{count}] result: {result} previousResult: {previousOcrResult} calculation: {result - previousOcrResult}");

            //        if (IsWithinTolerance(result, previousOcrResult.Value))
            //        {
            //            Console.WriteLine($"[{count}] WithinTolerance");

            //            CheckBikeLimits(result, ocrResult, count, true);

            //            return result;
            //        }
            //        else
            //        {
            //            Send(new Json
            //            {
            //                Count = count,
            //                Confidence = GetConfidence(ocrResult),
            //                OcrResultText = ocrResult.Text,
            //                //WithinTolerance = false,
            //            });

            //            return null;
            //        }
            //    }
            //    else
            //    {
            //        Send(new Json
            //        {
            //            Count = count,
            //            Confidence = GetConfidence(ocrResult),
            //            OcrResultText = ocrResult.Text,
            //        });

            //        return null;
            //    }
            //}

            //if (ocrResult.Confidence >= youTubeFeed.ConfidenceLevel && OcrResultHasDigits(ocrResult))
            //{
            //    int result = ExtractDigits(ocrResult);

            //    if (result - previousOcrResult < _tolerance)
            //    {
            //        CheckBikeLimits(result, ocrResult, count);

            //        return result;
            //    }
            //    else
            //    {
            //        Send(new Json
            //        {
            //            Count = count,
            //            Confidence = GetConfidence(ocrResult),
            //            OcrResultText = ocrResult.Text,
            //            WithinTolerance = false,
            //        });

            //        return null;
            //    }  
            //}
            //else
            //{
            //    if (OcrResultHasDigits(ocrResult))
            //    {
            //        int result = ExtractDigits(ocrResult);


            //        Console.WriteLine($"[{count}] result: {result} previousResult: {previousOcrResult} calculation: {result - previousOcrResult}");

            //        if (result - previousOcrResult < _tolerance)
            //        {
            //            Console.WriteLine($"[{count}] WithinTolerance");
            //            CheckBikeLimits(result, ocrResult, count, true);
            //            return result;
            //        }
            //        else
            //        {
            //            Send(new Json
            //            {
            //                Count = count,
            //                Confidence = GetConfidence(ocrResult),
            //                OcrResultText = ocrResult.Text,
            //                WithinTolerance = false,
            //            });

            //            return null;
            //        }
            //    }
            //    else
            //    {
            //        Send(new Json
            //        {
            //            Count = count,
            //            Confidence = GetConfidence(ocrResult),
            //            OcrResultText = ocrResult.Text,
            //        });

            //        return null;
            //    }
            //}
        }

        private static void CheckBikeLimits(int result, OcrResult ocrResult, int count)
        {
            result = GetResistance(result);

            //if (result < _bikeLowerLimit)
            //{
            //    Send(new Json
            //    {
            //        Confidence = GetConfidence(ocrResult),
            //        OcrResultText = ocrResult.Text,
            //        Result = 1,
            //        WithinTolerance = withinTolerance
            //    });

            //    return result;
            //}
            //else if (result > _bikeUpperLimit)
            //{
            //    Send(new Json
            //    {
            //        Confidence = GetConfidence(ocrResult),
            //        OcrResultText = ocrResult.Text,
            //        Result = 16,
            //        WithinTolerance = withinTolerance
            //    });

            //    return result;
            //}
            //else
            //{
            Send(new Json
            {
                Count = count,
                Confidence = GetConfidence(ocrResult),
                OcrResultText = ocrResult.Text,
                Result = result,
                WithinTolerance = true
                //WithinTolerance = withinTolerance
            });

            //return result;
            //}
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
            return (resistance <= _bikeUpperLimit) ? resistance : _bikeUpperLimit;
        }

        private static int IsWithinBikeLowerLimit(int resistance)
        {
            return (resistance >= _bikeLowerLimit) ? resistance : _bikeLowerLimit;
        }

        private static void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static bool IsWithinTolerance(int result, int previousResult)
        {
            if (result < previousResult)
            {
                return previousResult - result <= _tolerance;
            }
            else
            {
                return result - previousResult <= _tolerance;
            }
        }
    }
}