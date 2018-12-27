using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRobotModel.CommonModel
{
	public class AccessToken
	{
		public Authorize authorize { get; set; }
	}

	public class Authorize
	{
		public string user_name { get; set; }
		public int authorize_time { get; set; }
		public string token_type { get; set; }
		public int expires_in { get; set; }
		public int user_id { get; set; }
		public string access_token { get; set; }
	}
}
