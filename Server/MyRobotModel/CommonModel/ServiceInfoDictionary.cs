using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
    public class ServiceInfoDictionary
    {
        public static ConcurrentDictionary<string, ServiceInfoModel> ServiceInfoDic = new ConcurrentDictionary<string, ServiceInfoModel>();//服务信息字典
    }
}
