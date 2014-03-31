using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;


namespace EmguCVTest1
{
    class Program
    {
        static void Main(string[] args)
        {
            // get the chess board width
            Console.WriteLine("Chess board width");
            Int32 width = Convert.ToInt32(Console.ReadLine());

            // get the chess board height
            Console.WriteLine("Chess board height");
            Int32 height = Convert.ToInt32(Console.ReadLine());

            // define the chess board size
            Size patternSize = new Size(width, height);

            // get the input gray scale image
            Image<Gray, Byte> InputImage = new Image<Gray, Byte>("difference.png");

            // show the input image
            CvInvoke.cvShowImage("gray scale input image", InputImage.Ptr);

            // create a buffer to store the chess board corner locations
            PointF[] corners = new PointF[] { };

            // find the chess board corners
            corners = CameraCalibration.FindChessboardCorners(InputImage, patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS);
            if (corners == null)
            {
                Console.WriteLine("Nothing found");
                CvInvoke.cvWaitKey(0);
                return;
            }
            for (int i = 0; i < corners.Length; i++)
            {
                Console.WriteLine("(" + corners[i].X + ", " + corners[i].Y + ")");
            }

            // draw the chess board corner markers on the image
            CameraCalibration.DrawChessboardCorners(InputImage, patternSize, corners);

            // show the result
            CvInvoke.cvShowImage("Result", InputImage.Ptr);

            // pause
            CvInvoke.cvWaitKey(0);
        }
    }
}
