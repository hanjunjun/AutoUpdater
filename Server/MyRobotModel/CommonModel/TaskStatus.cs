using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 运行状态
        /// </summary>
        RUN = 1,

        /// <summary>
        /// 停止状态
        /// </summary>
        STOP = 0
    }

    public enum TaskModifyStatus
    {
        /// <summary>
        /// 需要更新的任务
        /// </summary>
        Yes=1,

        /// <summary>
        /// 不需要更新
        /// </summary>
        No=0
    }

    /// <summary>
    /// 任务配置的方式
    /// </summary>
    public enum TaskStore
    {
        DB = 1,
        XML = 2
    }


}
