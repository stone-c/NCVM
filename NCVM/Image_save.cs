using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Resources;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

//using AForge
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.FFMPEG;
using AForge.Controls;

namespace NCVM
{
    class Image_save
    {
        public static Mutex image_mutex = new Mutex();
        public static Bitmap bm = null;                        // 获得的原图像（帧）
        public static Bitmap bm_show = Resource1.hcnc_logo;    // 用于展示的图像（帧）
        public static Bitmap bm_save = null;                   // 用于存储的图像（帧）
        //bm_save = new Bitmap(960, 720);

        private static Size imageshow = new Size(600, 450);    // 用于展示的Size
        private static Size imagesave = new Size(960, 720);

        private static FilterInfoCollection videoDevices;      // 摄像头设备
        private static VideoCaptureDevice videoSource;         // 视频的来源选择
        private static VideoSourcePlayer videoSourcePlayer;    // AForge控制控件
        private static VideoFileWriter writer;                 // 写入到视频

        private static System.Timers.Timer timer_rec;          // 回放计时器

        static Bitmap wmlogo = Resource1.wmlogo;               // 界面logo
        static Bitmap wmlogo0 = Resource1.wmlogo1;             // 存储水印logo

        private static int rec_counter = 0;                    // 回放计时器

        static string[] wms = new string[7];                   // 消息字符串

        public static bool video_event = false;                // 摄像头开关事件触发
        public static bool record_event = false;               // 录像开关事件触发

        public static int capset = 0;

        //private static int tim_start = 0;
        //private static int tim_stop = 0;
        //private static int tim_count = 0;

        #region WaterMark Setting

        private static PointF zero1 = new PointF(40, 40);      // logo在界面左上角的位置
        private static PointF zero = new PointF(0, 0);         // logo截取位置

        private static Size logosize = new Size(180, 120);     // logo缩放尺寸
        private static Size logosize1 = new Size(356, 247);    // logo原尺寸

        private static RectangleF logo_rect1 = new RectangleF(zero1, logosize);
        private static RectangleF logo_rect = new RectangleF(zero, logosize1);

        public static float bottom = 940.0F;                  // 水印底部位置
        public static float left = 20.0F;

        private static Font drawFont = new Font("Arial", 22);
        private static SolidBrush drawBrush = new SolidBrush(Color.White);
        private static PointF drawPoint = new PointF(20.0F, 20.0F);

        private static PointF drawPoint_x = new PointF(left, bottom - 220);
        private static PointF drawPoint_y = new PointF(left, bottom - 190);
        private static PointF drawPoint_z = new PointF(left, bottom - 160);
        private static PointF drawPoint_f = new PointF(left, bottom - 100);
        private static PointF drawPoint_s = new PointF(left, bottom - 70);
        private static PointF drawPoint_g = new PointF(left, bottom - 30);
        private static PointF drawPoint_t = new PointF(1140.0F, 910.0F);

        #endregion

