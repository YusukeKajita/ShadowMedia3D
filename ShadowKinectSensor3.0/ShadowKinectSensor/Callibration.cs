using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Blob;

namespace ShadowKinectSensor
{
    public class Callibration
    {
        public class OffsetCallib
        {
            public const float height = 0.15f;
            public const float radius = 0.02f;
        }

        public class CallibrationPallameter
        {
            public float dx;
            public float dy;
            public float dz;
            public float rx;
            public float ry;
            public float rz;
        }

        public IplImage iplimage_row;
        public IplImage iplimage_gray;
        public MainWindow mainwindow { set; get; }
        public CvBlobs blobs;
        public SkeletonPoint testresultpoint;

        public SkeletonPoint ResultPoint_Zero;
        public SkeletonPoint ResultPoint_X;
        public SkeletonPoint ResultPoint_Z;

        public Callibration()
        {

        }

        public void Set_PointTest(byte[] pixs, int width, int height, int byteperpixels, int times)
        {
            this.iplimage_row = IplImage.FromPixelData(new CvSize(width, height), byteperpixels, pixs);
            this.iplimage_gray = Cv.CreateImage(new CvSize(width, height), BitDepth.U8, 1);
            unsafe
            {
                byte* p = (byte*)iplimage_row.ImageData;

                for (int i = 0; i < iplimage_row.Height; ++i)
                {
                    for (int j = 0; j < iplimage_row.Width; ++j)
                    {

                        if ((int)p[i * iplimage_row.WidthStep + j * byteperpixels + 0] + (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 1] + 50 < (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 2])
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 255;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 255;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 255;   //赤
                        }
                        else
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 0;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 0;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 0;   //赤
                        }

                    }
                }
            }
            Cv.CvtColor(iplimage_row, iplimage_gray, ColorConversion.BgraToGray);
            this.blobs = new CvBlobs();
            this.blobs.Label(this.iplimage_gray);
            this.blobs.FilterByArea(20, 1500);
            CvBlob blob = this.blobs.LargestBlob();
            DepthImagePoint dip = new DepthImagePoint();
            dip.X = blob.Rect.Left + blob.Rect.Width * 1 / 2;
            dip.Y = blob.Rect.Top + blob.Rect.Height / 2;
            dip.Depth = this.mainwindow.DepthPixelRow[dip.X + dip.Y * 640].Depth;

            dip = this.mainwindow.DepthImagePoints[blob.Rect.Left + blob.Rect.Width * 1 / 2 + (blob.Rect.Top + blob.Rect.Height / 2) * 640];

