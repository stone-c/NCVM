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
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

//HNCAPI
using HNCAPI_INTERFACE;

namespace NCVM
{
    class HNC_Connect
    {
        private static HncApi Api = new HncApi();

        public static string progN = null;
        public static string progN1 = null;
        public static string progNp = null;
        public static double pos_X = 1.0;
        public static double pos_Y = 0.0;
        public static double pos_Z = 0.0;
        public static double speed_F = 0.0;
        public static double speed_S = 0.0;

        private static int cyc_ch = 0;
        public static int cyc = 0;
        public static int gline = 0;
        public static int gsymbol = 0;

        public static string gcode_path;

        public static bool start_transfer = false;
        public static bool progch_event = false;
        public static bool load_event = false;
        public static bool cyc_event = false;
        public static bool iscon = false;

        public static string ipnr = Setting_save.con_ipnl;
        public static string ipnl = Setting_save.con_ipnr;
        public static UInt16 portn = Convert.ToUInt16(Setting_save.con_port);

        /// <summary>
        /// HNC接口获取信息线程初始化
        /// </summary>
        private static void thread_init()
        {
            //ipnl = Setting_save.con_ipnl;
            //ipnr = Setting_save.con_ipnr;
            //portn = (UInt16)Convert.ToInt32(Setting_save.con_port);
        }

        /// <summary>
        /// HNC接口获取信息线程
        /// </summary>
        public static void thread_connect()
        {
            thread_init();
            Task connect = new Task(hnc_con);
            //Thread connect = new Thread(hnc_con);
            //connect.IsBackground = true;
            connect.Start();
        }

        /// <summary>
        /// 循环获取所需HNC状态信息
        /// </summary>
        public static void hnc_con()
        {
            if (!iscon)
            {
                return;
            }

            // 循环获取HNC状态信息
            Api.HNC_ChannelGetValue(5, 0, 0, ref gsymbol);         // G代码代号
            Api.HNC_FprogGetFullName(0, ref progNp);               // G代码名称
            if (progNp == null)
            //if ((progNp == null)&&(start_transfer))
            {
                //MessageBox.Show("提示：请加载G代码");
                return;
            }

            if (progNp[0] == '/')
            {
                progN = progNp.Remove(0, 13);
            }
            else
            {
                progN = progNp.Remove(0, 8);
            }
            if (progN1 != progN)
            {
                progN1 = progN;
                progch_event = true;
                //String temp_r = "mnt/hgfs/hnc/share/HNC8_V1.26.04" + progN;
                String temp_r = "/h/lnc8/prog/" + progN;
                String temp_l = gcode_path + progN;
                int retr = getfile(temp_r, temp_l);
                if (retr != 0)
                {
                    MessageBox.Show("G代码下载失败，错误码：" + retr.ToString());
                }
                else
                {
                    load_event = true;
                }
            }
            Api.HNC_AxisGetValue(6, 0, ref pos_X);                 // X位置
            Api.HNC_AxisGetValue(6, 1, ref pos_Y);                 // Y位置
            Api.HNC_AxisGetValue(6, 2, ref pos_Z);                 // Z位置
            Api.HNC_ChannelGetValue(18, 0, 0, ref cyc);            // 循环启动
            Api.HNC_ChannelGetValue(32, 0, 0, ref gline);          // G代码行号
            Api.HNC_ChannelGetValue(47, 0, 0, ref speed_S);        // 主轴转速
            Api.HNC_ChannelGetValue(6, 0, 0, ref speed_F);         // 进给速度

            // 循环启动变化触发事件
            if (cyc_ch != cyc)
            {
                cyc_ch = cyc;
                cyc_event = true;
            }

        }

        /// <summary>
        /// 连接机床网络
        /// </summary>
        /// <param name="ipr">
        /// 机床ip
        /// </param>
        /// <param name="ipl">
        /// 本地ip
        /// </param>
        /// <param name="port">
        /// 机床端口
        /// </param>
        /// <returns>
        /// true：连接成功
        /// false：连接失败
        /// </returns>
        public static bool start_connect(string ipr, string ipl, UInt16 port)
        {

            ipnl = ipl;
            ipnr = ipr;
            portn = port;
            int ret;

            // 初始化
            ret = Api.HNC_NetInit(ipnl, 9090, "NCVM");
            if (ret != 0)
            {
                MessageBox.Show("初始化失败");
            }

            // 连接
            ret = Api.HNC_NetConnect(ipnr, portn);

            Thread.Sleep(10);

            // 检测是否已连接
            if (Api.HNC_NetIsConnect(ipnr, portn) == 0)
            {
                iscon = true;
            }
            return iscon;
        }

        /// <summary>
        /// 检查是否已经连接
        /// </summary>
        /// <returns>
        /// true：已连接
        /// false：未连接
        /// </returns>
        public static bool check_connect()
        {
            if (ipnr == null)
            {
                return false;
            }

            if (Api.HNC_NetIsConnect(ipnr, portn) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据路径从机床获取G代码文件
        /// </summary>
        /// <param name="rpath">
        /// 机床端路径
        /// </param>
        /// <param name="lpath">
        /// 本地路径
        /// </param>
        /// <returns>
        /// 0：成功
        /// 其他：失败
        /// </returns>
        public static Int32 getfile(String rpath, String lpath)
        {
            Int32 ret = FtpApi.DownloadFile(rpath, lpath, ipnr);
            return ret;
        }

    }

}