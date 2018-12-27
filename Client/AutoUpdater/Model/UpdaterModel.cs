using System;
using System.Runtime.InteropServices;

namespace MyRobotModel
{
    #region 需要更新的文件列表
    /// <summary>
    /// 需要更新的文件列表
    /// </summary>
    public class UpdaterModel
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// 包总大小  单位：字节
        /// </summary>
        public long 包总大小 { get; set; } = 0;

        /// <summary>
        /// 记录读了多少字节  单位：字节
        /// </summary>
        public long 记录读了多少字节 { get; set; } = 0;

        /// <summary>
        /// 是否下载完成
        /// </summary>
        public bool 是否下载完成 { get; set; } = false;

        /// <summary>
        /// 分段数量 这个文件一共多少段
        /// </summary>
        public int 分段数量 { get; set; } = 0;

        /// <summary>
        /// 请求段
        /// </summary>
        public int 请求段 { get; set; } = 0;

        /// <summary>
        /// 已读段
        /// </summary>
        public int 已读段 { get; set; } = 0;

        /// <summary>
        /// 分段大小
        /// </summary>
        public int 分段大小 { get; set; } = 1024;

        /// <summary>
        /// 文件总长度 单位：字节
        /// </summary>
        public long 文件总长度 { get; set; }
    }
    #endregion

    #region 请求文件信息
    public class 请求文件信息
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public Guid FileId { get; set; }

        public long 文件读取开始位置 { get; set; }

        public long 文件读取长度 { get; set; }
    }
    #endregion

    #region 从服务器下载的需要更新的文件列表
    public class FileInformation
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string BaseFilePath { get; set; }
        public string FileMd5 { get; set; }

        /// <summary>
        /// 文件总长度 单位：字节
        /// </summary>
        public long 文件总长度 { get; set; }
    }
    //[StructLayout(LayoutKind.Sequential)]
    //public struct FileInformations
    //{
    //    /// <summary>
    //    /// 文件ID
    //    /// </summary>
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public Guid FileId;
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public string FileName;
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public string FilePath;
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public string BaseFilePath;
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public string FileMd5;

    //    /// <summary>
    //    /// 文件总长度 单位：字节
    //    /// </summary>
    //    [MarshalAs(UnmanagedType.LPStr)]
    //    public long 文件总长度;
    //}
    #endregion
}
