using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.DataModel
{
    #region POCO classes

    // SysCompany
    ///<summary>
    /// 服务器大区
    ///</summary>
    public class SysCompany
    {

        ///<summary>
        /// 机构Id
        ///</summary>
        public System.Guid SysCompanyId { get; set; } // SysCompanyId (Primary key)

        ///<summary>
        /// 机构名称
        ///</summary>
        public string SysCompanyName { get; set; } // SysCompanyName (length: 50)

        ///<summary>
        /// 机构父节点Id
        ///</summary>
        public System.Guid? SysCompanyFatherId { get; set; } // SysCompanyFatherId

        ///<summary>
        /// 机构类型
        ///</summary>
        public int? CompanyType { get; set; } // CompanyType

        ///<summary>
        /// 机构状态
        ///</summary>
        public int? CompanyStatus { get; set; } // CompanyStatus

        ///<summary>
        /// 机构代码
        ///</summary>
        public string CompanyCode { get; set; } // CompanyCode (length: 50)

        ///<summary>
        /// 机构负责人
        ///</summary>
        public string CompanyPicPerson { get; set; } // CompanyPicPerson (length: 20)

        ///<summary>
        /// 机构负责邮箱号
        ///</summary>
        public string CompanyEmail { get; set; } // CompanyEmail (length: 50)

        ///<summary>
        /// 机构地址
        ///</summary>
        public string CompanyAddress { get; set; } // CompanyAddress (length: 200)

        ///<summary>
        /// 机构logo名字
        ///</summary>
        public string CompanyLogoName { get; set; } // CompanyLogoName (length: 100)

        ///<summary>
        /// 保留小数点位数
        ///</summary>
        public string ReserveDecimalPoint { get; set; } // ReserveDecimalPoint (length: 20)

        ///<summary>
        /// 机构模板路径
        ///</summary>
        public string CompanyModel { get; set; } // CompanyModel (length: 200)

        ///<summary>
        /// 机构排序号
        ///</summary>
        public int? CompanyOrderIndex { get; set; } // CompanyOrderIndex

        ///<summary>
        /// 机构首页类型
        ///</summary>
        public int? CompanyHomeType { get; set; } // CompanyHomeType

        ///<summary>
        /// 机构顶部图片
        ///</summary>
        public string CompanyTopImgUrl { get; set; } // CompanyTopImgUrl (length: 200)

        ///<summary>
        /// 机构Logo图片
        ///</summary>
        public string CompanyLogoImgUrl { get; set; } // CompanyLogoImgUrl (length: 200)

        ///<summary>
        /// 所属版本
        ///</summary>
        public string Version { get; set; } // Version (length: 40)

        ///<summary>
        /// 打印图标
        ///</summary>
        public string PrintImg { get; set; } // PrintImg (length: 200)

        ///<summary>
        /// 创建时间
        ///</summary>
        public System.DateTime? CreateTime { get; set; } // CreateTime

        ///<summary>
        /// 备注
        ///</summary>
        public string CompanyDesc { get; set; } // CompanyDesc (length: 500)

        ///<summary>
        /// 是否删除
        ///</summary>
        public bool? Isdelete { get; set; } // Isdelete
    }

    // SysQuartzTask
    ///<summary>
    /// 全区调度任务
    ///</summary>
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.36.1.0")]
    public class SysQuartzTask
    {

        ///<summary>
        /// 任务Id
        ///</summary>
        public System.Guid QuartzTaskId { get; set; } // QuartzTaskId (Primary key)

        ///<summary>
        /// 机构Id
        ///</summary>
        public System.Guid? SysCompanyId { get; set; } // SysCompanyId

        ///<summary>
        /// 任务名称
        ///</summary>
        public string QuartzTaskName { get; set; } // QuartzTaskName (length: 50)

        ///<summary>
        /// 任务类型（0：定时任务 1：接入服务）
        ///</summary>
        public int? QuartzTaskType { get; set; } // QuartzTaskType

        ///<summary>
        /// 接口参数
        ///</summary>
        public string QuartzTaskParams { get; set; } // QuartzTaskParams (length: 8000)

        ///<summary>
        /// 任务状态（0：停止 1：启动）
        ///</summary>
        public int? QuartzTaskStatus { get; set; } // QuartzTaskStatus

        ///<summary>
        /// 任务最近运行时间
        ///</summary>
        public System.DateTime? RecentRunTime { get; set; } // RecentRunTime

        ///<summary>
        /// 任务下次运行时间
        ///</summary>
        public System.DateTime? NextFireTime { get; set; } // NextFireTime

        ///<summary>
        /// 任务执行频率Cron
        ///</summary>
        public string CronExpressionString { get; set; } // CronExpressionString (length: 500)

        ///<summary>
        /// 修改状态
        ///</summary>
        public int? IsModify { get; set; } // IsModify

        ///<summary>
        /// 任务程序集部件名称
        ///</summary>
        public string PartName { get; set; } // PartName (length: 500)

        ///<summary>
        /// 任务程序集类名
        ///</summary>
        public string ClassName { get; set; } // ClassName (length: 500)

        ///<summary>
        /// 任务开始时间
        ///</summary>
        public System.DateTime? StartTime { get; set; } // StartTime

        ///<summary>
        /// 任务结束时间
        ///</summary>
        public System.DateTime? EndTime { get; set; } // EndTime

        ///<summary>
        /// 重试次数
        ///</summary>
        public int? RetryCount { get; set; } // RetryCount

        ///<summary>
        /// 接口请求类型
        ///</summary>
        public int? RequestType { get; set; } // RequestType

        ///<summary>
        /// 接口超时时间
        ///</summary>
        public int? TimeOut { get; set; } // TimeOut

        ///<summary>
        /// 接口Url
        ///</summary>
        public string QuartzTaskUrl { get; set; } // QuartzTaskUrl (length: 8000)

        ///<summary>
        /// 备注
        ///</summary>
        public string QuartzTaskDesc { get; set; } // QuartzTaskDesc (length: 500)

        ///<summary>
        /// 是否删除
        ///</summary>
        public bool? Isdelete { get; set; } // Isdelete

        ///<summary>
        /// 机构名称
        ///</summary>
        public string SysCompanyName { get; set; } // SysCompanyName (length: 50)
    }

    // SysQuartzTaskLog
    ///<summary>
    /// 调度任务执行日志
    ///</summary>
    [System.CodeDom.Compiler.GeneratedCode("EF.Reverse.POCO.Generator", "2.36.1.0")]
    public class SysQuartzTaskLog
    {

        ///<summary>
        /// 任务执行日志Id
        ///</summary>
        public System.Guid SysQuartzTaskLogId { get; set; } // SysQuartzTaskLogId (Primary key)

        ///<summary>
        /// 任务Id
        ///</summary>
        public System.Guid? QuartzTaskId { get; set; } // QuartzTaskId

        ///<summary>
        /// 任务开始时间
        ///</summary>
        public System.DateTime? StartTime { get; set; } // StartTime

        ///<summary>
        /// 任务结束时间
        ///</summary>
        public System.DateTime? EndTime { get; set; } // EndTime

        ///<summary>
        /// 执行结果
        ///</summary>
        public string ExecuteResult { get; set; } // ExecuteResult (length: 8000)

        ///<summary>
        /// 是否成功
        ///</summary>
        public bool? IsSuccess { get; set; } // IsSuccess

        ///<summary>
        /// 第几次执行
        ///</summary>
        public int? ExecuteCount { get; set; } // ExecuteCount

        ///<summary>
        /// 遗留问题
        ///</summary>
        public string LeftQuestionResult { get; set; } // LeftQuestionResult (length: 8000)

        ///<summary>
        /// 执行类型（0：手动  1：自动）
        ///</summary>
        public int? ExecutionType { get; set; } // ExecutionType

        ///<summary>
        /// 任务日志所属机构
        ///</summary>
        public System.Guid? SysQuartzTaskLogCid { get; set; } // SysQuartzTaskLogCid
    }
    #endregion
}
