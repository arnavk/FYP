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
//using Coding4Fun.Kinect.Wpf;
using System.IO; 
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;

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

        public struct Square
        {
            public double startX;
            public double startY;
            public double endX;
            public double endY;
            //public PointF topLeft;
            //public PointF topRight;
            //public PointF bottomLeft;
            //public PointF bottomRight;
        }

        bool closing = false;
        const int skeletonCount = 6; 
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        ScreenPosition[,] screenMap = new ScreenPosition [480, 640];

        Square[,] wallSquares;
        
        double[] originalBackground = new double[640 * 480 * 4];
//        double[] checkerboardedBackground = new double[640 * 480 * 4];
        int frames = 0;
        bool mapping = false;
        bool calibrating = false;
        bool calibrationComplete = false;
        bool addingOriginal = true;
        bool subtractingCheckerboarded = true;
        System.Windows.Shapes.Rectangle[,] grid;
        int gridSize = 10;
        System.Windows.Shapes.Line rightArm;
        System.Windows.Shapes.Line leftArm;

        System.Windows.Point lastKnownPoint;
        System.Windows.Media.Color currentColor;
        Server TCPServer;
        Boolean isPainting;
        Boolean initialized;

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
            grid = new System.Windows.Shapes.Rectangle[gridSize, gridSize];
            for (i = 0; i < gridSize; i++)
            {
                for (j = 0; j < gridSize; j++)
                {
                    grid[i, j] = new System.Windows.Shapes.Rectangle
                    {
                        Fill = System.Windows.Media.Brushes.Blue,
                        Width = MainCanvas.ActualWidth / gridSize,
                        Height = MainCanvas.ActualHeight / gridSize,
                    };
                    MainCanvas.Children.Add(grid[i, j]);
                    Canvas.SetLeft(grid[i, j], j * (gridSize / 100.0) * MainCanvas.ActualWidth);
                    Canvas.SetTop(grid[i, j], i * (gridSize / 100.0) * MainCanvas.ActualHeight);
                    // Console.WriteLine("(" + Canvas.GetLeft(grid[i, j]) + ", " + Canvas.GetTop(grid[i, j]) + ")");
                    grid[i, j].Visibility = Visibility.Hidden;
                }
            }
            //for (i = 0; i < 50; i++)
            //{
            //    grid[i] = new System.Windows.Shapes.Rectangle
            //    {
            //        Fill = System.Windows.Media.Brushes.Blue,
            //        Width = MainCanvas.ActualWidth / 10,
            //        Height = MainCanvas.ActualHeight / 10,
            //    };
            //    MainCanvas.Children.Add(grid[i]);
            //    Canvas.SetLeft(grid[i], (((i * 20) + ((1 - (i / 5) % 2) * 10)) % 100) * MainCanvas.ActualWidth / 100);
            //    Canvas.SetTop(grid[i], ((int)(i / 5) * 10 * MainCanvas.ActualHeight / 100));
            //    grid[i].Visibility = Visibility.Hidden;
            //}

            rightEllipse.Visibility = Visibility.Hidden;
            leftEllipse.Visibility = Visibility.Hidden;
            centerEllipse.Visibility = Visibility.Hidden;
            imageTest.Visibility = Visibility.Hidden;
            Canvas.SetTop(imageTest, 0);
            Canvas.SetLeft(imageTest, 0);

            rightArm = new System.Windows.Shapes.Line();
            rightArm.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            rightArm.X1 = 0;
            rightArm.X2 = 0;
            rightArm.Y1 = 0;
            rightArm.Y2 = 00;
            rightArm.HorizontalAlignment = HorizontalAlignment.Left;
            rightArm.VerticalAlignment = VerticalAlignment.Center;
            rightArm.StrokeThickness = 20;
            MainCanvas.Children.Add(rightArm);
            rightArm.Visibility = Visibility.Hidden;

            leftArm = new System.Windows.Shapes.Line();
            leftArm.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            leftArm.X1 = 0;
            leftArm.X2 = 0;
            leftArm.Y1 = 0;
            leftArm.Y2 = 00;
            leftArm.HorizontalAlignment = HorizontalAlignment.Center;
            leftArm.VerticalAlignment = VerticalAlignment.Center;
            leftArm.StrokeThickness = 20;
            MainCanvas.Children.Add(leftArm);
            leftArm.Visibility = Visibility.Hidden;

            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            clothImage.Visibility = Visibility.Hidden;
            inkCanvas.Visibility = Visibility.Hidden;
            currentColor = Colors.CadetBlue;
            isPainting = false;
            initialized = false;
            TCPServer = new Server();
            TCPServer.ServerEvent += TCPServer_ServerEvent;

        }

        void TCPServer_ServerEvent(string s)
        {
            //MessageBox.Show(s);
            Console.WriteLine(s);
            if (s.StartsWith("/stop"))
                isPainting = false;
            else
            {
                string colorHex = "#" + s.Substring(7);
                Console.WriteLine("YOLO " + colorHex);
                isPainting = true;
                System.Drawing.Color c = System.Drawing.ColorTranslator.FromHtml(colorHex);
                currentColor = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            }

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
                Smoothing = 0.75f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f
            };
            sensor.SkeletonStream.Enable(parameters);
            //sensor.ElevationAngle = 5;
            //sensor.SkeletonStream.Enable();

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); 
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            try
            {
                sensor.Start();
                sensor.ElevationAngle = 5;
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

                        //chessboard.Visibility = Visibility.Visible;

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
                        //chessboard.Visibility = Visibility.Hidden;
                        updateGridVisibility(Visibility.Hidden); // hide the grid
                    }
                    return;
                }



                //Console.WriteLine("Width yo: " + colorFrame.Width);
                // do the calcuation magic
                calculateMappingMatrix();
                calibrating = false;
                calibrationComplete = true;

            }

        }

        private void updateGridVisibility(Visibility visibility)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if ((i % 2 == 0 && j % 2 == 1) || (i % 2 == 1 && j % 2 == 0))
                        grid[i, j].Visibility = visibility;
                }
            }
        }


        private void updateDifferenceMatrix(byte[] pixels, bool add)
        {
            int multiplier = -1;
            if (add)
                multiplier = 1;
            int i = 0;
            for (i = 0; i < pixels.Length; i++)
            {
                originalBackground[i] += multiplier * pixels[i];
            }
        }
        private void calculateMappingMatrix()
        {
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
                    val = 0;
                else
                    val = 255;
                image[i] = val;
                image[i + 1] = val;
                image[i + 2] = val;
                image[i + 3] = val;
            }
            int stride = 640 * 4;
            //byte[] flippedImage = new byte[image.Length];

            //int r, c;
            //for (r = 0; r < 480; r++)
            //{
            //    for (c = 639; c >= 0; c--)
            //    {
            //        int crev = 639 - c;
            //        int fi = r * 640 + 
            //        i+=4;
            //    }
            //}
            
            imageTest.Visibility = Visibility.Visible;
            imageTest.Source =
                 BitmapSource.Create(640, 480,
                32, 32, PixelFormats.Bgr32, null, image, stride);

            //imageTest.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            //ScaleTransform flipTrans = new ScaleTransform();
            //flipTrans.ScaleX = -1;
            ////flipTrans.ScaleY = -1;
            //imageTest.RenderTransform = flipTrans;
            
            saveImage(imageTest, "difference");
            //flipImage();

            //saveImage(image, "difference2");
            PointF[] corners = getGridCorners(gridSize - 1);
            if (corners == null)
            {
                Console.WriteLine("No corners found, try again."); // TODO - Make this an alert
                return;
                
            }
            int r = 0, c = 0, size = gridSize - 1;
            wallSquares = new Square[size - 1, size - 1];
            for (i = 0; i < size - 1; i++)
            {
                for (int j = 0; j < size - 1; j++)
                {
                    wallSquares[i, j] = new Square { startX = 0, startY = 0, endX = 0, endY = 0 };
                }
            }
            //if (corners[corners.Length - 1].X < corners[0].X)
            //{
            //    for (i = 0; i < corners.Length / 2; i++) ;
            //    {
            //        PointF temp = corners[i];
            //        corners[i] = corners[corners.Length - 1 - i];
            //        corners[corners.Length - 1 - i] = temp;
            //    }

            //}


            for (i = 0; i < corners.Length; i++)
            {
                //Console.WriteLine("(" + corners[i].X + ", " + corners[i].Y + ")");
                corners[i].X = 640 - corners[i].X;
            }
            Console.WriteLine("transformed");
            for (i = 0; i < size; i++)
            {
                for (int j = 0; j < size/2; j++)
                {
                    int c1 = i * size + j, c2 = i * size + (size - 1 - j);
                    //Console.WriteLine(c1 + " and " + c2);
                    PointF temp = corners[c1];
                    corners[c1] = corners[c2];
                    corners[c2] = temp;
                }
            }

            for (i = 0; i < corners.Length; i++)
            {
                r = i / size;
                c = i % size;
                //c = size - 2 - c;
                //Console.WriteLine("(" + corners[i].X + ", " + corners[i].Y + ")");
                if (r != size - 1 && c != size - 1)
                {
                    wallSquares[r, c].startX = corners[i].X;
                    wallSquares[r, c].startY = corners[i].Y;
                }
                if (r != 0 && c != 0)
                {
                    wallSquares[r - 1, c - 1].endX = corners[i].X;
                    wallSquares[r - 1, c - 1].endY = corners[i].Y;
                }
                //if (r != size - 1 && c >= 0)
                //{
                //    wallSquares[r, c].startX = corners[i].X;
                //    wallSquares[r, c].startY = corners[i].Y;
                //}
                //if (r != 0 && c != size - 2)
                //{
                //    wallSquares[r - 1, c + 1].endX = corners[i].X;
                //    wallSquares[r - 1, c + 1].endY = corners[i].Y;
                //}
            }

            imageTest.Visibility = Visibility.Hidden;
            inkCanvas.Visibility = Visibility.Visible;
            //testPositioning();
        }

        private void testPositioning()
        {
            if (leftEllipse.Visibility == Visibility.Hidden)
            {
                leftEllipse.Visibility = Visibility.Visible;
            }

            for (int i = 210; i < 365; i++)
            {
                for (int j = 145; j < 350; j++)
                {
                    System.Windows.Point p1 = new System.Windows.Point(j, i);
                    System.Windows.Point p2 = convertToScreenPoint(p1);
                    //Console.WriteLine("(" + p1.X + ", " + p1.Y + ")\t-- >\t(" + p2.X + ", " + p2.Y + ")");
                    Canvas.SetLeft(leftEllipse, p2.X);
                    Canvas.SetTop(leftEllipse, p2.Y);
                }
            }

            


            
        }
        private void flipImage()
        {
            System.Drawing.Image i = System.Drawing.Image.FromFile("difference.png");
            i.RotateFlip(RotateFlipType.RotateNoneFlipX);
            i.Save("difference2.png");
        }

        private System.Windows.Point convertKinectPointToScreenPoint(System.Windows.Point wallPoint)
        {
            return convertToScreenPoint(new System.Windows.Point(640 - wallPoint.X, wallPoint.Y));
        }

        private System.Windows.Point convertToScreenPoint(System.Windows.Point wallPoint)
        {
            int i, j;
            System.Windows.Point screenPoint = new System.Windows.Point(0, 0);
            for (i = 0; i < gridSize - 2; i++)
            {
                for (j = 0; j < gridSize - 2; j++)
                {
                    if (wallPoint.X > wallSquares[i, j].startX && wallPoint.X <= wallSquares[i, j].endX && wallPoint.Y > wallSquares[i, j].startY && wallPoint.Y <= wallSquares[i, j].endY)
                    {
                        double px = (wallPoint.X - wallSquares[i, j].startX) / (wallSquares[i, j].endX - wallSquares[i, j].startX);
                        double py = (wallPoint.Y - wallSquares[i, j].startY) / (wallSquares[i, j].endY - wallSquares[i, j].startY);
                        screenPoint.X = (px * MainCanvas.ActualWidth / gridSize) + Canvas.GetLeft(grid[i + 1, j + 1]);
                        screenPoint.Y = (py * MainCanvas.ActualHeight/ gridSize) + Canvas.GetTop(grid[i + 1, j + 1]);

                        //if (px > 1)
                        //{
                        //    Console.WriteLine("Start :\t(" + wallSquares[i, j].startX + ", " + wallSquares[i, j].startY + ")");
                        //    Console.WriteLine("End : \t(" + wallSquares[i, j].endX + ", " + wallSquares[i, j].endY + ")");
                        //    Console.WriteLine("Point :\t(" + wallPoint.X + ", " + wallPoint.Y + ")");
                        //    Console.WriteLine( px + " = " + (wallPoint.X - wallSquares[i, j].startX) + "/" + (wallSquares[i, j].endY - wallSquares[i, j].startY));
                        //}

                        

                        //screenPoint.X = Canvas.GetLeft(grid[i + 1, j + 1]);
                        //screenPoint.Y = Canvas.GetTop(grid[i + 1, j + 1]);
                        return screenPoint;
                    }
                }
            }
            //Console.WriteLine("Screen Point: "+screenPoint.ToString());
            return screenPoint;
        }
        private void saveImage(byte[] array, String name)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = BitmapSource.Create(640, 480,
                96, 96, PixelFormats.Bgr32, null, array, 640*4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
            using (FileStream stream = new FileStream(name + ".png", FileMode.Create))
                encoder.Save(stream);
        }
        private void saveImage(System.Windows.Controls.Image image, String name)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
            using (FileStream stream = new FileStream(name + ".png", FileMode.Create))
                encoder.Save(stream);
        }

        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {

            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                //Console.WriteLine("Loggin, yo!");
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
                DepthImagePoint leftHandDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //left elbow
                DepthImagePoint leftElbowDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowLeft].Position);
                
                //left shoulder
                DepthImagePoint leftShoulderDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderLeft].Position);
                
                //right hand
                DepthImagePoint rightHandDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);
                //right elbow
                DepthImagePoint rightElbowDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ElbowRight].Position);
                //right shoulder
                DepthImagePoint rightShoulderDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderRight].Position);

                //right hip
                DepthImagePoint rightHipDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipRight].Position);

                //right hip
                DepthImagePoint leftHipDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HipLeft].Position);

                //mid shoulder
                DepthImagePoint midShoulderDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.ShoulderCenter].Position);

                


                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftHandColorPoint =
                    depth.MapToColorImagePoint(leftHandDepthPoint.X, leftHandDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30); 
                //left elbow
                ColorImagePoint leftElbowColorPoint =
                    depth.MapToColorImagePoint(leftElbowDepthPoint.X, leftElbowDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                //left shoulder
                ColorImagePoint leftShoulderColorPoint =
                    depth.MapToColorImagePoint(leftShoulderDepthPoint.X, leftShoulderDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                //right hand
                ColorImagePoint rightHandColorPoint =
                    depth.MapToColorImagePoint(rightHandDepthPoint.X, rightHandDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                
                //right elbow
                ColorImagePoint rightElbowColorPoint =
                    depth.MapToColorImagePoint(rightElbowDepthPoint.X, rightElbowDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                
                //right shoulder
                ColorImagePoint rightShoulderColorPoint =
                    depth.MapToColorImagePoint(rightShoulderDepthPoint.X, rightShoulderDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);


                ColorImagePoint rightHipColorPoint =
                    depth.MapToColorImagePoint(rightHipDepthPoint.X, rightHipDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint leftHipColorPoint =
                    depth.MapToColorImagePoint(leftHipDepthPoint.X, leftHipDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);

                ColorImagePoint midShoulderColorPoint=
                   depth.MapToColorImagePoint(midShoulderDepthPoint.X, midShoulderDepthPoint.Y,
                   ColorImageFormat.RgbResolution640x480Fps30);


                //Set location
                DrawBone(leftArm, rightHandColorPoint, rightElbowColorPoint);
                DrawBone(rightArm, rightElbowColorPoint, rightShoulderColorPoint);

                CameraPosition(leftEllipse, rightHandColorPoint);
                CameraPosition(rightEllipse, rightElbowColorPoint);
                CameraPosition(centerEllipse, rightShoulderColorPoint);

                Paint(convertToScreenPoint(new System.Windows.Point (640 - rightHandColorPoint.X, rightHandColorPoint.Y)));
                showClothing(rightShoulderColorPoint, headColorPoint, leftShoulderColorPoint);
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

        private void DrawBone(System.Windows.Shapes.Line line, ColorImagePoint point1, ColorImagePoint point2)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            //Canvas.SetLeft(element, point.X - element.Width / 2);
            //Canvas.SetTop(element, point.Y - element.Height / 2);

                if (line.Visibility == Visibility.Hidden)
            {
                line.Visibility = Visibility.Visible;
                Console.WriteLine("Made shizz visible");
            }


            System.Windows.Point screenPoint1 = convertToScreenPoint(new System.Windows.Point(640 - point1.X, point1.Y));
            System.Windows.Point screenPoint2 = convertToScreenPoint(new System.Windows.Point(640 - point2.X, point2.Y));
            //Console.WriteLine("(" + point.X + ", " + point.Y + ")\t-- >\t(" + screenPoint.X + ", " + screenPoint.Y + ")");
            line.X1 = screenPoint1.X;
            line.X2 = screenPoint2.X;
            line.Y1 = screenPoint1.Y;
            line.Y2 = screenPoint2.Y;
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


            System.Windows.Point screenPoint = convertToScreenPoint(new System.Windows.Point (640 - point.X, point.Y));
            Console.WriteLine("(" + point.X + ", " + point.Y + ")\t-- >\t("+ screenPoint.X + ", " + screenPoint.Y + ")");
            Canvas.SetLeft(element, screenPoint.X - element.Width / 2);
            Canvas.SetTop(element, screenPoint.Y - element.Height / 2);
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

        private PointF[] getGridCorners(int numberOfCorners)
        {
            // get the chess board width
            //Console.WriteLine("Chess board width");
            //Int32 width = Convert.ToInt32(Console.ReadLine());

            // get the chess board height
            //Console.WriteLine("Chess board height");
            //Int32 height = Convert.ToInt32(Console.ReadLine());

            // define the chess board size
            System.Drawing.Size patternSize = new System.Drawing.Size(numberOfCorners, numberOfCorners);

            // get the input gray scale image
            //Image<Gray, Byte> InputImage1 = new Image<Gray, Byte>("difference.png");
            //InputImage1._Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL);
            //InputImage1.Save("difference2.png");
            Image<Gray, Byte> InputImage = new Image<Gray, Byte>("difference.png");
            // show the input image
            //CvInvoke.cvShowImage("gray scale input image", InputImage.Ptr);

            // create a buffer to store the chess board corner locations
            PointF[] corners = new PointF[] { };

            // find the chess board corners
            corners = CameraCalibration.FindChessboardCorners(InputImage, patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH | Emgu.CV.CvEnum.CALIB_CB_TYPE.FILTER_QUADS);
            
            
            //if (corners == null)
            //{
            //    Console.WriteLine("Nothing found");
            //    CvInvoke.cvWaitKey(0);
            //    return corners;
            //}
            //for (int i = 0; i < corners.Length; i++)
            //{
            //    Console.WriteLine("(" + corners[i].X + ", " + corners[i].Y + ")");
            //}

            // draw the chess board corner markers on the image
            //CameraCalibration.DrawChessboardCorners(InputImage, patternSize, corners);

            // show the result
            //CvInvoke.cvShowImage("Result", InputImage.Ptr);
            return corners;
            // pause
            //CvInvoke.cvWaitKey(0);
        }
        


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true; 
            StopKinect(kinectSensorChooser1.Kinect);
            //DetectCorners();
        }

        private void MouseDoubleClickOccured(object sender, MouseButtonEventArgs e)
        {
            startButton.Visibility = Visibility.Hidden;
            
            //rectangle.Height = MainCanvas.ActualHeight / 10;
            //rectangle.Width = MainCanvas.ActualWidth / 10;
            //Canvas.SetLeft(rectangle, 0);
            //Canvas.SetTop(rectangle, 0);
            mapping = true;
            kinectColorViewer1.Visibility = Visibility.Hidden;
            calibrating = true;
            calibrationComplete = false;
            
        }

        public void Paint(System.Windows.Point nextPoint)
        {
            if (nextPoint.X == 0 && nextPoint.Y == 0)
            {
                initialized = false;
                return;
            }
            if (!initialized)
            {
                initialized = true;
                lastKnownPoint = new System.Windows.Point();
                lastKnownPoint = nextPoint;
                Console.WriteLine("Replacing point");
                return;
            }
            if (!isPainting)
            {
                lastKnownPoint = nextPoint;
                return;
            }
            Line line = new Line();
            line.StrokeDashCap = PenLineCap.Round;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.Stroke = new SolidColorBrush(currentColor);
            line.StrokeThickness = 10;

            line.X1 = lastKnownPoint.X;
            line.Y1 = lastKnownPoint.Y;
            line.X2 = nextPoint.X;
            line.Y2 = nextPoint.Y;

            lastKnownPoint = nextPoint;

            inkCanvas.Children.Add(line);
        }

        private void showClothing(ColorImagePoint rightShoulder, ColorImagePoint midShoulder, ColorImagePoint leftShoulder)
        {
            clothImage.Source = new BitmapImage(new Uri(@"shirt.png", UriKind.Relative));
            System.Windows.Point right = convertKinectPointToScreenPoint(new System.Windows.Point(rightShoulder.X, rightShoulder.Y));
            System.Windows.Point mid = convertKinectPointToScreenPoint(new System.Windows.Point(midShoulder.X, midShoulder.Y));
            System.Windows.Point left = convertKinectPointToScreenPoint(new System.Windows.Point(leftShoulder.X, leftShoulder.Y));
            if ((right.X == 0 && right.Y == 0) || (right.X == 0 && right.Y == 0))
            {
                clothImage.Visibility = Visibility.Hidden;
                Console.WriteLine("No image");
                return;
            }
            clothImage.Visibility = Visibility.Visible;
            clothImage.Width = Math.Abs(right.X - left.X) * 1.5;
            clothImage.Height = clothImage.Width * 1.4;
            Canvas.SetLeft(clothImage, (right.X + left.X)/2.0 - (clothImage.Width/2.0));//Math.Min(right.X, left.X) - clothImage.Width * 0.25);
            if (mid.Y == 0)
            {
                Canvas.SetTop(clothImage, right.Y - clothImage.Height * 0.2); // replace with shoulder centre
                clothImage.Visibility = Visibility.Hidden;
            }
            else
                Canvas.SetTop(clothImage, mid.Y);//(mid.Y + right.Y)/2.0); 
        }
    }
}
