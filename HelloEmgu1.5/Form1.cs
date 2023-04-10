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
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using Emgu.CV.Features2D;
using System.Xml.Linq;


namespace HelloEmgu1._5
{
    public partial class Form1 : Form
    {
        //*********veriables**********\\
        private VideoCapture _capture;
        private Thread _captureThread;
        private Robot robot;// serial control classusing robot.cs file 
        private int _threshold = 155;
        int hMin, sMin, vMin, hMin2, sMin2, vMin2 = 0; // setting the min values of saturation, value, and hue
        int sMax, vMax, sMax2, vMax2 = 255; // setting the max values of saturation, and value.
        int hMax, hMax2 = 179;// setting the min values of  hue
        bool runFlag = false;
        bool calFlag = false;
        int leftMotorOffset = 0;
        int rightMotorOffset = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(0);// changing the arg to a different number will change the video signal. 
            _captureThread = new Thread(Displaywebcam);
            _captureThread.Start();
           

            robot = new Robot("COM4");
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

                frame = DilationThenErosion(frame);
                frame = ErosionThenDilation(frame);

                frame = ThresholdCreater(frame);
                BinaryPictureBox.Image = frame.ToBitmap();
                var binaryLine = Slicing(frame, binaryImageOL, binaryImageIL, binaryImageCent, binaryImageIR, binaryImageOR);

                //Creating HSV Channels
                Mat[] hsvChannels = hsvFrame.Split();

                //*****************************************BEGIN OF FIRST MERG********************************************//
                Mat hueFilter = new Mat();
                CvInvoke.InRange(hsvChannels[0], new ScalarArray(hMin), new ScalarArray(hMax), hueFilter);
                Invoke(new Action(() => { hPictureBox.Image = hueFilter.ToBitmap(); }));

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
                var yellowLine = Slicing(mergedImage, mergedImageOL, mergedImageIL, mergedImageCent, mergedImageIR, mergedImageOR);


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
                var redLine = Slicing(mergedImage2, megedImage2OL, mergedImage2IL, mergedImage2Cent, mergedImage2IR, mergedImage2OR);

                //*****************************************END OF SECOND MERG********************************************//



                //***************************************************************State change logic *******************************************//

                if (runFlag)
                {

                    if (yellowLine.Cent >= yellowLine.IL && yellowLine.Cent >= yellowLine.IR)
                    {
                        //Thread.Sleep(100);// pausing the thread by 100 ms.
                        robot.Move('W');// Slow forward
                    }
                    else if (yellowLine.Cent <= yellowLine.IL)
                    {
                        robot.Move('R');//soft right
                    }
                    else if (yellowLine.Cent <= yellowLine.IR)
                    {
                        robot.Move('L'); //soft left
                    }
                    else if (yellowLine.OL >= yellowLine.IL)
                    {
                        robot.Move('H');//Hard Right
                    }
                    else if (yellowLine.OR >= yellowLine.IR)
                    {
                        robot.Move('T');//Hard Left
                    }
                    else if (redLine.Cent + redLine.IL + redLine.IR > yellowLine.Cent)
                    {
                        robot.Move('S');//Stop
                    }
                }
                else robot.Move('S');
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
            sMin = sTrackBarMin.Value;
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
            vMax = vTrackBarMax.Value;
            vMaxLabel.Text = $"{vMax}";
        }

        //*****************************Merg Two*****************************//
        private void hTrackBarMin2_Scroll(object sender, EventArgs e)
        {
            hMin2 = hTrackBarMin2.Value;
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
            sMax2 = sTrackBarMax2.Value;
            sMaxLabel2.Text = $"{sMax2}";
        }
        private void vTrackBarMin2_Scroll(object sender, EventArgs e)
        {
            vMin2 = vTrackBarMin2.Value;
            vMinLabel2.Text = $"{vMin2}";
        }
        private void vTrackBarMax2_Scroll(object sender, EventArgs e)
        {
            vMax2 = vTrackBarMax2.Value;
            vMaxLabel2.Text = $"{vMax2}";
        }

