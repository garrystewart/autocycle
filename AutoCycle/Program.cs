// See https://aka.ms/new-console-template for more information

using IronOcr;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

Console.WriteLine("AutoCycle");

UdpClient udpClient = new UdpClient(5005);
byte bikeLowerLimit = 1;
byte bikeUpperLimit = 16;
ushort delay = 0; // millseconds
bool troubleshooting = true;

TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;

IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);
ironTesseract.Language = OcrLanguage.Financial;

//Rectangle rectangle = new Rectangle(1739, 1020, 71, 39);
//Rectangle rectangle = new Rectangle(1740, 1019, 73, 41);
Rectangle rectangle = new Rectangle(467, 1031, 108, 33);

IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

int count = 0;

while (true)
{
    Bitmap printScreenBitmap = new Bitmap(1920, 1080);
    Size size = new Size(printScreenBitmap.Width, printScreenBitmap.Height);
    Graphics graphics = Graphics.FromImage(printScreenBitmap);
    graphics.CopyFromScreen(0, 0, 0, 0, size);
    Bitmap croppedBitmap = printScreenBitmap.Clone(rectangle, printScreenBitmap.PixelFormat);

    using (OcrInput ocrInput = new())
    {
        ocrInput.AddImage(croppedBitmap);

        if (troubleshooting)
        {
            ocrInput.SaveAsImages($"C:\\Users\\garry\\OneDrive\\Desktop\\Test\\{count}", OcrInput.ImageType.BMP);
        }       
        
        OcrResult ocrResult = ironTesseract.Read(ocrInput);

        byte[] bytes;
        
        if (byte.TryParse(ocrResult.Text, out byte result))
        {
            if (result < bikeLowerLimit)
            {
                if (troubleshooting)
                {
                    bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! {result} is below bike lower limit. Sending 1...");
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes("1");
                }

                udpClient.Send(bytes, bytes.Length, ipEndPoint);
            }
            else if (result > bikeUpperLimit)
            {
                if (troubleshooting)
                {
                    bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! {result} is above bike upper limit. Sending 16...");                    
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes("16");
                }

                udpClient.Send(bytes, bytes.Length, ipEndPoint);
            }
            else
            {
                if (troubleshooting)
                {
                    bytes = Encoding.ASCII.GetBytes($"{count}: SUCCESS! {result}");                    
                }
                else
                {
                    bytes = Encoding.ASCII.GetBytes(result.ToString());
                }

                udpClient.Send(bytes, bytes.Length, ipEndPoint);
            }            
        }
        else
        {
            Regex regex = new Regex("\\d");
            Match match = regex.Match(ocrResult.Text);

            if (match.Success)
            {
                if (byte.TryParse(match.Value, out byte regexResult))
                {
                    if (regexResult < bikeLowerLimit)
                    {
                        if (troubleshooting)
                        {
                            bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! {regexResult} (regex parsed) is below bike lower limit. Sending 1...");
                        }
                        else
                        {
                            bytes = Encoding.ASCII.GetBytes("1");
                        }

                        udpClient.Send(bytes, bytes.Length, ipEndPoint);
                    }
                    else if (regexResult > bikeUpperLimit)
                    {
                        if (troubleshooting)
                        {
                            bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! {regexResult} (regex parsed) is above bike upper limit. Sending 16...");
                        }
                        else
                        {
                            bytes = Encoding.ASCII.GetBytes("16");
                        }

                        udpClient.Send(bytes, bytes.Length, ipEndPoint);
                    }
                    else
                    {
                        if (troubleshooting)
                        {
                            bytes = Encoding.ASCII.GetBytes($"{count}: SUCCESS! {regexResult} (regex parsed)");
                        }
                        else
                        {
                            bytes = Encoding.ASCII.GetBytes(regexResult.ToString());
                        }
                        
                        udpClient.Send(bytes, bytes.Length, ipEndPoint);
                    }
                }
                else
                {
                    if (troubleshooting)
                    {
                        bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! Unable to parse {match.Value} (regex) into byte");
                        udpClient.Send(bytes, bytes.Length, ipEndPoint);
                    }
                }
            }
            else
            {
                if (troubleshooting)
                {
                    bytes = Encoding.ASCII.GetBytes($"{count}: ERROR! Unable to parse {ocrResult.Text} into byte as no regex match");
                    udpClient.Send(bytes, bytes.Length, ipEndPoint);
                }
                else
                {
                    // do nothing
                }
            }            
        }        
    }

    count++;

    Thread.Sleep(1000 + delay);
    delay = 0;
}