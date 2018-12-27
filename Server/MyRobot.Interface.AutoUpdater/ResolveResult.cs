/*************************************
 * 名称：解析程序
 * 功能：
 * 作者：
 * 时间：2017-03-20
 * 通讯类型：网络
 * ***********************************/
using Infrastructure;
using Infrastructure.File;
using Infrastructure.Log;
using Infrastructure.MsgPack;
using MyRobot.CommunicatePackage.Core;
using MyRobotModel;
using MyRobotModel.CommonModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MyRobot.Interface.AutoUpdater
{
    public class ResolveResult : IDataResolve
    {
        public static List<FileInformation> FileListFileInformation = null;//文件列表实体
        //public static List<FileByteContent> FileByteContents = null;//文件内容实体
        public static byte[] datas;//文件列表byte
        private string _baseDir = AppDomain.CurrentDomain.BaseDirectory;//客户端根目录
        public string _DeviceID;
        public string _DeviceName;

        /// <summary>
        /// 设备初始化规则
        /// </summary>
        /// <param name="strDevice"></param>
        /// <param name="dbType"></param>
        public void GetRules(string strDevice, string dbType, ref string param1)
        {
            _DeviceID = strDevice;
            _DeviceName = strDevice;

            //加载客户端缓存文件
            var fileHelper = new FileHelper();
            //测试手动获取文件位置
            FileListFileInformation = fileHelper.GetAllFiles(new System.IO.DirectoryInfo($@"{_baseDir}\UpdateTempFile"));
            //FileByteContents = FileHelper.FileByteContentsList;
            datas = MsgPackHelper<List<FileInformation>>.Pack(FileListFileInformation);
            //datas = SerializableHelper.ObjectToBytes(FileListFileInformation);
            WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"加载内存：文件列表{FileListFileInformation.Count}个", Guid.NewGuid().ToString());
        }

        public bool IsContinueConnecting()
        {
            return true;
        }

        /// <summary>
        /// 设备解析数据标准接口
        /// </summary>
        /// <param name="strSource">需要解析的数据流</param>
        /// <param name="strCmd">回写数据流</param>
        /// <param name="eventId">事件ID</param>
        /// <param name="isDisconnect">是否断开连接，True：断开  False：不断开</param>
        /// <param name="isMass">是否群发</param>
        /// <param name="strAdvance">扩展参数</param>
        /// <param name="connId">客户端Id</param>
        /// <param name="spreadObject">客户端附加参数</param>
        public void ParseResult(byte[] strSource, ref byte[] strCmd, string eventId, ref bool isDisconnect, ref bool isMass,ref string strAdvance, string connId, ref ConcurrentDictionary<string, SpreadModel> spreadObject,ref bool IsUpdater,ref byte[] 扩展参数)
        {
            try
            {
                IsUpdater = true;
                byte[] len = new byte[4];
                Array.Copy(strSource, 2, len, 0, 4);
                int lens = BitConverter.ToInt32(len, 0);
                byte[] realDatas = new byte[lens];//真实数据大小
                                                  //去掉包头
                Array.Copy(strSource, 11, realDatas, 0, lens);

                FileInformation sendUpdateContents = new FileInformation();
                请求文件信息 updateList = MsgPackHelper<请求文件信息>.UnPack(realDatas);
                WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"收到文件ID：{updateList.FileId}", Guid.NewGuid().ToString());
                //sendUpdateContents = FileByteContents.Where(x => x.FileId==updateList).ToList().First();//获取文件id对应的文件
                sendUpdateContents = FileListFileInformation.Where(x => x.FileId == updateList.FileId).ToList().First();
                WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"whereId：{sendUpdateContents.FileId}", Guid.NewGuid().ToString());
                //包头7字节
                //2048长度来存放文件路径
                //剩下的放文件内容
                byte[] filePath = new byte[2048];
                byte[] basePath= Encoding.Default.GetBytes(sendUpdateContents.BaseFilePath);//文件路径
                Array.Copy(basePath,0,filePath,0,basePath.Length);
                扩展参数 = filePath;
                strAdvance = sendUpdateContents.FilePath+"|"+ sendUpdateContents.FileId + "|" + updateList.文件读取开始位置 + "|" + updateList.文件读取长度;
                WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"返回：{strAdvance}", Guid.NewGuid().ToString());
                //strCmd = CopyByte(filePath,FileHelper.GetFileBytes(sendUpdateContents.FilePath));

                //strCmd = ByteHelper.PackByte(strCmd, Command.UpdateDatasList);//封包
                //WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"发送文件ID：{updateList} 大小：{strCmd.Length}字节", Guid.NewGuid().ToString());
                if (connId != null)
                {
                    if (spreadObject.ContainsKey(connId))
                    {
                        //更新客户端对象扩展信息
                        spreadObject[connId].connId = connId;
                        spreadObject[connId].DeviceCode = "AutoUpdaterService";
                        spreadObject[connId].AccessAddress = strAdvance;
                        spreadObject[connId].LastTimeHandleTime = DateTime.Now;
                        spreadObject[connId].OnlineTime = TimeHelper.GetTimeLong(spreadObject[connId].FirstTimeConnectTime, DateTime.Now, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"{strAdvance}         {ex}", eventId);
            }
        }

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
        /// 对象建立连接成功之后对扩展对象进行初始化
        /// </summary>
        /// <param name="connId">连接对象</param>
        /// <param name="spreadObject">客户端附加参数</param>
        /// <param name="strAdvance">扩展参数</param>
        /// <param name="strCmd">回写命令</param>
        /// <param name="isDisconnect">是否重连</param>
        public void OnLinkStart(string connId, ref ConcurrentDictionary<string, SpreadModel> spreadObject, string strAdvance, ref byte[] strCmd, ref bool isDisconnect)
        {
            try
            {
                SpreadModel item = new SpreadModel();
                if (connId != null)
                {
                    item.connId = connId;
                    item.AccessAddress = strAdvance;
                    item.FirstTimeConnectTime = DateTime.Now;
                    if (spreadObject.ContainsKey(connId))
                    {
                        //更新客户端对象扩展信息
                        spreadObject[connId] = item;
                    }
                    else
                    {
                        //新增客户端扩展信息
                        spreadObject.TryAdd(connId, item);
                    }
                    byte[] 保留字段 = new byte[2092];
                    strCmd = CopyByte(保留字段, datas);//保留字段+文件列表
                    strCmd = ByteHelper.PackByte(strCmd, Command.UpdateTempList);
                    WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"发送更新列表{FileListFileInformation.Count}个 大小{strCmd.Length}字节", Guid.NewGuid().ToString());
                }
            }
            catch (Exception e)
            {
                WriteDeviceLog.WriteLog("Log\\" + _DeviceName + "\\解析日志", $@"{strAdvance}         {e}", Guid.NewGuid().ToString());
                isDisconnect = true;//断开连接
            }
        }

        /// <summary>
        /// 连接结束或者失败之后对扩展对象进行移除
        /// </summary>
        /// <param name="connId">连接对象</param>
        /// <param name="spreadObject">扩展对象</param>
        /// <param name="strAdvance">扩展参数</param>
        public void OnLinkEnd(string connId, ref ConcurrentDictionary<string, SpreadModel> spreadObject, string strAdvance)
        {

        }
    }
}
