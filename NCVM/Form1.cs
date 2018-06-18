using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using AForge.Video.FFMPEG;

namespace NCVM
{
    public partial class NCVM_Form : Form
    {
        public Bitmap bm = null;                                // 原图像
        public Bitmap bm_show = null;                           // 用于picturebox的图像
        public Bitmap bm_save = null;                           // 用于存储的图像

        System.Timers.Timer timer_count;
        System.Timers.Timer timer_replay;

        public static bool isopen = false;                      // 

        Bitmap hcnc;


        public static string his_path;                         // 历史记录路径
        public static string gcode_path;                       // G代码路径

        private string gcode_prog = null;                      // G代码名
        private String temp_path = null;                       // 临时路径

        private static Size savelogo = new Size(180, 120);     // 

        Bitmap wmlogo = Resource1.wmlogo;
        Bitmap wmlogo0 = Resource1.wmlogo1;
        Bitmap wmlogo1;

        private int gcode_counter = 0;

        public int mode = 0;

        Form freplay = new Form3();
        Form fconnect = new Form2();

        //设置窗口选项
        public static bool wm_allowed;                        //
        public static bool auto_rec;                          //
        public static bool start_record = false;              //
        public static bool isreplay = false;                  //
        public static bool isconopen = false;                 //
        public static bool repalypause = false;               // 回放暂停
        public static bool ispgavaliable = true;              // Position&Gcode显示

        private static int rec_counter = 0;                   //

        private static Size imageshow = new Size(600, 450);   //

        private double txt_time = 0.0;                        //

        string tim;                                           //
        string replay_t;                                      //

        private static int counter;                           //
        private static int gcode_lines;                       //
        private string retim;                                 //

        public static string FileV;
        public static string[] wm = new string[7];
        string[] saves = new string[8];
        string load_item;

        StreamWriter sw;
        FileStream fsw;

        LinkedList<string[]> link_save = new LinkedList<string[]>();
        LinkedList<string> link_read = new LinkedList<string>();
        List<string> list_read = new List<string>();

        public static Mutex record_mutex = new Mutex();

        public NCVM_Form()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximumSize = new Size(870, 560);
            this.MinimumSize = new Size(645, 560);

            fconnect.Location = this.Location;

            #region Timer

            // 通用Timer触发
            timer_count = new System.Timers.Timer();
            timer_count.Elapsed += new System.Timers.ElapsedEventHandler(tick_count);
            //timer_count.AutoReset = true;
            timer_count.Interval = 40;

            timer_count.Start();

            // 回放Timer触发
            timer_replay = new System.Timers.Timer();
            timer_replay.Elapsed += new System.Timers.ElapsedEventHandler(replay_count);
            timer_replay.AutoReset = true;
            timer_replay.Interval = 55;

            #endregion

            #region WaterMark Init

            label13.Parent = pictureBox1;
            label13.BackColor = Color.Transparent;
            label13.Visible = false;
            //label21.Location = new Point(890, 249);

            #endregion

            #region WindowsMediaPlayer

            axWindowsMediaPlayer1.Location = new System.Drawing.Point(13, 16);
            axWindowsMediaPlayer1.Size = new Size(600, 450);
            //axWindowsMediaPlayer1.
            axWindowsMediaPlayer1.Visible = false;

            #endregion

            hcnc = Resource1.hcnc_logo;

            start_video(true);
            pictureBox1.Image = hcnc;
            counter = 0;
            wmlogo1 = new Bitmap(wmlogo0, savelogo);

            Image_save.video_event = true;

            pictureBox1.BackColor = Color.FromArgb(182, 181, 179);

            listBox1.Items.Add("Waiting...");
        }