        //*****************START STOP ************************************//
        private void startButton_Click(object sender, EventArgs e)
        {
            runFlag = true;
        }
        private void SaveOffsets_Click(object sender, EventArgs e)
        {
            SaveOffset();
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            runFlag = false;
        }
        private void LoadOffsets_Click(object sender, EventArgs e)
        {
            LoadOffset();

            for(int i = 0; i < leftMotorOffset; i++)
            {
                robot.Move('2');
            }
            for (int i = 0; i < rightMotorOffset; i++)
            {
                robot.Move('4');
            }
        }
        //****************Motor Speed Trim ****************************//
        private void TrimLeftDown_Click(object sender, EventArgs e)
        {
            robot.Move('1');
            if (leftMotorOffset - 1 < 0)
            leftMotorOffset--;
        }
        private void TrimRightUp_Click(object sender, EventArgs e)
        {
            robot.Move('4');
            rightMotorOffset++;
        }
        private void TrimRightDown_Click(object sender, EventArgs e)
        {
            robot.Move('3');
            if(rightMotorOffset - 1 < 0)
            rightMotorOffset--;
        }
        private void TrimLeftUp_Click(object sender, EventArgs e)
        {
            robot.Move('2');
            leftMotorOffset++;
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
        private Mat DilationThenErosion(Mat inputFrame)
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
        private PixCount Slicing(Mat inputFrame, Label OL, Label IL, Label Cent, Label IR, Label OR)
        {
            Mat frame = inputFrame.Clone();

            //count the number of white pixels
            var counts = new PixCount();

            Image<Gray, byte> img = frame.ToImage<Gray, byte>();

            //Calculates the outter left slice
            for (int x = 0; x < (frame.Width / 5); x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) counts.OL++;

                }
            }
            //Calculates the inner left slice
            for (int x = (frame.Width / 5); x < (frame.Width / 5) * 2; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) counts.IL++;
                }
            }
            //Calculates the center slice
            for (int x = (frame.Width / 5) * 2; x < (frame.Width / 5) * 3; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) counts.Cent++;
                }
            }
            //Calculates the inner right slice
            for (int x = (frame.Width / 5) * 3; x < (frame.Width / 5) * 4; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) counts.IR++;
                }
            }
            //Calculates the outter right slice
            for (int x = (frame.Width / 5) * 4; x < (frame.Width / 5) * 5; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    if (img.Data[y, x, 0] == 255) counts.OR++;
                }
            }



            //displays their respective white pixel count to form1
            Invoke(new Action(() =>
            {
                OL.Text = $"{counts.OL}";
                IL.Text = $"{counts.IL}";
                Cent.Text = $"{counts.Cent}";
                IR.Text = $"{counts.IR}";
                OR.Text = $"{counts.OR}";
            }));



            return counts;

        }
        private void SaveOffset()
        {
            // Create a new StreamWriter object and open the file for writing
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Bowman\Documents\Programming\GitHub\HelloEmgu1.5\OffSets.txt"))
            {

                //int hMin, sMin, vMin, hMin2, sMin2, vMin2
                // Write the text to the file
                writer.WriteLine(hMin);
                writer.WriteLine(hMax);
                writer.WriteLine(sMin);
                writer.WriteLine(sMax);
                writer.WriteLine(vMin);
                writer.WriteLine(vMax);

                writer.WriteLine(hMin2);
                writer.WriteLine(hMax2);
                writer.WriteLine(sMin2);
                writer.WriteLine(sMax2);
                writer.WriteLine(vMin2);
                writer.WriteLine(vMax2);

                writer.WriteLine(leftMotorOffset);
                writer.WriteLine(rightMotorOffset);
            }
        }
        private void LoadOffset()
        {
            string line;

            using (StreamReader reader = new StreamReader(@"C:\Users\Bowman\Documents\Programming\GitHub\HelloEmgu1.5\OffSets.txt"))
            {
                hMin = int.Parse(reader.ReadLine());
                hTrackBarMin.Value = hMin;
                hMinLabel.Text = $"{hMin}";

                hMax = int.Parse(reader.ReadLine());
                hTrackBarMax2.Value = hMax;
                hMaxLabel.Text = $"{hMax}";

                sMin = int.Parse(reader.ReadLine());
                sTrackBarMin2.Value = sMin;
                sMinLabel.Text = $"{sMin}";

                sMax = int.Parse(reader.ReadLine());
                sTrackBarMax.Value = sMax;
                sMaxLabel2.Text = $"{sMax}";

                vMin = int.Parse(reader.ReadLine());
                vTrackBarMin.Value = vMin;
                vMinLabel.Text = $"{vMin}";

                vMax = int.Parse(reader.ReadLine());
                vTrackBarMax2.Value = vMax;
                vMaxLabel2.Text = $"{vMax}";

//****************************************************************//
                hMin2 = int.Parse(reader.ReadLine());
                hTrackBarMin2.Value = hMin2;
                hMinLabel2.Text = $"{hMin2}";

                hMax2 = int.Parse(reader.ReadLine());
                hTrackBarMax2.Value = hMax2;
                hMaxLabel2.Text = $"{hMax2}";

                sMin2 = int.Parse(reader.ReadLine());
                sTrackBarMin2.Value = sMin2;
                sMinLabel2.Text = $"{sMin2}";

                sMax2 = int.Parse(reader.ReadLine());
                sTrackBarMax2.Value = sMax2;
                sMaxLabel2.Text = $"{sMax2}";

                vMin2 = int.Parse(reader.ReadLine());
                vTrackBarMin2.Value = vMin2;
                vMinLabel2.Text = $"{vMin2}";

                vMax2 = int.Parse(reader.ReadLine());
                vTrackBarMax2.Value = vMax2;
                vMaxLabel2.Text = $"{vMax2}";

//**************************************************************//
                line = reader.ReadLine();
                leftMotorOffset = int.Parse(line);
                line = reader.ReadLine();
                rightMotorOffset = int.Parse(line);

            }
        }
        
        //*****************************************FUNCTIONS/CLASSES END********************************************//

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            robot.Move('S');
            _captureThread.Abort();
            
        }

        //PixCount class is uses to organized the pixcount of the slices.
        public class PixCount
        {

            public int OL { get; set; }
            public int IL { get; set; }
            public int Cent { get; set; }
            public int IR { get; set; }
            public int OR { get; set; }


        }
       
    }

}
