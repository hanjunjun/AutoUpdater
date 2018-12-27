using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    [Serializable]
    public class SpreadModel
    {
        public SpreadModel()
        {
            connId = string.Empty;
            DeviceCode = string.Empty;
            strToken = string.Empty;
            AccessAddress = string.Empty;
            FirstTimeConnectTime = null;
            LastTimeHandleTime = null;
            strAdvance = string.Empty;
        }

        /// <summary>
        /// 服务端/客户端连接对象
        /// </summary>
        public string connId { get; set; }

        /// <summary>
        /// （可依恋老人机）设备编号
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
        /// 大耳马床垫登录Token
        /// </summary>
        public string strToken { get; set; }

        /// <summary>
        /// 接入地址
        /// </summary>
        public string AccessAddress { get; set; }

        /// <summary>
        /// 建立连接时间
        /// </summary>
        public DateTime? FirstTimeConnectTime { get; set; }

        /// <summary>
        /// 最近一次解析时间
        /// </summary>
        public DateTime? LastTimeHandleTime { get; set; }

        /// <summary>
        /// 在线时长
        /// </summary>
        public string OnlineTime { get; set; }

        /// <summary>
        /// 扩展字段
        /// </summary>
        public string strAdvance { get; set; }
    }
}
