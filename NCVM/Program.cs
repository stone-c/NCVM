using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCVM
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Setting_save.openfile("save.dat");
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Image_save.thread_video();
            Control.CheckForIllegalCrossThreadCalls = false;
            Application.Run(new NCVM_Form());
        }

        //private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    DialogResult dr = MessageBox.Show(e.ExceptionObject.ToString(), "错误! 程序即将退出", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    if (dr == DialogResult.OK)
        //    {
        //        Environment.Exit(0);
        //    }
        //    else
        //    {

        //    }
        //}
    }
}
