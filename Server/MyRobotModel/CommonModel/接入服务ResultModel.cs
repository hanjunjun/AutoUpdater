using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    public class 接入服务ResultModel
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 是否保存日志
        /// </summary>
        public bool IsSaveLog { get; set; } = true;

        /// <summary>
        /// 预留参数1
        /// </summary>
        public string Param1 { get; set; }

        /// <summary>
        /// 预留参数2
        /// </summary>
        public string Param2 { get; set; }

        /// <summary>
        /// 预留参数3
        /// </summary>
        public string Param3 { get; set; }

        /// <summary>
        /// 预留参数4
        /// </summary>
        public string Param4 { get; set; }
    }
}
