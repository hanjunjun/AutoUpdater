using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    public class TaskResultModel
    {
        /// <summary>
        /// 任务执行结果
        /// </summary>
        public string ActionResult { get; set; }

        /// <summary>
        /// 历史执行结果
        /// </summary>
        public string HistoryResult { get; set; }

        /// <summary>
        /// 任务实际开始时间
        /// </summary>
        public DateTime RealStartTime { get; set; }
    }
}
