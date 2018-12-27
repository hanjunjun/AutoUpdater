/************************************************************************
 * 文件名：Init_TCPServer.cs
 * 文件功能描述：网络TCP协议服务端
 * 作    者：  韩俊俊
 * 创建日期：2017年04月01日 20:36:38
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * 备注：该模块下变量不是共享的不要定义为static。
 * Copyright (c) 2017 Titan.Han . All Rights Reserved. 
 * ***********************************************************************/
using Amib.Threading;
using HPSocketCS;
using Infrastructure.Log;
using Infrastructure.Reflection;
using MyRobotModel.CommonModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using Infrastructure;
using Infrastructure.File;
using Infrastructures.Reflection;

namespace MyRobot.CommunicatePackage.Core
{
    public class InitTcpServer
    {
        #region 公共变量
        private static long 发包数量 = 0;
        private static readonly object _lock = new object();
        public string StrError;
        public string DbType;
        string _deviceIp;//设备ip
        string _hostIp;//主机ip
        string _port;//端口
        string _commProgramName;//部件名称
        string _name;//设备名称
        public int PeekBufferSize;
        public int LenSite;
        public int HeadSize;
        public int IsExistHead;
        public int RealLength;
        public int IsUnpack = 0;//是否启用拆包， 默认不启用
        public IDataResolve IResolve;                            //定义数据解析接口
        public object Obj = null;//定义反射对象
        public string StrDeviceId;//设备id
        DataTable _ds = new DataTable();
        string _sql = string.Empty;
        public static ConcurrentDictionary<string, IDataResolve> Dic = new ConcurrentDictionary<string, IDataResolve>();//存储实例化对象组件 存储部件
        public static ConcurrentDictionary<string, TcpPullServer<ClientInfo>> DicTcpServer = new ConcurrentDictionary<string, TcpPullServer<ClientInfo>>();//存储实例化对象组件 存储部件
        TcpPullServer<ClientInfo> _server = new TcpPullServer<ClientInfo>();//tcp对象
        public ConcurrentDictionary<string, string> DicConnId = new ConcurrentDictionary<string, string>();//存储客户端连接组件，Key：设备id  Value：连接对象，把每个客户端分类存储在对应的设备里（不能用静态，connid不同连接有重复）
        public ConcurrentDictionary<string, SpreadModel> SpreadObject = new ConcurrentDictionary<string, SpreadModel>();//静态扩展对象，存储每个客户端的额外信息。
        SmartThreadPool _smartThreadPool;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public InitTcpServer()
        {
            #region 初始化线程池
            STPStartInfo stp = new STPStartInfo();//线程详细配置参数  
                                                  //m_hThreadPool.STPStartInfo这个属性是只读属性，所以只能在实例化的时候设置  
            {
                /* stp.AsReadOnly();*///返回一个只读类型的STPStartInfo  
                                      //一个枚举值,储存工作项执行完成后是否调用回调方法,  
                                      //Never不调用,  
                                      //WhenWorkItemCanceled只有当工作项目被取消时调用  
                                      //WhenWorkItemNotCanceled只有当工作项目不取消调用  
                                      //Always调用  
                                      /*stp.CallToPostExecute = CallToPostExecute.Always;*///在这里选择总是回调  
                                                                                           //当工作项执行完成后,是否释放工作项的参数,如果释放,参数对象必须实现IDisposable接口  
                                                                                           //stp.DisposeOfStateObjects = true;
                                                                                           //当线程池中没有工作项时,闲置的线程等待时间,超过这个时间后,会释放掉这个闲置的线程,默认为60秒  
                stp.IdleTimeout = 300;//300s  
                                      //最大线程数,默认为25,  
                                      //注意,由于windows的机制,所以一般最大线程最大设置成25,  
                                      //如果设置成0的话,那么线程池将停止运行  
                stp.MaxWorkerThreads = 15;//15 thread  
                                          //只在STP执行Action<...>与Func<...>两种任务时有效  
                                          //在执行工作项的过程中,是否把参数传递到WorkItem中去,用做IWorkItemResult接口取State时使用,  
                                          //如果设置为false那么IWorkItemResult.State是取不到值的  
                                          //如果设置为true可以取到传入参数的数组  
                                          //stp.FillStateWithArgs = true;
                                          //最小线程数,默认为0,当没有工作项时,线程池最多剩余的线程数  
                stp.MinWorkerThreads = 5;//5 thread  
                                         //当工作项执行完毕后,默认的回调方法  
                                         //stp.PostExecuteWorkItemCallback = delegate (IWorkItemResult wir) { MessageBox.Show("ok" + wir.Result); };
                                         //是否需要等待start方法后再执行工作项,?默认为true,当true状态时,STP必须执行Start方法,才会为线程分配工作项  
                                         //stp.StartSuspended = true;
            }
            _smartThreadPool = new SmartThreadPool(stp);
            #endregion
        }
        #endregion

