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
using System.Threading;
using System.Net;
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using OpenCvSharp;


namespace ShadowKinectSensor
{

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region field
        private readonly string name = "KinectSensor";
        private readonly int fps = 30;

        private KinectSensor kinectsensor;
        public KinectSensor KinectSensor
        {
            get
            {
                return this.kinectsensor;
            }
        }
        private Task mytask_cipc_shadow;
        private CancellationTokenSource CTS_cipc_shadow;
        private FPSAdjuster.FPSAdjuster FPS_maintask { set; get; }
        private CIPC_CS.CLIENT.CLIENT cipc_shadow;
        private UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc_shadow;
        private byte[] data_shadow;

        private MinimizedWindow minimizedwindow;

        struct ShadowKinectSensorTerminal
        {
            public System.Net.Sockets.UdpClient udplient { set; get; }
            public System.Net.IPEndPoint RemoteEP;
            public byte[] data { set; get; }

            public System.Net.Sockets.TcpClient tcpclient { set; get; }
            public System.Net.Sockets.NetworkStream networkstream { set; get; }
            public System.IO.MemoryStream memorystream { set; get; }

            public CancellationTokenSource CTS;
            public Task receivetask;
        };


        private ShadowKinectSensorTerminal SKST;
        private Callibration.CallibrationPallameter CS;
        /// <summary>
        /// trackingのレベル　この数値が高い方が取れているポイントが多い
        /// </summary>
        private int TrackingLevel { set; get; }

        #region OptitrackMembers
        private bool IsOptRunning;

        // [NatNet] Our NatNet object
        private NatNetML.NatNetClientML m_NatNet;
        // [NatNet] Our NatNet Frame of Data object
        private NatNetML.FrameOfMocapData m_FrameOfData = new NatNetML.FrameOfMocapData();
        private NatNetML.ServerDescription desc = new NatNetML.ServerDescription();

        public class RigidBodyWithKinectID
        {
            public int KinectID { get; set; }
            public NatNetML.RigidBody RigidBody { get; set; }
        }

        public class OptiBoneWithKinectID
        {
            public string Name { get; set; }
            public int KinectId { get; set; }
        }


        private Dictionary<int, RigidBodyWithKinectID> m_rigidBodyDefinitions = new Dictionary<int, RigidBodyWithKinectID>();
        private Queue<NatNetML.FrameOfMocapData> m_FrameQueue = new Queue<NatNetML.FrameOfMocapData>();

        List<OptiBoneWithKinectID> m_optiBoneWithKinectIDs = new List<OptiBoneWithKinectID> {

            new OptiBoneWithKinectID{ Name = "Ab",         KinectId = 0},
            new OptiBoneWithKinectID{ Name = "Chest",          KinectId = 1},

            new OptiBoneWithKinectID{ Name = "Head",       KinectId = 3},
            new OptiBoneWithKinectID{ Name = "RUArm",       KinectId = 4},
            new OptiBoneWithKinectID{ Name = "RFArm",       KinectId = 5},
            new OptiBoneWithKinectID{ Name = "RHand",       KinectId = 6},

            new OptiBoneWithKinectID{ Name = "LUArm",       KinectId = 8},
            new OptiBoneWithKinectID{ Name = "LFArm",       KinectId = 9},
            new OptiBoneWithKinectID{ Name = "LHand",       KinectId = 10},

            new OptiBoneWithKinectID{ Name = "RThigh",      KinectId = 12},
            new OptiBoneWithKinectID{ Name = "RShin",       KinectId = 13},
            new OptiBoneWithKinectID{ Name = "RFoot",       KinectId = 14},
            new OptiBoneWithKinectID{ Name = "RToe",        KinectId = 15},
            new OptiBoneWithKinectID{ Name = "LThigh",      KinectId = 16},
            new OptiBoneWithKinectID{ Name = "LShin",       KinectId = 17},
            new OptiBoneWithKinectID{ Name = "LFoot",       KinectId = 18},
            new OptiBoneWithKinectID{ Name = "LToe",        KinectId = 19},};



        public int LowWord(int number)
        {
            return number & 0xFFFF;
        }

        public int HighWord(int number)
        {
            return ((number >> 16) & 0xFFFF);
        }

        #endregion OptitrackMembers

        public const int sendsize = 61440;

        private enum SendDataType
        {
            Depth,
            Skeleton,
            optidata,
            SKSTdata
        };

        private enum SendTo
        {
            CIPCServer,
            ShadowKinectSensorTerminal
        };

        private enum ReceiveFrom
        {
            Kinect,
            OptiTrack
        };
        private bool IsOptiRunning { set; get; }
        private bool IsKinectRunning { set; get; }

        private SendDataType senddatatype { set; get; }
        private SendTo sendto { set; get; }
        private ReceiveFrom receivefrom { set; get; }

        #region ColorImage
        private WriteableBitmap ColorImageBitmap;
        private Int32Rect ColorImageRect;
        private int ColorImageStride;
        private byte[] ColorImagePixelData;
        #endregion

        #region DepthImage
        private WriteableBitmap DepthImageBitmap;
        private Int32Rect DepthImageRect;
        private int DepthImageStride;
        private short[] DepthImagePixelData_short;
        public short[] DepthDataRow
        {
            get
            {
                return this.DepthImagePixelData_short;
            }
        }

        private DepthImagePixel[] DepthImagePixelData_DIP;
        public DepthImagePixel[] DepthPixelRow
        {
            get
            {
                return this.DepthImagePixelData_DIP;
            }
        }
        public DepthImagePoint[] DepthImagePoints { private set; get; }

        private SkeletonPoint[] SkeletonPoint_DepthCurrent;

        #endregion

        #region ShadowImage
        private WriteableBitmap PlayerShadowBitmap;
        private Int32Rect PlayerShadowRect;
        private int PlayerShadowStride;
        private byte[] PlayerShadowPixelData;
        private bool[] PlayerShadowBoolData;
        #endregion

        #region Skelton
        private Skeleton[] FrameSkeletons;
        private JointType[] joints;
        #endregion

        #region callib
        public SkeletonPoint SkeletonPoint_CallibTest;
        public bool Is_trytest { get; set; }
        public bool Is_CallibPointZero { get; set; }
        public bool Is_CallibPointX { get; set; }
        public bool Is_CallibPointZ { get; set; }


        public readonly int TestTimes = 300;
        public int TestTimeCurrent;
        public Callibration callibration;
        #endregion

        #endregion

        #region constructer and events
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "ShadowKinectSensor";
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();
            this.Title_TextBlock.Text = "工程名　：　" + System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            this.Closing += MainWindow_Closing;
            this.IsOptRunning = false;


            this.senddatatype = SendDataType.SKSTdata;
            this.GroupBox_Sendto_SKST.IsEnabled = true;
            this.GroupBox_Sendto_CIPCServer.IsEnabled = false;

            this.GroupBox_ReceiveFrom_CIPCServer.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Kinect.IsEnabled = true;
            this.GroupBox_ReceiveFrom_Optitrack.IsEnabled = false;

            this.sendto = SendTo.ShadowKinectSensorTerminal;
            this.receivefrom = ReceiveFrom.Kinect;

            this.RadioButton_ShadowKinectSensorTerminal.IsChecked = true;

            this.IsKinectRunning = false;
            this.IsOptiRunning = false;
            this.CS = new Callibration.CallibrationPallameter();
            this.CS.dx = 0;
            this.CS.dy = 0;
            this.CS.dz = 0;
            this.CS.ry = 0;
            this.CS.rx = 0;

            this.IsInit3Dmodelview = false;

            this.Is_trytest = false;