        /// <summary>
        /// 录像线程初始化
        /// </summary>
        private static void thread_init()
        {
            videoSourcePlayer = new AForge.Controls.VideoSourcePlayer();
            videoSource = new VideoCaptureDevice();
            writer = new VideoFileWriter();

            timer_rec = new System.Timers.Timer();
            timer_rec.Elapsed += new System.Timers.ElapsedEventHandler(rec_count);
            timer_rec.AutoReset = true;
            timer_rec.Interval = 40;
            timer_rec.Start();

            try
            {
                // 枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();   //没有找到摄像头设备
            }
            catch (ApplicationException ex)
            {
                videoDevices = null;
                DialogResult drdiv = MessageBox.Show(ex.Message + "没有找到摄像头设备,程序即将关闭", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (drdiv == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            }

            int temp_capdiv = 0;
            int temp_capsiz = 0;
            Size capsize = new Size(1280, 960);
            foreach (FilterInfo item in videoDevices)
            {
                if (item.Name == "LRCP  USB2.0")
                {
                    videoSource = new VideoCaptureDevice(videoDevices[temp_capdiv].MonikerString);
                    temp_capsiz = 0;
                    foreach (VideoCapabilities capab in videoSource.VideoCapabilities)/*(FilterInfoCollection[capdiv].MonikerString).VideoCapabilities[i])*/
                    {
                        if (capab.FrameSize == capsize)
                        {
                            capset = temp_capdiv * 100 + temp_capsiz;
                        }
                        temp_capsiz++;
                    }
                }

                temp_capdiv++;
            }
        }

        /// <summary>
        /// 录像线程生成
        /// </summary>
        public static void thread_video()
        {
            thread_init();
            Thread video = new Thread(get_status);
            video.IsBackground = true;
            video.Start();
        }

        /// <summary>
        /// 录像线程主函数，循环获取事件
        /// </summary>
        public static void get_status()
        {
            while (true)
            {
                if (video_event)
                {
                    video_event_func();
                    video_event = false;
                }
                if (record_event)
                {
                    record_event_func();
                    record_event = false;
                }
            }
        }

        /// <summary>
        /// 打开/关闭视频事件
        /// </summary>
        private static void video_event_func()
        {

            if (NCVM_Form.isopen)
            {
                try
                {
                    videoSource = new VideoCaptureDevice(videoDevices[capset / 100].MonikerString);
                    videoSource.DesiredFrameSize = videoSource.VideoCapabilities[capset % 100].FrameSize;
                    videoSource.NewFrame += show_video;
                    videoSource.Start();
                    videoSourcePlayer.VideoSource = videoSource;
                    videoSourcePlayer.Start();
                    timer_rec.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message);
                }
            }
            else
            {
                stop_flag = true;
                videoSource.Stop();
                videoSourcePlayer.Stop();

            }
        }

        /// <summary>
        /// 打开/关闭录像事件
        /// </summary>
        private static void record_event_func()
        {
            if (!NCVM_Form.start_record)
            {
                Image_save.stop_flag = true;
                Thread.Sleep(10);
                //writer.Close();
                //timer_rec.Stop();
                //fps_ch();
            }
        }

        /// <summary>
        /// 录像计时器
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void rec_count(object source, System.Timers.ElapsedEventArgs e)
        {
            rec_counter++;

        }

        [DllImport("winmm")]
        static extern uint timeGetTime();

        /// <summary>
        /// 摄像头新帧触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        static long last_image_time = -1;
        static bool avalable = false;
        static long count = 0;
        private static void show_video(object sender, NewFrameEventArgs eventArgs)
        {

            HNC_Connect.hnc_con();

            wms = NCVM_Form.wm;

            Bitmap temp_bm = (Bitmap)eventArgs.Frame.Clone();

            #region WaterMark Write

            switch (Form2.selected_color)
            {
                case 0:
                    drawBrush = new SolidBrush(Color.White);
                    break;
                case 1:
                    drawBrush = new SolidBrush(Color.Black);
                    break;
                case 2:
                    drawBrush = new SolidBrush(Color.Blue);
                    break;
                case 3:
                    drawBrush = new SolidBrush(Color.Red);
                    break;
                case 4:
                    drawBrush = new SolidBrush(Color.Yellow);
                    break;
                case 5:
                    drawBrush = new SolidBrush(Color.Green);
                    break;
                default:
                    drawBrush = new SolidBrush(Color.White);
                    break;
            }

            if (Form2.rl_changed)
            {
                Form2.rl_changed = false;
                drawPoint_x = new PointF(left, bottom - 220);
                drawPoint_y = new PointF(left, bottom - 190);
                drawPoint_z = new PointF(left, bottom - 160);
                drawPoint_f = new PointF(left, bottom - 100);
                drawPoint_s = new PointF(left, bottom - 70);
            }

            using (Graphics g = Graphics.FromImage(temp_bm))
            {

                g.DrawImage(wmlogo0, logo_rect1, logo_rect, GraphicsUnit.Pixel);
                if (Form2.wm_allowed)
                {
                    g.DrawString(wms[0], drawFont, drawBrush, drawPoint_x);
                    g.DrawString(wms[1], drawFont, drawBrush, drawPoint_y);
                    g.DrawString(wms[2], drawFont, drawBrush, drawPoint_z);
                    g.DrawString(wms[3], drawFont, drawBrush, drawPoint_f);
                    g.DrawString(wms[4], drawFont, drawBrush, drawPoint_s);
                    g.DrawString(wms[5], drawFont, drawBrush, drawPoint_g);
                    g.DrawString(wms[6], drawFont, drawBrush, drawPoint_t);
                }
                g.Dispose();


            }
            #endregion

            image_mutex.WaitOne();

            if (bm != null)
            {
                bm.Dispose();
            }

            bm = (Bitmap)temp_bm.Clone();

            image_mutex.ReleaseMutex();

            temp_bm.Dispose();

        }



        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(ref long lpFrequency);

        static bool stop_flag = false;

        /// <summary>
        /// 按时间写入视频流
        /// </summary>
        static void write_video()
        {
            long freq = 0;

            if (QueryPerformanceFrequency(ref freq) == false)
            {
                throw new Exception("不支持高精度计时.");
            }
            double count_per_millsec = freq / 1000.0;
            long start_time = 0, stop_time = 0;
            long now_time = 0;

            //writer.Open(NCVM_Form.FileV, 1280, 960, 25, VideoCodec.MPEG4, 5120000);

            QueryPerformanceCounter(ref start_time);
            while (!stop_flag)
            {
                if ((bm == null) || (!writer.IsOpen))
                {
                    continue;
                }

                QueryPerformanceCounter(ref now_time);
                while (now_time - start_time < 40 * count_per_millsec)
                {
                    QueryPerformanceCounter(ref now_time);
                }
                start_time = now_time;
                Image_save.image_mutex.WaitOne();
                Bitmap temp_bm = (Bitmap)bm.Clone();
                Image_save.image_mutex.ReleaseMutex();
                try
                {
                    writer.Open(NCVM_Form.FileV, 1280, 960, 25, VideoCodec.MPEG4, 5120000);
                    writer.WriteVideoFrame(temp_bm);
                }
                catch (AccessViolationException ex)
                {
                    //
                }
                temp_bm.Dispose();
                QueryPerformanceCounter(ref stop_time);
                //Console.Write("写入一帧耗时={0}" + "\n", (long)((stop_time - start_time) / count_per_millsec));
            }
            writer.Close();
        }

        /// <summary>
        /// 帧存储线程
        /// </summary>
        public static void thread_save()
        {
            stop_flag = false;
            Thread save = new Thread(write_video);
            save.IsBackground = true;
            save.Start();
        }

    }
    
}
