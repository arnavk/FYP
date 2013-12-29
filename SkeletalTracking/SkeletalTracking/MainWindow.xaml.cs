// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.IO; 

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public struct ScreenPosition
        {
            public double row;
            public double column;
        }

        bool closing = false;
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        ScreenPosition[,] screenMap = new ScreenPosition [480, 640];
        double[] originalBackground = new double[640 * 480 * 4];
//        double[] checkerboardedBackground = new double[640 * 480 * 4];
        int frames = 0;
        bool mapping = false;
        bool calibrating = false;
        bool calibrationComplete = false;
        bool addingOriginal = true;
        bool subtractingCheckerboarded = true;
        Rectangle[] grid = new Rectangle[50];

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //screenMap = new double [(int) Math.Ceiling(MainCanvas.Height), (int) Math.Ceiling(MainCanvas.Width)];
            int i, j;
            for (i = 0; i < 480; i++)
            {
                for (j = 0; j < 640; j++)
                {
                    screenMap[i, j] = new ScreenPosition { column = -1, row = -1 };
                }
            }
            int count = 10;
            for (i = 0; i < 50; i++)
            {
                grid[i] = new Rectangle
                {
                    Fill = Brushes.Blue,
                    Width = MainCanvas.ActualWidth / 10,
                    Height = MainCanvas.ActualHeight / 10,
                };
                MainCanvas.Children.Add(grid[i]);
                Canvas.SetLeft(grid[i], (((i * 20) + ((i / 5) % 2 * 10)) % 100) * MainCanvas.ActualWidth / 100);
                Canvas.SetTop(grid[i], ((int)(i / 5) * 10 * MainCanvas.ActualHeight / 100));
                grid[i].Visibility = Visibility.Hidden;
            }

            rightEllipse.Visibility = Visibility.Hidden;
            leftEllipse.Visibility = Visibility.Hidden;

            imageTest.Visibility = Visibility.Hidden;
            Canvas.SetTop(imageTest, 0);
            Canvas.SetLeft(imageTest, 0);

            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);

        }

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;

            StopKinect(old);

            KinectSensor sensor = (KinectSensor)e.NewValue;

            if (sensor == null)
            {
                return;
            }

            


            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);

            sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); 
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }
            
            //identifyColour(e);

            if (calibrating)
            {
                frames++;
                calibrate(e);
                return;
            }


            //Get a skeleton
            Skeleton first =  GetFirstSkeleton(e);

            if (first == null)
            {
                return; 
            }



            //set scaled position
            //ScalePosition(headImage, first.Joints[JointType.Head]);
            
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);

            GetCameraPoint(first, e); 

        }

        //returns true if the rectangle has to be shifted and no calculations shoud be done
        bool updateFrame()
        {
            frames++;
            if (frames % 11 == 0)
            {
                frames = 0;
                double left = Canvas.GetLeft(rectangle) + rectangle.Width;
                double top = Canvas.GetTop(rectangle);
                if (left + rectangle.Width > MainCanvas.ActualWidth)
                {
                    left = 0;
                    top += rectangle.Height;
                    if (top >= MainCanvas.ActualHeight)
                    {
                        mapping = false;
                        rectangle.Visibility = Visibility.Hidden;
                    }
                }
                Canvas.SetLeft(rectangle, left);
                Canvas.SetTop(rectangle, top);
                return false;
            }
            return true;
        }

        //set frames to 0 before invoking this method
        private bool countFrames(int count)
        {
            frames++;
            if (frames >= count)
            {
                frames = 0;
                return true;
            }
            return false;
        }

        private void calibrate(AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null || !calibrating)
                {
                    return;
                }

                if (frames < 30)
                    return;

                

                if (addingOriginal)
                {
                    if (frames < 60)
                    {
                        byte[] pixels = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(pixels);
                        updateDifferenceMatrix(pixels, true);//adding the next 10 frames
                        if (frames == 45)
                        {
                            //updateDifferenceMatrix(pixels, true);//adding the next 10 frames
                            saveImage(pixels, "nogrid");
                        }
                    }
                    else
                    {
                        addingOriginal = false;
                        updateGridVisibility(Visibility.Visible); // show the grid
                    }
                    return;
                }

                if (frames < 90)// drop 30 frames
                    return;

                if (subtractingCheckerboarded)
                {
                    if (frames < 120)
                    {
                        byte[] pixels = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(pixels);
                        updateDifferenceMatrix(pixels, false);
                        if (frames == 115)
                        {
                            saveImage(pixels, "grid");
                            //updateDifferenceMatrix(pixels, false);// subtract the next 10 frames
                        }
                    }
                    else
                    {
                        subtractingCheckerboarded = false;
                        updateGridVisibility(Visibility.Hidden); // hide the grid
                    }
                    return;
                }



                Console.WriteLine("Width yo: " + colorFrame.Width);
                // do the calcuation magic
                calculateMappingMatrix();
                calibrating = false;
                calibrationComplete = true;

            }

        }

        private void updateGridVisibility(Visibility visibility)
        {
            for (int i = 0; i < grid.Length; i++)
                grid[i].Visibility = visibility;
        }


        private void updateDifferenceMatrix(byte[] pixels, bool add)
        {
            int multiplier = -1;
            if (add)
                multiplier = 1;
            int i = 0;
            double size;
            for (i = 0; i < pixels.Length; i++)
            {
                originalBackground[i] += multiplier * pixels[i];
            }
            //    if (i % 4 == 0 && i > 0)
            //    {
            //        size = originalBackground[i - 4] * originalBackground[i - 4] + originalBackground[i - 3] * originalBackground[i - 3] + originalBackground[i - 2] * originalBackground[i - 2] + originalBackground[i - 1] * originalBackground[i - 1];
            //        size = Math.Sqrt(size);
            //        originalBackground[i - 4] /= size;
            //        originalBackground[i - 3] /= size;
            //        originalBackground[i - 2] /= size;
            //        originalBackground[i - 1] /= size;
            //    }
            //}
            //size = Math.Sqrt(originalBackground[i - 4] * originalBackground[i - 4] + originalBackground[i - 3] * originalBackground[i - 3] + originalBackground[i - 2] * originalBackground[i - 2] + originalBackground[i - 1] * originalBackground[i - 1]);
            //originalBackground[i - 4] /= size;
            //originalBackground[i - 3] /= size;
            //originalBackground[i - 2] /= size;
            //originalBackground[i - 1] /= size;
        }
        private void calculateMappingMatrix()
        {
            //Console.WriteLine("Matrix ho!");
            int i;
            byte val;
            byte[] image = new byte[originalBackground.Length];
            for (i = 0; i < originalBackground.Length; i++)
            {
                image[i] = (byte)(Math.Abs(originalBackground[i]/30));
            }
            for (i = 0; i < image.Length; i += 4)
            {
                if (Math.Sqrt(image[i] * image[i] + image[i + 1] * image[i + 1] + image[i + 2] * image[i + 2] + image[i + 3] * image[i + 3]) > 15)
                    val = 255;
                else
                    val = 0;
                image[i] = val;
                image[i + 1] = val;
                image[i + 2] = val;
                image[i + 3] = val;
            }

            //for (i = 0; i < image.Length; i++)
            //{
            //    if (image[i] > 10)
            //        image[i] = 255;
            //    else
            //        image[i] = 0;
            //}
            //for (int i = 0; i < originalBackground.Length; i++)
            //{
                //originalBackground[i] = 0; //i % 255;
            //}
            int stride = 640 * 4;
            Image image1 = new Image();

            imageTest.Visibility = Visibility.Visible;
            imageTest.Source =
                 BitmapSource.Create(640, 480,
                32, 32, PixelFormats.Bgr32, null, image, stride);
            saveImage(image, "difference");  
            
        }
        private void saveImage(byte[] array, String name)
        {
            Image image = new Image();
            image.Source = BitmapSource.Create(640, 480,
                96, 96, PixelFormats.Bgr32, null, array, 640*4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
            using (FileStream stream = new FileStream(name + ".png", FileMode.Create))
                encoder.Save(stream);
        }
        private void identifyColour(AllFramesReadyEventArgs e)
        {
            //Console.WriteLine("\nNew Frame");
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    return;
                }
                if (!mapping || !updateFrame())
                {
                    return;
                }

                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
                //Console.WriteLine(colorFrame.PixelDataLength);
                int stride = colorFrame.Width * 4;
                int error = 10;
                double top = Canvas.GetTop(rectangle), left = Canvas.GetLeft(rectangle), width = rectangle.Width, height = rectangle.Height;

                for (int i = 0; i < colorFrame.PixelDataLength; i += 4)
                {
                    //Console.WriteLine("(" + pixels[i] + "," + pixels[i + 1] + "," + pixels[i + 2] + ")");
                    if (Math.Abs(pixels[i] - 137) <= error && Math.Abs(pixels[i + 1] - 162) <= error && Math.Abs(pixels[i + 2] - 148) <= error)
                    {
                        int count = i / 4;
                        int r = (count / colorFrame.Width), c = (count % colorFrame.Width);
                        //Console.WriteLine("at (" + (count / colorFrame.Width) + "," + (count % colorFrame.Width) + ")");

                        ScreenPosition mappedPosition = new ScreenPosition { row = top + (height / 2), column = left + (width / 2) };
                        screenMap[r, c] = mappedPosition;

                        //int j, k;
                                                //for ( j = top; j < top + height; j ++ )
                        //{
                        //    for ( k = left; k < left + width; k++ )
                        //    {

                    }
                }




            }
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return;
                }
                

                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftColorPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightColorPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);


                //Set location
                
                CameraPosition(leftEllipse, leftColorPoint);
                CameraPosition(rightEllipse, rightColorPoint);
            }        
        }

        


        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null; 
                }

                
                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                         where s.TrackingState == SkeletonTrackingState.Tracked
                                         select s).FirstOrDefault();

                return first;

            }
        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            //Canvas.SetLeft(element, point.X - element.Width / 2);
            //Canvas.SetTop(element, point.Y - element.Height / 2);

            if (element.Visibility == Visibility.Hidden)
            {
                element.Visibility = Visibility.Visible;
                Console.WriteLine("Made shizz visible");
            }
            if (screenMap[point.Y, point.X].column == -1)
            {
                element.Visibility = Visibility.Hidden;
                return;
            }

            Canvas.SetLeft(element, screenMap[point.Y, point.X].column);
            Canvas.SetTop(element, screenMap[point.Y, point.X].row);
        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 
            
            //convert & scale (.3 = means 1/3 of joint distance)
            //Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            //Canvas.SetLeft(element, scaledJoint.Position.X);
            //Canvas.SetTop(element, scaledJoint.Position.Y); 
            
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true; 
            StopKinect(kinectSensorChooser1.Kinect); 
        }

        private void MouseDoubleClickOccured(object sender, MouseButtonEventArgs e)
        {
            startButton.Visibility = Visibility.Hidden;
            rectangle.Visibility = Visibility.Hidden;
            //rectangle.Height = MainCanvas.ActualHeight / 10;
            //rectangle.Width = MainCanvas.ActualWidth / 10;
            //Canvas.SetLeft(rectangle, 0);
            //Canvas.SetTop(rectangle, 0);
            mapping = true;
            kinectColorViewer1.Visibility = Visibility.Hidden;
            calibrating = true;
            calibrationComplete = false;
            
        }
    }
}
