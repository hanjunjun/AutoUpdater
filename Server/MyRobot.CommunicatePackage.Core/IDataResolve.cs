/************************************************************************
 * 文件名：IDataResolve.cs
 * 文件功能描述：解析接口层
 * 作    者：  韩俊俊
 * 创建日期：2017年04月01日 20:36:38
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * 备注：
 * Copyright (c) 2017 Titan.Han . All Rights Reserved. 
 * ***********************************************************************/
using MyRobotModel.CommonModel;
using System.Collections.Concurrent;

namespace MyRobot.CommunicatePackage.Core
{
    public interface IDataResolve
    {
        /// <summary>
        /// 获取初始值
        /// </summary>
        /// <param name="strDevice">设备Id</param>
        /// <param name="dbType">数据库配置</param>
        /// <param name="param1">扩展参数1</param>
        void GetRules(string strDevice, string dbType,ref string param1);

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
        void ParseResult(byte[] strSource, ref byte[] strCmd, string eventId, ref bool isDisconnect, ref bool isMass,ref string strAdvance, string connId,ref ConcurrentDictionary<string, SpreadModel> spreadObject,ref bool IsUpdater,ref byte[] 扩展参数);

        /// <summary>
        /// 是否一直保持连接（TCP）
        /// </summary>
        /// <returns>true 一直连接 false 每次通讯断一次</returns>
        bool IsContinueConnecting();

        /// <summary>
        /// 对象建立连接成功之后对扩展对象进行初始化
        /// </summary>
        /// <param name="connId">客户端Id</param>
        /// <param name="spreadObject">客户端附加参数</param>
        /// <param name="strAdvance">扩展参数</param>
        /// <param name="strCmd">回写数据流</param>
        /// <param name="isDisconnect">是否断开连接，True：断开  False：不断开</param>
        void OnLinkStart(string connId, ref ConcurrentDictionary<string, SpreadModel> spreadObject, string strAdvance, ref byte[] strCmd, ref bool isDisconnect);

        /// <summary>
        /// 连接结束或者失败之后对扩展对象进行移除
        /// </summary>
        /// <param name="connId">客户端Id</param>
        /// <param name="spreadObject">客户端附加参数</param>
        /// <param name="strAdvance">扩展参数</param>
        void OnLinkEnd(string connId, ref ConcurrentDictionary<string, SpreadModel> spreadObject, string strAdvance);
    }
}
