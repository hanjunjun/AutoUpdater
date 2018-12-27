using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace AutoUpdater.Common
{
    public class SerializableHelper
    {
        //#region 将json串转换为对象
        ///// <summary>
        ///// 将json串转换为对象
        ///// </summary>
        ///// <param name="jsonStr">json字符串</param>
        ///// <returns></returns>
        //public static T DeserializeObject<T>(string jsonStr)
        //{
        //    return JsonConvert.DeserializeObject<T>(jsonStr);
        //}
        //#endregion

        //#region 将对象转换为json串
        ///// <summary>
        ///// 将对象转换为json串
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="ojb"></param>
        ///// <returns></returns>
        //public static string SerializeObject<T>(T ojb)
        //{
        //    return JsonConvert.SerializeObject(ojb);
        //}
        //#endregion

        #region 对象序列化成byte[]
        /// <summary>
        /// 对象序列化成byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToBytes(object obj)
        {
            BinaryFormatter formmatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formmatter.Serialize(stream, obj);
            stream.Position = 0;
            return stream.GetBuffer();

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    IFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(ms, obj);
            //    return ms.GetBuffer();
            //}
        }
        #endregion

        #region byte[]序列化成对象
        /// <summary>
        /// byte[]序列化成对象
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static object BytesToObject(byte[] body)
        {
            MemoryStream stream = new MemoryStream();
            stream.Position = 0;
            stream.Write(body, 0, body.Length);
            stream.Flush();
            stream.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object obj = formatter.Deserialize(stream);
            return obj;

            //using (MemoryStream ms = new MemoryStream(bytes))
            //{
            //    IFormatter formatter = new BinaryFormatter();
            //    return formatter.Deserialize(ms);
            //}
        }
        #endregion

        #region 由结构体转换为byte数组
        /// <summary>
        /// 由结构体转换为byte数组
        /// </summary>
        public static byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }
        #endregion

        #region 由byte数组转换为结构体
        /// <summary>
        /// 由byte数组转换为结构体
        /// </summary>
        public static T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }

        /// <summary>
        /// byte[]转结构体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T BytesToStruct<T>(byte[] bytes)
        {
            Type strcutType = typeof(T);
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        #endregion
    }
}
