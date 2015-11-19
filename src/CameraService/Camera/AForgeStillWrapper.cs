using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


using AForge.Video;
using AForge.Video.DirectShow;

using System.Drawing;



namespace CameraService.Camera
{
    public class AForgeStillWrapper
    {
        // Fixed size queue after Richard Schneider http://stackoverflow.com/a/5852926
        private const int MAX_QUEUE = 1;
        public class FixedSizedQueue<T>
        {

            ConcurrentQueue<System.Drawing.Bitmap> q = new ConcurrentQueue<System.Drawing.Bitmap>();

            public int Limit { get; set; }

            // Private
                 
            public void Enqueue(System.Drawing.Bitmap obj)
            {
                q.Enqueue(obj);
                lock (this)
                {
                    System.Drawing.Bitmap overflow;
                    while (q.Count > Limit && q.TryDequeue(out overflow))
                    { 
                        overflow.Dispose();
                    }
                }
            }

            public bool TryDequeue(out System.Drawing.Bitmap obj)
            {
                return q.TryDequeue(out obj);
            }
        }


        private static FilterInfoCollection VideoFilterInfoCollection;

        // Frame array with the same order as VideoCaptureDevices
        private static VideoCaptureDevice[] VideoCaptureDeviceArray;
        private static FixedSizedQueue<Bitmap>[] FrameQueueArray;

        private static String _WebRootPath;

        // Timer for shutting down cameras that API is not used
        private const int TIME_INTERVAL_IN_MILLISECONDS = 1000;
        private static System.Threading.Timer _watchTimer;
        private const int MAX_INACTIVE_IN_MILLISECONDS = 10000;
        private static DateTime[] CameraLastUsedTime;

        // Settings default profiles
        private static System.Collections.Hashtable DefaultCameraProfiles = new System.Collections.Hashtable();

        public AForgeStillWrapper(String WebRootPath)
        {
            Startup(WebRootPath);
        }


        public static void Startup(String WebRootPath)
        {
            _WebRootPath = WebRootPath;

            VideoFilterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDeviceArray = new VideoCaptureDevice[VideoFilterInfoCollection.Count];
            FrameQueueArray = new FixedSizedQueue<Bitmap>[VideoFilterInfoCollection.Count];
            CameraLastUsedTime = new DateTime[VideoFilterInfoCollection.Count];

            // Def profiles name index
            DefaultCameraProfiles.Add("Integrated Camera", 8);
            DefaultCameraProfiles.Add("Microsoft LifeCam Studio", 9);
            DefaultCameraProfiles.Add("Microsoft® LifeCam Studio(TM)", 1);
            DefaultCameraProfiles.Add("Logitech QuickCam Pro 9000", 18);

            // Create devices
            int device = 0;
            foreach (FilterInfo fi in VideoFilterInfoCollection)
            {
                CreateDevice(device);
                PingDeviceWasAlive(device);
                ++device;
            }

            //CreateDevice(device -2);
            _watchTimer = new Timer(TimerCallback, null, TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);

        }

        private static void TimerCallback(Object state)
        {
            int device = 0;
            foreach (FilterInfo fi in VideoFilterInfoCollection)
            {
                if ((DateTime.Now - CameraLastUsedTime[device]).TotalMilliseconds > MAX_INACTIVE_IN_MILLISECONDS)
                {
                    if ((device >= 0 && device < VideoCaptureDeviceArray.Length))
                    {

                        VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];
                        if (SelectedDevice != null && SelectedDevice.IsRunning)
                        {
                            SelectedDevice.SignalToStop();
                            SelectedDevice.WaitForStop();
                        }
                    }
                }
                ++device;
            }

            // No ob check in private

            _watchTimer.Change(TIME_INTERVAL_IN_MILLISECONDS, Timeout.Infinite);
        }

        private static void CreateDevice(int device)
        {
            VideoCaptureDeviceArray[device] = new VideoCaptureDevice(VideoFilterInfoCollection[device].MonikerString);
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];

