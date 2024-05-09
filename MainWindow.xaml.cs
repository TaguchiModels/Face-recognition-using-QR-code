/*
  Written by Taguchi.
    QRコードをWebカメラで読む方法と OpenCvSharp で顔を検出する方法のサンプルコードです。
    VisualStudio 2022 でNugetから以下を入手しました。

    Nuget:OpenCvSharp4.Windows 
    Nuget:OpenCvSharp4.Extensions
    Nuget:OpenCvSharp4.WpfExtensions
    Nuget:ZXing.Net
    ----------------------------------------------------------

    This is sample code for how to read a QR code with a webcam and
    how to detect a face with OpenCvSharp.
    I got the following from Nuget in VisualStudio 2022.

    Nuget:OpenCvSharp4.Windows 
    Nuget:OpenCvSharp4.Extensions
    Nuget:OpenCvSharp4.WpfExtensions
    Nuget:ZXing.Net
    ----------------------------------------------------------
*/
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenCvSharp; // Nuget:OpenCvSharp4.Windows 
using OpenCvSharp.Extensions; // Nuget:OpenCvSharp4.Extensions
using OpenCvSharp.WpfExtensions; // Nuget:OpenCvSharp4.WpfExtensions
using ZXing.QrCode;
using ZXing; // Nuget:ZXing.Net
using System.IO;
using ZXing.Common;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace FaceRecognitionUsingQRcode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        static string facesFolder = AppDomain.CurrentDomain.BaseDirectory + @"faces\";
        static string tempfolder = AppDomain.CurrentDomain.BaseDirectory + @"faces\temp\";
        VideoCapture camera = new VideoCapture(0);
        string QRcode_read = "";
        int rc = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shooting begins
        /// </summary>
        private async void CaptureStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(facesFolder))
                {
                    ResultMessage.Text = "faces folder not found!";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }

                // Delete files in temporary folder.
                if (!Directory.Exists(tempfolder))
                {
                    ResultMessage.Text = "temporary folder not found!";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }

                string[] filePaths = Directory.GetFiles(tempfolder);
                foreach (string filePath in filePaths)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }

                ResultMessage.Text = "";
                txtQrcode.Text = "";
                QRcode_read = "";
                rc = 0;

                if (camera != null)
                {
                    camera.Dispose(); //Memory release
                }

                //capture_loop = false;
                ResultMessage.Background = System.Windows.Media.Brushes.White;
                camera = new VideoCapture(0);

                if (!camera.IsOpened())
                {
                    ResultMessage.Text = "Webcam not found!" + Environment.NewLine + "Please check your webcam.";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }

                //camera.Set(OpenCvSharp.CaptureProperty.AutoFocus, 0); // 0:Auto focus off
                //camera.Set(OpenCvSharp.CaptureProperty.Focus, 10); // 0-250 
                CaptureStart.IsEnabled = false;
                CaptureStop.IsEnabled = true;

                await QRcodeCaptureAsync();
                camera.Dispose(); //Memory release

                if (rc == 0)
                {
                    txtQrcode.Text = "QR code is '" + QRcode_read + "'";
                }
                else if (rc == -100)
                {
                    ResultMessage.Text = "The missing QR code is '" + QRcode_read + "'";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }
                else if (rc == -200)
                {
                    ResultMessage.Text = "It is not a QR code format.";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }
                else
                {
                    ResultMessage.Text = "System error!";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }

                Thread.Sleep(3000);

                camera = new VideoCapture(0);
                if (!camera.IsOpened())
                {
                    ResultMessage.Text = "Webcam not found! Please check your webcam.";
                    ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
                    return;
                }

                await FaceCaptureAsync();
                camera.Dispose(); //Memory release

                this.ImgBarcode.Source = GetCameraImage();

                // C++ call
                ProcessStartInfo app = new ProcessStartInfo
                {
                    FileName = "FaceRecognition.exe",
                    CreateNoWindow = true, // console window close
                    UseShellExecute = false, // Do not use shell functions
                    Arguments = QRcode_read // QR code
                };

                Process? p = Process.Start(app);
                p.WaitForExit(); // wait for process to finish
                int iExitCode = p.ExitCode;

                if (iExitCode == 0)
                {
                    string str_accuracy = File.ReadAllText(facesFolder + QRcode_read + @"\accuracy.txt");
                    float accuracy = float.Parse(str_accuracy);

                    // If the accuracy is less than 0.6, it can be determined that the person is the real person.
                    // If higher accuracy is required, it is possible to set an even lower value,
                    //  but please select a value with the understanding that the person in question may be identified as someone else.
                    ResultMessage.Text = accuracy < 0.6 ? "The system has authenticated you." : "The system could not authenticate you.";
                    txtQrcode.Text += " / accuracy is " + str_accuracy;
                }
                else
                {
                    ResultMessage.Text = "An error was detected in the system.";
                }

                ResultMessage.Background = System.Windows.Media.Brushes.Yellow;

                CaptureStop.IsEnabled = false;
                CaptureStart.IsEnabled = true;
            }
            catch (Exception)
            {
                ResultMessage.Text = "Application Exception!";
                ResultMessage.Background = System.Windows.Media.Brushes.Yellow;
            }
        }

        /// <summary>
        /// Shooting canceled
        /// </summary>
        private void CaptureStop_Click(object sender, RoutedEventArgs e)
        {
            if (camera != null)
            {
                camera.Dispose(); //Memory release
            }

            CaptureStop.IsEnabled = false;
            CaptureStart.IsEnabled = true;
            this.ImgBarcode.Source = GetCameraImage();

            ResultMessage.Background = System.Windows.Media.Brushes.White;
            ResultMessage.Text = "";
        }

        /// <summary>
        /// QR code Capture
        /// </summary>
        private async Task QRcodeCaptureAsync()
        {
            await Task.Run(() =>
            {
                Boolean loopFlg = true;
                ZXing.Result result;
                using (Mat img = new Mat())
                using (camera)
                {
                    while (loopFlg)
                    {
                        camera.Read(img);

                        if (img.Empty()) { break; }

                        this.Dispatcher.Invoke(() =>
                        {
                            Bitmap bitmap = img.ToBitmap();

                            MemoryStream memoryStream = new MemoryStream();
                            bitmap.Save(memoryStream, ImageFormat.Bmp);
                            byte[] byteArray = memoryStream.GetBuffer();
                            LuminanceSource source = new RGBLuminanceSource(byteArray, bitmap.Width, bitmap.Height);
                            var binarizer = new HybridBinarizer(source);
                            var binBitmap = new BinaryBitmap(binarizer);
                            QRCodeReader qrCodeReader = new QRCodeReader();
                            result = qrCodeReader.decode(binBitmap);

                            if (result == null)
                            {
                                this.ImgBarcode.Source = BitmapSourceConverter.ToBitmapSource(img);
                            }
                            else
                            {
                                bitmap.Dispose();

                                if (result.BarcodeFormat != ZXing.BarcodeFormat.QR_CODE)
                                {
                                    string message = "NOT IN QR CODE FORMAT!";
                                    Cv2.PutText(img, message, new OpenCvSharp.Point(10, 240), HersheyFonts.HersheyDuplex, 1.5, Scalar.Red, 2);
                                    this.ImgBarcode.Source = BitmapSourceConverter.ToBitmapSource(img);
                                    rc = -200;
                                }
                                else
                                {
                                    QRcode_read = result.Text;
                                    txtQrcode.Text = result.Text;
                                    string message = "OK! Please face the webcam.";
                                    Scalar color = Scalar.Green;
                                    string filename = facesFolder + QRcode_read + @"\" + QRcode_read + ".png";

                                    if (!File.Exists(filename))
                                    {
                                        rc = -100;
                                        message = "QR code file not found!";
                                        color = Scalar.Red;
                                    }

                                    OpenCvSharp.Rect rectMsg = new OpenCvSharp.Rect(10, 10, 600, 40);
                                    Cv2.Rectangle(img, rectMsg, new OpenCvSharp.Scalar(206, 230, 193), -1);
                                    Cv2.PutText(img, message, new OpenCvSharp.Point(20, 40), HersheyFonts.HersheyDuplex, 1.2, color, 2);
                                    this.ImgBarcode.Source = BitmapSourceConverter.ToBitmapSource(img);

                                    loopFlg = false; // loop stop
                                }
                            }
                        });
                    }
                }
            });
        }

        /// <summary>
        /// Face Capture
        /// </summary>
        private async Task FaceCaptureAsync()
        {
            await Task.Run(() =>
            {
                string message1 = "Authenticating,";
                string message2 = "please wait a moment.";
                Boolean loopFlg = true;
                int imgCounter = 0;
                DateTime startTime = DateTime.Now;
                using (Mat img = new Mat())
                using (camera)
                {
                    while (loopFlg)
                    {
                        camera.Read(img);

                        if (img.Empty()) { break; }

                        this.Dispatcher.Invoke(() =>
                        {
                            using (var cascade = new CascadeClassifier(@"haarcascade_frontalface_default.xml"))
                            {
                                foreach (OpenCvSharp.Rect rectDetect in cascade.DetectMultiScale(img))
                                {
                                    DateTime currentTime = DateTime.Now;
                                    TimeSpan elapsed = currentTime - startTime;

                                    if (elapsed.TotalSeconds >= 0.5)
                                    {
                                        Cv2.ImWrite(tempfolder + @"\face_" + imgCounter + ".jpg", img);
                                        startTime = currentTime;
                                        imgCounter++;
                                        if (imgCounter >= 10)
                                        {
                                            loopFlg = false;
                                        }
                                    }

                                    OpenCvSharp.Rect rect = new OpenCvSharp.Rect(rectDetect.X, rectDetect.Y, rectDetect.Width, rectDetect.Height);
                                    Cv2.Rectangle(img, rect, new OpenCvSharp.Scalar(255, 255, 0), 2);
                                }

                                OpenCvSharp.Rect rectMsg = new OpenCvSharp.Rect(10, 10, 600, 80);
                                Cv2.Rectangle(img, rectMsg, new OpenCvSharp.Scalar(206, 230, 193), -1);
                                Cv2.PutText(img, message1, new OpenCvSharp.Point(20, 40), HersheyFonts.HersheyDuplex, 1.3, Scalar.Green, 2);
                                Cv2.PutText(img, message2, new OpenCvSharp.Point(40, 80), HersheyFonts.HersheyDuplex, 1.3, Scalar.Green, 2);
                                this.ImgBarcode.Source = BitmapSourceConverter.ToBitmapSource(img);
                            }
                        });
                    }
                }
            });
        }

        /// <summary>
        /// Get logo mark
        /// </summary>
        private BitmapImage GetCameraImage()
        {
            BitmapImage bmpImage = new BitmapImage();
            using (FileStream stream = File.OpenRead(@"Images/camera.png"))
            {
                bmpImage.BeginInit();
                bmpImage.StreamSource = stream;
                bmpImage.DecodePixelWidth = 500;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.CreateOptions = BitmapCreateOptions.None;
                bmpImage.EndInit();
                bmpImage.Freeze();
            }

            return bmpImage;
        }
    }
}