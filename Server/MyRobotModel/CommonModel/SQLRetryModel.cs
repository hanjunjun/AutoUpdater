using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyRobotModel.DataModel;

namespace MyRobotModel.CommonModel
{
    public class SQLRetryModel
    {
        /// <summary>
        /// 需要执行的sql
        /// </summary>
        public string SQL { get; set; }

        /// <summary>
        /// 执行类型1：不带参数，2：带参数（写入执行日志model）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 参数实体
        /// </summary>
        public SysQuartzTaskLog ParamsModel { get; set; }
    }
}
