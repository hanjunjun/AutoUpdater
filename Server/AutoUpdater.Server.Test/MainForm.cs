using Infrastructure.Log;
using MyRobot.CommunicatePackage.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdater.Server.Test
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string errorMsg = string.Empty;
            InitTcpServer tcpService = new InitTcpServer();
            tcpService.DbType = "AutoUpdaterService";
            if (tcpService.Start(true, ref errorMsg))
            {
                MessageBox.Show("更新服务启动成功！", "提示");
                WriteDeviceLog.WriteLog("Log\\Log", "更新服务启动成功！", Guid.NewGuid().ToString());
            }
            else
            {
                MessageBox.Show($"更新服务启动失败！原因：{errorMsg}", "提示");
                WriteDeviceLog.WriteLog("Log\\Log", $"更新服务启动失败！原因：{errorMsg}", Guid.NewGuid().ToString());
            }
        }
    }
}
