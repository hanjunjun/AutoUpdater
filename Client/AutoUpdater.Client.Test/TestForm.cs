using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdater.Client.Test
{
    public partial class TestForm : Form
    {
        #region 成员和构造函数
        private static Task TaskThread = null;
        private static Object _mlock = new Object();
        public TestForm()
        {
            InitializeComponent();
        }
        #endregion

        #region 按钮Button
        /// <summary>
        /// 登录按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button1_Click(object sender, EventArgs e)
        {
            await UpdateTask();
        }

        /// <summary>
        /// 首次现实窗体触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            button1.PerformClick();
        }

        /// <summary>
        /// 退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 按钮状态变更
        /// </summary>
        /// <param name="bt"></param>
        /// <param name="flag"></param>
        private void SetFlag(Button bt, bool flag)
        {
            if (bt.InvokeRequired)
            {
                bt.Invoke(new EventHandler(delegate
                {
                    bt.Enabled = flag;
                }), null);
            }
            else
            {
                bt.Enabled = flag;
            }
        }
        #endregion

        #region 更新
        /// <summary>
        /// 更新线程
        /// </summary>
        /// <returns></returns>
        private async Task UpdateTask()
        {
            lock (_mlock)
            {

                if (TaskThread == null)
                {
                    TaskThread = Task.Run(() =>
                    {
                        try
                        {
                            SetFlag(button1, false);
                            AddMessage($"{DateTime.Now}：1.启动更新组件！");
                            Process process = new Process();
                            process.StartInfo = new ProcessStartInfo($@"{AppDomain.CurrentDomain.BaseDirectory}AutoUpdater.exe");
                            process.StartInfo.Arguments = "Updater"; //启动外部程序传参
                            process.StartInfo.UseShellExecute = false; //重定向
                            process.StartInfo.CreateNoWindow = true;
                            process.StartInfo.RedirectStandardInput = true;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            process.WaitForExit();
                            string output = process.StandardOutput.ReadToEnd(); //这种方式可以读到console，如果主程序已经关闭，无法读取到
                            AddMessage($"{DateTime.Now}：2.获取更新组件返回结果 {output}");
                            if (output.Trim() == "NoUpdater")
                            {
                                AddMessage($"{DateTime.Now}：3.当前系统为最新版本，无需更新！");
                            }
                            else
                            {
                                if (ConfigurationManager.AppSettings["UpdaterModel"] == "0")
                                {
                                    AddMessage($"{DateTime.Now}：4.是否重新尝试更新系统！等待用户选择！");
                                    //弹出提示让用户选择是否需要更新，后期可以改成控制台显示信息。
                                    MessageBox.Show(output, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    if (MessageBox.Show("是否重新尝试更新系统！", "提示", MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Information) == DialogResult.OK)
                                    {
                                        AddMessage($"{DateTime.Now}：5.用户选择重试更新！");
                                        Thread.Sleep(5000);
                                        TaskThread = null;
                                        SetFlag(button1, true);
                                        button1_Click(null, null);
                                        //this.button1_Click(null, null);
                                    }
                                    else
                                    {
                                        AddMessage($"{DateTime.Now}：6.用户选择取消！");
                                        SetFlag(button1, true);
                                        TaskThread = null;
                                    }
                                }
                                else
                                {
                                    AddMessage($"{DateTime.Now}：7.自动重试更新！");
                                    Thread.Sleep(5000);
                                    TaskThread = null;
                                    //自动重试更新，不提示
                                    SetFlag(button1, true);
                                    button1_Click(null, null);
                                    //this.button1_Click(null, null);
                                }
                            }


                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                    });
                }
                else
                {
                    Console.WriteLine("已经运行了禁止运行");
                }
            }
            await TaskThread;
        }

        /// <summary>
        /// 更新进度消息
        /// </summary>
        /// <param name="msg"></param>
        private void AddMessage(string msg)
        {
            if (richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new EventHandler(delegate
                {
                    this.richTextBox1.AppendText(msg + "\r\n");
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                }), null);

                //dlgShowMsg dlg=new dlgShowMsg(AddMessage);
                //richTextBox1.Invoke(dlg, msg);
            }
            else
            {
                richTextBox1.AppendText(msg + "\r\n");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            //richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //richTextBox1.ScrollToCaret();
        }
        #endregion
    }
}