            this.Is_CallibPointZero = false;
            this.Is_CallibPointX = false;
            this.Is_CallibPointZ = false;
            this.TestTimeCurrent = 0;
            this.SkeletonPoint_CallibTest = new SkeletonPoint();
            this.callibration = new Callibration();

        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.KinectStop();
            this.windowsclose();
        }

        /// <summary>
        /// メインウィンドウ以外のウィンドウを終了します
        /// </summary>
        private void windowsclose()
        {
            if (this.minimizedwindow != null)
            {
                this.minimizedwindow.Close();
                this.minimizedwindow = null;
            }
        }

        /// <summary>
        /// キネクトを正常終了します
        /// </summary>
        private void KinectStop()
        {
            try
            {
                if (this.kinectsensor != null)
                {
                    kinectsensor.AllFramesReady -= kinectsensor_AllFramesReady;
                    this.kinectsensor.Stop();
                    this.kinectsensor.Dispose();
                    this.kinectsensor = null;
                }
                this.IsKinectRunning = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// キネクトを探索し，起動します
        /// </summary>
        /// <returns></returns>
        private bool DiscoverKinect()
        {
            try
            {
                this.CheckandResetSensor();
                this.KinectStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }

        #endregion

        #region DiscoverKinectmethods
        /// <summary>
        /// キネクトの状態を確認し，初期化します
        /// </summary>
        private void CheckandResetSensor()
        {
            try
            {
                if (this.kinectsensor != null && this.kinectsensor.Status != KinectStatus.Connected)
                {
                    this.kinectsensor.ColorStream.Disable();
                    this.kinectsensor.DepthStream.Disable();
                    this.kinectsensor.SkeletonStream.Disable();
                    this.kinectsensor.Stop();
                    this.kinectsensor = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "キネクトの初期化に失敗しました。");
            }
        }

        /// <summary>
        /// キネクト選択したキネクトをスタートします
        /// </summary>
        private void KinectStart()
        {
            if (this.kinectsensor == null)
            {
                //                this.kinectsensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                this.kinectsensor = KinectSensor.KinectSensors[int.Parse(this.TextBox_Kinect_index.Text)];

                if (this.kinectsensor != null)
                {
                    this.kinectsensor.SkeletonStream.Enable();
                    this.kinectsensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    this.kinectsensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);


                    this.kinectsensor.AllFramesReady += kinectsensor_AllFramesReady;

                    this.kinectsensor.Start();

                    #region ColorStreamInit
                    ColorImageStream ColorStream = this.kinectsensor.ColorStream;
                    this.ColorImageBitmap = new WriteableBitmap(ColorStream.FrameWidth, ColorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    this.ColorImageRect = new Int32Rect(0, 0, ColorStream.FrameWidth, ColorStream.FrameHeight);
                    this.ColorImageStride = ColorStream.FrameWidth * ColorStream.FrameBytesPerPixel;
                    this.ColorImageBox.Source = this.ColorImageBitmap;

                    this.ColorImagePixelData = new byte[ColorStream.FramePixelDataLength];
                    #endregion

                    #region DepthStreamInit
                    DepthImageStream DepthStream = this.kinectsensor.DepthStream;
                    this.DepthImageBitmap = new WriteableBitmap(DepthStream.FrameWidth, DepthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                    this.DepthImageRect = new Int32Rect(0, 0, DepthStream.FrameWidth, DepthStream.FrameHeight);
                    this.DepthImageStride = DepthStream.FrameWidth * DepthStream.FrameBytesPerPixel;
                    this.DepthImageBox.Source = this.DepthImageBitmap;

                    this.DepthImagePixelData_short = new short[DepthStream.FramePixelDataLength];
                    this.DepthImagePixelData_DIP = new DepthImagePixel[DepthStream.FramePixelDataLength];
                    this.DepthImagePoints = new DepthImagePoint[DepthStream.FramePixelDataLength];
                    this.SkeletonPoint_DepthCurrent = new SkeletonPoint[DepthStream.FramePixelDataLength];
                    this.Init_3Dmodelview();
                    #endregion


                    #region PlayerShadow
                    this.PlayerShadowBitmap = new WriteableBitmap(DepthStream.FrameWidth, DepthStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                    this.PlayerShadowRect = new Int32Rect(0, 0, DepthStream.FrameWidth, DepthStream.FrameHeight);
                    this.PlayerShadowStride = DepthStream.FrameWidth * 4;
                    this.PlayerShadowBox.Source = this.PlayerShadowBitmap;
                    this.PlayerShadowPixelData = new byte[DepthStream.FrameWidth * DepthStream.FrameHeight * 4];

                    this.PlayerShadowBoolData = new bool[DepthStream.FramePixelDataLength];
                    #region Skeleton
                    this.FrameSkeletons = new Skeleton[this.kinectsensor.SkeletonStream.FrameSkeletonArrayLength];
                    #endregion
                    #endregion

                    this.callibration.mainwindow = this;
                }
                else
                {
                    throw new Exception("利用可能なキネクトが存在しません。");
                }
            }
            else
            {
                throw new Exception("キネクトの初期化ができません。");
            }
        }
        #endregion

        #region ALL FRAME READY

        #region kinect
        /// <summary>
        /// キネクトから配信される全データを解析・実行します
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectsensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame cFrame = e.OpenColorImageFrame())
            using (DepthImageFrame dFrame = e.OpenDepthImageFrame())
            using (SkeletonFrame sFrame = e.OpenSkeletonFrame())
            {
                try
                {
                    if (cFrame != null)
                    {
                        cFrame.CopyPixelDataTo(this.ColorImagePixelData);
                        this.ColorImageBitmap.WritePixels(this.ColorImageRect, this.ColorImagePixelData, this.ColorImageStride, 0);
                    }
                    if (dFrame != null)
                    {
                        dFrame.CopyPixelDataTo(this.DepthImagePixelData_short);
                        dFrame.CopyDepthImagePixelDataTo(this.DepthImagePixelData_DIP);
                        this.DepthImageBitmap.WritePixels(this.DepthImageRect, this.DepthImagePixelData_short, this.DepthImageStride, 0);
                        this.CreatePlayerDepthImage(dFrame, this.DepthImagePixelData_short, this.DepthImagePixelData_DIP);
                        this.kinectsensor.CoordinateMapper.MapColorFrameToDepthFrame(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30, this.DepthImagePixelData_DIP, this.DepthImagePoints);
                        this.kinectsensor.CoordinateMapper.MapDepthFrameToSkeletonFrame(DepthImageFormat.Resolution640x480Fps30, this.DepthImagePixelData_DIP, this.SkeletonPoint_DepthCurrent);
                        this.SetDepthModel();
                    }

                    if (sFrame != null)
                    {
                        Skeleton skeleton;
                        Brush brush1 = Brushes.Red;

                        sFrame.CopySkeletonDataTo(this.FrameSkeletons);
                        this.SkeltonGrid.Children.Clear();

                        this.BoneGroup.Children.Clear();


                        for (int i = 0; i < this.FrameSkeletons.Length; i++)
                        {
                            if (this.FrameSkeletons[i] != null)
                            {
                                skeleton = this.FrameSkeletons[i];
                                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    this.joints = new[] { JointType.Head, JointType.ShoulderCenter,
                                                        JointType.ShoulderLeft,JointType.Spine,JointType.ShoulderRight,
                                                        JointType.ShoulderCenter,JointType.HipCenter,JointType.HipLeft,
                                                        JointType.Spine,JointType.HipRight,JointType.HipCenter};
                                    this.SkeltonGrid.Children.Add(CreateFigure(skeleton, brush1, this.joints, this.SkeltonGrid));
                                    this.joints = new[] {JointType.HipLeft, JointType.KneeLeft,
                                                        JointType.AnkleLeft, JointType.FootLeft};
                                    this.SkeltonGrid.Children.Add(CreateFigure(skeleton, brush1, this.joints, this.SkeltonGrid));
                                    this.joints = new[] {JointType.HipRight, JointType.KneeRight,
                                                        JointType.AnkleRight, JointType.FootRight};
                                    this.SkeltonGrid.Children.Add(CreateFigure(skeleton, brush1, this.joints, this.SkeltonGrid));
                                    this.joints = new[] {JointType.ShoulderLeft, JointType.ElbowLeft,
                                                        JointType.WristLeft, JointType.HandLeft};
                                    this.SkeltonGrid.Children.Add(CreateFigure(skeleton, brush1, this.joints, this.SkeltonGrid));
                                    this.joints = new[] {JointType.ShoulderRight, JointType.ElbowRight,
                                                        JointType.WristRight, JointType.HandRight};
                                    this.SkeltonGrid.Children.Add(CreateFigure(skeleton, brush1, this.joints, this.SkeltonGrid));


                                }
                            }
                        }
                    }

                    lock (this.FrameSkeletons)
                    {
                        this.frameready();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// TODO:kinectが起動時，毎フレーム行うことを記述
        /// </summary>
        private void frameready()
        {
            this.Title_TextBlock.Text = "工程名　：　" + System.Diagnostics.Process.GetCurrentProcess().ProcessName
                            + "     " + this.kinectsensor.AccelerometerGetCurrentReading().X + ","
                            + this.kinectsensor.AccelerometerGetCurrentReading().Y + ","
                            + this.kinectsensor.AccelerometerGetCurrentReading().Z;
            if (this.Is_trytest)
            {
                if (this.TestTimeCurrent < this.TestTimes)
                {
                    this.callibration.Set_PointTest(this.ColorImagePixelData, this.ColorImageRect.Width, this.ColorImageRect.Height, 4, this.TestTimeCurrent);
                    this.TestTimeCurrent++;
                }
                else
                {
                    Cv.DestroyAllWindows();
                    this.TestTimeCurrent = 0;
                    this.Is_trytest = false;
                }
            }
            else if (this.Is_CallibPointZero)
            {
                if (this.TestTimeCurrent < this.TestTimes)
                {
                    this.callibration.Set_Point_Zero(this.ColorImagePixelData, this.ColorImageRect.Width, this.ColorImageRect.Height, 4, this.TestTimeCurrent);
                    this.TestTimeCurrent++;
                }
                else
                {
                    Cv.DestroyAllWindows();

                    this.TestTimeCurrent = 0;
                    this.Is_CallibPointZero = false;
                }
            }
            else if (this.Is_CallibPointX)
            {
                if (this.TestTimeCurrent < this.TestTimes)
                {
                    this.callibration.Set_Point_X(this.ColorImagePixelData, this.ColorImageRect.Width, this.ColorImageRect.Height, 4, this.TestTimeCurrent);
                    this.TestTimeCurrent++;
                }
                else
                {
                    Cv.DestroyAllWindows();

                    this.TestTimeCurrent = 0;
                    this.Is_CallibPointX = false;
                }
            }
            else if (this.Is_CallibPointZ)
            {
                if (this.TestTimeCurrent < this.TestTimes)
                {
                    this.callibration.Set_Point_Z(this.ColorImagePixelData, this.ColorImageRect.Width, this.ColorImageRect.Height, 4, this.TestTimeCurrent);
                    this.TestTimeCurrent++;
                }
                else
                {
                    Cv.DestroyAllWindows();

                    this.TestTimeCurrent = 0;
                    this.Is_CallibPointZ = false;
                }
            }
            this.CheckDirection();
        }

        private double deg;
        private double deg_past;
        private int count_test;
        private float X = 0;
        private float Z = 0;

        private bool BodyDirection;
        /// <summary>
        /// 
        /// </summary>
        private void CheckDirection()
        {
            try
            {
                foreach (var p in this.FrameSkeletons)
                {
                    if (p.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Microsoft.Kinect.JointType jt1 = JointType.ShoulderLeft;
                        Microsoft.Kinect.JointType jt2 = JointType.ShoulderRight;

                        if (p.Joints[jt1].TrackingState != JointTrackingState.NotTracked && p.Joints[jt2].TrackingState != JointTrackingState.NotTracked)
                        {
                            this.X = p.Joints[jt1].Position.X - p.Joints[jt2].Position.X;
                            this.Z = p.Joints[jt1].Position.Z - p.Joints[jt2].Position.Z;
                        }
                    }
                }
                float D = (float)Math.Pow((double)(this.X * this.X + this.Z * this.Z), 0.5);
                if (this.X >= 0)
                {
                    this.deg = Math.Acos(this.Z) / Math.PI * 180;
                }
                else
                {
                    this.deg = -Math.Acos(this.Z) / Math.PI * 180;
                }


                if (Math.Abs(this.deg - this.deg_past) > 5)
                {
                    this.TextBlock_Judge_Update.Text = "!!";
                    this.BodyDirection = !this.BodyDirection;
                    this.count_test = 20;
                }
                if (this.count_test > 0)
                {
                    --this.count_test;
                }
                else
                {
                    this.TextBlock_Judge_Update.Text = "…";
                }

                RotateTransform rot = new RotateTransform(this.deg * (this.BodyDirection ? 1 : -1));
                this.Rectangle_Judge_Allow.RenderTransform = rot;

                this.TextBlock_Judge_Direction.Text = this.BodyDirection == true ? "前" : "後";
                this.deg_past = this.deg;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ExchangeBoneLandR()
        {
            foreach (var p in this.FrameSkeletons)
            {
                if (p.TrackingState == SkeletonTrackingState.Tracked)
                {
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.AnkleLeft], p.Joints[Microsoft.Kinect.JointType.AnkleRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.ElbowLeft], p.Joints[Microsoft.Kinect.JointType.ElbowRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.HandLeft], p.Joints[Microsoft.Kinect.JointType.HandRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.HipLeft], p.Joints[Microsoft.Kinect.JointType.HipRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.KneeLeft], p.Joints[Microsoft.Kinect.JointType.KneeRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.ShoulderLeft], p.Joints[Microsoft.Kinect.JointType.ShoulderRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.WristLeft], p.Joints[Microsoft.Kinect.JointType.WristRight]);
                    this.JointExchange(p.Joints[Microsoft.Kinect.JointType.FootLeft], p.Joints[Microsoft.Kinect.JointType.FootRight]);
                }
            }
        }

        private void JointExchange(Microsoft.Kinect.Joint jointA, Microsoft.Kinect.Joint jointB)
        {
            Joint j = new Joint();
            j.Position = jointA.Position;
            j.TrackingState = jointA.TrackingState;

            jointA.Position = jointB.Position;
            jointA.TrackingState = jointB.TrackingState;

            jointB.Position = j.Position;
            jointB.TrackingState = j.TrackingState;
        }

        private void Button_Judge_LookFront_Click(object sender, RoutedEventArgs e)
        {
            this.LookFrontSetting(true);
        }

        private void LookFrontSetting(bool lookdir)
        {
            this.BodyDirection = lookdir;
        }

        /// <summary>
        /// 指定されたジョイントから形を形成します。
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="brush"></param>
        /// <param name="joints"></param>
        /// <param name="_grid"></param>
        /// <returns></returns>
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints, Grid _grid)
        {
            Polyline figure = new Polyline();
            figure.StrokeThickness = 2;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]], _grid));

                var mesh = new MeshGeometry3D();
                var model = new GeometryModel3D();
                mesh.Positions.Add(new Point3D(0.05, 0.05, 0));
                mesh.Positions.Add(new Point3D(0.05, -0.05, 0));
                mesh.Positions.Add(new Point3D(-0.05, 0.05, 0));
                mesh.Positions.Add(new Point3D(-0.05, -0.05, 0));
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);
                model.Geometry = mesh;
                model.Material = new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(255, 0, 0)));

                //TODO:Change
                model.Transform = new TranslateTransform3D(-skeleton.Joints[joints[i]].Position.X, skeleton.Joints[joints[i]].Position.Y, -skeleton.Joints[joints[i]].Position.Z);
                this.BoneGroup.Children.Add(model);

            }

            return figure;
        }

        /// <summary>
        /// 指定されたジョイントの場所を取得します。
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="_grid"></param>
        /// <returns></returns>
        private Point GetJointPoint(Joint joint, Grid _grid)
        {
            if (this.kinectsensor != null)
            {
                DepthImagePoint point =
                    this.kinectsensor.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.kinectsensor.DepthStream.Format);
                Point p = new Point(point.X, point.Y);

                Grid parents = _grid.Parent as Grid;

                p.X = (p.X * parents.ActualWidth / (double)(this.kinectsensor.DepthStream.FrameWidth));
                p.Y = (p.Y * parents.ActualHeight / (double)(this.kinectsensor.DepthStream.FrameHeight));
                return new Point(p.X, p.Y);
            }
            else
            {
                return new Point(0, 0);
            }

        }

        private void CreatePlayerDepthImage(DepthImageFrame dFrame, short[] p, DepthImagePixel[] Depth)
        {
            int PlayerIndex;
            int DepthBytePerPixel = 4;
            int max = dFrame.MaxDepth;
            int min = dFrame.MinDepth;
            short depth;
            byte intensity;
            for (int i = 0, j = 0; i < p.Length; i++, j += DepthBytePerPixel)
            {
                PlayerIndex = p[i] & DepthImageFrame.PlayerIndexBitmask;
                depth = Depth[i].Depth;
                intensity = (byte)(depth >= min && depth <= max ? depth : 0);
                if (PlayerIndex == 0)
                {
                    this.PlayerShadowPixelData[j] = intensity;
                    this.PlayerShadowPixelData[j + 1] = intensity;
                    this.PlayerShadowPixelData[j + 2] = intensity;
                    this.PlayerShadowBoolData[i] = false;
                }
                else
                {
                    this.PlayerShadowPixelData[j] = intensity;
                    this.PlayerShadowPixelData[j + 1] = intensity;
                    this.PlayerShadowPixelData[j + 2] = intensity;
                    this.PlayerShadowBoolData[i] = true;
                }

            }
            this.PlayerShadowBitmap.WritePixels(this.PlayerShadowRect, this.PlayerShadowPixelData, this.PlayerShadowStride, 0);
        }
        #endregion

        #region opti

        private void m_NatNet_OnFrameReady(NatNetML.FrameOfMocapData data, NatNetML.NatNetClientML client)
        {
            m_FrameQueue.Enqueue(data);
        }

        #endregion

        #endregion

        #region button events
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void KinectLunchButton_Click(object sender, RoutedEventArgs e)
        {
            this.KinectLunch();
        }

        /// <summary>
        /// キネクトを起動して，操作を制限します
        /// </summary>
        private void KinectLunch()
        {
            this.DiscoverKinect();

            this.OptCloseReceiveButton.IsEnabled = false;
            this.OptReceiveButton.IsEnabled = false;

            this.KinectLunchButton.IsEnabled = false;
            this.KinectStopButton.IsEnabled = true;
            this.CIPC_lunchbutton.IsEnabled = true;

            this.IsKinectRunning = true;
        }

        /// <summary>
        /// UIイベント　キネクト終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KinectStopButton_Click(object sender, RoutedEventArgs e)
        {
            this.KinectClose();
        }

        /// <summary>
        /// キネクトを終了して，制限していた操作を解放します
        /// </summary>
        private void KinectClose()
        {
            this.KinectStop();

            this.OptCloseReceiveButton.IsEnabled = false;
            this.OptReceiveButton.IsEnabled = true;

            this.KinectLunchButton.IsEnabled = true;
            this.KinectStopButton.IsEnabled = false;
            this.CIPC_lunchbutton.IsEnabled = false;
        }


        private void CIPC_lunchbutton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CIPCClientを起動します．CIPCServerの起動を確認してください．");
            this.CTS_cipc_shadow = new CancellationTokenSource();
            this.mytask_cipc_shadow = new Task(() => this.Main_Task_cipc(), CTS_cipc_shadow.Token);
            this.mytask_cipc_shadow.Start();
            this.CIPC_lunchbutton.IsEnabled = false;
            this.CIPC_closebutton.IsEnabled = true;
            this.KinectStopButton.IsEnabled = false;

            this.OptCloseReceiveButton.IsEnabled = false;
            this.OptReceiveButton.IsEnabled = false;


            this.DepthDataButton.IsEnabled = false;
            this.SkeletonDataButton.IsEnabled = false;
            this.OptitrackButton.IsEnabled = false;
        }

        /// <summary>
        /// cipc終了ボタンが押されたときの挙動を記述します．
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CIPC_closebutton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("CIPCClientを終了します．");
            this.cipc_stop();
            this.CIPC_lunchbutton.IsEnabled = true;
            this.CIPC_closebutton.IsEnabled = false;

            if (this.kinectsensor != null)
            {
                if (this.kinectsensor.IsRunning)
                {
                    this.KinectStopButton.IsEnabled = true;
                }
            }


            if (this.IsOptRunning)
            {
                this.OptCloseReceiveButton.IsEnabled = true;
            }

            this.DepthDataButton.IsEnabled = true;
            this.SkeletonDataButton.IsEnabled = true;
            this.OptitrackButton.IsEnabled = true;
        }

        /// <summary>
        /// cipcを終了します．
        /// </summary>
        private void cipc_stop()
        {
            try
            {
                this.CTS_cipc_shadow.Cancel();
                Thread.Sleep(100);
                this.cipc_shadow.Close();
                this.cipc_shadow = null;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SkeletonDataButtonChecked(object sender, RoutedEventArgs e)
        {
            this.senddatatype = SendDataType.Skeleton;
            this.GroupBox_Sendto_SKST.IsEnabled = false;
            this.GroupBox_Sendto_CIPCServer.IsEnabled = true;

            this.GroupBox_ReceiveFrom_CIPCServer.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Kinect.IsEnabled = true;
            this.GroupBox_ReceiveFrom_Optitrack.IsEnabled = false;

            this.sendto = SendTo.CIPCServer;
            this.receivefrom = ReceiveFrom.Kinect;

            this.StatesTextUpdate();
        }



        private void DepthDataButtonChecked(object sender, RoutedEventArgs e)
        {
            this.senddatatype = SendDataType.Depth;
            this.GroupBox_Sendto_SKST.IsEnabled = false;
            this.GroupBox_Sendto_CIPCServer.IsEnabled = true;

            this.GroupBox_ReceiveFrom_CIPCServer.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Kinect.IsEnabled = true;
            this.GroupBox_ReceiveFrom_Optitrack.IsEnabled = false;

            this.sendto = SendTo.CIPCServer;
            this.receivefrom = ReceiveFrom.Kinect;
            this.StatesTextUpdate();
        }

        private void OptitrackDataButtonChecked(object sender, RoutedEventArgs e)
        {
            this.senddatatype = SendDataType.optidata;
            this.GroupBox_Sendto_SKST.IsEnabled = false;
            this.GroupBox_Sendto_CIPCServer.IsEnabled = true;

            this.GroupBox_ReceiveFrom_CIPCServer.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Kinect.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Optitrack.IsEnabled = true;

            this.sendto = SendTo.CIPCServer;
            this.receivefrom = ReceiveFrom.OptiTrack;
            this.StatesTextUpdate();
        }
        private void RadioButton_ShadowKinectSensorTerminal_Checked(object sender, RoutedEventArgs e)
        {
            this.senddatatype = SendDataType.SKSTdata;
            this.GroupBox_Sendto_SKST.IsEnabled = true;
            this.GroupBox_Sendto_CIPCServer.IsEnabled = false;

            this.GroupBox_ReceiveFrom_CIPCServer.IsEnabled = false;
            this.GroupBox_ReceiveFrom_Kinect.IsEnabled = true;
            this.GroupBox_ReceiveFrom_Optitrack.IsEnabled = false;

            this.sendto = SendTo.ShadowKinectSensorTerminal;
            this.receivefrom = ReceiveFrom.Kinect;

            this.StatesTextUpdate();
        }
        private void StatesTextUpdate()
        {
            this.TextBlock_State.Text = "送信先：" + this.sendto.ToString() + "\n"
                + "データ元：" + this.receivefrom.ToString();
        }

        private void OptReceiveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.senddatatype = SendDataType.optidata;

                String strMachineName = Dns.GetHostName();
                IPHostEntry ipHost = Dns.GetHostEntry(strMachineName);
                if (this.m_NatNet != null)
                {
                    this.m_NatNet.Uninitialize();
                }
                this.m_NatNet = new NatNetML.NatNetClientML(0);

                // [NatNet] set a "Frame Ready" callback function (event handler) handler that will be
                // called by NatNet when NatNet receives a frame of data from the server application
                this.m_NatNet.OnFrameReady += new NatNetML.FrameReadyEventHandler(m_NatNet_OnFrameReady);

                if (m_NatNet.Initialize(this.opt_localAdress.Text, this.opt_ServerAddress.Text) != 0)
                {
                    throw new Exception("初期化失敗");
                }
                else
                {
                    MessageBox.Show("初期化成功．接続を開始します");
                }
                if (m_NatNet.GetServerDescription(desc) != 0)
                {
                    throw new Exception("接続失敗");
                }
                else
                {
                    MessageBox.Show("接続成功．通信を開始します");
                }

                //rigidBodyDefinitionsを更新
                List<NatNetML.DataDescriptor> dataDescriptors = new List<NatNetML.DataDescriptor>();
                m_rigidBodyDefinitions.Clear();
                dataDescriptors = m_NatNet.GetDataDescriptions();

                foreach (NatNetML.DataDescriptor d in dataDescriptors)
                {
                    if (d.type == (int)NatNetML.DataDescriptorType.eSkeletonData)
                    {
                        NatNetML.Skeleton sk = (NatNetML.Skeleton)d;

                        for (int i = 0; i < sk.nRigidBodies; i++)
                        {
                            NatNetML.RigidBody rb = sk.RigidBodies[i];
                            //MessageBox.Show(String.Format(rb.Name + "x:{0}, y:{1}, z:{2}",rb.offsetx, rb.offsety, rb.offsetz));

                            foreach (var optiBoneWithKinectID in m_optiBoneWithKinectIDs)
                            {
                                //kinectに該当する箇所を探す
                                if (0 <= rb.Name.IndexOf(optiBoneWithKinectID.Name))
                                {
                                    RigidBodyWithKinectID rigidBodyWithKinectID = new RigidBodyWithKinectID();
                                    rigidBodyWithKinectID.KinectID = optiBoneWithKinectID.KinectId;
                                    rigidBodyWithKinectID.RigidBody = rb;

                                    int uniqueId = 1000 * sk.ID + rb.ID;
                                    m_rigidBodyDefinitions.Add(uniqueId, rigidBodyWithKinectID);
                                }
                            }
                        }

                    }
                }
                //----
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            this.OptReceiveButton.IsEnabled = false;
            this.OptCloseReceiveButton.IsEnabled = true;
            this.KinectLunchButton.IsEnabled = false;
            this.CIPC_lunchbutton.IsEnabled = true;
            this.IsOptRunning = true;
        }

        private void OptCloseReceiveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                this.senddatatype = SendDataType.Skeleton;
                this.m_NatNet.OnFrameReady -= this.m_NatNet_OnFrameReady;
                this.OptReceiveButton.IsEnabled = true;
                this.OptCloseReceiveButton.IsEnabled = false;
                this.KinectLunchButton.IsEnabled = true;
                this.CIPC_lunchbutton.IsEnabled = false;
                this.IsOptRunning = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region main CIPC

        private void Main_Task_cipc()
        {
            this.SetupInTask();

            while (!this.CTS_cipc_shadow.IsCancellationRequested)
            {
                try
                {
                    switch (this.senddatatype)
                    {
                        case SendDataType.Depth:
                            this.CIPC_SEND_DEPTH();
                            break;
                        case SendDataType.Skeleton:
                            this.CIPC_SEND_SKELETON();
                            break;
                        case SendDataType.optidata:
                            this.CIPC_SEND_OPTI();
                            break;
                        case SendDataType.SKSTdata:
                            this.DataCreate_SKST();
                            break;
                        default:
                            throw new Exception("fatal error");
                    }

                    switch (this.sendto)
                    {
                        case SendTo.CIPCServer:
                            this.SendToCIPCServer();
                            break;
                        case SendTo.ShadowKinectSensorTerminal:
                            this.SendToSKST();
                            break;
                    }
                    this.FPS_maintask.Adjust();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }

            }
        }



        private void SendToCIPCServer()
        {
            try
            {
                if (this.receivefrom == ReceiveFrom.Kinect && this.IsKinectRunning == false)
                {
                    return;
                }
                if (this.receivefrom == ReceiveFrom.OptiTrack && this.IsOptRunning == false)
                {
                    return;
                }
                if (this.data_shadow == null)
                {
                    return;
                }
                if (this.data_shadow.Length == 0)
                {
                    return;
                }

                this.cipc_shadow.Update(ref this.data_shadow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendToSKST()
        {
            try
            {
                if (this.receivefrom == ReceiveFrom.Kinect && this.IsKinectRunning == false)
                {
                    return;
                }
                if (this.receivefrom == ReceiveFrom.OptiTrack && this.IsOptRunning == false)
                {
                    return;
                }
                if (this.data_shadow == null)
                {
                    return;
                }
                if (this.data_shadow.Length == 0)
                {
                    return;
                }
                this.SKST.udplient.Send(this.data_shadow, this.data_shadow.Length, this.SKST.RemoteEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SetupInTask()
        {
            switch (this.sendto)
            {
                case SendTo.CIPCServer:
                    try
                    {
                        this.FPS_maintask = new FPSAdjuster.FPSAdjuster();
                        try
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    this.FPS_maintask.Fps = int.Parse(this.tbx_fps.Text);
                                }
                                catch (Exception ex)
                                {
                                    this.FPS_maintask.Fps = this.fps;
                                }
                                this.cipc_shadow = new CIPC_CS.CLIENT.CLIENT(int.Parse(tbx_myPort.Text), tbx_remoteIP.Text, int.Parse(tbx_remotePort.Text), name, this.FPS_maintask.Fps);
                            }));
                            Thread.Sleep(100);
                            this.FPS_maintask.Start();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        this.cipc_shadow.Setup(CIPC_CS.CLIENT.MODE.Sender);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;
                case SendTo.ShadowKinectSensorTerminal:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack_SKST(IAsyncResult ar)
        {
            try
            {
                if (this.SKST.udplient != null)
                {
                    this.SKST.data = this.SKST.udplient.EndReceive(ar, ref this.SKST.RemoteEP);
                    UDP_PACKETS_CODER.UDP_PACKETS_DECODER dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER();
                    dec.Source = this.SKST.data;
                    this.CS.dx = (-1) * dec.get_float();
                    this.CS.dy = dec.get_float();
                    this.CS.dz = dec.get_float();
                    this.CS.ry = dec.get_float() * (float)Math.PI / 180;


                    //Console.WriteLine(dec.get_float().ToString() + dec.get_float().ToString() + dec.get_float().ToString() + dec.get_float().ToString());
                    this.SKST.udplient.BeginReceive(ReceiveCallBack_SKST, this.SKST.udplient);
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                this.SKST.udplient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// opti data send
        /// TODO:人数直す
        /// </summary>
        private void CIPC_SEND_OPTI()
        {
            try
            {


                if (m_FrameQueue.Count != 0)
                {
                    m_FrameOfData = m_FrameQueue.Dequeue();
                    m_FrameQueue.Clear();

                    Vector3D[] buffer = new Vector3D[20];

                    this.enc_shadow = null;
                    this.enc_shadow = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                    this.enc_shadow += 3;
                    this.enc_shadow += 1;

                    foreach (NatNetML.SkeletonData sk in m_FrameOfData.Skeletons)
                    {


                        foreach (NatNetML.RigidBodyData rbd in sk.RigidBodies)
                        {
                            int uniqueId = 1000 * HighWord(rbd.ID) + LowWord(rbd.ID);

                            RigidBodyWithKinectID rigidBodyWithKinectID;

                            if (m_rigidBodyDefinitions.TryGetValue(uniqueId, out rigidBodyWithKinectID))
                            {

                                int kinectId = rigidBodyWithKinectID.KinectID;



                                if (kinectId == 6 || kinectId == 10)
                                {
                                    //rotate offset

                                    float offsetX = rigidBodyWithKinectID.RigidBody.offsetx;
                                    float offsetY = rigidBodyWithKinectID.RigidBody.offsety;
                                    float offsetZ = rigidBodyWithKinectID.RigidBody.offsetz;
                                    Vector3D newVector = RotateFunctions.Rotate(offsetX, offsetY, offsetZ, rbd.qx, rbd.qy, rbd.qz, rbd.qw);


                                    //送信
                                    Vector3D pos = new Vector3D
                                    {
                                        x = rbd.x + newVector.x / 2.0f,
                                        y = rbd.y + newVector.y / 2.0f,
                                        z = rbd.z + newVector.z / 2.0f
                                    };

                                    buffer[kinectId + 1] = pos;
                                }


                                if (kinectId == 3)
                                {
                                    Vector3D pos = new Vector3D
                                    {
                                        x = rbd.x,
                                        y = rbd.y,
                                        z = rbd.z
                                    };

                                    buffer[kinectId - 1] = pos;

                                    //rotate offset
                                    float offsetX = 0.0f;
                                    float offsetY = 20.0f;
                                    float offsetZ = 0.0f;
                                    Vector3D newVector = RotateFunctions.Rotate(offsetX, offsetY, offsetZ, rbd.qx, rbd.qy, rbd.qz, rbd.qw);

                                    //送信

                                    pos = new Vector3D
                                    {
                                        x = rbd.x + newVector.x / 2.0f,
                                        y = rbd.y + newVector.y / 2.0f,
                                        z = rbd.z + newVector.z / 2.0f
                                    };

                                    buffer[kinectId] = pos;
                                }
                                else
                                {
                                    Vector3D pos = new Vector3D
                                    {
                                        x = rbd.x,
                                        y = rbd.y,
                                        z = rbd.z
                                    };

                                    buffer[kinectId] = pos;
                                }

                            }
                        }




                    }
                    this.enc_shadow += 0;
                    for (int i = 0; i < 20; i++)
                    {
                        this.enc_shadow += buffer[i].x;
                        this.enc_shadow += buffer[i].y;
                        this.enc_shadow += buffer[i].z;
                        this.enc_shadow += 2;
                    }

                    //this.enc_shadow += (float)0.0f;
                    //this.enc_shadow += (float)0.0f;
                    //this.enc_shadow += (float)0.0f;
                    //byte[] data = this.enc_shadow.data;
                    //this.cipc_shadow.Update(ref data);
                    //Thread.Sleep(5);
                    this.data_shadow = this.enc_shadow.data;

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.TargetSite + ex.Source);
            }
        }

        /// <summary>
        /// CIPCから深度情報を送信します．
        /// </summary>
        private void CIPC_SEND_DEPTH()
        {
            try
            {

                this.enc_shadow = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                foreach (var p in this.DepthImagePixelData_short)
                {
                    this.enc_shadow += p;
                }
                byte[] arraydata = this.enc_shadow.data;
                int divnum = this.enc_shadow.data.Length / sendsize;
                for (int t = 0; t < divnum; t++)
                {
                    var senddata = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                    senddata += (int)1;
                    senddata += (int)enc_shadow.data.Length;
                    senddata += (int)this.DepthImageRect.Width;
                    senddata += (int)this.DepthImageRect.Height;
                    senddata += (int)sendsize;
                    senddata += (int)t;
                    for (int k = t * sendsize; k < (t + 1) * sendsize; k++)
                    {
                        byte a = arraydata[k];
                        senddata += a;

                    }
                    this.data_shadow = senddata.data;
                    //this.cipc_shadow.Update(ref this.data_shadow);
                    //Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.TargetSite + ex.Source);
            }
        }

        /// <summary>
        /// CIPCからSKELETONDATAを送信します．
        /// </summary>
        private void CIPC_SEND_SKELETON()
        {
            try
            {
                this.TrackingLevel = 0;
                this.enc_shadow = null;
                this.enc_shadow = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                this.enc_shadow += 2;
                this.enc_shadow += this.FrameSkeletons.Length;
                for (int i = 0; i < FrameSkeletons.Length; i++)
                {
                    this.enc_shadow += i;
                    this.make_send_joint(i, JointType.HipCenter);
                    this.make_send_joint(i, JointType.Spine);
                    this.make_send_joint(i, JointType.ShoulderCenter);
                    this.make_send_joint(i, JointType.Head);
                    this.make_send_joint(i, JointType.ShoulderLeft);
                    this.make_send_joint(i, JointType.ElbowLeft);
                    this.make_send_joint(i, JointType.WristLeft);
                    this.make_send_joint(i, JointType.HandLeft);
                    this.make_send_joint(i, JointType.ShoulderRight);
                    this.make_send_joint(i, JointType.ElbowRight);
                    this.make_send_joint(i, JointType.WristRight);
                    this.make_send_joint(i, JointType.HandRight);
                    this.make_send_joint(i, JointType.HipLeft);
                    this.make_send_joint(i, JointType.KneeLeft);
                    this.make_send_joint(i, JointType.AnkleLeft);
                    this.make_send_joint(i, JointType.FootLeft);
                    this.make_send_joint(i, JointType.HipRight);
                    this.make_send_joint(i, JointType.KneeRight);
                    this.make_send_joint(i, JointType.AnkleRight);
                    this.make_send_joint(i, JointType.FootRight);
                }
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().X;
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().Y;
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().Z;
                this.data_shadow = this.enc_shadow.data;
                //byte[] data = this.enc_shadow.data;
                //this.cipc_shadow.Update(ref data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.TargetSite + ex.Source);
            }
        }

        private void make_send_joint(int index, JointType jt)
        {
            try
            {
                switch (this.sendto)
                {
                    case SendTo.ShadowKinectSensorTerminal:
                        float x = (float)this.FrameSkeletons[index].Joints[jt].Position.X;
                        float y = (float)this.FrameSkeletons[index].Joints[jt].Position.Y;
                        float z = (float)this.FrameSkeletons[index].Joints[jt].Position.Z;

                        float _x = x;
                        float _y = (float)(Math.Cos(-(float)this.CS.rx) * y - Math.Sin(-(float)this.CS.rx) * z);
                        float _z = (float)(Math.Sin(-(float)this.CS.rx) * y + Math.Cos(-(float)this.CS.rx) * z);

                        x = (float)(Math.Cos((float)this.CS.rz) * _x - Math.Sin((float)this.CS.rz) * _y);
                        y = (float)(Math.Sin((float)this.CS.rz) * _x + Math.Cos((float)this.CS.rz) * _y);
                        z = _z;

                        _x = (float)(Math.Cos((float)this.CS.ry) * x - Math.Sin((float)this.CS.ry) * z);
                        _y = y;
                        _z = (float)(Math.Sin((float)this.CS.ry) * x + Math.Cos((float)this.CS.ry) * z);

                        this.enc_shadow += _x + this.CS.dx;
                        this.enc_shadow += _y + this.CS.dy;
                        this.enc_shadow += _z + this.CS.dz;
                        break;
                    case SendTo.CIPCServer:
                        this.enc_shadow += (float)this.FrameSkeletons[index].Joints[jt].Position.X;
                        this.enc_shadow += (float)this.FrameSkeletons[index].Joints[jt].Position.Y;
                        this.enc_shadow += (float)this.FrameSkeletons[index].Joints[jt].Position.Z;
                        break;
                    default:
                        break;
                }
                switch (this.FrameSkeletons[index].Joints[jt].TrackingState)
                {
                    case JointTrackingState.NotTracked:
                        this.enc_shadow += 0;
                        this.TrackingLevel += 0;
                        break;
                    case JointTrackingState.Inferred:
                        this.enc_shadow += 1;
                        this.TrackingLevel += 1;
                        break;
                    case JointTrackingState.Tracked:
                        this.enc_shadow += 2;
                        this.TrackingLevel += 2;
                        break;
                    default:
                        this.enc_shadow += 9;
                        this.TrackingLevel += 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// SKSTに送信するデータを作成します。
        /// </summary>
        private void DataCreate_SKST()
        {
            try
            {
                if (this.kinectsensor == null)
                    return;
                if (!this.kinectsensor.IsRunning)
                    return;
                this.TrackingLevel = 0;
                this.enc_shadow = null;
                this.enc_shadow = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                this.enc_shadow += 2;
                this.enc_shadow += this.FrameSkeletons.Length;
                if (true)//bodydirectionを入れると幸せになる場所
                {
                    for (int i = 0; i < FrameSkeletons.Length; i++)
                    {
                        this.enc_shadow += i;
                        this.make_send_joint(i, JointType.HipCenter);
                        this.make_send_joint(i, JointType.Spine);
                        this.make_send_joint(i, JointType.ShoulderCenter);
                        this.make_send_joint(i, JointType.Head);
                        this.make_send_joint(i, JointType.ShoulderLeft);
                        this.make_send_joint(i, JointType.ElbowLeft);
                        this.make_send_joint(i, JointType.WristLeft);
                        this.make_send_joint(i, JointType.HandLeft);
                        this.make_send_joint(i, JointType.ShoulderRight);
                        this.make_send_joint(i, JointType.ElbowRight);
                        this.make_send_joint(i, JointType.WristRight);
                        this.make_send_joint(i, JointType.HandRight);
                        this.make_send_joint(i, JointType.HipLeft);
                        this.make_send_joint(i, JointType.KneeLeft);
                        this.make_send_joint(i, JointType.AnkleLeft);
                        this.make_send_joint(i, JointType.FootLeft);
                        this.make_send_joint(i, JointType.HipRight);
                        this.make_send_joint(i, JointType.KneeRight);
                        this.make_send_joint(i, JointType.AnkleRight);
                        this.make_send_joint(i, JointType.FootRight);
                    }
                }
                else
                {
                    for (int i = 0; i < FrameSkeletons.Length; i++)
                    {
                        this.enc_shadow += i;
                        this.make_send_joint(i, JointType.HipCenter);
                        this.make_send_joint(i, JointType.Spine);
                        this.make_send_joint(i, JointType.ShoulderCenter);
                        this.make_send_joint(i, JointType.Head);

                        this.make_send_joint(i, JointType.ShoulderRight);
                        this.make_send_joint(i, JointType.ElbowRight);
                        this.make_send_joint(i, JointType.WristRight);
                        this.make_send_joint(i, JointType.HandRight);

                        this.make_send_joint(i, JointType.ShoulderLeft);
                        this.make_send_joint(i, JointType.ElbowLeft);
                        this.make_send_joint(i, JointType.WristLeft);
                        this.make_send_joint(i, JointType.HandLeft);

                        this.make_send_joint(i, JointType.HipRight);
                        this.make_send_joint(i, JointType.KneeRight);
                        this.make_send_joint(i, JointType.AnkleRight);
                        this.make_send_joint(i, JointType.FootRight);

                        this.make_send_joint(i, JointType.HipLeft);
                        this.make_send_joint(i, JointType.KneeLeft);
                        this.make_send_joint(i, JointType.AnkleLeft);
                        this.make_send_joint(i, JointType.FootLeft);

                    }
                }
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().X;
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().Y;
                this.enc_shadow += (float)kinectsensor.AccelerometerGetCurrentReading().Z;
                this.enc_shadow += (int)this.TrackingLevel;
                this.data_shadow = this.enc_shadow.data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        private void RadioButton_ReceiveFrom_Kinect_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Sendto_SKST_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region FPSの指定と開始
                this.FPS_maintask = new FPSAdjuster.FPSAdjuster();
                try
                {
                    this.FPS_maintask.Fps = int.Parse(this.tbx_SKST_fps.Text);
                }
                catch (Exception ex)
                {
                    this.FPS_maintask.Fps = this.fps;
                }
                this.FPS_maintask.Start();
                #endregion

                this.SKST.memorystream = new System.IO.MemoryStream();
                this.SKST.tcpclient = new System.Net.Sockets.TcpClient();
                this.SKST.tcpclient.Connect(this.TextBox_SKST_remoteIP.Text, int.Parse(this.TextBox_SKST_TCP_remotePort.Text));
                this.SKST.tcpclient.SendBufferSize = 65536;
                this.SKST.networkstream = this.SKST.tcpclient.GetStream();
                
                this.SKST.CTS = new CancellationTokenSource();
                this.SKST.receivetask = new Task(this.receiveSKST, this.SKST.CTS.Token);
                this.SKST.receivetask.Start();

                #region UDPの接続先指定と受信開始
                this.SKST.RemoteEP = new IPEndPoint(IPAddress.Parse(this.TextBox_SKST_remoteIP.Text), int.Parse(this.TextBox_SKST_remotePort.Text));
                this.SKST.udplient = new System.Net.Sockets.UdpClient(int.Parse(this.TextBox_SKST_BindPort.Text));
                this.SKST.udplient.BeginReceive(this.ReceiveCallBack_SKST, this.SKST.udplient);
                #endregion
                #region タスクの起動　SKSTへ送信するタスク
                this.CTS_cipc_shadow = new CancellationTokenSource();
                this.mytask_cipc_shadow = new Task(() => this.Main_Task_cipc(), CTS_cipc_shadow.Token);
                this.mytask_cipc_shadow.Start();
                #endregion

                #region UI 制限
                this.Button_Sendto_SKST_Start.IsEnabled = false;
                this.Button_Sendto_SKST_Stop.IsEnabled = true;
                this.KinectStopButton.IsEnabled = false;
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void receiveSKST(object obj)
        {
            try
            {
                while (!this.SKST.CTS.IsCancellationRequested)
                {
                    if (this.SKST.tcpclient.Available > 0)
                    {
                        byte[] resdata = new byte[256];
                        this.SKST.memorystream = new System.IO.MemoryStream();
                        while (this.SKST.tcpclient.Available > 0)
                        {
                            int ressize = this.SKST.networkstream.Read(resdata, 0, resdata.Length);

                            if (ressize == 0)
                            {
                                Console.WriteLine("強制切断されました");
                                this.SKST.CTS.Cancel();
                                break;
                            }
                            this.SKST.memorystream.Write(resdata, 0, ressize);

                        }
                        Console.WriteLine(this.SKST.memorystream.Length);
                        byte[] data = this.SKST.memorystream.ToArray();
                        this.DataConvert_SKST_tcp(data);
                        this.SKST.memorystream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DataConvert_SKST_tcp(byte[] data)
        {
            try
            {
                Console.WriteLine(data.Length);
                UDP_PACKETS_CODER.UDP_PACKETS_DECODER dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER(data);

                string str = dec.get_string();
                Console.WriteLine(str);
                if (str == "LightCallib_Zero")
                {
                    this.Is_CallibPointZero = true;
                }
                else if (str == "LightCallib_X")
                {
                    this.Is_CallibPointX = true;
                }
                else if (str == "LightCallib_Z")
                {
                    this.Is_CallibPointZ = true;
                }
                else if (str == "LightCallib_Calc")
                {
                    Callibration.CallibrationPallameter cp = new Callibration.CallibrationPallameter();
                    cp = this.callibration.Callcurate();
                    this.CS = cp;
                }
                else if (str == "KinectStop")
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.KinectStop();
                    }));
                }
                else if (str == "KinectStart")
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.KinectLunch();
                    }));
                }
                else if (str == "Capture")
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Create3DmodelandSendSKST();
                    }));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Create3DmodelandSendSKST()
        {
            try
            {
                this.TabControl_View.SelectedIndex = 1;
                this.TabControl_Modelview.SelectedIndex = 0;

                Thread.Sleep(100);

                this.CaptureModel();

                this.TabControl_View.SelectedIndex = 1;
                this.TabControl_Modelview.SelectedIndex = 1;
                Thread.Sleep(200);
                this.SendSKST_3Dmodel_Pos_div();
                Thread.Sleep(200);
                this.SendSKST_3Dmodel_Indecsis();
                Thread.Sleep(200);
                this.SendSKST_3Dmodel_Skeleton();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendSKST_3Dmodel_Skeleton()
        {
            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string str = "3DModel3";
            enc += str;
            if (this.skeleton_3Dmodel != null)
            {
                enc += (int)this.skeleton_3Dmodel.Joints.Count;
            }
            else
            {
                enc += (int)0;
            }
            Console.WriteLine("clear");
            if (this.skeleton_3Dmodel != null)
            {
                for (int i = 0; i < this.skeleton_3Dmodel.Joints.Count; i++)
                {

                    float x = (float)this.skeleton_3Dmodel.Joints[(JointType)i].Position.X;
                    float y = (float)this.skeleton_3Dmodel.Joints[(JointType)i].Position.Y;
                    float z = (float)this.skeleton_3Dmodel.Joints[(JointType)i].Position.Z;

                    float _x = x;
                    float _y = (float)(Math.Cos(-(float)this.CS.rx) * y - Math.Sin(-(float)this.CS.rx) * z);
                    float _z = (float)(Math.Sin(-(float)this.CS.rx) * y + Math.Cos(-(float)this.CS.rx) * z);


                    x = (float)(Math.Cos((float)this.CS.rz) * _x - Math.Sin((float)this.CS.rz) * _y);
                    y = (float)(Math.Sin((float)this.CS.rz) * _x + Math.Cos((float)this.CS.rz) * _y);
                    z = _z;

                    _x = (float)(Math.Cos((float)this.CS.ry) * x - Math.Sin((float)this.CS.ry) * z);
                    _y = y;
                    _z = (float)(Math.Sin((float)this.CS.ry) * x + Math.Cos((float)this.CS.ry) * z);

                    enc += _x + this.CS.dx;
                    enc += _y + this.CS.dy;
                    enc += _z + this.CS.dz;
                }
            }
            Console.WriteLine("Finish");

            byte[] data = enc.data;
            this.SKST.networkstream.Write(data, 0, data.Length);

        }

        private void SendSKST_3Dmodel_Indecsis()
        {
            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string str = "3DModel2";
            enc += str;
            enc += (int)this.mesh_3dmodel.TriangleIndices.Count;
            Console.WriteLine("clear");
            for (int i = 0; i < this.mesh_3dmodel.TriangleIndices.Count; i++)
            {
                enc += (int)this.mesh_3dmodel.TriangleIndices[i];
            }
            Console.WriteLine(this.mesh_3dmodel.TriangleIndices.Count.ToString());
            byte[] data = enc.data;
            this.SKST.networkstream.Write(data, 0, data.Length);
        }

        private void SendSKST_3Dmodel_Pos_div()
        {
            //TODO:60kbyte以下
            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string str = "3DModel1";
            enc += str;
            enc += (int)this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel / this.databit_3Dmodel;

            Console.WriteLine("clear");
            for (int i = 0; i < this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel / this.databit_3Dmodel; i++)
            {
                float x = (float)this.mesh_3dmodel.Positions[i].X;
                float y = (float)this.mesh_3dmodel.Positions[i].Y;
                float z = (float)this.mesh_3dmodel.Positions[i].Z;

                float _x = x;
                float _y = (float)(Math.Cos(-(float)this.CS.rx) * y - Math.Sin(-(float)this.CS.rx) * z);
                float _z = (float)(Math.Sin(-(float)this.CS.rx) * y + Math.Cos(-(float)this.CS.rx) * z);

                x = (float)(Math.Cos((float)this.CS.rz) * _x - Math.Sin((float)this.CS.rz) * _y);
                y = (float)(Math.Sin((float)this.CS.rz) * _x + Math.Cos((float)this.CS.rz) * _y);
                z = _z;

                _x = (float)(Math.Cos((float)this.CS.ry) * x - Math.Sin((float)this.CS.ry) * z);
                _y = y;
                _z = (float)(Math.Sin((float)this.CS.ry) * x + Math.Cos((float)this.CS.ry) * z);

                enc += _x + this.CS.dx;
                enc += _y + this.CS.dy;
                enc += _z + this.CS.dz;
            }
            Console.WriteLine(this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel / this.databit_3Dmodel);
            Console.WriteLine(enc.data.Length);
            byte[] data = enc.data;

            int datasize = data.Length;
            const int OnePacketLength = 614400;
            //分割数
            int div_num = datasize / OnePacketLength;
            //あまり
            int mod_num = datasize % OnePacketLength;
            //あまりがある場合は＋１
            div_num = (mod_num == 0 ? div_num : div_num + 1);

            byte[][] DivinedData = new byte[div_num][];
            for (int i = 0; i < div_num; i++)
            {
                DivinedData[i] = (i < div_num -1 ? new byte[OnePacketLength] : new byte[mod_num]);
                for (int j = 0; j < DivinedData[i].Length; j++)
                {
                    DivinedData[i][j] = data[i * OnePacketLength + j];
                }
            }

            this.SendSKST_DivData(DivinedData);
        }

        private void SendSKST_DivData(byte[][] DivinedData)
        {
            if (DivinedData == null)
            {
                return;
            }
            if (DivinedData.Length == 0)
            {
                return;
            }

            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string str = "DivStart";
            enc += str;
            enc += DivinedData.Length;//分割数
            enc += DivinedData[0].Length;//一パケットあたりの長さ
            enc += DivinedData[DivinedData.Length - 1].Length;//あまり
            byte[] data = enc.data;
            this.SKST.networkstream.Write(data, 0, data.Length);

            Thread.Sleep(200);
            for (int i = 0; i < DivinedData.Length; i++)
            {
                UDP_PACKETS_CODER.UDP_PACKETS_ENCODER _enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                string _str = "DivData";
                _enc += _str;
                _enc += i;
                _enc += DivinedData[i];
                byte[] _data = _enc.data;
                this.SKST.networkstream.Write(_data, 0, _data.Length);
                Thread.Sleep(200);
            }

            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER __enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string __str = "DivFinish";
            __enc += __str;
            byte[] __data = __enc.data;
            this.SKST.networkstream.Write(__data, 0, __data.Length);
        }


        private void SendSKST_3Dmodel_Pos()
        {
            UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
            string str = "3DModel1";
            enc += str;
            enc += (int)this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel;

            Console.WriteLine("clear");
            for (int i = 0; i < this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel; i++)
            {
                float x = (float)this.mesh_3dmodel.Positions[i].X;
                float y = (float)this.mesh_3dmodel.Positions[i].Y;
                float z = (float)this.mesh_3dmodel.Positions[i].Z;

                float _x = x;
                float _y = (float)(Math.Cos(-(float)this.CS.rx) * y - Math.Sin(-(float)this.CS.rx) * z);
                float _z = (float)(Math.Sin(-(float)this.CS.rx) * y + Math.Cos(-(float)this.CS.rx) * z);

                x = (float)(Math.Cos((float)this.CS.rz) * _x - Math.Sin((float)this.CS.rz) * _y);
                y = (float)(Math.Sin((float)this.CS.rz) * _x + Math.Cos((float)this.CS.rz) * _y);
                z = _z;

                _x = (float)(Math.Cos((float)this.CS.ry) * x - Math.Sin((float)this.CS.ry) * z);
                _y = y;
                _z = (float)(Math.Sin((float)this.CS.ry) * x + Math.Cos((float)this.CS.ry) * z);

                enc += _x + this.CS.dx;
                enc += _y + this.CS.dy;
                enc += _z + this.CS.dz;
            }
            Console.WriteLine(this.mesh_3dmodel.Positions.Count / this.databit_3Dmodel);
            Console.WriteLine(enc.data.Length);
            byte[] data = enc.data;
            this.SKST.networkstream.Write(data, 0, data.Length);
        }

        private void Button_Sendto_SKST_Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CTS_cipc_shadow.Cancel();

                this.mytask_cipc_shadow.Wait(100);

                if (this.SKST.udplient != null)
                {
                    this.SKST.udplient.Close();
                    this.SKST.udplient = null;
                }

                this.SKST.CTS.Cancel();
                this.SKST.receivetask.Wait();

                this.SKST.networkstream.Close();
                this.SKST.tcpclient.Close();
                this.SKST.memorystream.Close();

                this.CIPC_lunchbutton.IsEnabled = true;
                this.CIPC_closebutton.IsEnabled = false;

                this.Button_Sendto_SKST_Start.IsEnabled = true;
                this.Button_Sendto_SKST_Stop.IsEnabled = false;
                this.KinectStopButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }




        private void Slider_Model_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Update_CameraPos();
        }

        private void Update_CameraPos()
        {
            try
            {
                double p = this.Slider_Rotate.Value / 180 * Math.PI;
                this.Camera_1.Position = new System.Windows.Media.Media3D.Point3D(this.Slider_Length.Value * Math.Cos(p) + this.Slider_X.Value, this.Slider_Height.Value, this.Slider_Length.Value * Math.Sin(p) + Slider_Z.Value);
                this.Camera_1.LookDirection = new System.Windows.Media.Media3D.Vector3D(-this.Slider_Length.Value * Math.Cos(p), -this.Slider_Height.Value, -this.Slider_Length.Value * Math.Sin(p));
                this.Camera_2.Position = new System.Windows.Media.Media3D.Point3D(this.Slider_Length.Value * Math.Cos(p) + this.Slider_X.Value, this.Slider_Height.Value, this.Slider_Length.Value * Math.Sin(p) + Slider_Z.Value);
                this.Camera_2.LookDirection = new System.Windows.Media.Media3D.Vector3D(-this.Slider_Length.Value * Math.Cos(p), -this.Slider_Height.Value, -this.Slider_Length.Value * Math.Sin(p));

            }
            catch (Exception ex)
            {

            }
        }

        public int databit = 20;
        public int databit_3Dmodel = 20;
        private int acttime = 0;
        public bool IsInit3Dmodelview { set; get; }

        private MeshGeometry3D mesh = new MeshGeometry3D();
        private GeometryModel3D model = new GeometryModel3D();
        private ModelVisual3D visualmodel_current = new ModelVisual3D();


        private ModelVisual3D Bone3D = new ModelVisual3D();
        private Model3DGroup BoneGroup = new Model3DGroup();



        private MeshGeometry3D mesh_cube = new MeshGeometry3D();
        private GeometryModel3D model_cube = new GeometryModel3D();
        private ModelVisual3D visualmodel_cube = new ModelVisual3D();


        private _3DTools.ScreenSpaceLines3D wire_3dmodel = new _3DTools.ScreenSpaceLines3D();
        private MeshGeometry3D mesh_3dmodel = new MeshGeometry3D();
        private GeometryModel3D model_3dmodel = new GeometryModel3D();
        private ModelVisual3D visualmodel_3Dmodel = new ModelVisual3D();

        private Skeleton skeleton_3Dmodel;

        /// <summary>
        /// TODO: update 3D models
        /// </summary>
        private void SetDepthModel()
        {
            try
            {
                if (this.acttime < this.Slider_Frame.Value)
                {
                    this.acttime++;
                    return;
                }

                if (this.TabControl_Modelview.SelectedIndex == 0)
                {
                    this.databit = (int)Slider_DPI.Value;

                    for (int i = 0; i < this.DepthImageRect.Height / databit; i++)
                    {
                        for (int j = 0; j < this.DepthImageRect.Width / databit; j++)
                        {
                            this.mesh.Positions[j + i * this.DepthImageRect.Width / databit] = new Point3D(this.SkeletonPoint_DepthCurrent[(j * databit) + (i * databit) * this.DepthImageRect.Width].X, this.SkeletonPoint_DepthCurrent[(j * databit) + (i * databit) * this.DepthImageRect.Width].Y, this.SkeletonPoint_DepthCurrent[(j * databit) + (i * databit) * this.DepthImageRect.Width].Z);
                        }
                    }
                    this.mesh.TriangleIndices.Clear();
                    Point3D zero = new Point3D(0, 0, 0);
                    for (int i = 0; i < this.DepthImageRect.Height / databit - 1; i++)
                    {
                        for (int j = 0; j < this.DepthImageRect.Width / databit - 1; j++)
                        {
                            if (mesh.Positions[j + i * this.DepthImageRect.Width / databit] != zero
                                && mesh.Positions[j + 1 + i * this.DepthImageRect.Width / databit] != zero
                                && mesh.Positions[j + (i + 1) * this.DepthImageRect.Width / databit] != zero
                                && mesh.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit] != zero
                                )
                            {
                                mesh.TriangleIndices.Add(j + i * this.DepthImageRect.Width / databit);
                                mesh.TriangleIndices.Add(j + 1 + i * this.DepthImageRect.Width / databit);
                                mesh.TriangleIndices.Add(j + (i + 1) * this.DepthImageRect.Width / databit);

                                mesh.TriangleIndices.Add(j + 1 + i * this.DepthImageRect.Width / databit);
                                mesh.TriangleIndices.Add(j + 1 + (i + 1) * this.DepthImageRect.Width / databit);
                                mesh.TriangleIndices.Add(j + (i + 1) * this.DepthImageRect.Width / databit);
                            }

                        }
                    }

                    this.mesh_cube.Positions[0] = new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmax.Value);
                    this.mesh_cube.Positions[1] = new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmax.Value);
                    this.mesh_cube.Positions[2] = new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmax.Value);
                    this.mesh_cube.Positions[3] = new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmax.Value);
                    this.mesh_cube.Positions[4] = new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmin.Value);
                    this.mesh_cube.Positions[5] = new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmin.Value);
                    this.mesh_cube.Positions[6] = new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmin.Value);
                    this.mesh_cube.Positions[7] = new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmin.Value);
                }

                this.acttime = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.Source);
            }
        }

        private double Calculate_Distance(Point3D p1, Point3D p2)
        {
            return Math.Pow((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z), 0.5);
        }

        /// <summary>
        /// TODO: Setup_3DmodelView
        /// </summary>
        private void Init_3Dmodelview()
        {
            try
            {
                if (this.IsInit3Dmodelview)
                {
                    return;
                }
                #region create and add object to view1
                #region Depthcurrent model
                Point3D p = new Point3D();
                for (int i = 0; i < this.DepthImageRect.Height; i++)
                {
                    for (int j = 0; j < this.DepthImageRect.Width; j++)
                    {
                        p = new Point3D(0, 0, 0);
                        this.mesh.Positions.Add(p);
                    }
                }
                this.model.Geometry = this.mesh;
                this.model.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                this.visualmodel_current.Content = this.model;
                this.Viewport3D_1.Children.Add(this.visualmodel_current);
                #endregion
                #region Box model
                #region Create Box model in the View1

                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmax.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmax.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmax.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmax.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmin.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymax.Value, Slider_Cube_Zmin.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmin.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmin.Value));
                this.mesh_cube.Positions.Add(new Point3D(Slider_Cube_Xmax.Value, Slider_Cube_Ymin.Value, Slider_Cube_Zmin.Value));

                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(4);
                this.mesh_cube.TriangleIndices.Add(7);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(7);
                this.mesh_cube.TriangleIndices.Add(4);

                this.mesh_cube.TriangleIndices.Add(7);
                this.mesh_cube.TriangleIndices.Add(3);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(7);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(3);

                this.mesh_cube.TriangleIndices.Add(1);
                this.mesh_cube.TriangleIndices.Add(5);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(1);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(5);

                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(2);
                this.mesh_cube.TriangleIndices.Add(1);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(1);
                this.mesh_cube.TriangleIndices.Add(2);

                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(4);
                this.mesh_cube.TriangleIndices.Add(5);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(5);
                this.mesh_cube.TriangleIndices.Add(4);


                this.mesh_cube.TriangleIndices.Add(5);
                this.mesh_cube.TriangleIndices.Add(1);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(5);
                this.mesh_cube.TriangleIndices.Add(0);
                this.mesh_cube.TriangleIndices.Add(1);

                this.mesh_cube.TriangleIndices.Add(3);
                this.mesh_cube.TriangleIndices.Add(7);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(3);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(7);

                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(2);
                this.mesh_cube.TriangleIndices.Add(3);
                this.mesh_cube.TriangleIndices.Add(6);
                this.mesh_cube.TriangleIndices.Add(3);
                this.mesh_cube.TriangleIndices.Add(2);

                #endregion
                this.model_cube.Geometry = this.mesh_cube;
                this.model_cube.Material = new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(0, 0, 255)));
                this.visualmodel_cube.Content = this.model_cube;
                this.Viewport3D_1.Children.Add(this.visualmodel_cube);
                #endregion
                #region Bone model
                this.Bone3D.Content = this.BoneGroup;
                this.Viewport3D_1.Children.Add(this.Bone3D);
                #endregion
                #endregion
                #region Create and Add object to view4
                for (int i = 0; i < this.DepthImageRect.Height; i++)
                {
                    for (int j = 0; j < this.DepthImageRect.Width; j++)
                    {
                        p = new Point3D(0, 0, 0);
                        this.mesh_3dmodel.Positions.Add(p);
                    }
                }
                this.model_3dmodel.Geometry = this.mesh_3dmodel;
                this.model_3dmodel.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                this.visualmodel_3Dmodel.Content = this.model_3dmodel;
                this.Viewport3D_2.Children.Add(this.visualmodel_3Dmodel);
                #endregion
                this.IsInit3Dmodelview = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Button_3Dmodel_Click(object sender, RoutedEventArgs e)
        {
            this.CaptureModel();

        }

        private void CaptureModel()
        {
            try
            {
                for (int i = 0; i < this.FrameSkeletons.Length; i++)
                {
                    if (this.FrameSkeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        this.skeleton_3Dmodel = this.FrameSkeletons[i];
                    }
                }

                this.databit_3Dmodel = 5;
                Point3D zero = new Point3D(0, 0, 0);
                for (int i = 0; i < this.DepthImageRect.Height / databit_3Dmodel; i++)
                {
                    for (int j = 0; j < this.DepthImageRect.Width / databit_3Dmodel; j++)
                    {
                        this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel] = new Point3D(
                            this.SkeletonPoint_DepthCurrent[(j * databit_3Dmodel) + (i * databit_3Dmodel) * this.DepthImageRect.Width].X,
                            this.SkeletonPoint_DepthCurrent[(j * databit_3Dmodel) + (i * databit_3Dmodel) * this.DepthImageRect.Width].Y,
                            this.SkeletonPoint_DepthCurrent[(j * databit_3Dmodel) + (i * databit_3Dmodel) * this.DepthImageRect.Width].Z
                            );
                    }
                }
                this.mesh_3dmodel.TriangleIndices.Clear();
                for (int i = 0; i < this.DepthImageRect.Height / databit_3Dmodel - 1; i++)
                {
                    for (int j = 0; j < this.DepthImageRect.Width / databit_3Dmodel - 1; j++)
                    {
                        if (
                                this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].Z > this.Slider_Cube_Zmin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].Z > this.Slider_Cube_Zmin.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Z > this.Slider_Cube_Zmin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Z > this.Slider_Cube_Zmin.Value

                            && this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].Z < this.Slider_Cube_Zmax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].Z < this.Slider_Cube_Zmax.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Z < this.Slider_Cube_Zmax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Z < this.Slider_Cube_Zmax.Value

                            && this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].Y < this.Slider_Cube_Ymax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].Y < this.Slider_Cube_Ymax.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Y < this.Slider_Cube_Ymax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Y < this.Slider_Cube_Ymax.Value

                            && this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].Y > this.Slider_Cube_Ymin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].Y > this.Slider_Cube_Ymin.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Y > this.Slider_Cube_Ymin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].Y > this.Slider_Cube_Ymin.Value

                            && this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].X < this.Slider_Cube_Xmax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].X < this.Slider_Cube_Xmax.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].X < this.Slider_Cube_Xmax.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].X < this.Slider_Cube_Xmax.Value

                            && this.mesh_3dmodel.Positions[j + i * this.DepthImageRect.Width / databit_3Dmodel].X > this.Slider_Cube_Xmin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel].X > this.Slider_Cube_Xmin.Value
                            && this.mesh_3dmodel.Positions[j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].X > this.Slider_Cube_Xmin.Value
                            && this.mesh_3dmodel.Positions[j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel].X > this.Slider_Cube_Xmin.Value

                            )
                        {
                            mesh_3dmodel.TriangleIndices.Add(j + i * this.DepthImageRect.Width / databit_3Dmodel);
                            mesh_3dmodel.TriangleIndices.Add(j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel);
                            mesh_3dmodel.TriangleIndices.Add(j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel);

                            mesh_3dmodel.TriangleIndices.Add(j + 1 + i * this.DepthImageRect.Width / databit_3Dmodel);
                            mesh_3dmodel.TriangleIndices.Add(j + 1 + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel);
                            mesh_3dmodel.TriangleIndices.Add(j + (i + 1) * this.DepthImageRect.Width / databit_3Dmodel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.TargetSite);
            }
        }

        private void Button_3Dmodel_Choise_Filename_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "lackmodeldata_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".L3D"; // Default file name
            dlg.DefaultExt = ".L3D"; // Default file extension
            dlg.Filter = "Lack 3Dmodel data (.L3D)|*.L3D"; // Filter files by extension
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = false;

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.TextBox_Filename.Text = dlg.FileName;
            }
        }

        private void Button_3Dmodel_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Callib_test_Click(object sender, RoutedEventArgs e)
        {
            this.Is_trytest = true;
        }

        private void Button_Callib_Zero_Click(object sender, RoutedEventArgs e)
        {
            this.Is_CallibPointZero = true;
        }

        private void Button_Callib_X_Click(object sender, RoutedEventArgs e)
        {
            this.Is_CallibPointX = true;

        }

        private void Button_Callib_Z_Click(object sender, RoutedEventArgs e)
        {
            this.Is_CallibPointZ = true;

        }

        private void Button_Callib_Calc_Click(object sender, RoutedEventArgs e)
        {
            Callibration.CallibrationPallameter cp = new Callibration.CallibrationPallameter();
            cp = this.callibration.Callcurate();
            this.CS = cp;
        }

        private void Button_Callib_Save1_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.dx = this.CS.dx;
            Properties.Settings.Default.dy = this.CS.dy;
            Properties.Settings.Default.dz = this.CS.dz;
            Properties.Settings.Default.rx = this.CS.rx;
            Properties.Settings.Default.ry = this.CS.ry;
            Properties.Settings.Default.rz = this.CS.rz;
            Properties.Settings.Default.Save();

        }

        private void Button_Callib_Load1_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
            
            this.CS = new Callibration.CallibrationPallameter();
            this.CS.dx = Properties.Settings.Default.dx;
            this.CS.dy = Properties.Settings.Default.dy;
            this.CS.dz = Properties.Settings.Default.dz;
            this.CS.rx = Properties.Settings.Default.rx;
            this.CS.ry = Properties.Settings.Default.ry;
            this.CS.rz = Properties.Settings.Default.rz;
        }

        private void Button_Callib_Load3_Click(object sender, RoutedEventArgs e)
        {
            Callib1.Default.Reload();

            this.CS = new Callibration.CallibrationPallameter();
            this.CS.dx = Callib1.Default.dx;
            this.CS.dy = Callib1.Default.dy;
            this.CS.dz = Callib1.Default.dz;
            this.CS.rx = Callib1.Default.rx;
            this.CS.ry = Callib1.Default.ry;
            this.CS.rz = Callib1.Default.rz;
        }

        private void Button_Callib_Save3_Click(object sender, RoutedEventArgs e)
        {
            
            Callib1.Default.dx = this.CS.dx;
            Callib1.Default.dy = this.CS.dy;
            Callib1.Default.dz = this.CS.dz;
            Callib1.Default.rx = this.CS.rx;
            Callib1.Default.ry = this.CS.ry;
            Callib1.Default.rz = this.CS.rz;
            Callib1.Default.Save();
        }

        private void Button_Callib_Load2_Click(object sender, RoutedEventArgs e)
        {
            Callib2.Default.Reload();

            this.CS = new Callibration.CallibrationPallameter();
            this.CS.dx = Callib2.Default.dx;
            this.CS.dy = Callib2.Default.dy;
            this.CS.dz = Callib2.Default.dz;
            this.CS.rx = Callib2.Default.rx;
            this.CS.ry = Callib2.Default.ry;
            this.CS.rz = Callib2.Default.rz;
        }

        private void Button_Callib_Save2_Click(object sender, RoutedEventArgs e)
        {
            Callib2.Default.dx = this.CS.dx;
            Callib2.Default.dy = this.CS.dy;
            Callib2.Default.dz = this.CS.dz;
            Callib2.Default.rx = this.CS.rx;
            Callib2.Default.ry = this.CS.ry;
            Callib2.Default.rz = this.CS.rz;
            Callib2.Default.Save();
        }

        private void Button_Smoll_Click(object sender, RoutedEventArgs e)
        {
            this.WindowMinimize();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowMinimize();
        }

        private void WindowMinimize()
        {
            if (this.minimizedwindow == null)
            {
                this.minimizedwindow = new MinimizedWindow();
                this.minimizedwindow.mainwindow = this;
            }
            this.Hide();
            this.minimizedwindow.Show();
        }


    }
}
