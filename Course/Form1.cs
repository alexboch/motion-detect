using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;

namespace Course
{

    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private VideoWriter _videoWriter;
        private bool _captureInProgress;
        private Mat _frame1, _frame2;
        private Mat _differenceImage;
        private Mat _thresholdImage;
        private Mat _grayImage1, _grayImage2;
        private const int MaxSens = 150;
        private const int MinSens = 0;
        private int _sensBase = 50;
        private int _sensitivityValue = 50;
        private int _sensStep;
        
        private const int BLUR_SIZE = 10;
        private const string IdleStr = "Ожидание";
        private const string RecordStr = "Запись";
        private const string NomotionStr = "Нет движения";
        private const int Fps = 60;
        private Size _writeSize=new Size(640,480);
        /// <summary>
        /// Объект синхронизации
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Размер отображаемого изображения
        /// </summary>
        private Size _imageSize = new Size(640, 480);

        public Size ImageSize
        {

            get
            {
                lock (_lock)
                {
                    return _imageSize;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _imageSize = value;
                }
            }
        }

        /// <summary>
        /// Соотношение сторон
        /// </summary>
        private const float AspectRatio = 4.0f / 3.0f;

        public Form1()
        {
            InitializeComponent();
            _capture = new VideoCapture(0);

            _capture.ImageGrabbed += ProcessFrame;
            _frame1 = new Mat();
            _frame2 = new Mat();
            _differenceImage = new Mat();
            _thresholdImage = new Mat();
            _grayImage1 = new Mat();
            _grayImage2 = new Mat();
            _sensStep = (int)Math.Round((MaxSens-MinSens)/ (float)sensBar.Maximum);
            sensBar.Value = (MaxSens-_sensitivityValue) / _sensStep;
        }



        /// <summary>
        /// Новый размер с сохранением соотношения сторон
        /// </summary>
        /// <param name="imgBoxSize"></param>
        /// <returns></returns>
        private Size GetNewSize(Size imgBoxSize)
        {
            //int sz = Math.Min(imgBoxSize.Height, imgBoxSize.Width);
            //return new Size(sz,sz);
            int newWidth = (int)(imgBoxSize.Height * AspectRatio);
            return new Size(newWidth, imgBoxSize.Height);
        }

        /// <summary>
        /// Обработка кадра в отдельном потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ProcessFrame(object sender, EventArgs arg)
        {
            try
            {

                if (_capture != null && _capture.Ptr != IntPtr.Zero)
                {

                    _capture.Read(_frame1); //чтение первого кадра
                    //_grayImage1=new Mat(_frame1.Size,_frame1.Depth,1);
                    CvInvoke.CvtColor(_frame1, _grayImage1, ColorConversion.Bgr2Gray); //перевод в черно-белый
                    _capture.Read(_frame2); //чтение второго кадра
                    CvInvoke.CvtColor(_frame2, _grayImage2, ColorConversion.Bgr2Gray);
                    CvInvoke.AbsDiff(_grayImage1, _grayImage2, _differenceImage); //разница между кадрами
                    
                    CvInvoke.Threshold(_differenceImage, _thresholdImage, _sensitivityValue, 255, ThresholdType.Binary);
                    CvInvoke.Blur(_thresholdImage, _thresholdImage,
                        new Size(BLUR_SIZE, BLUR_SIZE), new Point(-1, -1)); //размытие
                    CvInvoke.Threshold(_thresholdImage, _thresholdImage, _sensitivityValue, 255, ThresholdType.Binary);
                   
                    bool motionDetected = MotionDetector.DetectMotion(_thresholdImage, ref _frame1);
                    CvInvoke.Resize(_thresholdImage,_thresholdImage,ImageSize);
                    imageBox1.Image = _thresholdImage;
                    statusLabel.Invoke(new Action(() => statusLabel.Text = motionDetected ? RecordStr : NomotionStr));

                    Mat resizedFrame = new Mat();

                    CvInvoke.Resize(_frame1, resizedFrame, ImageSize); //изменение размера
                    capturedImageBox.Image = resizedFrame;
                    Mat frameToWrite = new Mat();
                    if (_writeSize != _frame1.Size)
                    {
                        CvInvoke.Resize(_frame1, frameToWrite, _writeSize);
                        _videoWriter.Write(frameToWrite);
                    }
                    else _videoWriter.Write(_frame1);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                _videoWriter.Dispose();
                _capture?.Dispose();
            }

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            
            ImageSize = GetNewSize(capturedImageBox.Size);
        }

        private void statusLabel_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void sensBar_ValueChanged(object sender, EventArgs e)
        {
            var trackBar = sender as TrackBar;
            int val = trackBar.Value;
            _sensitivityValue = MaxSens- val * _sensStep;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_capture != null)
                {
                    if (_captureInProgress)
                    {
                        //stop the capture
                        captureButton.Text = "Start";
                        _capture.Pause();
                        statusLabel.Text = IdleStr;
                       
                    }
                    else
                    {
                        //start the capture
                        ImageSize = GetNewSize(capturedImageBox.Size);
                        captureButton.Text = "Stop";
                        int codec = VideoWriter.Fourcc('D', 'I', 'V', '3');
                        string format = "video-{0:yyyy-MM-dd_hh-mm-ss}.avi";
                        string fileName = String.Format(format, DateTime.Now);
                        _videoWriter = new VideoWriter(fileName, -1, Fps, _writeSize, true);
                        if (!_videoWriter.IsOpened)
                        {
                            MessageBox.Show("ERROR: Failed to initialize video writing");
                        }
                        _capture.Start();
                    }

                    _captureInProgress = !_captureInProgress;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
                _videoWriter.Dispose();
                if (_capture != null)
                    _capture.Dispose();
            }
            
        }


    }
}
