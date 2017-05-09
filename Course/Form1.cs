using System;
using System.Drawing;
using System.Reflection.Emit;
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
        private VideoCapture _capture = null;
        private bool _captureInProgress; 
        private Mat _frame1,_frame2;
        private Mat _differenceImage;
        private Mat _thresholdImage;
        private Mat _grayImage1, _grayImage2;
        private const int SENSITIVITY_VALUE = 40;
        private const int BLUR_SIZE = 10;
        private const string IDLE_STR = "Ожидание";
        private const string RECORD_STR = "Запись";
        private const string NOMOTION_STR = "Нет движения";
        /// <summary>
        /// Соотношение сторон
        /// </summary>
        private const float AspectRatio = 4.0f / 3.0f;

        public Form1()
        {
            InitializeComponent();
            _capture = new VideoCapture(0);
            
            _capture.ImageGrabbed += ProcessFrame;
            _frame1=new Mat();
            _frame2=new Mat();
            _differenceImage =new Mat();
            _thresholdImage=new Mat();
            _grayImage1=new Mat();
            _grayImage2=new Mat();
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
            int newWidth =(int)(imgBoxSize.Height * AspectRatio);
            return new Size(newWidth,imgBoxSize.Height);
        }

        /// <summary>
        /// Обработка кадра в отдельном потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {

                _capture.Read(_frame1);
                //_grayImage1=new Mat(_frame1.Size,_frame1.Depth,1);
                CvInvoke.CvtColor(_frame1,_grayImage1,ColorConversion.Bgr2Gray);
                _capture.Read(_frame2);
                CvInvoke.CvtColor(_frame2,_grayImage2,ColorConversion.Bgr2Gray);
                CvInvoke.AbsDiff(_grayImage1,_grayImage2,_differenceImage);
                CvInvoke.Threshold(_differenceImage, _thresholdImage, SENSITIVITY_VALUE, 255, ThresholdType.Binary);
                CvInvoke.Blur(_thresholdImage,_thresholdImage,
                    new Size(BLUR_SIZE,BLUR_SIZE),new Point(-1,-1));
                CvInvoke.Threshold(_thresholdImage, _thresholdImage, SENSITIVITY_VALUE, 255, ThresholdType.Binary);
                bool motionDetected = MotionDetector.DetectMotion(_thresholdImage, ref _frame1);
                statusLabel.Invoke(new Action(() => statusLabel.Text = motionDetected ? RECORD_STR : NOMOTION_STR));
                Mat resizedFrame = new Mat();
                CvInvoke.Resize(_frame1,resizedFrame,GetNewSize(capturedImageBox.Size));
                capturedImageBox.Image = resizedFrame;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Text = "Start";
                    _capture.Pause();
                    statusLabel.Text = IDLE_STR;
                }
                else
                {
                    //start the capture
                    captureButton.Text = "Stop";
                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
               
                    
            }

        }

        
    }
}
