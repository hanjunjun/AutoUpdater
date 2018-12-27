using System;
using System.Configuration;
using System.Windows.Forms;

namespace AutoUpdater
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                //Thread.Sleep(10*1000);//延迟10秒方便附加调试
                if (args.Length == 0)
                {
                    //如果不是通过主程序启动直接退出更新组件
                    System.Environment.Exit(0);
                }
                else
                {
                    if (args[0] != "Updater")
                    {
                        //传参不是updater退出程序
                        System.Environment.Exit(0);
                    }
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //加载服务端配置，连接服务器获取需要更新的文件
                string serverIp = ConfigurationManager.AppSettings["ServerIP"];
                int serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
                string callBackExeName = ConfigurationManager.AppSettings["CallbackExeName"];
                string title = ConfigurationManager.AppSettings["Title"];
                long pageSize= long.Parse(ConfigurationManager.AppSettings["PageSize"]);
                MainForm form = new MainForm(serverIp, serverPort, callBackExeName, title, pageSize);
                Application.Run(form);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }
    }
}
