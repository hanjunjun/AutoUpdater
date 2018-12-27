using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    public class EmailRetryModel
    {
        /// <summary>
        /// 发件人
        /// </summary>
        public string EmailUserName { get; set; }

        /// <summary>
        /// 收件人列表
        /// </summary>
        public string[] SJEmailList { get; set; }

        /// <summary>
        /// 邮件大标题
        /// </summary>
        public string Biaoti { get; set; }

        /// <summary>
        /// 邮件标题内容
        /// </summary>
        public string BiaotiMsg { get; set; }

        /// <summary>
        /// 邮件内容
        /// </summary>
        public string NeiRong { get; set; }

        /// <summary>
        /// 发件人邮箱密码
        /// </summary>
        public string EmailPassWord { get; set; }
    }
}
