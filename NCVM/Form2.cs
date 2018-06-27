using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace NCVM
{
    public partial class Form2 : Form
    {
        public static bool connect_event = false;
        public static bool wm_allowed = true;
        public static bool rl_changed = false;

        private static string iplf3;
        private static string iprf3;
        private static string portf3;

        public static int selected_color = 0;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            iplf3 = Setting_save.con_ipnl;
            iprf3 = Setting_save.con_ipnr;
            portf3 = Setting_save.con_port;

            textBox1.Text = iprf3;
            textBox2.Text = portf3;
            textBox3.Text = iplf3;

            checkBox1.Checked = true;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            this.MaximumSize = new Size(354, 376);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '.')
            {
                e.Handled = true;  //非以上键则禁止输入
            }
            if (e.KeyChar == '.' && textBox1.Text.Trim() == "") e.Handled = true; //禁止第一个字符就输入小数点
            //if (e.KeyChar == '.' && textBox1.Text.Contains(".")) e.Handled = true; //禁止输入多个小数点
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UInt16 port;
            int porti;
            string pstr;
            if ((textBox1.Text == null) || (textBox3.Text == null))
            {
                MessageBox.Show("请输入IP!");
            }
            if (textBox2.Text == null)
            {
                MessageBox.Show("请输入端口号!");
            }
            pstr = textBox2.Text;
            porti = (pstr[0] - '0') * 10000 + (pstr[1] - '0') * 1000 + (pstr[2] - '0') * 100 + (pstr[3] - '0') * 10 + (pstr[4] - '0');
            port = (UInt16)porti;
            //HNC_Connect.start_connect(textBox1.Text, port);
            if (!HNC_Connect.start_connect(textBox1.Text, textBox3.Text, port))
            {
                MessageBox.Show("连接失败，请重试");
                return;
            }
            connect_event = true;
            this.Visible = false;
            HNC_Connect.start_transfer = true;
            Setting_save.savefile("save.dat");
            //HNC_Connect.thread_connect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            NCVM_Form.isconopen = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selected_color = comboBox1.SelectedIndex;
            //this.Visible = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            wm_allowed = checkBox1.Checked;

            if(checkBox1.Checked)
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            rl_changed = true;

            if (comboBox2.SelectedIndex==0)
            {
                Image_save.left = 20.0F;
                Image_save.bottom = 940.0F;
            }
            else if(comboBox2.SelectedIndex==1)
            {
                Image_save.left = 1050.0F;
                Image_save.bottom = 940.0F;
            }
            else
            {
                Image_save.left = 1050.0F;
                Image_save.bottom = 230.0F;
            }

        }
    }

}
