/************************************************************************
 * 文件名：TcpClientWithTimeout
 * 文件功能描述：xx控制层
 * 作    者：  韩俊俊
 * 创建日期：2018/10/30 10:30:28
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * Copyright (c) 2017 Titan.Han. All Rights Reserved. 
 * ***********************************************************************/
using System;
using System.Net.Sockets;
using System.Threading;


namespace AutoUpdater.Common
{
    public class TcpClientWithTimeout
    {
        #region 构造函数
        protected string HostName;
        protected int Port;
        protected int TimeoutMilliseconds;
        protected TcpClient Connection;
        protected bool Connected;
        protected Exception Exception;
        public TcpClientWithTimeout(string hostname, int port, int timeout_milliseconds)
        {
            HostName = hostname;
            Port = port;
            TimeoutMilliseconds = timeout_milliseconds*1000;
        }
        #endregion

        public TcpClient Connect()
        {
            // kick off the thread that tries to connect
            Connected = false;
            Exception = null;
            Thread thread = new Thread(new ThreadStart(BeginConnect));
            thread.IsBackground = true; // 作为后台线程处理
            // 不会占用机器太长的时间
            thread.Start();

            // 等待如下的时间
            thread.Join(TimeoutMilliseconds);

            if (Connected == true)
            {
                // 如果成功就返回TcpClient对象
                thread.Abort();
                return Connection;
            }
            if (Exception != null)
            {
                // 如果失败就抛出错误
                thread.Abort();
                throw Exception;
            }
            else
            {
                // 同样地抛出错误
                thread.Abort();
                string message = string.Format("TcpClient connection to {0}:{1} timed out",
                    HostName, Port);
                throw new TimeoutException(message);
            }
        }
        protected void BeginConnect()
        {
            try
            {
                Connection = new TcpClient(HostName, Port);
                // 标记成功，返回调用者
                Connected = true;
            }
            catch (Exception ex)
            {
                // 标记失败
                Exception = ex;
            }
        }
    }
}