        #region 停止所有服务/停止指定服务
        /// <summary>
        /// 停止所有服务
        /// </summary>
        public void Stop()
        {
            if (DicTcpServer == null) return;
            foreach (KeyValuePair<string, TcpPullServer<ClientInfo>> kvp in DicTcpServer)
            {
                TcpPullServer<ClientInfo> sk = kvp.Value;
                sk.Destroy();
            }
        }

        /// <summary>
        /// 停止指定
        /// </summary>
        /// <param name="deviceId"></param>
        public bool Stop(string deviceId)
        {
            try
            {
                InitConfig(DbType);
                DicTcpServer[deviceId].Destroy();
                InitServiceInfo(false);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                //InitMonitorService.AutoResetEvent.Set();
            }
        }
        #endregion

        #region 初始化服务对象
        public void InitServiceInfo(bool isStarted)
        {
            if (ServiceInfoDictionary.ServiceInfoDic.ContainsKey(StrDeviceId))
            {
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].DeviceId = StrDeviceId;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].DeviceName = _name;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].CommProgramName = _commProgramName;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].IsStarted = isStarted;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].IP = _hostIp;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].Port = _port;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].ConnId = string.Empty;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].DeviceType = 1;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].SpreadModel = SpreadObject;
                ServiceInfoDictionary.ServiceInfoDic[StrDeviceId].DicConnId = DicConnId;
            }
            else
            {
                ServiceInfoModel model = new ServiceInfoModel();
                model.DeviceId = StrDeviceId;
                model.DeviceName = _name;
                model.CommProgramName = _commProgramName;
                model.IsStarted = isStarted;
                model.IP = _hostIp;
                model.Port = _port;
                model.ConnId = string.Empty;
                model.DeviceType = 1;
                model.SpreadModel = SpreadObject;
                model.DicConnId = DicConnId;
                ServiceInfoDictionary.ServiceInfoDic.TryAdd(StrDeviceId, model);
            }
        }
        #endregion

        #region 初始化数据
        public void InitConfig(string dbType)
        {
            if (dbType == "ORACLE")
            {
                
            }
            else if (dbType == "SQLSERVER")
            {
               
            }
            else if (dbType == "XIAOWEIDB")
            {
               
            }
            else
            {
                _deviceIp = "0.0.0.0";
                _hostIp = "0.0.0.0";
                _port = ConfigurationManager.AppSettings["AutoUpdaterServerPort"];
                _commProgramName = "MyRobot.Interface.AutoUpdater.dll";
                _name = DbType;
                StrDeviceId = DbType;
                PeekBufferSize = 0;
                LenSite = 0;
                HeadSize = 11;
                IsExistHead = 0;
                RealLength = 0;
                IsUnpack = 1;
            }
        }
        #endregion

        #region 启动服务
        public bool Start(bool isStart, ref string errorMsg)
        {
            try
            {
                InitConfig(DbType);//初始化配置
                Obj = ObjectReflection.CreateObject(_commProgramName.Substring(0, _commProgramName.IndexOf(".dll")));
                IResolve = Obj as IDataResolve;
                if (!Dic.ContainsKey(StrDeviceId))
                {
                    Dic.TryAdd(StrDeviceId, IResolve);
                }
                else
                {
                    Dic[StrDeviceId] = IResolve;
                }
                string param1 = string.Empty;
                IResolve.GetRules(StrDeviceId, DbType, ref param1);

                _server = new TcpPullServer<ClientInfo>();
                // 设置服务器事件
                _server.OnPrepareListen -= new TcpServerEvent.OnPrepareListenEventHandler(OnPrepareListen);
                _server.OnAccept -= new TcpServerEvent.OnAcceptEventHandler(OnAccept);
                _server.OnSend -= new TcpServerEvent.OnSendEventHandler(OnSend);
                _server.OnReceive -= new TcpPullServerEvent.OnReceiveEventHandler(OnReceive);
                _server.OnClose -= new TcpServerEvent.OnCloseEventHandler(OnClose);
                _server.OnShutdown -= new TcpServerEvent.OnShutdownEventHandler(OnShutdown);

                _server.OnPrepareListen += new TcpServerEvent.OnPrepareListenEventHandler(OnPrepareListen);
                _server.OnAccept += new TcpServerEvent.OnAcceptEventHandler(OnAccept);
                _server.OnSend += new TcpServerEvent.OnSendEventHandler(OnSend);
                _server.OnReceive += new TcpPullServerEvent.OnReceiveEventHandler(OnReceive);
                _server.OnClose += new TcpServerEvent.OnCloseEventHandler(OnClose);
                _server.OnShutdown += new TcpServerEvent.OnShutdownEventHandler(OnShutdown);
                _server.IpAddress = _hostIp;
                _server.Port = ushort.Parse(_port);
                if (isStart)
                {
                    // 启动服务
                    if (_server.Start())
                    {

                        InitServiceInfo(true);
                        //Log4Helper.WriteInfoLog(this.GetType(),
                        //    $@"设备id:{StrDeviceId}  设备名称:{_name}  $Server Start OK -> ({_hostIp}:{_port})");
                        if (!DicTcpServer.ContainsKey(StrDeviceId))
                        {
                            DicTcpServer.TryAdd(StrDeviceId, _server);
                        }
                        else
                        {
                            DicTcpServer[StrDeviceId] = _server;
                        }
                        return true;
                    }
                    else
                    {
                        InitServiceInfo(false);
                        string msg = $@"加载【{_name}】失败！×原因：{
                                string.Format("$Server Start Error -> {0}({1}) Port:{2}", _server.ErrorMessage,
                                    _server.ErrorCode, _port)
                            }";
                        //EmailHelper.SendQQEmail(InitDeviceScheduler.发件人邮箱,
                        //    InitDeviceScheduler.收件人邮箱列表.Split(','),
                        //    InitDeviceScheduler.ServiceName, InitDeviceScheduler.ServiceName + $@"加载【{_name}】失败！",
                        //    $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}：" + InitDeviceScheduler.ServiceName +
                        //    msg,
                        //    InitDeviceScheduler.发件人邮箱授权码);
                        errorMsg = msg;
                        return false;
                    }
                }
                else
                {
                    InitServiceInfo(false);
                    return false;
                }
            }
            catch (Exception e)
            {
                //Log4Helper.WriteErrorLog(this.GetType(), $@"设备id:{StrDeviceId}  设备名称:{_name}  " + e.ToString());
                errorMsg = e.ToString();
                return false;
            }
            finally
            {
                //InitMonitorService.AutoResetEvent.Set();
            }
        }
        #endregion

        #region 服务端事件
        HandleResult OnPrepareListen(IntPtr soListen)
        {
            //InitMonitorService.AutoResetEvent.Set();
            return HandleResult.Ok;
        }

        HandleResult OnAccept(IntPtr connId, IntPtr pClient)
        {
            return AcceptPool(connId);
            //var result = smartThreadPool.QueueWorkItem(() =>
            //{
            //    return AcceptPool(connId);
            //});
            //return result.Result;
        }

        private HandleResult AcceptPool(IntPtr connId)
        {
            try
            {
                string strAdvance = string.Empty;
                byte[] strCmd = new byte[0];
                bool IsDisconnect = false; //是否强制断开客户端
                string ip = string.Empty;
                ushort sport = 0;

                if (_server.GetRemoteAddress(connId, ref ip, ref sport))
                {
                    strAdvance = string.Format(" > [{0},OnAccept] -> PASS({1}:{2})", connId, ip.ToString(), sport);
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\Accept客户端", strAdvance, Guid.NewGuid().ToString());
                }
                else
                {
                    strAdvance = string.Format(" > [{0},OnAccept] -> Server_GetClientAddress() Error", connId);
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\Accept客户端", strAdvance, Guid.NewGuid().ToString());
                }
                ClientInfo clientInfo = new ClientInfo();
                clientInfo.ConnId = connId;
                clientInfo.IpAddress = ip;
                clientInfo.Port = sport;
                if (_server.SetExtra(connId, clientInfo) == false)
                {
                    //给客户端连接加载连接参数失败，处理出错。
                }
                //连接断开时要干掉客户端对象。不然会增长过大。
                if (DicConnId.ContainsKey(connId.ToString()))
                {
                    DicConnId[connId.ToString()] = StrDeviceId;
                }
                else
                {
                    DicConnId.TryAdd(connId.ToString(), StrDeviceId); //客户端连接池
                }

                //if (SpreadObject.ContainsKey(connId))
                //{
                //    SpreadObject[connId] = new SpreadModel();
                //}
                //else
                //{
                //    SpreadObject.TryAdd(connId, new SpreadModel());//客户端扩展对象池   
                //}
                Thread.Sleep(1500);
                if (Dic.ContainsKey(StrDeviceId))
                {
                    IResolve = Dic[StrDeviceId];
                    IResolve.OnLinkStart(connId.ToString(), ref SpreadObject, strAdvance, ref strCmd, ref IsDisconnect);
                    //是否强制断开连接True：强制断开客户端 False 不执行断开操作
                    if (IsDisconnect)
                    {
                        return HandleResult.Error;
                    }
                    if (strCmd.Length > 0)
                    {
                        if (_server.Send(connId, strCmd, strCmd.Length) == false)
                        {
                            WriteDeviceLog.WriteLog("Log\\" + _name + "\\OnConnect",
                                "   应答没有发出去。strCmd：" + Encoding.Default.GetString(strCmd), Guid.NewGuid().ToString());
                        }
                    }
                }

                InitServiceInfo(true);
                return HandleResult.Ok;
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + _name + "\\Accept客户端", ex.ToString(), Guid.NewGuid().ToString());
                return HandleResult.Error;
            }
            finally
            {
                //InitMonitorService.AutoResetEvent.Set();
            }
        }

        HandleResult OnClose(IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            return ClosePool(connId, enOperation, errorCode);
            //var result = smartThreadPool.QueueWorkItem(() =>
            //{
            //    return ClosePool(connId, enOperation, errorCode);
            //});
            //return result.Result;
        }

        private HandleResult ClosePool(IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            try
            {
                ClientInfo clientInfo = _server.GetExtra(connId);
                string.Format(" > [{0},OnAccept] -> PASS({1}:{2})", connId, clientInfo.IpAddress, clientInfo.Port);
                if (errorCode == 0)
                {
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\Close客户端",
                        string.Format(" > [{0},OnClose] -> On({1}:{2})", connId, clientInfo.IpAddress, clientInfo.Port),
                        Guid.NewGuid().ToString());
                }
                else
                {
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\Close客户端",
                        string.Format(" > [{0},OnError] -> OP:{1},CODE:{2} -> On({3}:{4})", connId, enOperation,
                            errorCode, clientInfo.IpAddress, clientInfo.Port), Guid.NewGuid().ToString());
                }
                if (_server.RemoveExtra(connId) == false)
                {
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\Close客户端",
                        string.Format(" > [{0},OnClose] -> SetConnectionExtra({0}, null) fail -> On({1}:{2})", connId,
                            clientInfo.IpAddress, clientInfo.Port), Guid.NewGuid().ToString());
                }
                //连接断开删除客户端对象
                string dicConnIdValue;
                DicConnId.TryRemove(connId.ToString(), out dicConnIdValue);
                SpreadModel value = null;
                SpreadObject.TryRemove(connId.ToString(), out value);
                //服务信息
                InitServiceInfo(true);
                return HandleResult.Ok;
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + _name + "\\Close客户端", ex.ToString(), Guid.NewGuid().ToString());
                return HandleResult.Error;
            }
            finally
            {
                //InitMonitorService.AutoResetEvent.Set();
            }
        }

        HandleResult OnSend(IntPtr connId, byte[] bytes)
        {
            //发送成功
            return HandleResult.Ok;
        }

        private bool GetDataLength(IntPtr connId, ref int reallength)
        {
            IntPtr bufferPtr = Marshal.AllocHGlobal(7);
            try
            {
                //Peek从数据包中窥视数据，不会影响缓存数据的大小
                if (_server.Peek(connId, bufferPtr, 7) == FetchResult.Ok)
                {
                    byte[] bufferBytes = new byte[7];
                    Marshal.Copy(bufferPtr, bufferBytes, 0, 7);
                    byte[] len = new byte[4];
                    Array.Copy(bufferBytes, 2, len, 0, 4);
                    int lens = BitConverter.ToInt32(len, 0);
                    reallength = lens;


                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);//释放申请的内存空间
            }
        }

        HandleResult OnReceive(IntPtr connId, int length)
        {
            //针对同一连接的数据它是同步的，针对不同连接才是异步执行。
            //根据数据库中的配置参数来控制拆包解包规则。
            try
            {
                if (IsUnpack == 0)
                {
                    //不启用拆包
                    IntPtr recivebuffer = Marshal.AllocHGlobal(length);
                    if (_server.Fetch(connId, recivebuffer, length) == FetchResult.Ok)
                    {
                        byte[] sendBytes = new byte[length];
                        Marshal.Copy(recivebuffer, sendBytes, 0, length);
                        Marshal.FreeHGlobal(recivebuffer);//释放申请的内存空间
                        return HandleDeviceMessage(sendBytes, length, connId);
                    }
                }
                else
                {
                    int reallength = 0;
                    int bytesRead = 0;//包大小
                    if (GetDataLength(connId, ref reallength))
                    {
                        while (length >= reallength + HeadSize)
                        {
                            length = length - reallength - HeadSize;
                            IntPtr recivebuffer = Marshal.AllocHGlobal(reallength + HeadSize);
                            //Fetch从数据包中捞出数据，改变缓存数据大小
                            if (_server.Fetch(connId, recivebuffer, reallength + HeadSize) == FetchResult.Ok)
                            {
                                //上面可以加配置，来控制该设备是否启用线程池处理数据或是启用单线程处理。
                                //使用线程池不会出现length缓存区溢出的情况。

                                byte[] sendBytes = new byte[reallength + HeadSize];
                                bytesRead = sendBytes.Length;
                                Marshal.Copy(recivebuffer, sendBytes, 0, bytesRead);
                                Marshal.FreeHGlobal(recivebuffer);//释放申请的内存空间
                                //return HandleDeviceMessage(sendBytes, bytesRead, connId);
                                var result = _smartThreadPool.QueueWorkItem(() =>
                                  {
                                      return HandleDeviceMessage(sendBytes, bytesRead, connId);
                                  });
                            }
                            GetDataLength(connId, ref reallength);
                        }
                    }
                }
                return HandleResult.Ok;
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + _name + "\\OnReceiveError", e.ToString(), Guid.NewGuid().ToString());
                return HandleResult.Error;
            }
        }

        HandleResult OnShutdown()
        {
            InitConfig(DbType);
            //服务关闭了
            WriteDeviceLog.WriteLog("Log\\" + _name + "\\Shutdown客户端", $@" > [OnShutdown] 端口：{_port} 设备ID：{StrDeviceId} 设备名称：{_name}", Guid.NewGuid().ToString());
            //服务信息
            InitServiceInfo(false);
            //InitMonitorService.AutoResetEvent.Set();
            return HandleResult.Ok;
        }
        #endregion

        #region 解析程序
        private HandleResult HandleDeviceMessage(byte[] handleData, int bytesRead, IntPtr connId)
        {
            lock (_lock)
            {
                try
                {
                    bool IsUpdater = false;
                    string strAdvance = string.Empty;
                    byte[] strCmd = new byte[0];
                    byte[] 扩展参数 = new byte[0];
                    bool IsDisconnect = false; //是否强制断开客户端
                    bool IsMass = false; //是否群发
                    IntPtr connIDKey;

                    ClientInfo clientInfo = _server.GetExtra(connId); //extra.Get(connId);
                    if (clientInfo != null)
                    {
                        strAdvance =
                            $@" > [{clientInfo.ConnId},OnReceive] -> {clientInfo.IpAddress}:{clientInfo.Port} ({
                                    bytesRead
                                } bytes)";
                    }
                    else
                    {
                        strAdvance = $@" > [{connId},OnReceive] -> ({bytesRead} bytes)";
                    }
                    if (Dic.ContainsKey(StrDeviceId))
                    {
                        string EventID = Guid.NewGuid().ToString();
                        IResolve = Dic[StrDeviceId];
                        IResolve.ParseResult(handleData, ref strCmd, EventID, ref IsDisconnect, ref IsMass, ref strAdvance,
                            connId.ToString(), ref SpreadObject, ref IsUpdater, ref 扩展参数);
                        //是否是更新服务
                        if (IsUpdater)
                        {
                            byte[] data = new byte[0];
                            FileStream fs=null;
                            //获得文件所在路径  
                            string filePath = strAdvance.Split('|')[0].Trim();
                            string 文件Id= strAdvance.Split('|')[1].Trim();
                            long 文件读取开始位置= long.Parse(strAdvance.Split('|')[2].Trim());
                            long 文件读取长度 = long.Parse(strAdvance.Split('|')[3].Trim());

                            byte[] 文件IdBytes = Encoding.Default.GetBytes(文件Id);//36字节
                            //打开文件  
                            try
                            {
                                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                                fs.Position = 文件读取开始位置;//设置开始读取位置
                                byte[] bytes = new byte[文件读取长度];
                                fs.Read(bytes, 0, (int)文件读取长度);//读取指定位置的数据
                                byte[] tempData = new byte[0];
                                byte[] 文件总长bytes = new byte[8];
                                文件总长bytes = BitConverter.GetBytes(文件读取长度);
                                tempData = CopyByte(扩展参数, 文件IdBytes);//文件路径+文件id
                                tempData = CopyByte(tempData, 文件总长bytes);//+文件总长度
                                tempData = CopyByte(tempData, bytes);//文件路径+文件id

                                tempData = ByteHelper.PackByte(tempData, Command.UpdateDatasList);//封包
                                                                                                  //向connId发送命令
                                if (_server.Send(connId, tempData, tempData.Length) == false)
                                {
                                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\ResponseError应答",
                                        strAdvance + "   应答没有发出去。",
                                        Guid.NewGuid().ToString());
                                }
                                else
                                {
                                    WriteDeviceLog.WriteLog("Log\\" + StrDeviceId + "\\解析日志", $@"{DateTime.Now}:发送包大小{tempData.Length}，路径：{filePath}" + "\r\n", Guid.NewGuid().ToString());
                                    //WriteDeviceLog.WriteLog("Log\\" + _name + "\\更新日志",
                                    //    $"发送包大小{tempData.Length}",
                                    //    Guid.NewGuid().ToString());
                                }
                                fs.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                            //尚未读取的文件内容长度  
                            //long left = fs.Length;
                            //long 文件总长 = left;
                            //byte[] 文件总长bytes=new byte[8];
                            //文件总长bytes = BitConverter.GetBytes(文件总长);
                            ////存储读取结果  
                            //byte[] bytes = new byte[10 * 1024 * 1024];//10MB每秒
                            //                                          //每次读取长度  
                            //int maxLength = bytes.Length;
                            ////读取位置  
                            //long start = 0;
                            ////实际返回结果长度  
                            //int num = 0;
                            //if (left == 0)
                            //{
                            //    发包数量++;
                            //    //如果是空文件
                            //    bytes = new byte[left];
                            //    num = fs.Read(bytes, 0, Convert.ToInt32(left));
                            //    byte[] tempData =new byte[0];
                            //    tempData = CopyByte(扩展参数, 文件IdBytes);//文件路径+文件id
                            //    tempData = CopyByte(tempData, 文件总长bytes);//+文件总长度
                            //    tempData = CopyByte(tempData, bytes);//文件路径+文件id

                            //    tempData = ByteHelper.PackByte(tempData, Command.UpdateDatasList);//封包
                            //                                                                      //向connId发送命令
                            //    if (_server.Send(connId, tempData, tempData.Length) == false)
                            //    {
                            //        WriteDeviceLog.WriteLog("Log\\" + _name + "\\ResponseError应答",
                            //            strAdvance + "   应答没有发出去。",
                            //            Guid.NewGuid().ToString());
                            //    }
                            //    else
                            //    {
                            //        WriteDeviceLog.WriteText(AppDomain.CurrentDomain.BaseDirectory + "Log\\更新日志.txt", $@"{DateTime.Now}:发包数量：{发包数量}，发送包大小{tempData.Length}，路径：{filePath}" + "\r\n");
                            //        //WriteDeviceLog.WriteLog("Log\\" + _name + "\\更新日志",
                            //        //    $"发送包大小{tempData.Length}",
                            //        //    Guid.NewGuid().ToString());
                            //    }

                            //}
                            ////当文件未读取长度大于0时，不断进行读取  
                            //while (left > 0)
                            //{
                            //    fs.Position = start;
                            //    num = 0;
                            //    if (left < maxLength)
                            //    {
                            //        bytes = new byte[left];
                            //        num = fs.Read(bytes, 0, Convert.ToInt32(left));
                            //    }

                            //    else
                            //    {
                            //        num = fs.Read(bytes, 0, maxLength);

                            //    }
                            //    Console.WriteLine($"文件原始长度{bytes.Length} 总长度{data.Length}");
                            //    byte[] tempData = new byte[0];
                            //    tempData = CopyByte(扩展参数, 文件IdBytes);//文件路径+文件id
                            //    tempData = CopyByte(tempData, 文件总长bytes);//+文件总长度
                            //    tempData = CopyByte(tempData, bytes);//文件路径+文件id
                            //    tempData = ByteHelper.PackByte(tempData, Command.UpdateDatasList);//封包
                            //                                                                  //向connId发送命令
                            //    if (_server.Send(connId, tempData, tempData.Length) == false)
                            //    {
                            //        WriteDeviceLog.WriteLog("Log\\" + _name + "\\ResponseError应答",
                            //            strAdvance + "   应答没有发出去。",
                            //            Guid.NewGuid().ToString());
                            //    }
                            //    else
                            //    {
                            //        发包数量++;
                            //        WriteDeviceLog.WriteText(AppDomain.CurrentDomain.BaseDirectory + "Log\\更新日志.txt", $@"{DateTime.Now}:发包数量：{发包数量}，发送包大小{tempData.Length}，路径：{filePath}" + "\r\n");
                            //        //WriteDeviceLog.WriteLog("Log\\" + _name + "\\更新日志",
                            //        //    $"发送包大小{tempData.Length}",
                            //        //    Guid.NewGuid().ToString());
                            //    }
                            //    Console.WriteLine($"包头+长度+文件内容长度{tempData.Length}");
                            //    if (num == 0)
                            //        break;
                            //    start += num;
                            //    left -= num;
                            //    //Thread.Sleep(888);
                            //}
                            //fs.Close();
                            
                        }
                        //是否强制断开连接True：强制断开客户端 False 不执行断开操作
                        if (IsDisconnect)
                        {
                            return HandleResult.Error;
                        }
                        //是否群发命令 True：群发命令 False：不群发
                        if (IsMass)
                        {
                            //启用群发命令
                            if (strCmd.Length > 0)
                            {
                                if (DicConnId != null)
                                {
                                    foreach (KeyValuePair<string, string> kvp in DicConnId)
                                    {
                                        connIDKey = (IntPtr)int.Parse(kvp.Key);
                                        if (kvp.Value.Contains(StrDeviceId))
                                        {
                                            if (_server.Send((IntPtr)int.Parse(kvp.Key), strCmd, strCmd.Length) == false) //拿到kvp.Key客户端对象
                                            {
                                                WriteDeviceLog.WriteLog("Log\\" + _name + "\\ResponseError应答",
                                                    strAdvance +
                                                    $"   应答没有发出去。发送对象：{_server.GetExtra((IntPtr)int.Parse(kvp.Key)).IpAddress}:{_server.GetExtra((IntPtr)int.Parse(kvp.Key)).Port} strCmd：" +
                                                    Encoding.Default.GetString(strCmd), Guid.NewGuid().ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (strCmd.Length > 0)
                            {
                                if (_server.Send(connId, strCmd, strCmd.Length) == false) //拿到kvp.Key客户端对象
                                {
                                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\ResponseError应答",
                                        strAdvance +
                                        $"   应答没有发出去。strCmd：" +
                                        Encoding.Default.GetString(strCmd), Guid.NewGuid().ToString());
                                }
                            }
                        }
                        //是否一直保持连接，True：服务端一直连着客户端，False：服务端解析完成客户端的数据断开客户端。
                        if (IResolve.IsContinueConnecting() == false)
                        {
                            return HandleResult.Error;
                        }
                    }
                    return HandleResult.Ok;
                }
                catch (Exception e)
                {
                    WriteDeviceLog.WriteLog("Log\\" + _name + "\\HandleDeviceMessageError", e.ToString(),
                        Guid.NewGuid().ToString());
                    return HandleResult.Error;
                }
            }
        }
        #endregion

        /// <summary>
        /// 连接2个数组
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] CopyByte(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);
            return c;
        }

    }
}
