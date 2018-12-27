using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    [Serializable]
    public class ServiceInfoModel
    {
        public ServiceInfoModel()
        {
            DeviceId = string.Empty;
            DeviceName = string.Empty;
            CommProgramName = string.Empty;
            IsStarted = false;
            IP = string.Empty;
            Port = string.Empty;
            ConnId = string.Empty;
            DeviceType = 0;
            SpreadModel = new ConcurrentDictionary<string, SpreadModel>();
            DicConnId = new ConcurrentDictionary<string, string>();
        }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 部件名称
        /// </summary>
        public string CommProgramName { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 客户端对象内存地址
        /// </summary>
        public string ConnId { get; set; }

        /// <summary>
        /// 1：TCP服务端
        /// 2：TCP客户端
        /// 3：Websocket服务端
        /// 4：Websocket客户端
        /// </summary>
        public int DeviceType { get; set; }

        /// <summary>
        /// 服务扩展属性
        /// </summary>
        public ConcurrentDictionary<string, SpreadModel> SpreadModel { get; set; }

        /// <summary>
        /// TCP服务端模式下和WebSocket模式下客户端连接池信息
        /// </summary>
        public ConcurrentDictionary<string, string> DicConnId { get; set; }


    }
}
