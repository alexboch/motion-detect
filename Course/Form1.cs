using System;
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
        private Mat _frame;
        
        public Form1()
        {
            InitializeComponent();
            _capture=new VideoCapture(0);

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
