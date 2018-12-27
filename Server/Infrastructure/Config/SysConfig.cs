using System;

namespace Infrastructure.Config
{
    /// <summary>
    /// 系统所有配置
    /// </summary>
    public class SysConfig
    {
        #region 调度任务
        /// <summary>
        /// 数据库连接字符串信息
        /// </summary>
        [PathMap(Key = "SqlConnect")]
        public static string SqlConnect { get; set; }

        /// <summary>
        /// 任务配置的存储方式
        /// </summary>
        [PathMap(Key = "StorageMode")]
        public static int StorageMode { get; set; }

        /// <summary>
        /// 是否Debug模式1:是 0：否
        /// </summary>
        [PathMap(Key = "IsDebug")]
        public static int IsDebug { get; set; }

        /// <summary>
        /// 调度服务名称
        /// </summary>
        [PathMap(Key = "ServiceName")]
        public static string ServiceName { get; set; }

        /// <summary>
        /// 数据库操作超时时间
        /// </summary>
        [PathMap(Key = "DbTimeOut")]
        public static string DbTimeOut { get; set; }
        #endregion

        #region 监控服务状态
        /// <summary>
        /// 是否使用邮件发送
        /// </summary>
        [PathMap(Key = "IsUseEmail")]
        public static string IsUseEmail { get; set; }

        /// <summary>
        /// 邮件发件人用户名
        /// </summary>
        [PathMap(Key = "EmailUserName")]
        public static string EmailUserName { get; set; }

        /// <summary>
        /// 邮件发件人密码ctqxnyfwqgfiibjg
        /// </summary>
        [PathMap(Key = "EmailPassWord")]
        public static string EmailPassWord { get; set; }

        /// <summary>
        /// 收件人Email   韩俊俊，周亮亮，周阳，魏代成，张昊,1353646838@qq.com,870052861@qq.com,1490449046@qq.com,404952263@qq.com,52836554@qq.com
        /// </summary>
        [PathMap(Key = "SJEmailList")]
        public static string SJEmailList { get; set; }
        #endregion

        #region 其他
        /// <summary>
        /// 失败SQL执行队列 定时器  单位：分钟
        /// </summary>
        [PathMap(Key = "SQLQueue")]
        public static string SQLQueue { get; set; }
        #endregion

        /// <summary>
        /// 是否等待所有任务执行完成
        /// </summary>
        [PathMap(Key = "WaitForJobsToComplete")]
        public static string WaitForJobsToComplete { get; set; }
    }
}
