using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel
{
    public  class FileInformation
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

    /// <summary>
    /// 文件内容
    /// </summary>
    public class FileByteContent
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// 文件内容
        /// </summary>
        public byte[] FileBytes { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        /// <summary>
        /// \\test.txt
        /// </summary>
        public string BaseFilePath { get; set; }
        public string FileMd5 { get; set; }
    }
}