            SkeletonPoint sp = mainwindow.KinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, dip);
            sp.Y -= OffsetCallib.height;
            sp.Z += OffsetCallib.radius;
            this.testresultpoint.X = (sp.X + this.testresultpoint.X * times) / (times + 1);
            this.testresultpoint.Y = (sp.Y + this.testresultpoint.Y * times) / (times + 1);
            this.testresultpoint.Z = (sp.Z + this.testresultpoint.Z * times) / (times + 1);
            this.mainwindow.TextBlock_Callib_Test_x_re.Text = this.testresultpoint.X.ToString();
            this.mainwindow.TextBlock_Callib_Test_y_re.Text = this.testresultpoint.Y.ToString();
            this.mainwindow.TextBlock_Callib_Test_z_re.Text = this.testresultpoint.Z.ToString();

            IplImage iplimage_rendertest;
            iplimage_rendertest = Cv.CreateImage(iplimage_gray.Size, BitDepth.U8, 3);
            this.blobs.RenderBlobs(this.iplimage_gray, iplimage_rendertest);
            Cv.ShowImage("test", iplimage_rendertest);

            this.iplimage_row.Dispose();
            this.iplimage_gray.Dispose();
            iplimage_rendertest.Dispose();


        }
        public SkeletonPoint Get_ResultTest()
        {
            return this.testresultpoint;
        }

        public void Set_Point_Zero(byte[] pixs, int width, int height, int byteperpixels, int times)
        {
            this.iplimage_row = IplImage.FromPixelData(new CvSize(width, height), byteperpixels, pixs);
            this.iplimage_gray = Cv.CreateImage(new CvSize(width, height), BitDepth.U8, 1);
            unsafe
            {
                byte* p = (byte*)iplimage_row.ImageData;

                for (int i = 0; i < iplimage_row.Height; ++i)
                {
                    for (int j = 0; j < iplimage_row.Width; ++j)
                    {

                        if ((int)p[i * iplimage_row.WidthStep + j * byteperpixels + 0] + (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 1] + 50 < (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 2])
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 255;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 255;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 255;   //赤
                        }
                        else
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 0;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 0;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 0;   //赤
                        }

                    }
                }
            }
            Cv.CvtColor(iplimage_row, iplimage_gray, ColorConversion.BgraToGray);
            this.blobs = new CvBlobs();
            this.blobs.Label(this.iplimage_gray);
            this.blobs.FilterByArea(20, 1500);
            CvBlob blob = this.blobs.LargestBlob();
            DepthImagePoint dip = new DepthImagePoint();
            dip.X = blob.Rect.Left + blob.Rect.Width * 1 / 3;
            dip.Y = blob.Rect.Top + blob.Rect.Height / 2;
            dip.Depth = this.mainwindow.DepthPixelRow[dip.X + dip.Y * 640].Depth;

            dip = this.mainwindow.DepthImagePoints[blob.Rect.Left + blob.Rect.Width * 1 / 2 + (blob.Rect.Top + blob.Rect.Height / 2) * 640];

            SkeletonPoint sp = mainwindow.KinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, dip);
            sp.Y -= OffsetCallib.height;
            sp.Z += OffsetCallib.radius;
            this.ResultPoint_Zero.X = (sp.X + this.ResultPoint_Zero.X * times) / (times + 1);
            this.ResultPoint_Zero.Y = (sp.Y + this.ResultPoint_Zero.Y * times) / (times + 1);
            this.ResultPoint_Zero.Z = (sp.Z + this.ResultPoint_Zero.Z * times) / (times + 1);
            IplImage iplimage_rendertest;
            iplimage_rendertest = Cv.CreateImage(iplimage_gray.Size, BitDepth.U8, 3);
            this.blobs.RenderBlobs(this.iplimage_gray, iplimage_rendertest);
            Cv.ShowImage("test", iplimage_rendertest);
            this.iplimage_row.Dispose();
            this.iplimage_gray.Dispose();
            iplimage_rendertest.Dispose();
        }

        public void Set_Point_X(byte[] pixs, int width, int height, int byteperpixels, int times)
        {
            this.iplimage_row = IplImage.FromPixelData(new CvSize(width, height), byteperpixels, pixs);
            this.iplimage_gray = Cv.CreateImage(new CvSize(width, height), BitDepth.U8, 1);
            unsafe
            {
                byte* p = (byte*)iplimage_row.ImageData;

                for (int i = 0; i < iplimage_row.Height; ++i)
                {
                    for (int j = 0; j < iplimage_row.Width; ++j)
                    {

                        if ((int)p[i * iplimage_row.WidthStep + j * byteperpixels + 0] + (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 1] + 50 < (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 2])
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 255;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 255;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 255;   //赤
                        }
                        else
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 0;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 0;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 0;   //赤
                        }

                    }
                }
            }
            Cv.CvtColor(iplimage_row, iplimage_gray, ColorConversion.BgraToGray);
            this.blobs = new CvBlobs();
            this.blobs.Label(this.iplimage_gray);
            this.blobs.FilterByArea(20, 1500);
            CvBlob blob = this.blobs.LargestBlob();
            DepthImagePoint dip = new DepthImagePoint();
            dip.X = blob.Rect.Left + blob.Rect.Width * 1 / 3;
            dip.Y = blob.Rect.Top + blob.Rect.Height / 2;
            dip.Depth = this.mainwindow.DepthPixelRow[dip.X + dip.Y * 640].Depth;

            dip = this.mainwindow.DepthImagePoints[blob.Rect.Left + blob.Rect.Width * 1 / 2 + (blob.Rect.Top + blob.Rect.Height / 2) * 640];

            SkeletonPoint sp = mainwindow.KinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, dip);
            sp.Y -= OffsetCallib.height;
            sp.Z += OffsetCallib.radius;
            this.ResultPoint_X.X = (sp.X + this.ResultPoint_X.X * times) / (times + 1);
            this.ResultPoint_X.Y = (sp.Y + this.ResultPoint_X.Y * times) / (times + 1);
            this.ResultPoint_X.Z = (sp.Z + this.ResultPoint_X.Z * times) / (times + 1);
            IplImage iplimage_rendertest;
            iplimage_rendertest = Cv.CreateImage(iplimage_gray.Size, BitDepth.U8, 3);
            this.blobs.RenderBlobs(this.iplimage_gray, iplimage_rendertest);
            Cv.ShowImage("test", iplimage_rendertest);
            this.iplimage_row.Dispose();
            this.iplimage_gray.Dispose();
            iplimage_rendertest.Dispose();
        }

        public void Set_Point_Z(byte[] pixs, int width, int height, int byteperpixels, int times)
        {
            this.iplimage_row = IplImage.FromPixelData(new CvSize(width, height), byteperpixels, pixs);
            this.iplimage_gray = Cv.CreateImage(new CvSize(width, height), BitDepth.U8, 1);
            unsafe
            {
                byte* p = (byte*)iplimage_row.ImageData;

                for (int i = 0; i < iplimage_row.Height; ++i)
                {
                    for (int j = 0; j < iplimage_row.Width; ++j)
                    {

                        if ((int)p[i * iplimage_row.WidthStep + j * byteperpixels + 0] + (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 1] + 50 < (int)p[i * iplimage_row.WidthStep + j * byteperpixels + 2])
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 255;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 255;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 255;   //赤
                        }
                        else
                        {
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 0] = 0;   //青
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 1] = 0;   //緑
                            p[i * iplimage_row.WidthStep + j * byteperpixels + 2] = 0;   //赤
                        }

                    }
                }
            }
            Cv.CvtColor(iplimage_row, iplimage_gray, ColorConversion.BgraToGray);
            this.blobs = new CvBlobs();
            this.blobs.Label(this.iplimage_gray);
            this.blobs.FilterByArea(20, 1500);
            CvBlob blob = this.blobs.LargestBlob();
            DepthImagePoint dip = new DepthImagePoint();
            dip.X = blob.Rect.Left + blob.Rect.Width * 1 / 3;
            dip.Y = blob.Rect.Top + blob.Rect.Height / 2;
            dip.Depth = this.mainwindow.DepthPixelRow[dip.X + dip.Y * 640].Depth;

            dip = this.mainwindow.DepthImagePoints[blob.Rect.Left + blob.Rect.Width * 1 / 2 + (blob.Rect.Top + blob.Rect.Height / 2) * 640];

            SkeletonPoint sp = mainwindow.KinectSensor.CoordinateMapper.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, dip);
            sp.Y -= OffsetCallib.height;
            sp.Z += OffsetCallib.radius;
            this.ResultPoint_Z.X = (sp.X + this.ResultPoint_Z.X * times) / (times + 1);
            this.ResultPoint_Z.Y = (sp.Y + this.ResultPoint_Z.Y * times) / (times + 1);
            this.ResultPoint_Z.Z = (sp.Z + this.ResultPoint_Z.Z * times) / (times + 1);
            IplImage iplimage_rendertest;
            iplimage_rendertest = Cv.CreateImage(iplimage_gray.Size, BitDepth.U8, 3);
            this.blobs.RenderBlobs(this.iplimage_gray, iplimage_rendertest);
            Cv.ShowImage("test", iplimage_rendertest);
            this.iplimage_row.Dispose();
            this.iplimage_gray.Dispose();
            iplimage_rendertest.Dispose();
        }

        public SkeletonPoint Get_ResultZero()
        {
            return this.ResultPoint_Zero;
        }
        public SkeletonPoint Get_ResultX()
        {
            return this.ResultPoint_X;
        }
        public SkeletonPoint Get_ResultZ()
        {
            return this.ResultPoint_Z;
        }

        public CallibrationPallameter Callcurate()
        {
            CallibrationPallameter CP = new CallibrationPallameter();
            CP.dx = -this.ResultPoint_Zero.X;
            CP.dy = -this.ResultPoint_Zero.Y;
            CP.dz = -this.ResultPoint_Zero.Z;
            SkeletonPoint spz =new SkeletonPoint();
            spz.X = this.ResultPoint_Zero.X-this.ResultPoint_Z.X;
            spz.Y = this.ResultPoint_Zero.Y-this.ResultPoint_Z.Y;
            spz.Z = this.ResultPoint_Zero.Z-this.ResultPoint_Z.Z;
            //単位ベクトルZ
            SkeletonPoint spz_mono = new SkeletonPoint();
            spz_mono.X = spz.X / (float)Math.Pow((double)(spz.X * spz.X + spz.Y * spz.Y + spz.Z * spz.Z), 0.5);
            spz_mono.Y = spz.Y / (float)Math.Pow((double)(spz.X * spz.X + spz.Y * spz.Y + spz.Z * spz.Z), 0.5);
            spz_mono.Z = spz.Z / (float)Math.Pow((double)(spz.X * spz.X + spz.Y * spz.Y + spz.Z * spz.Z), 0.5);
            SkeletonPoint spx = new SkeletonPoint();
            spx.X = this.ResultPoint_X.X - this.ResultPoint_Zero.X;
            spx.Y = this.ResultPoint_X.Y - this.ResultPoint_Zero.Y;
            spx.Z = this.ResultPoint_X.Z - this.ResultPoint_Zero.Z;
            
            //単位ベクトルX
            SkeletonPoint spx_mono = new SkeletonPoint();
            spx_mono.X = spx.X / (float)Math.Pow((double)(spx.X * spx.X + spx.Y * spx.Y + spx.Z * spx.Z), 0.5);
            spx_mono.Y = spx.Y / (float)Math.Pow((double)(spx.X * spx.X + spx.Y * spx.Y + spx.Z * spx.Z), 0.5);
            spx_mono.Z = spx.Z / (float)Math.Pow((double)(spx.X * spx.X + spx.Y * spx.Y + spx.Z * spx.Z), 0.5);

            //単位ベクトルY
            SkeletonPoint spy_mono = new SkeletonPoint();
            spy_mono.X = spx_mono.Y * spz_mono.Z - spx_mono.Z * spz_mono.Y;
            spy_mono.Y = spx_mono.Z * spz_mono.X - spx_mono.X * spz_mono.Z;
            spy_mono.Z = spx_mono.X * spz_mono.Y - spx_mono.Y * spz_mono.X;

            

            if (spz_mono.X > 0)
            {
                CP.ry = (float)Math.Acos((double)spz_mono.Z);
            }
            else
            {
                CP.ry = -(float)Math.Acos((double)spz_mono.Z);
            }
            CP.rz = (float)Math.Atan2((double)spy_mono.X, (double)spy_mono.Y);
            CP.rx = (float)Math.Atan2((double)spy_mono.Z, (double)spy_mono.Y);


            
            CP.dx = (float)Math.Cos((double)CP.ry) * (-this.ResultPoint_Zero.X) - (float)Math.Sin((double)CP.ry) * (-this.ResultPoint_Zero.Z);
            CP.dz = (float)Math.Sin((double)CP.ry) * (-this.ResultPoint_Zero.X) + (float)Math.Cos((double)CP.ry) * (-this.ResultPoint_Zero.Z);

            float D = (float)Math.Pow(this.ResultPoint_Zero.Z * this.ResultPoint_Zero.Z + this.ResultPoint_Zero.Y * this.ResultPoint_Zero.Y, 0.5);
            double rDz = Math.Atan2(Math.Abs(this.ResultPoint_Zero.Y), this.ResultPoint_Zero.Z);
            CP.dy = D * (float)Math.Sin(rDz - (double)CP.rx);

            this.mainwindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.mainwindow.TextBlock_Callib_X.Text = CP.dx.ToString();
                this.mainwindow.TextBlock_Callib_Y.Text = CP.dy.ToString();
                this.mainwindow.TextBlock_Callib_Z.Text = CP.dz.ToString();
                this.mainwindow.TextBlock_Callib_rx.Text = CP.rx.ToString();
                this.mainwindow.TextBlock_Callib_rz.Text = CP.rz.ToString();
                this.mainwindow.TextBlock_Callib_ry.Text = CP.ry.ToString();
                this.mainwindow.TextBlock_Callib_D.Text = D.ToString();

            }));
            
            return CP;
        }
    }
}
