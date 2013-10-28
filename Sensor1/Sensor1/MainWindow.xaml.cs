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
using Sensor1;

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Server TCPServer;
        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        double left = 0;
        double top = 0;
        double width;
        double height;
        public MainWindow()
        {
            InitializeComponent();
            width = MainCanvas.ActualWidth;
            height = MainCanvas.ActualHeight;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TCPServer = new Server();
            TCPServer.ServerEvent += TCPServer_ServerEvent;

            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
        }

        void TCPServer_ServerEvent(string s)
        {
            //MessageBox.Show(s);
            if (s.Equals("blue")) changeColor(Colors.Blue);
            if (s.Equals("red")) changeColor(Colors.Red);
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




            //var parameters = new TransformSmoothParameters
            //{
            //    Smoothing = 0.3f,
            //    Correction = 0.0f,
            //    Prediction = 0.0f,
            //    JitterRadius = 1.0f,
            //    MaxDeviationRadius = 0.5f
            //};
            //sensor.SkeletonStream.Enable(parameters);

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

            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e);

            if (first == null)
            {
                return;
            }



            //set scaled position
            ScalePosition(headImage, first.Joints[JointType.Head]);
            ScalePosition(leftEllipse, first.Joints[JointType.HandLeft]);
            ScalePosition(rightEllipse, first.Joints[JointType.HandRight]);

            GetCameraPoint(first, e);

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
                CameraPosition(headImage, headColorPoint);
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

            double newLeft = MainCanvas.ActualWidth - point.X;
            
            Canvas.SetLeft(element, newLeft - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

            if (newLeft < left || newLeft > left + width)// || scaledJoint.Position.Y < top || scaledJoint.Position.Y > top + height)
            {
                //element.Visibility = Visibility.Hidden;
            }
            else
            {
                element.Visibility = Visibility.Visible;
            }

        }

        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            //Joint scaledJoint = joint.ScaleTo(1280, 720); 

            //convert & scale (.3 = means 1/3 of joint distance)
            //Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);
            Joint scaledJoint = joint.ScaleTo((int)MainCanvas.ActualWidth, (int)MainCanvas.ActualHeight);
            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }

        private void changeColor(Color s)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            Application.Current.Resources["br4"] = new SolidColorBrush(s);
                            rightEllipse.Fill = (SolidColorBrush)Application.Current.Resources["br4"];
                            leftEllipse.Fill = (SolidColorBrush)Application.Current.Resources["br4"];
                        }
                        ));
            }
        }

        private void AdjustColorViewer(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            left = HorizontalStartSlider.Value * 0.01 * MainCanvas.ActualWidth;
            width = (HorizontalEndSlider.Value - HorizontalStartSlider.Value) * 0.01 * MainCanvas.ActualWidth;
            Canvas.SetLeft(positioningRectangle, left);
            positioningRectangle.Width = width;

            if (VerticalStartSlider == null || VerticalEndSlider == null)
                return;
            //top = VerticalStartSlider.Value * 0.01 * MainCanvas.ActualHeight;
            //height = (VerticalStartSlider.Value - VerticalStartSlider.Value) * 0.01 * MainCanvas.ActualHeight;
            //Canvas.SetTop(positioningRectangle, height);
            //positioningRectangle.Height = height;
        }
    }
}
