using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelloEmgu1._5
{
    public partial class Form1 : Form
    {

        private VideoCapture _capture;
        private Thread _captureThread;
        private int _threshold = 155;
        int hMin, sMin, vMin,hMin2,sMin2,vMin2 = 0;
        int sMax, vMax,sMax2,vMax2 = 255;
        int hMax, hMax2 = 179;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0);// changing the arg to a different number will change the video signal. 
            _captureThread = new Thread(Displaywebcam);
            _captureThread.Start();
        }



        private void Displaywebcam()
        {
            while (_capture.IsOpened)
            {
                //frame maint
                Mat frame = _capture.QueryFrame();
                //resize
                frame = ResizeFrame(frame);
                //display the image in the pichtureBox
                emguPictureBox.Image = frame.ToBitmap();
                //Creating HSV frame
                Mat hsvFrame = new Mat();
                CvInvoke.CvtColor(frame, hsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);

                frame = ThresholdCreater(frame);
                BinaryPictureBox.Image = frame.ToBitmap();
                Slicing(frame, binaryImageOL, binaryImageIL, binaryImageCent, binaryImageIR, binaryImageOR);

                //Creating HSV Channels
                Mat[] hsvChannels = hsvFrame.Split();

//*****************************************BEGIN OF FIRST MERG********************************************//
                Mat hueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(hMin), new ScalarArray(hMax), hueFilter);
                Invoke(new Action( () => { hPictureBox.Image = hueFilter.ToBitmap(); }));

                Mat saturationFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(sMin), new ScalarArray(sMax), saturationFilter);
                Invoke(new Action(() => { sPictureBox.Image = saturationFilter.ToBitmap(); }));

                Mat valueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(vMin), new ScalarArray(vMax), valueFilter);
                Invoke(new Action(() => { vPictureBox.Image = valueFilter.ToBitmap(); }));

                Mat mergedImage = new Mat();
                CvInvoke.BitwiseAnd(hueFilter, saturationFilter, mergedImage);
                CvInvoke.BitwiseAnd(mergedImage, valueFilter, mergedImage);

                mergedImage = DilationThenErosion(mergedImage);
                mergedImage = ErosionThenDilation(mergedImage);

                Invoke(new Action(() => { mergedPictureBox.Image = mergedImage.ToBitmap(); }));
                Slicing(mergedImage, mergedImageOL, mergedImageIL, mergedImageCent, mergedImageIR, mergedImageOR);
//*****************************************END OF FIRST MERG********************************************//

                //*****************************************BEGIN OF SECOND MERG********************************************//
                Mat hueFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(hMin2), new ScalarArray(hMax2), hueFilter2);
                Invoke(new Action(() => { hPictureBox2.Image = hueFilter2.ToBitmap(); }));

                Mat saturationFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(sMin2), new ScalarArray(sMax2), saturationFilter2);
                Invoke(new Action(() => { sPictureBox2.Image = saturationFilter2.ToBitmap(); }));

                Mat valueFilter2 = new Mat();
                CvInvoke.InRange(hsvChannels[1], new ScalarArray(vMin2), new ScalarArray(vMax2), valueFilter2);
                Invoke(new Action(() => { vPictureBox2.Image = valueFilter2.ToBitmap(); }));

                Mat mergedImage2 = new Mat();
                CvInvoke.BitwiseAnd(hueFilter2, saturationFilter2, mergedImage2);
                CvInvoke.BitwiseAnd(mergedImage2, valueFilter2, mergedImage2);

                mergedImage2 = DilationThenErosion(mergedImage2);
                mergedImage2 = ErosionThenDilation(mergedImage2);

                Invoke(new Action(() => { mergedPictureBox2.Image = mergedImage2.ToBitmap(); }));
                Slicing(mergedImage2, megedImage2OL, mergedImage2IL, mergedImage2Cent, mergedImage2IR, mergedImage2OR);

                //*****************************************END OF SECOND MERG********************************************//
            }

        }

 //*****************************************TRACKBARS********************************************//
        //Threshold Trackbar
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            _threshold = trackBar1.Value;
        }

        //****************************Merg One******************************//
        private void hTrackBarMin_Scroll(object sender, EventArgs e)
        {
            hMin = hTrackBarMin.Value;
            hMinLabel.Text = $"{hMin}";
        }
        private void hTrackBarMax_Scroll_1(object sender, EventArgs e)
        {
            hMax = hTrackBarMax.Value;
            hMaxLabel.Text = $"{hMax}";
        }
        private void sTrackBarMin_Scroll(object sender, EventArgs e)
        {
            sMin= sTrackBarMin.Value;
            sMinLabel.Text = $"{sMin}";
        }
        private void sTrackBarMax_Scroll(object sender, EventArgs e)
        {
            sMax = sTrackBarMax.Value;
            sMaxLabel.Text = $"{sMax}";
        }
        private void vTrackBarMin_Scroll(object sender, EventArgs e)
        {
            vMin = vTrackBarMin.Value;
            vMinLabel.Text = $"{vMin}";
        }
        private void vTrackBarMax_Scroll(object sender, EventArgs e)
        {
            vMax= vTrackBarMax.Value;
            vMaxLabel.Text = $"{vMax}";
        }

        //*****************************Merg Two*****************************//
        private void hTrackBarMin2_Scroll(object sender, EventArgs e)
        {
            hMin2= hTrackBarMin2.Value;
            hMinLabel2.Text = $"{hMin2}";
        }
        private void hTrackBarMax2_Scroll(object sender, EventArgs e)
        {
            hMax2 = hTrackBarMax2.Value;
            hMaxLabel2.Text = $"{hMax2}";
        }
        private void sTrackBarMin2_Scroll(object sender, EventArgs e)
        {
            sMin2 = sTrackBarMin2.Value;
            sMinLabel2.Text = $"{sMin2}";
        }



        private void sTrackBarMax2_Scroll(object sender, EventArgs e)
        {
            sMax2= sTrackBarMax2.Value;
            sMaxLabel2.Text = $"{sMax2}";
        }
        private void vTrackBarMin2_Scroll(object sender, EventArgs e)
        {
            vMin2= vTrackBarMin2.Value;
            vMinLabel2.Text = $"{vMin2}";
        }
        private void vTrackBarMax2_Scroll(object sender, EventArgs e)
        {
            vMax2= vTrackBarMax2.Value;
            vMaxLabel2.Text = $"{vMax2}";
        }