        /// <summary>
        /// 将G代码加载到listbox1中，调用前需要listBox1.Item.Clear();
        /// </summary>
        /// <param name="a">
        /// G代码所在路径
        /// </param>
        private bool read_gcode(string a)
        {
            try
            {
                listBox1.Items.Clear();
            }
            catch (NullReferenceException ex)
            {
                // Ignore
            }

            if (a == null)
            {
                return false;
            }

            string str;

            gcode_counter = 0;
            StreamReader sr = new StreamReader(a, Encoding.Default);

            String line = null;
            while ((line = sr.ReadLine()) != null)
            {
                str = gcode_counter.ToString("d4") + "  " + line;
                if (line.StartsWith("M98"))
                {
                    //HNC_Connect.getfile("/h/lnc8/prog/O" + line.Replace("M98P", ""), "/");
                }
                listBox1.Items.Add(str);
                gcode_counter++;
            }
            gcode_lines = gcode_counter;

            listBox1.SelectedIndex = 0;
            if (listBox1.Items.Count != gcode_counter)
            {
                return false;
            }
            sr.Dispose();
            return true;
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if (!isreplay)
            {
                start_video(!isopen);
            }
            else
            {
                if (!repalypause)
                {
                    repalypause = true;
                    Start_button.Text = "继续回放";
                    axWindowsMediaPlayer1.Ctlcontrols.pause();
                    timer_replay.Stop();
                }
                else
                {
                    repalypause = false;
                    Start_button.Text = "暂停回放";
                    axWindowsMediaPlayer1.Ctlcontrols.play();
                    timer_replay.Start();
                }
            }
        }

