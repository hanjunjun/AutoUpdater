using System;
using System.Runtime.InteropServices;

namespace MyRobotModel.CommonModel
{
    [StructLayout(LayoutKind.Sequential)]
    public class ClientInfo
    {
        public IntPtr ConnId { get; set; }
        public string IpAddress { get; set; }
        public ushort Port { get; set; }
    }
}