            FrameQueueArray[device] = new FixedSizedQueue<Bitmap>();
            FrameQueueArray[device].Limit = MAX_QUEUE;
            if (SelectedDevice != null)
            {
                int profile = 0;
                if (DefaultCameraProfiles.ContainsKey(VideoFilterInfoCollection[device].Name))
                {
                    profile = (int)DefaultCameraProfiles[VideoFilterInfoCollection[device].Name];
                }
                SelectedDevice.VideoResolution = SelectedDevice.VideoCapabilities[profile];

                if (VideoFilterInfoCollection[device].Name.Equals("Microsoft® LifeCam Studio(TM)"))
                {
                    SetCameraControlPropertyManual(device, AForge.Video.DirectShow.CameraControlProperty.Exposure, -6);
                    SetCameraControlPropertyManual(device, AForge.Video.DirectShow.CameraControlProperty.Focus, 30);
                }
                SelectedDevice.NewFrame += delegate (object sender, NewFrameEventArgs eventArgs)
                {

                    FrameQueueArray[device].Enqueue((Bitmap)eventArgs.Frame.Clone());
                };

                // Init lastused timestamp here
                CameraLastUsedTime[device] = DateTime.Now;
            }
        }

        public static bool PingDeviceWasAlive(int device)
        {
            if (device >= 0 && device < VideoCaptureDeviceArray.Length)
            {
                CameraLastUsedTime[device] = DateTime.Now;
                if (device >= 0 && device < VideoCaptureDeviceArray.Length)
                {

                    VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];
                    if (!SelectedDevice.IsRunning)
                    {
                        SelectedDevice.Start();
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static async Task<Bitmap> GetFrame(int device = 0)
        {
            Bitmap frame;
            if (!PingDeviceWasAlive(device))
            {
                FrameQueueArray[device].TryDequeue(out frame);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
            int tries = 0;
            while(!FrameQueueArray[device].TryDequeue(out frame) && tries < 100)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
                ++tries;
            }
            return frame;
        }

        // Information
        public class DeviceInformation
        {
            public string name { get; set; }
            public int device { get; set; }
            public ViedoResolutionProfile ViedoResolutionProfile { get; set; }
            public IEnumerable<ViedoResolutionProfile> VideoResolutionProfiles { get; set; }
            public IEnumerable<ContainerForCameraControlPropery> CameraControlProperties { get; set; }
        }

        public static IEnumerable<DeviceInformation> ListDevices()
        {
            List<DeviceInformation> ret = new List<DeviceInformation>();

            int device = 0;
            foreach (FilterInfo fi in VideoFilterInfoCollection)
            {
                DeviceInformation dev = new DeviceInformation();
                dev.name = fi.Name;
                dev.device = device;
                dev.ViedoResolutionProfile = GetVideoResolutionProfile(device);
                dev.VideoResolutionProfiles = GetVideoResolutionProfiles(device);
                dev.CameraControlProperties = GetCameraControlProperties(device);
                ret.Add(dev);
                ++device;
            }
            return ret;
        }

        public class ViedoResolutionProfile
        {
            public int index { get; set; }
            public VideoCapabilities VideoResolution { get; set; }
        }


        public static ViedoResolutionProfile GetVideoResolutionProfile(int device = 0)
        {
            ViedoResolutionProfile ret = new ViedoResolutionProfile();
            ret.index = -1;

            if (!(device >= 0 && device < VideoCaptureDeviceArray.Length))
            {
                return ret; ;
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];
            ret.index = Array.IndexOf<VideoCapabilities>(SelectedDevice.VideoCapabilities, SelectedDevice.VideoResolution);
            ret.VideoResolution = SelectedDevice.VideoResolution;
            return ret;

        }

        public static IEnumerable<ViedoResolutionProfile> GetVideoResolutionProfiles(int device = 0)
        {
            List<ViedoResolutionProfile> ret = new List<ViedoResolutionProfile>();
            if (!(device >= 0 && device < VideoCaptureDeviceArray.Length || VideoCaptureDeviceArray[device] == null))
            {
                return ret; ;
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];

            int index = 0;
            foreach (VideoCapabilities vc in SelectedDevice.VideoCapabilities)
            {
                ViedoResolutionProfile vp = new ViedoResolutionProfile();
                vp.index = index;
                vp.VideoResolution = vc;
                ret.Add(vp);
                ++index;
            }

            return ret;

        }

        public static String SetVideoResolutionProfile(int device = 0, int profile = 0)
        {
            if (!(device >= 0 && device < VideoCaptureDeviceArray.Length))
            {
                return "";
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];

            SelectedDevice.VideoResolution = SelectedDevice.VideoCapabilities[profile];
            return SelectedDevice.VideoCapabilities[profile].FrameSize.ToString();

        }

        public class ContainerForCameraControlPropery
        {
            public string name { get; set; }
            public int value { get; set; }
            public string flag { get; set; }


            public ContainerForCameraControlPropery(string name, int value = 0, string flag = "None")
            {
                this.name = name;
                this.value = value;
                this.flag = flag;
            }

            public ContainerForCameraControlPropery(AForge.Video.DirectShow.CameraControlProperty name, int value = 0, AForge.Video.DirectShow.CameraControlFlags flag = AForge.Video.DirectShow.CameraControlFlags.None)
            {
                this.name = name.ToString();
                this.value = value;
                this.flag = flag.ToString();
            }
        }


        public static IEnumerable<ContainerForCameraControlPropery> GetCameraControlProperties(int device)
        {
            List<ContainerForCameraControlPropery> ret = new List<ContainerForCameraControlPropery>();

            if (!(device >= 0 && device < VideoCaptureDeviceArray.Length))
            {
                return ret;
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];

            foreach (AForge.Video.DirectShow.CameraControlProperty CameraPropery in Enum.GetValues(typeof(AForge.Video.DirectShow.CameraControlProperty)))
            {
                int valueOut = 0;
                CameraControlFlags cameraControlFlagOut = CameraControlFlags.None;
                SelectedDevice.GetCameraProperty(CameraPropery, out valueOut, out cameraControlFlagOut);
                ContainerForCameraControlPropery prop = new ContainerForCameraControlPropery(CameraPropery, valueOut, cameraControlFlagOut);
                ret.Add(prop);
            }

            return ret;
        }


        public static ContainerForCameraControlPropery SetCameraControlPropertyManual(int device, AForge.Video.DirectShow.CameraControlProperty CameraProperty, int Value)
        {
            ContainerForCameraControlPropery ret = new ContainerForCameraControlPropery(CameraProperty, -1, CameraControlFlags.None);
            if ((!(device >= 0 && device < VideoCaptureDeviceArray.Length)))
            {
                return ret;
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];
            if (SelectedDevice == null)
            {
                return ret;
            }
            // Get old to check for none
            int valueOut = 0;
            CameraControlFlags cameraControlFlagOut = CameraControlFlags.None;
            SelectedDevice.GetCameraProperty(CameraProperty, out valueOut, out cameraControlFlagOut);

            if (!cameraControlFlagOut.Equals(CameraControlFlags.None))
            {
                // Set if auto or manual
                SelectedDevice.SetCameraProperty(CameraProperty, Value, CameraControlFlags.Manual);
            }
            // Get new
            SelectedDevice.GetCameraProperty(CameraProperty, out valueOut, out cameraControlFlagOut);
            ret = new ContainerForCameraControlPropery(CameraProperty, valueOut, cameraControlFlagOut);

            return ret;
        }

        public static ContainerForCameraControlPropery SetCameraControlPropertyAuto(int device, AForge.Video.DirectShow.CameraControlProperty CameraProperty)
        {
            ContainerForCameraControlPropery ret = new ContainerForCameraControlPropery(CameraProperty, -1, CameraControlFlags.None);
            if ((!(device >= 0 && device < VideoCaptureDeviceArray.Length)))
            {
                return ret;
            }
            VideoCaptureDevice SelectedDevice = VideoCaptureDeviceArray[device];
            if (SelectedDevice == null)
            {
                return ret;
            }
            // Get old to check for none
            int valueOut = 0;
            CameraControlFlags cameraControlFlagOut = CameraControlFlags.None;
            SelectedDevice.GetCameraProperty(CameraProperty, out valueOut, out cameraControlFlagOut);

            if (!cameraControlFlagOut.Equals(CameraControlFlags.None))
            {
                // Set if auto or manual
                SelectedDevice.SetCameraProperty(CameraProperty, valueOut, CameraControlFlags.Auto);
            }
            // Get new
            SelectedDevice.GetCameraProperty(CameraProperty, out valueOut, out cameraControlFlagOut);
            ret = new ContainerForCameraControlPropery(CameraProperty, valueOut, cameraControlFlagOut);

            return ret;
        }

    }
}
