using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace NCVM
{
    class FtpApi
    {
        public const UInt32 MAX_DISPLAY_NUM = 512;
        public const UInt32 MAX_LINE_SIZE = 128;

        [DllImport("ftp.dll", EntryPoint = "UploadFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 UploadFile(String remotepath, String localpath, String ipaddr);

        [DllImport("ftp.dll", EntryPoint = "DownloadFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 DownloadFile(String remotepath, String localpath, String ipaddr);

        [DllImport("ftp.dll", EntryPoint = "GetDirInfo", CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 GetDirInfo(String remotepath, String ipaddr, Byte[] info);

        [DllImport("ftp.dll", EntryPoint = "RemoveFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 RemoveFile(String remotepath, String ipaddr);

        [DllImport("ftp.dll", EntryPoint = "RenameFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 RenameFile(String remotepath, String newname, String ipaddr);

        public static Int32 GetDirInfo(String remotepath, String ipaddr, List<String> info)
        {
            Byte[] data = new Byte[MAX_DISPLAY_NUM * MAX_LINE_SIZE];
            Int32 ret = GetDirInfo(remotepath, ipaddr, data);
            if (ret != 0)
            {
                return ret;
            }

            String content = Encoding.Default.GetString(data).Trim('\0');

            String[] arr = content.Split('\n');
            for (Int32 i = 0; i < arr.Length; i++)
            {
                if (arr[i].Length > 0)
                {
                    info.Add(arr[i]);
                }
            }

            return 0;
        }
    }
}
