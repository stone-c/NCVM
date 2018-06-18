using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NCVM
{
    class Setting_save
    {

        private static string savefilepath = null;             // 路径加文件名

        public static string res_path = null;                  // 资源路径
        public static string his_path = null;                  // 历史路径
        public static string scp_path = null;                  // 截频路径
        public static string con_ipnl = null;                  // 本地ip
        public static string con_ipnr = null;                  // 远程ip
        public static string con_port = null;                  // 端口
        public static string ffmpeg_p = null;                  // ffmpeg路径

        private static StreamWriter sw;                        // 文件写入流

        /// <summary>
        /// 打开保存文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void openfile(string path)
        {
            savefilepath = path;

            if (!File.Exists(savefilepath))
            {
                DialogResult dr1 = MessageBox.Show("未找到存储文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (dr1 == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
                return;
            }

            string fileread = File.ReadAllText(savefilepath);

            string[] pieces1 = fileread.Split('\n');

            res_path = pieces1[0].Replace("\r", "");
            his_path = pieces1[1].Replace("\r", "");
            scp_path = pieces1[2].Replace("\r", "");
            con_ipnl = pieces1[3].Replace("\r", "");
            con_ipnr = pieces1[4].Replace("\r", "");
            con_port = pieces1[5].Replace("\r", "");
            ffmpeg_p = pieces1[6].Replace("\r", "");

            //Image_save.FFmpegPath = ffmpeg_p;

            NCVM_Form.his_path = his_path;
            NCVM_Form.gcode_path = res_path;

            Form3.his_path = his_path;

            HNC_Connect.gcode_path = res_path;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void savefile(string path)
        {
            savefilepath = path;

            if (!File.Exists(savefilepath))
            {
                File.CreateText(savefilepath);
            }

            con_ipnl = HNC_Connect.ipnl;
            con_ipnr = HNC_Connect.ipnr;
            con_port = HNC_Connect.portn.ToString();

            sw = new StreamWriter(savefilepath);

            sw.WriteLine(res_path);
            sw.WriteLine(his_path);
            sw.WriteLine(scp_path);
            sw.WriteLine(con_ipnl);
            sw.WriteLine(con_ipnr);
            sw.WriteLine(con_port);
            sw.WriteLine(ffmpeg_p);

            sw.Close();
        }

    }

}