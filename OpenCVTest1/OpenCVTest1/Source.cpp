// Video Image PSNR and SSIM
#include <iostream> // for standard I/O
#include <string>   // for strings
#include <iomanip>  // for controlling float print precision
#include <sstream>  // string to number conversion

#include <opencv2/imgproc/imgproc.hpp>  // Gaussian Blur
#include <opencv2/core/core.hpp>        // Basic OpenCV structures (cv::Mat, Scalar)
#include <opencv2/highgui/highgui.hpp>  // OpenCV window I/O
#include <opencv2/calib3d/calib3d.hpp>

using namespace std;
using namespace cv;

int main( int argc, char** argv )
{
    if( argc != 2)
    {
     cout <<" Usage: display_image ImageToLoadAndDisplay" << endl;
     return -1;
    }
	Size patternsize(11, 11); //number of centers
    Mat image;
	vector<Point2f> centers;
    image = imread(argv[1], IMREAD_COLOR); // Read the file

    if(! image.data ) // Check for invalid input
    {
        cout << "Could not open or find the image" << std::endl ;
        return -1;
    }
	int i = 11, flag = 0;
	while (!flag && i > 1)
	{
		Size patternsize( i, i);
		flag = findChessboardCorners(image,patternsize,centers);
		cout<<flag<<endl;
		i = i - 1;
	}
	if (flag)
	{
		Size foundPatternsize( i + 1, i + 1);
	    bool patternfound = findChessboardCorners(image,foundPatternsize,centers);

		//cout<<patternfound<<endl;
		drawChessboardCorners(image, foundPatternsize, Mat(centers), patternfound);
	}
    //cvNamedWindow("window");
    /*while(1){
        imshow("window",image);
        cvWaitKey(33);
    }*/

	//namedWindow( "Display window", WINDOW_AUTOSIZE ); // Create a window for display.
    imshow( "Display window", image );                // Show our image inside it.

    waitKey(0); // Wait for a keystroke in the window
    return 0;
}