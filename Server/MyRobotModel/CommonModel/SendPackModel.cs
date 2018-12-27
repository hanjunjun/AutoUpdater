using System;

namespace MyRobotModel.CommonModel
{
    [Serializable]
    public class SendPackModel
    {
        /// <summary>
        /// 操作标识
        /// </summary>
        public int OperateType { get; set; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 设备类型
        /// 1：TCP服务端
        /// 2：TCP客户端
        /// 3：Websocket服务端
        /// 4：Websocket客户端
        /// </summary>
        public int DeviceType { get; set; }

        /// <summary>
        /// 连接对象的内存
        /// </summary>
        public string ConnId { get; set; }
    }
}
