using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace NCVM
{
    public partial class Form3 : Form
    {
        //public static string his_path = "D:/NCVM/History/";
        //public static string his_path = Setting_save.his_path;
        public static string his_path;

        public static int selected;

        public static string selecteds = null;
        public static string delete = null;

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.MaximumSize = new Size(246, 586);

            LookFile(his_path);

        }

        public void LookFile(string pathname)
        {
            listBox1.Items.Clear();
            String dir1;
            if (pathname.Trim().Length == 0)//判断文件名不为空            
            {
                return;
            }            //获取文件夹下的所有文件和文件夹       
            string[] files = Directory.GetFileSystemEntries(pathname);
            try
            {
                foreach (string dir in files)
                {
                    dir1 = dir.Remove(0, his_path.Length);
                    listBox1.Items.Add(dir1);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();//防止有些文件无权限访问，屏蔽异常       
            }
        }

        private void load_button_Click(object sender, EventArgs e)
        {

            if (listBox1.SelectedItem != null)
            {
                selected = listBox1.SelectedIndex;
                selecteds = listBox1.SelectedItem.ToString();
                this.Visible = false;
            }
        }

        private void refresh_button_Click(object sender, EventArgs e)
        {
            LookFile(his_path);
        }

        private void delete_button_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {

                delete = listBox1.SelectedItem.ToString();
                string pathname = his_path + delete + @"\";
                Directory.Delete(pathname, true);
                LookFile(his_path);
            }
        }

    }

}
