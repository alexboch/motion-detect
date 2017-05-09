using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace Course
{
    class MotionDetector
    {
        public static bool DetectMotion(Mat thresholdImage, ref Mat cameraFeed)
        {
            
            Mat temp=new Mat();
            thresholdImage.CopyTo(temp);
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(temp, contours,null,RetrType.External,
                ChainApproxMethod.ChainApproxSimple );
            
            return contours.Size > 0;
        }
    }
}
