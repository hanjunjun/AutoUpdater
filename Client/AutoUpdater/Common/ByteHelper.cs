using System;

namespace AutoUpdater.Common
{
    public class ByteHelper
    {
        /// <summary>
        /// 封包
        /// 包头7字节：
        /// 固定标识：0xAA       1字节
        /// 命令字节：           1字节
        /// 包长度               4字节
        /// crc                  1字节  
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] PackByte(byte[] bytes, Command cmd)
        {
            byte GuDingBiaoShi1 = 0xAA;//{ 0xaa, 0xab };
            byte Commandmsg = (byte)cmd;
            byte[] len = new byte[8];//包长度 1字节
            long length = bytes.Length;
            len = BitConverter.GetBytes(length);
            
            byte[] crc = new byte[1] { 0x00 };//crc 1字节

            byte[] newbytes = new byte[11 + bytes.Length];
            newbytes[0] = GuDingBiaoShi1;
            newbytes[1] = Commandmsg;
            //Array.Copy(GuDingBiaoShi, 0, newbytes, 0, 2);//组装标识
            Array.Copy(len, 0, newbytes, 2, 8);
            Array.Copy(crc, 0, newbytes, 10, 1);
            Array.Copy(bytes, 0, newbytes, 11, bytes.Length);
            return newbytes;
        }
    }

    public enum Command
    {
        /// <summary>
        /// 更新缓存文件列表
        /// </summary>
        UpdateTempList = 0x01,

        /// <summary>
        /// 需要更新的文件bytes列表
        /// </summary>
        UpdateDatasList = 0x02,

        /// <summary>
        /// 
        /// </summary>
        SendNeedUpdateGUIDList = 0x03
    }
}
