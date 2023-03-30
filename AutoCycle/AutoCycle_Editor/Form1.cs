using IronOcr;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;
using static IronOcr.OcrResult;
using System.Windows.Forms;
using AutoCycle.Models;

namespace AutoCycle_Editor
{
    public partial class Form1 : Form
    {
        private const int _screenWidth = 1920;
        private const int _screenHeight = 1080;

        private static int _bikeLowerLimit = 1;
        private static int _bikeUpperLimit = 12;

        private static Dictionary<int, int> _profile;

        private const int _poll = 1000;
        private const bool _isWhiteTextOnBlackBackground = true;
        private static Rectangle _areaToMonitor = new Rectangle(1740, 1019, 73, 41);

        private readonly static UdpClient _udpClient = new UdpClient(5005);
        private readonly static IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 5005);

        public Form1()
        {
            InitializeComponent();
            _profile = GetProfile(Convert.ToInt32(8));
        }

        private async void captureVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog captureVideoOpenFileDialog = new();
            FolderBrowserDialog captureVideoFolderBrowserDialog = new();

            if (captureVideoOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                captureVideoFolderBrowserDialog.Description = "Where do you want to save the capture files?";
                captureVideoFolderBrowserDialog.ShowDialog();

                Process.Start($"\"C:\\Program Files (x86)\\Windows Media Player\\wmplayer.exe\" /fullscreen \"{captureVideoOpenFileDialog.FileName}\"");
            }

            TesseractConfiguration tesseractConfiguration = new TesseractConfiguration();
            tesseractConfiguration.PageSegmentationMode = TesseractPageSegmentationMode.SingleLine;
            tesseractConfiguration.WhiteListCharacters = "-0123456789%";

            IronTesseract ironTesseract = new IronTesseract(tesseractConfiguration);

            using (StreamWriter streamWriter = File.AppendText($"{captureVideoFolderBrowserDialog.SelectedPath}\\capture.txt"))
            {
                int count = 0;

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

                        //streamWriter.WriteLine($"{count},{ocrResult.Text},{ocrResult.Confidence}");
                        //streamWriter.Flush();
                        await AppendToTextFile(streamWriter, $"{count},{ocrResult.Text},{ocrResult.Confidence}");

                        //ocrInput.SaveAsImages($"{captureVideoFolderBrowserDialog.SelectedPath}\\{count}", OcrInput.ImageType.BMP);
                        await SaveImage(ocrInput, $"{captureVideoFolderBrowserDialog.SelectedPath}\\{count}");

                        //await AddRow(count, ocrResult, croppedBitmap);
                    }

                    count++;
                    Thread.Sleep(_poll);
                }
            }
        }

        private Task AppendToTextFile(StreamWriter streamWriter, string line)
        {
            streamWriter.WriteLine(line);
            streamWriter.Flush();

            return Task.CompletedTask;
        }

        private Task SaveImage(OcrInput ocrInput, string path)
        {
            ocrInput.SaveAsImages(path, OcrInput.ImageType.BMP);

            return Task.CompletedTask;
        }

        private Task AddRow(int count, OcrResult ocrResult, Bitmap croppedBitmap)
        {
            dataGridView1.Rows.Add(count, ocrResult.Text, ocrResult.Confidence, croppedBitmap);
            dataGridView1.Refresh();

            return Task.CompletedTask;
        }

        private void openCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog captureFolderBrowserDialog = new();

            if (captureFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines($"{captureFolderBrowserDialog.SelectedPath}\\capture.txt");

                foreach (string line in lines)
                {
                    string[] items = line.Split(',');

                    dataGridView1.Rows.Add(items[0], items[1], items[2], new Bitmap($"{captureFolderBrowserDialog.SelectedPath}\\{items[0]}_0.bmp"));
                }
            }
        }

        private void saveCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog captureFolderBrowserDialog = new();

            if (captureFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter streamWriter = new($"{captureFolderBrowserDialog.SelectedPath}\\capture2.txt"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        streamWriter.WriteLine($"{row.Cells[0].Value},{row.Cells[1].Value},{row.Cells[2].Value}");
                    }

                    streamWriter.Close();
                }
            }
        }

        private void startCycleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog captureFolderBrowserDialog = new();

            if (captureFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines($"{captureFolderBrowserDialog.SelectedPath}\\capture2.txt");

                if (IsFileValid(lines))
                {
                    Process.Start($"\"C:\\Program Files (x86)\\Windows Media Player\\wmplayer.exe\" /fullscreen \"{captureFolderBrowserDialog.SelectedPath}\\Video.mp4\"");

                    //Thread.Sleep(5000);

                    foreach (string line in lines)
                    {
                        string[] items = line.Split(',');

                        Send(new Json
                        {
                            Count = Convert.ToInt32(items[0]),
                            Result = GetResistance(Convert.ToInt32(items[1])),
                            //WithinTolerance = withinTolerance
                        });

                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void Send(Json json)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(json));
            _udpClient.Send(bytes, bytes.Length, _ipEndPoint);
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

        private static int GetResistance(int result)
        {
            return _profile[result];
        }

        private bool IsFileValid(string[] lines)
        {
            foreach (string line in lines)
            {
                string[] items = line.Split(',');

                if (string.IsNullOrWhiteSpace(items[0]) || string.IsNullOrWhiteSpace(items[1]) || string.IsNullOrWhiteSpace(items[2]))
                {
                    MessageBox.Show("File not valid");
                    return false;
                }
            }

            return true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                dataGridView1.CurrentRow.Cells[1].Value = textBox1.Text;
                textBox1.Text = string.Empty;

                GoToNextRow();
            }
        }

        private void cmdUsePrevious_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentRow.Cells[1].Value = dataGridView1.Rows[dataGridView1.CurrentRow.Index - 1].Cells[1].Value;
            GoToNextRow();
        }

        private void cmdUseNext_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentRow.Cells[1].Value = dataGridView1.Rows[dataGridView1.CurrentRow.Index + 1].Cells[1].Value;
            GoToNextRow();
        }

        private void cmdMakeNegative_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value) > -1)
            {
                dataGridView1.CurrentRow.Cells[1].Value = Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value) * -1;
                GoToNextRow();
            }            
        }

        private void cmdMakePositive_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentRow.Cells[1].Value = Math.Abs(Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value));
            GoToNextRow();
        }

        private void GoToNextRow()
        {
            dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.CurrentRow.Index + 1].Cells[1];
        }
    }
}