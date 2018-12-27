using AutoUpdater.Common;
//using AutoUpdater.Model;
using MyRobotModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoUpdater
{
    public partial class MainForm : Form
    {
        private static long 分段读取基准 = 0;//11MB
        private static double 总共下载字节数 = 0;
        private static long 包数量 = 0;
        private static long _pageSize = 10 * 1024 * 1024;//2MB
        private string _baseDir = AppDomain.CurrentDomain.BaseDirectory;//客户端根目录
        public static List<FileInformation> FileListFileInformation = null;
        //public List<FileByteContent> FileByteContentsList = null;
        private static string _callBackExeName;   //自动升级完成后，要启动的exe的名称。
        private static string _callBackPath; //自动升级完成后，要启动的exe的完整路径。      
        private static List<UpdaterModel> _UpdateList = new List<UpdaterModel>();
        private string _title = string.Empty;
        //tcp客户端
        private NetworkStream networkStream = null;

        private TcpClient tcpClient = null;
        public MainForm(string serverIp, int serverPort, string callBackExeName, string title, long pageSize)
        {
            try
            {
                分段读取基准 = pageSize;
                InitializeComponent();//加载窗体
                                      //连接服务器
                AccessServer(serverIp, serverPort);
                //初始化数据
                DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                _callBackExeName = callBackExeName;
                _callBackPath = dir.Parent.FullName + "\\" + _callBackExeName;
                this.Text = title;
                _title = title;
                //获取主程序是否需要更新
                if (IsNeedToUpdate())
                {
                    //1.需要更新先关闭主程序
                    FileHelper.KillProcess(_callBackExeName);
                    GetUpdateList();

                }
                else
                {
                    Console.WriteLine("NoUpdater");
                    //不需要更新，关闭自己
                    System.Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", ex.ToString(), Guid.NewGuid().ToString());
                Console.WriteLine(ex.ToString());
                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// 连接服务器获取更新文件
        /// </summary>
        /// <param name="serverIp"></param>
        /// <param name="serverPort"></param>
        public void AccessServer(string serverIp, int serverPort)
        {
            try
            {
                tcpClient = new TcpClientWithTimeout(serverIp,serverPort,int.Parse(ConfigurationManager.AppSettings["ConnectTimeOutSeconds"])).Connect();  //创建一个TcpClient对象，自动分配主机IP地址和端口号  
                tcpClient.ReceiveBufferSize = 10 * 1024 * 1024;//2*1024*1024;//默认的接收缓冲区大小，单位字节。
                //tcpClient.Connect(serverIp, serverPort);   //连接服务器，其IP和端口号为127.0.0.1和51888  

                networkStream = tcpClient.GetStream();
                BinaryWriter bw = new BinaryWriter(networkStream);

                //byte[] sendBytes = Encoding.Default.GetBytes("你好服务器，我是客户端");
                //bw.Write(sendBytes);  //向服务器发送字符串  

                //开个线程去接收数据
                Thread threadDataReceive = new Thread(new ThreadStart(ReceiveDatas));
                threadDataReceive.IsBackground = true;
                threadDataReceive.Start();

                //开个线程去计算下载速度
                Thread countDownSpeed = new Thread(new ThreadStart(CountDownSpeed));
                countDownSpeed.IsBackground = true;
                countDownSpeed.Start();
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                Console.WriteLine(e.ToString());
                System.Environment.Exit(0);
            }
        }

        /// <summary>
        /// 计算下载速度
        /// </summary>
        private void CountDownSpeed()
        {

            while (true)
            {

                Thread.Sleep(4000);
                //计算下载速度
                var 经过的秒数 = 3;
                double 平均每秒下载速度 = 总共下载字节数 / 经过的秒数;
                总共下载字节数 = 0;
                string displayDownSpeed = string.Empty;
                var 标题 = string.Empty;

                if (平均每秒下载速度 < 1024)
                {
                    //b 字节
                    displayDownSpeed = $"{平均每秒下载速度}B/s";
                    //Console.WriteLine(displayDownSpeed);
                }
                else if (平均每秒下载速度 >= 1024 && 平均每秒下载速度 < 1048576)
                {
                    //kb
                    displayDownSpeed = $"{Math.Round(平均每秒下载速度 / 1024, 0)}KB/s";
                    //Console.WriteLine(displayDownSpeed);
                }
                else if (平均每秒下载速度 >= 1048576 && 平均每秒下载速度 < 1073741824)
                {
                    //mb
                    displayDownSpeed = $"{(平均每秒下载速度 / 1024 / 1024).ToString("#0.0")}MB/s";
                    //Console.WriteLine(displayDownSpeed);
                }
                else
                {
                    //gb
                    displayDownSpeed = $"{(平均每秒下载速度 / 1024 / 1024 / 1024).ToString("#0.0")}G/s";
                    //Console.WriteLine(displayDownSpeed);
                }
                //this.Text=title += $"                                {displayDownSpeed}";
                标题 = $"{_title}--{displayDownSpeed}";
                IncreaceForm(标题);
            }
        }

        public void SendUpdater()
        {
            try
            {
                SetUpdateProgressBarMax(_UpdateList.Count);//下载进度条总值
                                                           //发送需要下载的文件列表list
                BinaryWriter bw = new BinaryWriter(networkStream);
                int i = 0;
                foreach (var item in _UpdateList)
                {
                    i++;

                    while (true)
                    {
                        IncreaceLabel($"正在下载第{i}个文件...                               总共有{_UpdateList.Count}个文件需要更新");

                        //分段请求文件，一段下载完了才去请求下一段
                        long 文件开始读取位置 = 0;
                        //long pageSize = 1*1024*1024;//1MB
                        long 文件总长度 = item.文件总长度;
                        long 分段数量 = 文件总长度 / 分段读取基准;//

                        if (文件总长度 == 0)
                        {
                            分段数量++;
                        }
                        else
                        {
                            if (文件总长度 % 分段读取基准 > 0)
                            {
                                分段数量++;
                            }
                        }


                        SetDownFileMax((int)分段数量);
                        //Console.WriteLine($"最大值:{this.fileDown.Maximum}");
                        ClearDownFile();//清空文件下载进度条
                                        //Console.WriteLine($"清空值");
                        for (int pageIndex = 1; pageIndex <= 分段数量; pageIndex++)
                        {
                            IncreaceDownFile(1);//文件下载进度+1
                                                //Console.WriteLine($"当前值:{this.fileDown.Value}");
                            请求文件信息 model = new 请求文件信息();
                            model.FileId = item.FileId;
                            //如果是最后一段
                            if (pageIndex == 分段数量)
                            {
                                model.文件读取开始位置 = 文件开始读取位置;
                                model.文件读取长度 = 文件总长度 - 文件开始读取位置;
                                byte[] sendBytess = MsgPackHelper<请求文件信息>.Pack(model);
                                sendBytess = ByteHelper.PackByte(sendBytess, Command.SendNeedUpdateGUIDList);
                                bw.Write(sendBytess);  //向服务器发送文件id

                                item.请求段 += 1;
                                DateTime startTime = DateTime.Now;
                                while (true)
                                {
                                    DateTime endTime = DateTime.Now;
                                    if ((endTime.Ticks - startTime.Ticks) / 10000000 > long.Parse(ConfigurationManager.AppSettings["ReceiveTimeOutSeconds"]))
                                    {
                                        WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", "最后一个请求段超时！", Guid.NewGuid().ToString());
                                        StartMainPrograme("最后一个请求段超时！");
                                    }
                                    //已读段 读完一个包就+1；
                                    if (item.请求段 == item.已读段)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                //其他段

                                model.文件读取开始位置 = 文件开始读取位置;
                                model.文件读取长度 = 分段读取基准;
                                byte[] sendBytes = MsgPackHelper<请求文件信息>.Pack(model);
                                sendBytes = ByteHelper.PackByte(sendBytes, Command.SendNeedUpdateGUIDList);
                                bw.Write(sendBytes);  //向服务器发送文件id

                                item.请求段 += 1;
                                DateTime startTime = DateTime.Now;
                                while (true)
                                {
                                    DateTime endTime = DateTime.Now;
                                    if ((endTime.Ticks - startTime.Ticks) / 10000000 > long.Parse(ConfigurationManager.AppSettings["ReceiveTimeOutSeconds"]))
                                    {
                                        WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", "请求段超时！", Guid.NewGuid().ToString());
                                        StartMainPrograme("请求段超时！");
                                    }
                                    //已读段 读完一个包就+1；
                                    if (item.请求段 == item.已读段)
                                    {
                                        文件开始读取位置 = 文件开始读取位置 + 分段读取基准;
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                }
                                //WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $@"{DateTime.Now}{i}  发送文件ID：{item.FileId}", Guid.NewGuid().ToString());
                            }

                        }

                        //判断一个文件是否下载完成
                        if (item.是否下载完成 || item.请求段 == item.已读段)
                        {

                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $@"{DateTime.Now}{i}  文件ID：{item.FileId} 下载完成！", Guid.NewGuid().ToString());
                    IncreaceProgress(1);//进度条累加
                }
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $@"{DateTime.Now} 全部更新完成！", Guid.NewGuid().ToString());
                IncreaceLabel($"系统更新完成！");

                //更新完成之后启动主程序
                Thread startMainPrograme = new Thread(new ThreadStart(CountDownActionFun));
                startMainPrograme.IsBackground = true;
                startMainPrograme.Start();
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                StartMainPrograme(e.ToString());
            }
        }

        /// <summary>
        /// 更新完成之后-倒计时5秒执行启动主程序
        /// </summary>
        private void CountDownActionFun()
        {
            var i = 5;
            while (true)
            {
                IncreaceLabel($"系统更新完成！{i}秒之后启动主程序！");
                Thread.Sleep(1000);
                i--;
                if (i == 0)
                {
                    break;
                }
            }
            //启动主程
            StartMainPrograme("");
        }

        private byte[] GetDataLength(ref long reallength, ref string 文件路径, ref Guid 文件Id, ref long 文件总长度)
        {
            //包头11+文件路径2048+文件Id 36+文件总长度8
            byte[] bufferPtr = new byte[11 + 2048 + 36 + 8];
            string str文件Id = string.Empty;
            try
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $"缓冲区长度：{tcpClient.Available}", Guid.NewGuid().ToString());
                networkStream.Read(bufferPtr, 0, 11 + 2048 + 36 + 8);
                byte[] len = new byte[8];
                Array.Copy(bufferPtr, 2, len, 0, 8);
                long lens = BitConverter.ToInt64(len, 0);//实际长度
                reallength = lens;
                if (bufferPtr[1] == 0x02)
                {
                    //文件路径
                    byte[] filePath = new byte[2048];
                    Array.Copy(bufferPtr, 11, filePath, 0, 2048);
                    文件路径 = _baseDir.Substring(0, _baseDir.Length - 1) +
                           Encoding.Default.GetString(filePath).Trim().Replace("\0", ""); //文件路径
                    文件路径 = 文件路径.Replace("\\", "\\\\");

                    //文件id
                    byte[] fileId = new byte[36];
                    Array.Copy(bufferPtr, 11 + 2048, fileId, 0, 36);
                    str文件Id = Encoding.Default.GetString(fileId);
                    文件Id = Guid.Parse(str文件Id);

                    //文件总长度
                    byte[] fileLength = new byte[8];
                    Array.Copy(bufferPtr, 11 + 2048 + 36, fileLength, 0, 8);
                    文件总长度 = BitConverter.ToInt64(fileLength, 0);
                }
                return bufferPtr;
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString() + "解析文件Id字符串" + str文件Id, Guid.NewGuid().ToString());
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 文件内容 *已经废弃
        /// </summary>
        /// <param name="datas"></param>
        private void HandleUpdateContent(object datas)
        {
            //拆包组包
            byte[] data = (byte[])datas;
            if (data[0] != 0xAA) return;

            byte[] len = new byte[4];
            Array.Copy(data, 2, len, 0, 4);
            long lens = BitConverter.ToInt32(len, 0);
            byte[] realDatas = new byte[lens];//真实数据大小
                                              //去掉包头
            Array.Copy(data, 7, realDatas, 0, lens);
            //文件内容长度
            long 文件内容长度 = realDatas.Length - 2048;
            //获取文件base路径
            byte[] baseFilepath = new byte[2048];
            Array.Copy(realDatas, 0, baseFilepath, 0, 2048);
            string strBaseFilepath = Encoding.Default.GetString(baseFilepath);
            //获取文件内容
            byte[] fileContents = new byte[文件内容长度];//文件内容长度
            Array.Copy(realDatas, 2048, fileContents, 0, 文件内容长度);
            UpdaterStart(strBaseFilepath, fileContents);
        }

        //文件列表
        private void HandleUpdateList(object datas)
        {
            try
            {
                //拆包组包
                byte[] data = (byte[])datas;
                if (data[0] != 0xAA) return;

                byte[] len = new byte[8];
                Array.Copy(data, 2, len, 0, 8);
                long lens = BitConverter.ToInt64(len, 0);
                byte[] realDatas = new byte[lens - 2092];//真实数据大小
                                                         //去掉包头
                Array.Copy(data, 11 + 2092, realDatas, 0, lens - 2092);
                //Array.Copy(data, 2092, realDatas, 0, lens-2092);//去掉开头的保留段
                FileListFileInformation = MsgPackHelper<List<FileInformation>>.UnPack(realDatas);
                //FileListFileInformation = SerializableHelper.BytesToStruct<List<FileInformation>>(realDatas);
                //FileListFileInformation = (List<FileInformation>)SerializableHelper.BytesToObject(realDatas);
                ////拆包组包
                //byte[] data = (byte[])datas;
                //if (data[0] != 0xAA) return;

                //byte[] len = new byte[4];
                //Array.Copy(data, 2, len, 0, 4);
                //int lens = BitConverter.ToInt32(len, 0);
                //byte[] realDatas = new byte[lens];//真实数据大小
                //                                  //去掉包头
                //Array.Copy(data, 7, realDatas, 0, lens);
                //if (data[1] == 0x01)
                //{
                //    FileListFileInformation = MsgPackHelper<List<FileInformation>>.UnPack(realDatas);
                //}
                //else if (data[1] == 0x02)
                //{
                //    var FileByteContentsList = MsgPackHelper<List<FileByteContent>>.UnPack(realDatas);
                //    UpdaterStart(FileByteContentsList);
                //}
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", ex.ToString(), Guid.NewGuid().ToString());
                Console.WriteLine(ex.ToString());
            }

        }

        public void ReceiveDatas()
        {
            while (true)
            {
                try
                {
                    if (!networkStream.CanRead) //DataAvailable
                    {
                        continue;
                    }
                    else
                    {
                        if (networkStream.DataAvailable)
                        {

                            DateTime startTime = DateTime.Now; //开始统计时间
                            if (tcpClient.Available == 0) continue;
                            //Console.WriteLine($"第一次收到字节{tcpClient.Available}");
                            long reallength = 0;
                            string 文件路径 = string.Empty;
                            Guid 文件Id = new Guid();
                            long 文件总长度 = 0;
                            double 下载的文件长度 = 0;
                            //包头11+文件路径2048+文件Id 36+文件总长度4
                            //最小的一个包长度11 包头大小就是11如果第一次接受到的数据没有包头的大小，<不能解析包头 报错

                            if (tcpClient.Available < 2103) continue; //第一次进来的时候 不加这个会报错 第一次进来的包太小了不存在包头直接异常
                            总共下载字节数 = 总共下载字节数 + tcpClient.Available;
                            byte[] packBytes =
                                GetDataLength(ref reallength, ref 文件路径, ref 文件Id, ref 文件总长度); //11 + 2048+36+4
                            //string strFilePath = string.Empty;
                            //if (packBytes[1] == 0x02)
                            //{
                            //    byte[] filePath = new byte[2048];
                            //    Array.Copy(packBytes, 11, filePath, 0, 2048);
                            //    strFilePath = _baseDir + Encoding.Default.GetString(filePath).Trim().Replace("\\", "").Replace("\0", "");//文件路径
                            //    strFilePath = strFilePath.Replace("\\", "\\\\");
                            //    if (System.IO.File.Exists(strFilePath))
                            //    {
                            //        System.IO.File.Delete(strFilePath);
                            //    }
                            //}
                            //进度条就用  已读大小/包总大小 

                            byte[] realData = new byte[0];
                            byte[] datas = new byte[0];
                            datas = CopyByte(packBytes, datas);
                            //long 记录读了多少字节 = 0 + packBytes.Length; //11+2048
                            if (packBytes[0] == 0xaa && packBytes[1] == 0x02 &&
                                _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().记录读了多少字节 == 0)
                            {
                                _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().FileId = 文件Id;
                                _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().包总大小 = 文件总长度;
                                //清空进度条
                                //ClearDownFile();
                                //SetDownFileMax((int)文件总长度);
                                //删除现有文件
                                if (System.IO.File.Exists(文件路径))
                                {
                                    System.IO.File.Delete(文件路径);
                                }
                            }

                            //_UpdateList.Where(x=>x.FileId== 文件Id).FirstOrDefault().记录读了多少字节= packBytes.Length; //11+2048
                            long 记录读了多少字节 = packBytes.Length;
                            if (记录读了多少字节 == reallength + 11)
                            {
                                //0字节文件
                                包数量++;
                                //0字节文件
                                //datas = datas.Concat(realData).ToArray();
                                datas = CopyByte(datas, realData);
                                记录读了多少字节 += 0;
                                //Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 11}");

                                //Console.WriteLine($"最后一个包大小{0}");
                                //判断一个文件是否下载完了
                                //已读段数量+1
                                _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().已读段 += 1;
                                _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().是否下载完成 = true;
                                ////获得当前包的路径
                                //if (datas[1] == 0x02)
                                //{
                                //    byte[] filePath = new byte[2048];
                                //    Array.Copy(datas, 11, filePath, 0, 2048);
                                //    文件路径 = _baseDir.Substring(0, _baseDir.Length - 1) +
                                //                   Encoding.Default.GetString(filePath).Trim().Replace("\0", ""); //文件路径
                                //    文件路径 = 文件路径.Replace("\\", "\\\\");
                                //}

                                ////获得包内容--0字节包直接写0
                                ////byte[] valueBytes = new byte[datas.Length - (11 + 2048)];
                                ////Array.Copy(datas, 11 + 2048, valueBytes, 0, datas.Length - (11 + 2048));
                                ////追加写入文件
                                //byte[] valueBytes = new byte[0];
                                if (datas[1] == 0x02)
                                {
                                    if (!Directory.Exists(Path.GetDirectoryName(文件路径)))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(文件路径));
                                    }

                                    byte[] valueBytes = new byte[0];
                                    FileStream fs = new FileStream(文件路径, FileMode.Append);
                                    BinaryWriter bw = new BinaryWriter(fs);
                                    bw.Write(valueBytes, 0, valueBytes.Length);
                                    bw.Close();
                                    fs.Close();
                                    下载的文件长度 = _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().文件总长度;
                                    AddUpdateMsg(Path.GetFileName(文件路径), 文件路径, 下载的文件长度, true);
                                }

                                //WriteDeviceLog.WriteText(AppDomain.CurrentDomain.BaseDirectory + "Log\\更新日志.txt",
                                //    $@"{DateTime.Now}:完成第{包数量}个包，0字节包。读取完成一个包记录大小：{记录读了多少字节},实际大小:{datas.Length}，头:{
                                //            datas[0]
                                //        }，路径:{文件路径}" + "\r\n");
                            }


                            //Console.WriteLine($"&&&&&&&&&&&{tcpClient.Available}");
                            while (记录读了多少字节 < reallength + 11)
                            {

                                //Console.WriteLine($"*********{tcpClient.Available}");
                                if (tcpClient.Available == 0) continue;
                                //Console.WriteLine($"收到字节{tcpClient.Available}");

                                if (reallength + 11 - 记录读了多少字节 < _pageSize)
                                {
                                    //Console.WriteLine($"缓冲区大小{tcpClient.Available}");
                                    long 最后一个包大小 = reallength + 11 - 记录读了多少字节;
                                    if (tcpClient.Available < 最后一个包大小) continue;
                                    realData = new byte[最后一个包大小];
                                    总共下载字节数 = 总共下载字节数 + tcpClient.Available;
                                    networkStream.Read(realData, 0, (int)最后一个包大小);
                                    //datas = datas.Concat(realData).ToArray();
                                    //datas = CopyByte(datas, realData);
                                    记录读了多少字节 += 最后一个包大小;
                                    if (packBytes[0] == 0xaa && packBytes[1] == 0x02)
                                    {
                                        _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().记录读了多少字节 +=
                                            realData.Length;
                                        //IncreaceDownFile(realData.Length);
                                    }

                                    //Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 11}");

                                    //Console.WriteLine($"最后一个包大小{最后一个包大小}");

                                    ////获得当前包的路径
                                    //if (datas[1] == 0x02)
                                    //{
                                    //    byte[] filePath = new byte[2048];
                                    //    Array.Copy(datas, 11, filePath, 0, 2048);
                                    //    文件路径 = _baseDir.Substring(0, _baseDir.Length - 1) +
                                    //                   Encoding.Default.GetString(filePath).Trim()
                                    //                       .Replace("\0", ""); //文件路径
                                    //    文件路径 = 文件路径.Replace("\\", "\\\\");
                                    //}

                                    ////获得包内容
                                    ////byte[] valueBytes = new byte[datas.Length - (11 + 2048)];
                                    ////Array.Copy(datas, 11 + 2048, valueBytes, 0, datas.Length - (11 + 2048));
                                    ////追加写入文件
                                    if (datas[1] == 0x02)
                                    {
                                        if (!Directory.Exists(Path.GetDirectoryName(文件路径)))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(文件路径));
                                        }

                                        FileStream fs = new FileStream(文件路径, FileMode.Append);
                                        BinaryWriter bw = new BinaryWriter(fs);
                                        bw.Write(realData, 0, realData.Length);
                                        bw.Close();
                                        fs.Close();
                                    }

                                    if (datas[1] == 0x01)
                                    {
                                        datas = CopyByte(datas, realData);
                                    }
                                }
                                else if (reallength + 11 - 记录读了多少字节 == _pageSize)
                                {
                                    //Console.WriteLine($"缓冲区大小{tcpClient.Available}");
                                    long 最后一个包大小 = reallength + 11 - 记录读了多少字节;
                                    if (tcpClient.Available < 最后一个包大小) continue;
                                    realData = new byte[最后一个包大小];
                                    总共下载字节数 = 总共下载字节数 + tcpClient.Available;
                                    networkStream.Read(realData, 0, (int)最后一个包大小);
                                    //datas = datas.Concat(realData).ToArray();
                                    //datas = CopyByte(datas, realData);
                                    记录读了多少字节 += 最后一个包大小;
                                    if (packBytes[0] == 0xaa && packBytes[1] == 0x02)
                                    {
                                        _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().记录读了多少字节 +=
                                            realData.Length;
                                        //IncreaceDownFile(realData.Length);
                                    }

                                    //Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 11}");
                                    //获得当前包的路径

                                    //if (datas[1] == 0x02)
                                    //{
                                    //    byte[] filePath = new byte[2048];
                                    //    Array.Copy(datas, 11, filePath, 0, 2048);
                                    //    文件路径 = _baseDir.Substring(0, _baseDir.Length - 1) +
                                    //                   Encoding.Default.GetString(filePath).Trim()
                                    //                       .Replace("\0", ""); //文件路径
                                    //    文件路径 = 文件路径.Replace("\\", "\\\\");
                                    //}

                                    ////获得包内容
                                    ////byte[] valueBytes = new byte[datas.Length - (11 + 2048)];
                                    ////Array.Copy(datas, 11 + 2048, valueBytes, 0, datas.Length - (11 + 2048));
                                    ////追加写入文件
                                    if (datas[1] == 0x02)
                                    {
                                        if (!Directory.Exists(Path.GetDirectoryName(文件路径)))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(文件路径));
                                        }

                                        FileStream fs = new FileStream(文件路径, FileMode.Append);
                                        BinaryWriter bw = new BinaryWriter(fs);
                                        bw.Write(realData, 0, realData.Length);
                                        bw.Close();
                                        fs.Close();
                                    }

                                    if (datas[1] == 0x01)
                                    {
                                        datas = CopyByte(datas, realData);
                                    }
                                }
                                else
                                {
                                    var 包大小 = tcpClient.Available;
                                    realData = new byte[包大小];
                                    总共下载字节数 = 总共下载字节数 + tcpClient.Available;
                                    networkStream.Read(realData, 0, 包大小);
                                    //datas = datas.Concat(realData).ToArray();
                                    //datas = CopyByte(datas, realData);
                                    记录读了多少字节 += 包大小;
                                    if (packBytes[0] == 0xaa && packBytes[1] == 0x02)
                                    {
                                        _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().记录读了多少字节 +=
                                            realData.Length;
                                        //IncreaceDownFile(realData.Length);
                                    }

                                    //Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 11}");
                                    ////获得当前包的路径
                                    //if (datas[1] == 0x02)
                                    //{
                                    //    byte[] filePath = new byte[2048];
                                    //    Array.Copy(datas, 11, filePath, 0, 2048);
                                    //    文件路径 = _baseDir.Substring(0, _baseDir.Length - 1) +
                                    //                   Encoding.Default.GetString(filePath).Trim()
                                    //                       .Replace("\0", ""); //文件路径
                                    //    文件路径 = 文件路径.Replace("\\", "\\\\");
                                    //}

                                    ////获得包内容
                                    ////var datalength = datas.Length - 2055;
                                    ////byte[] valueBytes = new byte[datalength];
                                    ////Array.Copy(datas, 2055, valueBytes, 0, datalength);
                                    ////追加写入文件
                                    if (datas[1] == 0x02)
                                    {
                                        if (!Directory.Exists(Path.GetDirectoryName(文件路径)))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(文件路径));
                                        }

                                        FileStream fs = new FileStream(文件路径, FileMode.Append);
                                        BinaryWriter bw = new BinaryWriter(fs);
                                        bw.Write(realData, 0, realData.Length);
                                        bw.Close();
                                        fs.Close();
                                    }

                                    if (datas[1] == 0x01)
                                    {
                                        datas = CopyByte(datas, realData);
                                    }
                                }

                                //已读段数量+1
                                if (datas[1] == 0x02)
                                {
                                    _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().已读段 += 1;
                                }

                                if (记录读了多少字节 == reallength + 11 && packBytes[0] == 0xaa && packBytes[1] == 0x02)
                                {
                                    包数量++;
                                    //收集到一个完整的包就退出去解析
                                    //WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $@"读取完成一个包记录大小：{记录读了多少字节},实际大小:{datas.Length}，头:{datas[0]}", Guid.NewGuid().ToString());
                                    WriteDeviceLog.WriteLog("Log\\" + "\\更新日志",
                                        $@"{DateTime.Now}:完成第{包数量}个包，读取完成一个包记录大小：{记录读了多少字节}，头:{datas[0]}，路径:{文件路径}" +
                                        "\r\n", Guid.NewGuid().ToString());
                                    //判断一个文件是否下载完了
                                    if (_UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().记录读了多少字节 ==
                                        _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().包总大小)
                                    {
                                        //只有第一个包才会加载
                                        _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().是否下载完成 = true;
                                        下载的文件长度 = _UpdateList.Where(x => x.FileId == 文件Id).FirstOrDefault().文件总长度;
                                        AddUpdateMsg(Path.GetFileName(文件路径), 文件路径, 下载的文件长度, true);
                                    }

                                    break;
                                }
                            }



                            if (packBytes[1] == 0x01)
                            {
                                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateList), datas);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", ex.ToString(), Guid.NewGuid().ToString());
                    StartMainPrograme(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        //public void ReceiveDatas()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            if (!networkStream.CanRead)//DataAvailable
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                if (networkStream.DataAvailable)
        //                {
        //                    if (tcpClient.Available == 0) continue;
        //                    Console.WriteLine($"第一次收到字节{tcpClient.Available}");
        //                    long reallength = 0;
        //                    byte[] packBytes = GetDataLength(ref reallength);//7+2048 包头+文件路径
        //                                                                     //string strFilePath = string.Empty;
        //                                                                     //if (packBytes[1] == 0x02)
        //                                                                     //{
        //                                                                     //    byte[] filePath = new byte[2048];
        //                                                                     //    Array.Copy(packBytes, 7, filePath, 0, 2048);
        //                                                                     //    strFilePath = _baseDir + Encoding.Default.GetString(filePath).Trim().Replace("\\", "").Replace("\0", "");//文件路径
        //                                                                     //    strFilePath = strFilePath.Replace("\\", "\\\\");
        //                                                                     //    if (System.IO.File.Exists(strFilePath))
        //                                                                     //    {
        //                                                                     //        System.IO.File.Delete(strFilePath);
        //                                                                     //    }
        //                                                                     //}


        //                    byte[] realData = new byte[0];
        //                    byte[] datas = new byte[0];
        //                    datas = CopyByte(packBytes, datas);
        //                    long 记录读了多少字节 = 0 + packBytes.Length;//7+2048
        //                    if (记录读了多少字节 == reallength + 7)
        //                    {
        //                        包数量++;
        //                        //0字节文件
        //                        //datas = datas.Concat(realData).ToArray();
        //                        datas = CopyByte(datas, realData);
        //                        记录读了多少字节 += 0;
        //                        Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 7}");

        //                        Console.WriteLine($"最后一个包大小{0}");

        //                        //获得当前包的路径
        //                        string strFilePath1 = string.Empty;
        //                        if (datas[1] == 0x02)
        //                        {
        //                            byte[] filePath = new byte[2048];
        //                            Array.Copy(datas, 7, filePath, 0, 2048);
        //                            strFilePath1 = _baseDir.Substring(0, _baseDir.Length - 1) + Encoding.Default.GetString(filePath).Trim().Replace("\0", "");//文件路径
        //                            strFilePath1 = strFilePath1.Replace("\\", "\\\\");
        //                        }
        //                        //获得包内容
        //                        byte[] valueBytes = new byte[datas.Length - (7 + 2048)];
        //                        Array.Copy(datas, 7 + 2048, valueBytes, 0, datas.Length - (7 + 2048));
        //                        //追加写入文件
        //                        if (datas[1] == 0x02)
        //                        {
        //                            if (!Directory.Exists(Path.GetDirectoryName(strFilePath1)))
        //                            {
        //                                Directory.CreateDirectory(Path.GetDirectoryName(strFilePath1));
        //                            }
        //                            FileStream fs = new FileStream(strFilePath1, FileMode.Append);
        //                            BinaryWriter bw = new BinaryWriter(fs);
        //                            bw.Write(valueBytes, 0, valueBytes.Length);
        //                            bw.Close();
        //                            fs.Close();
        //                            //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateContent), datas);
        //                        }
        //                        WriteDeviceLog.WriteText(AppDomain.CurrentDomain.BaseDirectory + "Log\\更新日志.txt", $@"{DateTime.Now}:完成第{包数量}个包，0字节包。读取完成一个包记录大小：{记录读了多少字节},实际大小:{datas.Length}，头:{datas[0]}，路径:{strFilePath1}" + "\r\n");
        //                    }
        //                    while (记录读了多少字节 < reallength + 7)
        //                    {
        //                        string strFilePath1 = string.Empty;
        //                        if (tcpClient.Available == 0) continue;
        //                        Console.WriteLine($"收到字节{tcpClient.Available}");

        //                        if (reallength + 7 - 记录读了多少字节 < _pageSize)
        //                        {
        //                            Console.WriteLine($"缓冲区大小{tcpClient.Available}");
        //                            long 最后一个包大小 = reallength + 7 - 记录读了多少字节;
        //                            if (tcpClient.Available < 最后一个包大小) continue;
        //                            realData = new byte[最后一个包大小];
        //                            networkStream.Read(realData, 0, (int)最后一个包大小);
        //                            //datas = datas.Concat(realData).ToArray();
        //                            datas = CopyByte(datas, realData);
        //                            记录读了多少字节 += 最后一个包大小;
        //                            Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 7}");

        //                            Console.WriteLine($"最后一个包大小{最后一个包大小}");

        //                            //获得当前包的路径
        //                            if (datas[1] == 0x02)
        //                            {
        //                                byte[] filePath = new byte[2048];
        //                                Array.Copy(datas, 7, filePath, 0, 2048);
        //                                strFilePath1 = _baseDir.Substring(0, _baseDir.Length - 1) + Encoding.Default.GetString(filePath).Trim().Replace("\0", "");//文件路径
        //                                strFilePath1 = strFilePath1.Replace("\\", "\\\\");
        //                            }
        //                            //获得包内容
        //                            byte[] valueBytes = new byte[datas.Length - (7 + 2048)];
        //                            Array.Copy(datas, 7 + 2048, valueBytes, 0, datas.Length - (7 + 2048));
        //                            //追加写入文件
        //                            if (datas[1] == 0x02)
        //                            {
        //                                if (!Directory.Exists(Path.GetDirectoryName(strFilePath1)))
        //                                {
        //                                    Directory.CreateDirectory(Path.GetDirectoryName(strFilePath1));
        //                                }
        //                                FileStream fs = new FileStream(strFilePath1, FileMode.Append);
        //                                BinaryWriter bw = new BinaryWriter(fs);
        //                                bw.Write(valueBytes, 0, valueBytes.Length);
        //                                bw.Close();
        //                                fs.Close();
        //                                //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateContent), datas);
        //                            }
        //                        }
        //                        else if (reallength + 7 - 记录读了多少字节 == _pageSize)
        //                        {
        //                            Console.WriteLine($"缓冲区大小{tcpClient.Available}");
        //                            long 最后一个包大小 = reallength + 7 - 记录读了多少字节;
        //                            if (tcpClient.Available < 最后一个包大小) continue;
        //                            realData = new byte[最后一个包大小];
        //                            networkStream.Read(realData, 0, (int)最后一个包大小);
        //                            //datas = datas.Concat(realData).ToArray();
        //                            datas = CopyByte(datas, realData);
        //                            记录读了多少字节 += 最后一个包大小;
        //                            Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 7}");
        //                            //获得当前包的路径

        //                            if (datas[1] == 0x02)
        //                            {
        //                                byte[] filePath = new byte[2048];
        //                                Array.Copy(datas, 7, filePath, 0, 2048);
        //                                strFilePath1 = _baseDir.Substring(0, _baseDir.Length - 1) + Encoding.Default.GetString(filePath).Trim().Replace("\0", "");//文件路径
        //                                strFilePath1 = strFilePath1.Replace("\\", "\\\\");
        //                            }
        //                            //获得包内容
        //                            byte[] valueBytes = new byte[datas.Length - (7 + 2048)];
        //                            Array.Copy(datas, 7 + 2048, valueBytes, 0, datas.Length - (7 + 2048));
        //                            //追加写入文件
        //                            if (datas[1] == 0x02)
        //                            {
        //                                if (!Directory.Exists(Path.GetDirectoryName(strFilePath1)))
        //                                {
        //                                    Directory.CreateDirectory(Path.GetDirectoryName(strFilePath1));
        //                                }
        //                                FileStream fs = new FileStream(strFilePath1, FileMode.Append);
        //                                BinaryWriter bw = new BinaryWriter(fs);
        //                                bw.Write(valueBytes, 0, valueBytes.Length);
        //                                bw.Close();
        //                                fs.Close();
        //                                //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateContent), datas);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var 包大小 = tcpClient.Available;
        //                            realData = new byte[包大小];
        //                            networkStream.Read(realData, 0, 包大小);
        //                            //datas = datas.Concat(realData).ToArray();
        //                            datas = CopyByte(datas, realData);
        //                            记录读了多少字节 += 包大小;
        //                            Console.WriteLine($"已读大小{记录读了多少字节}，总大小{reallength + 7}");
        //                            //获得当前包的路径
        //                            if (datas[1] == 0x02)
        //                            {
        //                                byte[] filePath = new byte[2048];
        //                                Array.Copy(datas, 7, filePath, 0, 2048);
        //                                strFilePath1 = _baseDir.Substring(0, _baseDir.Length - 1) + Encoding.Default.GetString(filePath).Trim().Replace("\0", "");//文件路径
        //                                strFilePath1 = strFilePath1.Replace("\\", "\\\\");
        //                            }
        //                            //获得包内容
        //                            var datalength = datas.Length - 2055;
        //                            byte[] valueBytes = new byte[datalength];
        //                            Array.Copy(datas, 2055, valueBytes, 0, datalength);
        //                            //追加写入文件
        //                            if (datas[1] == 0x02)
        //                            {
        //                                if (!Directory.Exists(Path.GetDirectoryName(strFilePath1)))
        //                                {
        //                                    Directory.CreateDirectory(Path.GetDirectoryName(strFilePath1));
        //                                }
        //                                FileStream fs = new FileStream(strFilePath1, FileMode.Append);
        //                                BinaryWriter bw = new BinaryWriter(fs);
        //                                bw.Write(valueBytes, 0, valueBytes.Length);
        //                                bw.Close();
        //                                fs.Close();
        //                                //ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateContent), datas);
        //                            }
        //                        }


        //                        if (datas.Length == reallength + 7 && packBytes[1] == 0x02)
        //                        {
        //                            包数量++;
        //                            //收集到一个完整的包就退出去解析
        //                            //WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", $@"读取完成一个包记录大小：{记录读了多少字节},实际大小:{datas.Length}，头:{datas[0]}", Guid.NewGuid().ToString());
        //                            WriteDeviceLog.WriteText(AppDomain.CurrentDomain.BaseDirectory+"Log\\更新日志.txt", $@"{DateTime.Now}:完成第{包数量}个包，读取完成一个包记录大小：{记录读了多少字节},实际大小:{datas.Length}，头:{datas[0]}，路径:{strFilePath1}" + "\r\n");
        //                            break;
        //                        }
        //                    }


        //                    if (packBytes[1] == 0x01)
        //                    {
        //                        ThreadPool.QueueUserWorkItem(new WaitCallback(HandleUpdateList), datas);
        //                    }
        //                }
        //            }
        //        }
        //        catch(Exception ex)
        //        {
        //            MessageBox.Show(ex.ToString());
        //            Console.WriteLine(ex.ToString());
        //            StartMainPrograme();
        //        }
        //    }
        //}

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

        /// <summary>
        /// 是否需要更新
        /// </summary>
        /// <returns></returns>
        public bool IsNeedToUpdate()
        {
            DateTime startTime = DateTime.Now;
            try
            {
                while (true)
                {
                    DateTime endTime = DateTime.Now;
                    if ((endTime.Ticks - startTime.Ticks) / 10000000 > long.Parse(ConfigurationManager.AppSettings["ReceiveTimeOutSeconds"]))
                    {
                        WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", "获取更新数据超时！", Guid.NewGuid().ToString());
                        Console.WriteLine("获取更新数据超时！");
                        System.Environment.Exit(0);

                    }
                    if (FileListFileInformation != null)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
                foreach (var item in FileListFileInformation)
                {
                    var filePath = _baseDir + item.BaseFilePath;
                    if (!System.IO.File.Exists(filePath))
                    {
                        //文件不存在，创建并写入文件
                        return true;
                    }
                    else
                    {
                        if (System.IO.Path.GetFileName(filePath) == Process.GetCurrentProcess().ProcessName + ".exe")
                        {
                            continue;
                        }
                        if (FileHelper.GetFileMd5(filePath) != item.FileMd5)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                StartMainPrograme(e.ToString());
                return true;
            }
        }

        /// <summary>
        /// 获取更新列表
        /// </summary>
        /// <returns></returns>
        public void GetUpdateList()
        {
            try
            {
                foreach (var item in FileListFileInformation)
                {
                    var filePath = _baseDir + item.BaseFilePath;
                    //if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    //{
                    //    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    //}
                    if (!System.IO.File.Exists(filePath))
                    {
                        //文件不存在，创建并写入文件
                        _UpdateList.Add(new UpdaterModel()
                        {
                            FileId = item.FileId,
                            文件总长度 = item.文件总长度
                        });
                    }
                    else
                    {
                        //比对md5，不同则替换
                        if (System.IO.Path.GetFileName(filePath) == Process.GetCurrentProcess().ProcessName + ".exe")
                        {
                            //过滤掉更新程序，更新程序永不更新。
                            continue;
                        }
                        var md5 = FileHelper.GetFileMd5(filePath);
                        if (md5 != item.FileMd5)
                        {
                            //md5不同
                            _UpdateList.Add(new UpdaterModel()
                            {
                                FileId = item.FileId,
                                文件总长度 = item.文件总长度
                            });
                        }
                    }
                }
                //发送文件id
                Thread threadUpdaterThread = new Thread(new ThreadStart(SendUpdater));
                threadUpdaterThread.IsBackground = true;
                threadUpdaterThread.Start();
                ////发送需要下载的文件列表list
                //BinaryWriter bw = new BinaryWriter(networkStream);
                //int i = 0;
                //foreach (var item in _UpdateList)
                //{
                //    i++;
                //    byte[] sendBytes = MsgPackHelper<Guid>.Pack(item);
                //    sendBytes = ByteHelper.PackByte(sendBytes, Command.SendNeedUpdateGUIDList);
                //    bw.Write(sendBytes);  //向服务器发送文件id
                //    //WriteDeviceLog.WriteLog("Log\\" +  "\\更新日志", $@"{i}  更新文件ID：{item}", Guid.NewGuid().ToString());
                //}

            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                Console.WriteLine(e.ToString());
            }
        }

        public void UpdaterStart(string baseFilePath, byte[] fileContents)
        {
            try
            {

                //比对本地和服务器文件列表
                //1.先判断文件夹是否存在
                //2.在判断文件是否存在
                //3.如果文件存在判断md5是否一致
                var filePath = $@"{_baseDir}{baseFilePath.Trim()}";
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    FileHelper.BytesToFile(fileContents, filePath);

                }
                catch (Exception e)
                {
                    WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                    AddUpdateMsg(Path.GetFileName(filePath), filePath, 0, false);
                }
                finally
                {

                }
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", e.ToString(), Guid.NewGuid().ToString());
                IncreaceLabel(e.Message);
                StartMainPrograme(e.ToString());
            }
        }

        /// <summary>
        /// 启动主程序
        /// </summary>
        /// <param name="resultStr"></param>
        public void StartMainPrograme(string resultStr)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = _callBackExeName; //启动的应用程序名称  
                //startInfo.Arguments = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}" + $" NEW";
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(startInfo);
                Console.WriteLine(resultStr);
                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + "\\更新日志", ex.ToString(), Guid.NewGuid().ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FormAPI.SetFormTop(this.Handle);
            //开启线程更新
            //Thread thread = new Thread(new ThreadStart(UpdaterStart));
            //thread.IsBackground = true;
            //thread.Start();
        }

        /// <summary>
        /// 设置进度条最大值
        /// </summary>
        /// <param name="max"></param>
        public void SetUpdateProgressBarMax(int max)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new EventHandler(delegate
                {
                    this.progressBar1.Maximum = max;
                }), null);
            }
            else
            {
                this.progressBar1.Maximum = max;
            }
        }

        /// <summary>
        /// 进度条递增
        /// </summary>
        public void IncreaceProgress(int value)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new EventHandler(delegate
                {
                    this.progressBar1.Value += value;
                }), null);
            }
            else
            {
                this.progressBar1.Value += value;
            }
        }

        /// <summary>
        /// 设置进度条最大值
        /// </summary>
        /// <param name="max"></param>
        public void SetDownFileMax(int max)
        {
            if (this.fileDown.InvokeRequired)
            {
                this.fileDown.Invoke(new EventHandler(delegate
                {
                    this.fileDown.Maximum = max;
                }), null);
            }
            else
            {
                this.fileDown.Maximum = max;
            }
        }

        /// <summary>
        /// 进度条递增
        /// </summary>
        public void IncreaceDownFile(int value)
        {
            if (this.fileDown.InvokeRequired)
            {
                this.fileDown.Invoke(new EventHandler(delegate
                {
                    this.fileDown.Value += value;
                }), null);
            }
            else
            {
                this.fileDown.Value += value;
            }
            Thread.Sleep(100);
        }

        /// <summary>
        /// 重置进度条
        /// </summary>
        public void ClearDownFile()
        {
            Thread.Sleep(1000);
            if (this.fileDown.InvokeRequired)
            {
                this.fileDown.Invoke(new EventHandler(delegate
                {
                    this.fileDown.Value = 0;
                }), null);
            }
            else
            {
                this.fileDown.Value = 0;
            }
        }

        /// <summary>
        /// 加载更新日志
        /// </summary>
        /// <param name="num"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="isUpdateSuccess"></param>
        public void AddUpdateMsg(string fileName, string filePath, double fileSize, bool isUpdateSuccess)
        {
            if (this.dataGridView1.InvokeRequired)
            {
                this.dataGridView1.Invoke(new EventHandler(delegate
                {
                    int index = this.dataGridView1.Rows.Add();
                    this.dataGridView1.Rows[index].Cells[0].Value = this.dataGridView1.Rows.Count;
                    this.dataGridView1.Rows[index].Cells[1].Value = fileName;
                    this.dataGridView1.Rows[index].Cells[2].Value = filePath;
                    this.dataGridView1.Rows[index].Cells[3].Value = FileHelper.GetFileSizedisplay(fileSize);
                    this.dataGridView1.Rows[index].Cells[4].Value = isUpdateSuccess ? imageList1.Images[0] : imageList1.Images[1];//加载图标

                    dataGridView1.MultiSelect = false;
                    // dgvRecView.Rows[dgvRecView.Rows.Count - 1].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[0];
                }), null);
            }
            else
            {
                int index = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[index].Cells[0].Value = this.dataGridView1.Rows.Count;
                this.dataGridView1.Rows[index].Cells[1].Value = fileName;
                this.dataGridView1.Rows[index].Cells[2].Value = filePath;
                this.dataGridView1.Rows[index].Cells[3].Value = FileHelper.GetFileSizedisplay(fileSize);
                this.dataGridView1.Rows[index].Cells[4].Value = isUpdateSuccess ? imageList1.Images[0] : imageList1.Images[1];//加载图标

                dataGridView1.MultiSelect = false;
                // dgvRecView.Rows[dgvRecView.Rows.Count - 1].Selected = true;
                dataGridView1.CurrentCell = dataGridView1.Rows[this.dataGridView1.Rows.Count - 1].Cells[0];
            }

        }

        /// <summary>
        /// 显示状态信息
        /// </summary>
        /// <param name="updateMsg"></param>
        public void IncreaceLabel(string updateMsg)
        {
            if (this.label1.InvokeRequired)
            {
                this.label1.Invoke(new EventHandler(delegate
                {
                    this.label1.Text = updateMsg;
                }), null);
            }
            else
            {
                this.label1.Text = updateMsg;
            }
        }

        /// <summary>
        /// 线程操作窗体名称
        /// </summary>
        /// <param name="updateMsg"></param>
        public void IncreaceForm(string updateMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new EventHandler(delegate
                {
                    this.Text = updateMsg;
                }), null);
            }
            else
            {
                this.Text = updateMsg;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var msgBox = MessageBox.Show("是否关闭更新程序并重启主程序？\r\n是：关闭更新程序并启动主程序。\r\n否：关闭更新程序。", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (msgBox ==
                DialogResult.Yes)
            {
                StartMainPrograme("");
            }
            else if (msgBox == DialogResult.No)
            {
                System.Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