        /// <summary>
        /// 打开/关闭摄像头
        /// </summary>
        private void start_video(bool start)
        {

            if (start)
            {
                isopen = true;
                Image_save.video_event = true;
                Start_button.Text = "关闭摄像";
                this.timer_count.Enabled = true;

            }
            else
            {
                isopen = false;
                Image_save.video_event = true;
                Start_button.Text = "打开摄像";
                if (!isreplay)
                {
                    //
                }
                start_record = false;
                Thread.Sleep(10);
                Image_save.record_event = true;

                this.timer_count.Enabled = false;

            }
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        private void get_status()
        {

            if (isreplay)
            {
                return;
            }

            #region XYZFS

            wm[0] = "X  " + HNC_Connect.pos_X.ToString("000.000") + " mm";
            wm[1] = "Y  " + HNC_Connect.pos_Y.ToString("000.000") + " mm";
            wm[2] = "Z  " + HNC_Connect.pos_Z.ToString("000.000") + " mm";

            wm[3] = "F  " + HNC_Connect.speed_F.ToString("0.00") + " mm/min";
            wm[4] = "S  " + HNC_Connect.speed_S.ToString("0.00") + " rpm";

            #endregion

            if (listBox1.SelectedIndex > 8)
            {
                listBox1.TopIndex = listBox1.SelectedIndex - 7;
            }

            //if (HNC_Connect.cyc != 0)
            if (listBox1.Text != "")
            {
                //Thread.Sleep(100);
                try
                {
                    wm[5] = listBox1.SelectedItem.ToString();
                }
                catch (NullReferenceException ex)
                {
                    listBox1.SelectedIndex = 0;
                    wm[5] = listBox1.SelectedItem.ToString();
                }
            }
            wm[6] = DateTime.Now.TimeOfDay.ToString().Remove(8);
            load_item = Form3.selecteds;
        }

        /// <summary>
        /// 界面以及水印X、Y、Z、F、S字符串统一管理
        /// </summary>
        private void stringm()
        {

            get_status();
            label_x.Text = wm[0].Substring(3, 8);
            label_y.Text = wm[1].Substring(3, 8);
            label_z.Text = wm[2].Substring(3, 8);
            label_f.Text = wm[3].Substring(3, 4);
            label_s.Text = wm[4].Substring(3, 4);

        }

        /// <summary>
        /// 录像开始
        /// </summary>
        /// <param name="t">
        /// 录像及数据资料存储路径
        /// </param>
        private void record_video(string t)
        {
            string FileF = his_path + t + "/";
            string FileT = FileF + t.ToString() + ".txt";

            FileV = FileF + t.ToString() + ".avi";

            if (!File.Exists(FileT))
            {
                fsw = new FileStream(FileT, FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fsw);
            }
            gcode_prog = HNC_Connect.progN/*.Remove(0, 8)*/;
            if (gcode_prog != null)
            {
                File.Copy(gcode_path + gcode_prog, FileF + t.ToString() + ".nc");
            }
            //fsw.Dispose();
            //File.Copy(gcode_path + gcode_prog, FileF + t.ToString() + ".nc");

        }

        [DllImport("winmm")]
        static extern uint timeGetTime();
        static long start;
        static long end;
        private void video_button_Click(object sender, EventArgs e)
        {
            if (!isopen)
            {
                MessageBox.Show("请先打开摄像头！", "提示", MessageBoxButtons.OK);
                return;
            }

            //record();
            tim = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2") + DateTime.Now.Day.ToString("d2") + DateTime.Now.Hour.ToString("d2") + DateTime.Now.Minute.ToString("d2") + DateTime.Now.Second.ToString("d2");

            if (!start_record)
            {
                creatDic(tim);
                record_video(tim);
                Image_save.record_event = true;
                Thread.Sleep(10);
                start = timeGetTime();
                start_record = true;
                //Image_save.writer.Open(NCVM_Form.FileV, 1280, 720, 25, VideoCodec.MPEG4, 5120000);
                //Image_save.writer.Open(FileV, 1280, 960, 25, VideoCodec.MPEG4, 5120000);
                //Image_save.writer.Open(FileV, 1280, 960, 25, VideoCodec.MPEG4, 5120000);
                Image_save.thread_save();

                video_button.Text = "结束录像";
                label13.Visible = true;
                Start_button.Enabled = false;
                replay_button.Enabled = false;
                //checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                rec_counter = 0;
                //timer_rec.Start();
            }
            else
            {
                end = timeGetTime();

                start_record = false;
                Thread.Sleep(10);
                Image_save.record_event = true;
                video_button.Text = "开始录像";
                label13.Visible = false;
                Start_button.Enabled = true;
                replay_button.Enabled = true;

                checkBox2.Enabled = true;
                if (gcode_prog != null)
                {
                    listBox1.SelectedIndex = 0;
                }

                Thread.Sleep(40);
                //writer.Close();
                sw.Close();
            }
        }


        /// <summary>
        /// 生成以时间命名的文件夹
        /// </summary>
        /// <param name="t">
        /// 获取的时间
        /// </param>
        /// <returns>
        /// 返回路径全称
        /// </returns>
        private string creatDic(string t)
        {
            String sPath = his_path + t;
            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            //record_mutex.WaitOne();
            return sPath;
        }

        /// <summary>
        /// 回放计时，用于加载字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void replay_count(object source, System.Timers.ElapsedEventArgs e)
        {

            #region replay exit

            if (isreplay)
            {
                string status = null;
                try
                {
                    status = axWindowsMediaPlayer1.playState.ToString();
                }
                catch (Exception ex)
                {
                    //Ignore
                }

                if ((status == "wmppsStopped") || (status == "wmppsMediaEnded"))
                {
                    isreplay = false;

                    axWindowsMediaPlayer1.close();
                    axWindowsMediaPlayer1.Visible = false;
                    start_video(true);

                    video_button.Enabled = true;
                    Start_button.Enabled = true;
                    replay_button.Text = "回  放";
                    //pictureBox1.Image = hcnc;
                    list_read.Clear();
                    //listBox1.Items.Clear();
                    timer_replay.Stop();
                    if (HNC_Connect.iscon)
                    {
                        while (!read_gcode(gcode_path + HNC_Connect.progN)) ;
                    }
                    else
                    {
                        listBox1.Items.Clear();
                        listBox1.Items.Add("Waiting...");
                    }
                    Thread.Sleep(40);
                    try
                    {
                        sw.Close();
                    }
                    catch (NullReferenceException ex)
                    {
                        //Ignore
                    }
                    listBox1.SelectedIndex = 0;
                }
            }

            #endregion

            retim = read_data(replay_t, retim);
            stringm();
        }

        /// <summary>
        /// 通用计时，用于记录窗体跟随，加载回放记录，回放完成后返回正常界面
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void tick_count(object source, System.Timers.ElapsedEventArgs e)
        {

            stringm();
            if (ispgavaliable)
            {
                freplay.Location = new System.Drawing.Point(this.Location.X + 860, this.Location.Y + 1);
            }
            else
            {
                freplay.Location = new System.Drawing.Point(this.Location.X + 645, this.Location.Y + 1);
            }
            counter++;
            rec_counter++;

            if (load_item != null)
            {
                repaly(load_item);
                Form3.selecteds = null;
                load_item = null;
            }

            if (!isreplay)
            {

                if (Image_save.bm != null)
                {

                    Image_save.image_mutex.WaitOne();
                    Bitmap temp_bm = (Bitmap)Image_save.bm.Clone();
                    Image_save.image_mutex.ReleaseMutex();

                    Image last = pictureBox1.Image;
                    pictureBox1.Image = new Bitmap(temp_bm, new Size(600, 450));
                    last.Dispose();
                    temp_bm.Dispose();

                    listBox1.SelectedIndex = HNC_Connect.gline;
                }
            }

            if ((!HNC_Connect.iscon) || (isreplay))
            {
                video_button.Enabled = false;
            }
            else if (HNC_Connect.iscon && (!checkBox2.Checked))
            {
                video_button.Enabled = true;
            }

            if (start_record)
            {
                sw.Write("T" + rec_counter.ToString("d5") + "\n");
                sw.Write(wm[0] + "\n");
                sw.Write(wm[1] + "\n");
                sw.Write(wm[2] + "\n");
                sw.Write(wm[3] + "\n");
                sw.Write(wm[4] + "\n");
                sw.Write("L" + wm[5].Remove(4) + "\n");
                sw.Write("N" + "\n");
            }

            if (HNC_Connect.progch_event)
            {
                HNC_Connect.progch_event = false;

                string progn = HNC_Connect.progN/*.Remove(0, 8)*/;
                label21.Text = progn;

                Thread.Sleep(100);
                if (HNC_Connect.load_event)
                {
                    HNC_Connect.load_event = false;
                    listBox1.Items.Clear();
                    temp_path = gcode_path + progn;
                    while (!read_gcode(temp_path)) ;
                    listBox1.SelectedIndex = 0;
                }

            }


            if (checkBox2.Checked)
            {
                if (HNC_Connect.cyc_event == true)
                {
                    HNC_Connect.cyc_event = false;

                    tim = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2") + DateTime.Now.Day.ToString("d2") + DateTime.Now.Hour.ToString("d2") + DateTime.Now.Minute.ToString("d2") + DateTime.Now.Second.ToString("d2");

                    if (HNC_Connect.cyc != 0)
                    {
                        creatDic(tim);
                        record_video(tim);
                        Image_save.record_event = true;
                        Thread.Sleep(10);
                        start_record = true;
                        //Image_save.writer.Open(NCVM_Form.FileV, 1280, 720, 25, VideoCodec.MPEG4, 5120000);
                        //Image_save.writer.Open(NCVM_Form.FileV, 1280, 960, 25, VideoCodec.MPEG4, 5120000);
                        Image_save.thread_save();
                        video_button.Text = "结束录像";
                        label13.Visible = true;
                        Start_button.Enabled = false;
                        replay_button.Enabled = false;
                        //checkBox1.Enabled = false;
                        checkBox2.Enabled = false;
                        rec_counter = 0;
                    }
                    else
                    {
                        start_record = false;
                        Thread.Sleep(10);
                        Image_save.record_event = true;
                        video_button.Text = "开始录像";
                        label13.Visible = false;
                        Start_button.Enabled = true;
                        replay_button.Enabled = true;
                        //checkBox1.Enabled = true;
                        checkBox2.Enabled = true;
                        listBox1.SelectedIndex = 0;
                        Thread.Sleep(40);
                        sw.Close();
                    }
                }
            }

            if (HNC_Connect.cyc != 0)
            {
                listBox1.SelectedIndex = HNC_Connect.gline;
            }
            //Marshal.ReleaseComObject();
        }

        private void replay_button_Click(object sender, EventArgs e)
        {
            if (!isreplay)
            {
                if (!freplay.Visible)
                {
                    freplay.Visible = true;
                }
                else
                {
                    freplay.Visible = false;
                }

            }
            else
            {
                isreplay = false;

                axWindowsMediaPlayer1.close();
                axWindowsMediaPlayer1.Visible = false;
                start_video(true);
                video_button.Enabled = true;
                screenshot_button.Enabled = true;
                Start_button.Enabled = true;
                replay_button.Text = "回  放";
                try
                {
                    list_read.Clear();
                    listBox1.Items.Clear();
                }
                catch (Exception ex)
                {
                    // Ignore
                }

                if (HNC_Connect.iscon)
                {
                    while (!read_gcode(gcode_path + HNC_Connect.progN)) ;
                }
                else
                {
                    listBox1.Items.Clear();
                    listBox1.Items.Add("Waiting...");
                }
                timer_replay.Stop();
                listBox1.SelectedIndex = 0;
            }

        }

        /// <summary>
        /// 回放记录
        /// </summary>
        /// <param name="folder">
        /// 路径名
        /// </param>
        private void repaly(string folder)
        {

            if (folder == null)
            {
                return;
            }

            string replay_a = his_path + folder + @"\" + folder;
            string replay_v = his_path + folder + @"\" + folder + ".avi";
            string replay_g = his_path + folder + @"\" + folder + ".nc";
            replay_t = his_path + folder + @"\" + folder + ".txt";

            isreplay = true;
            axWindowsMediaPlayer1.uiMode = "none";
            axWindowsMediaPlayer1.Visible = true;
            video_button.Enabled = false;
            screenshot_button.Enabled = false;
            start_record = false;
            start_video(false);

            replay_button.Text = "结束回放";
            Start_button.Text = "暂停回放";
            StreamReader tem = new StreamReader(replay_t);

            retim = tem.ReadLine();

            isopen = true;

            Thread.Sleep(40);

            listBox1.Items.Clear();
            while (!read_gcode(replay_g)) ;
            Thread.Sleep(40);
            timer_replay.Start();
            axWindowsMediaPlayer1.URL = his_path + folder + "/" + folder + ".avi";
            string last_t = null;
            string file = File.ReadAllText(replay_t);

            string[] pieces1 = file.Split('\n');

            foreach (string str1 in pieces1)
            {
                if (str1 == "" || str1 == "\n") //str1为空时不存储
                {
                    continue;
                }
                else if (str1[0] == 'T')
                {
                    last_t = str1;
                    list_read.Add(str1);
                }
                else //存储有效的数据
                {
                    list_read.Add(str1);
                }
            }

            txt_time = Convert.ToDouble(last_t.Remove(0, 1)) * 0.5;
            //txt_time = (last_t[1] - '0') * 500 + (last_t[2] - '0') * 50 + (last_t[3] - '0') * 5 + (last_t[4] - '0') * 0.5 + (last_t[5] - '0') * 0.05;

        }


        /// <summary>
        /// 按照时间读取回放数据
        /// </summary>
        /// <param name="path">
        /// 回放数据路径
        /// </param>
        /// <param name="time">
        /// 数据时间
        /// </param>
        /// <returns>
        /// 返回下一个时间
        /// </returns>
        private string read_data(string path, string time)
        {
            if (!isreplay)
            {
                return "N";
            }

            string temp1;
            string get_time = time;
            string l = null;

            char r1;

            double ta = 0.0F;

            double total_length = axWindowsMediaPlayer1.currentMedia.duration;       // 视频总长
            double coe = total_length / txt_time;                                    // 调整倍率

            while ((temp1 = list_read.First()) != null)
            {
                if (!isreplay)
                {
                    return "N";
                }
                if (list_read.Count < 5)
                {
                    link_read.Clear();
                    Thread.Sleep(40);
                    return "N";
                }

                try
                {
                    ta = axWindowsMediaPlayer1.Ctlcontrols.currentPosition;
                    list_read.Remove(list_read.First());
                }
                catch (Exception e)
                {
                    //Ignore
                }
                r1 = temp1[0];
                switch (r1)
                {
                    case 'X':
                        wm[0] = temp1;
                        break;
                    case 'Y':
                        wm[1] = temp1;
                        break;
                    case 'Z':
                        wm[2] = temp1;
                        break;
                    case 'F':
                        wm[3] = temp1;
                        break;
                    case 'S':
                        wm[4] = temp1;
                        break;
                    case 'L':
                        l = temp1.Remove(0, 1);
                        int line = Convert.ToInt32(l);
                        //int line = (l[0] - '0') * 1000 + (l[1] - '0') * 100 + (l[2] - '0') * 10 + (l[3] - '0');
                        try
                        {
                            listBox1.SelectedIndex = line;
                            if (line > 7)
                                listBox1.TopIndex = line - 5;
                        }
                        catch (Exception ex)
                        {
                            //Ignore
                        }
                        break;
                    case 'T':
                        get_time = temp1.Remove(0, 1);
                        //double rep_time = (temp1[1] - '0') * 500 + (temp1[2] - '0') * 50 + (temp1[3] - '0') * 5 + (temp1[4] - '0') * 0.5 + (temp1[5] - '0') * 0.05;

                        double rep_time = Convert.ToDouble(get_time) * 0.5;

                        if (rep_time * coe > ta)
                        {
                            timer_replay.Stop();
                            Thread.Sleep(150);
                            timer_replay.Start();
                            break;
                        }
                        else if (ta - rep_time * coe > 0.1)
                        {
                            break;
                        }
                        else
                        {
                            return get_time;
                        }
                    default:

                        break;
                }
            }
            return get_time;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            auto_rec = checkBox2.Checked;
            if (!isreplay)
            {
                if (checkBox2.Checked)
                {
                    video_button.Enabled = false;
                }
                else
                {
                    video_button.Enabled = true;
                }
            }
        }

        private void NCVM_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Setting_save.savefile("save.dat");
            Environment.Exit(0);
        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            if (fconnect.Visible == false)
            {
                fconnect.Visible = true;
                isconopen = true;
            }
            else
            {
                fconnect.Visible = false;
                isconopen = false;
            }
        }

