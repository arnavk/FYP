using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CanvasTest;
using System.Drawing;

namespace CanvasTest
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

        System.Windows.Point lastKnownPoint;
        System.Windows.Media.Color currentColor;
        Server TCPServer;
        Boolean isPainting;
        Boolean initialized;

        public void Paint(System.Windows.Point nextPoint)
        {
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

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(this);
                Paint(p);
            }
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            currentColor = Colors.CadetBlue;
            isPainting = false;
            initialized = false;
            TCPServer = new Server();
            TCPServer.ServerEvent += TCPServer_ServerEvent;
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            Paint(e.GetPosition(this));
        }

        void TCPServer_ServerEvent(string s)
        {
            //MessageBox.Show(s);
            Console.WriteLine(s);
            if(s.StartsWith("/stop"))
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
    }
}