//*****************************************FUNCTIONS BENGIN********************************************//
        private Mat ResizeFrame(Mat inputFrame)
        {
            Mat frame = inputFrame.Clone();

            int newHeight = (frame.Size.Height * emguPictureBox.Size.Width) / frame.Size.Width;
            Size newSize = new Size(emguPictureBox.Size.Width, newHeight);
            CvInvoke.Resize(frame, frame, newSize);

            return frame;
        }
        private Mat ThresholdCreater(Mat inputFrame)
        {
            Mat frame = inputFrame.Clone();

            //convert to gray
            CvInvoke.CvtColor(frame, frame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            //binary threhold
            CvInvoke.Threshold(frame, frame, _threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            return frame;
        }
        private Mat DilationThenErosion (Mat inputFrame)
        {
            Mat Frame = inputFrame.Clone();

            Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(3, 3), new Point(1, 1));
            CvInvoke.Dilate(Frame, Frame, kernel, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar());
            CvInvoke.Erode(Frame, Frame, kernel, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar());

            return Frame;
        }
        private Mat ErosionThenDilation(Mat inputFrame)
        {
            Mat Frame = inputFrame.Clone();

            Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(3, 3), new Point(1, 1));
            CvInvoke.Erode(Frame, Frame, kernel, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar());
            CvInvoke.Dilate(Frame, Frame, kernel, new Point(1, 1), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar());

            return Frame;
        }

        private void Slicing(Mat inputFrame, Label OL, Label IL, Label Cent, Label IR, Label OR)
        {
            Mat frame = inputFrame.Clone();

            //count the number of white pixels
            int whitePixelsExtendedOutterLeft = 0;
            int whitePixelsExtendedInnerLeft = 0;
            int whitePixelsExtendedCent = 0;
            int whitePixelsExtendedInnerRight = 0;
            int whitePixelsExtendedOutterRight = 0;

            Image<Gray, byte> img = frame.ToImage<Gray, byte>();

            //Calculates the outter left slice
            for (int x = 0; x < (frame.Width / 5); x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) whitePixelsExtendedOutterLeft++;
                }
            }
            //Calculates the inner left slice
            for (int x = (frame.Width / 5); x < (frame.Width / 5) * 2; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) whitePixelsExtendedInnerLeft++;
                }
            }
            //Calculates the center slice
            for (int x = (frame.Width / 5) * 2; x < (frame.Width / 5) * 3; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) whitePixelsExtendedCent++;
                }
            }
            //Calculates the inner right slice
            for (int x = (frame.Width / 5) * 3; x < (frame.Width / 5) * 4; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) whitePixelsExtendedInnerRight++;
                }
            }
            //Calculates the outter right slice
            for (int x = (frame.Width / 5) * 4; x < (frame.Width / 5) * 5; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) whitePixelsExtendedOutterRight++;
                }
            }

            //displays their respective white pixel count to form1
            Invoke(new Action(() =>
            {
                OL.Text = $"{whitePixelsExtendedOutterLeft}";
                IL.Text = $"{whitePixelsExtendedInnerLeft}";
                Cent.Text = $"{whitePixelsExtendedCent}";
                IR.Text = $"{whitePixelsExtendedInnerRight}";
                OR.Text = $"{whitePixelsExtendedOutterRight}";
            }));
        }

//*****************************************FUNCTIONS END********************************************//

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }
    }
}