        private void screenshot_button_Click(object sender, EventArgs e)
        {
            System.Drawing.Bitmap screenshot = Image_save.bm;
            screenshot.Save(Setting_save.scp_path + (DateTime.Now.Ticks / 10000).ToString() + ".bmp");
        }

        private void label14_Click(object sender, EventArgs e)
        {
            if (ispgavaliable)
            {
                ispgavaliable = false;
                label6.Visible = false;
                label7.Visible = false;
                label21.Visible = false;
                panel1.Visible = false;
                listBox1.Visible = false;
                screenshot_button.Location = new Point(220, 475);
                video_button.Location = new Point(320, 475);
                Start_button.Location = new Point(420, 475);
                replay_button.Location = new Point(520, 475);
                this.Size = new Size(645, 560);
                label14.Text = ">\n>";
            }
            else
            {
                ispgavaliable = true;
                this.Size = new Size(870, 560);
                ispgavaliable = true;
                label6.Visible = true;
                label7.Visible = true;
                label21.Visible = true;
                panel1.Visible = true;
                listBox1.Visible = true;
                Start_button.Location = new Point(646, 475);
                screenshot_button.Location = new Point(415, 475);
                video_button.Location = new Point(535, 475);
                replay_button.Location = new Point(755, 475);
                label14.Text = "<\n<";
            }
        }

        private void label14_MouseEnter(object sender, EventArgs e)
        {
            label14.BorderStyle = BorderStyle.None;
        }

        private void label14_MouseLeave(object sender, EventArgs e)
        {
            label14.BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
