using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdater.Common
{
    public class WriteDeviceLog
    {
        private static readonly object _lock = new object();
        //队列元素
        static ConcurrentQueue<Tuple<string, string>> logQueue = new ConcurrentQueue<Tuple<string, string>>();
        
        static Task writeTask = default(Task);

        static ManualResetEvent pause = new ManualResetEvent(false);//开始是无信号的

        static WriteDeviceLog()
        {
            //开一个长时间运行的task
            writeTask = new Task((object obj) =>
            {
                while (true)
                {
                    pause.WaitOne();//等待信号到来
                    pause.Reset();//设置无信号
                    List<string[]> temp = new List<string[]>();
                    foreach (var logItem in logQueue)
                    {
                        string logPath = logItem.Item1;
                        string logMergeContent = String.Concat(logItem.Item2, Environment.NewLine);//, Environment.NewLine, ""
                        string[] logArr = temp.FirstOrDefault(d => d[0].Equals(logPath));//取出路径相同的记录
                        if (logArr != null)
                        {
                            //如果找到相同路径的记录，就在写入内容后面加上。
                            logArr[1] = string.Concat(logArr[1], logMergeContent);
                        }
                        else
                        {
                            //如果没找到相同路径的记录，加一个新的list
                            logArr = new string[] { logPath, logMergeContent };
                            temp.Add(logArr);
                        }
                        Tuple<string, string> val = default(Tuple<string, string>);
                        logQueue.TryDequeue(out val);//删除队列头的元素
                    }
                    foreach (string[] item in temp)//写入文件
                    {
                        WriteText(item[0], item[1]);
                    }

                }
            }
            , null
            , TaskCreationOptions.LongRunning);//意味着该任务将长时间运行，因此他不是在线程池中执行。
            writeTask.Start();
        }
        //public static void WriteLog(String preFile, String infoData)
        //{
        //    WriteLog(string.Empty, preFile, infoData);
        //}
        public static void WriteLog(String customDirectory, String infoData, string EventID="")
        {
            //先干掉排查问题
            string logPath = GetLogPath(customDirectory, string.Empty);
            string logContent = String.Concat(DateTime.Now, EventID == "" ? "" : $@" 【事件ID：{EventID}】 ", infoData);
            logQueue.Enqueue(new Tuple<string, string>(logPath, logContent));
            pause.Set();//设置有信号激活线程
        }

        private static string GetLogPath(String customDirectory, String preFile)
        {
            string newFilePath = string.Empty;
            String logDir = string.IsNullOrEmpty(customDirectory) ? Path.Combine(Environment.CurrentDirectory, "Log") : AppDomain.CurrentDomain.BaseDirectory+"\\"+customDirectory;
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            string extension = ".log";
            string fileNameNotExt = String.Concat(preFile, DateTime.Now.ToString("yyyyMMdd"));
            String fileName = String.Concat(fileNameNotExt, extension);
            string fileNamePattern = string.Concat(fileNameNotExt, "(*)", extension);
            List<string> filePaths = Directory.GetFiles(logDir, fileNamePattern, SearchOption.TopDirectoryOnly).ToList();

            if (filePaths.Count > 0)
            {
                int fileMaxLen = filePaths.Max(d => d.Length);
                string lastFilePath = filePaths.Where(d => d.Length == fileMaxLen).OrderByDescending(d => d).FirstOrDefault();
                if (new FileInfo(lastFilePath).Length > 1 * 1024 * 1024 * 1024)
                {
                    string no = new Regex(@"(?is)(?<=\()(.*)(?=\))").Match(Path.GetFileName(lastFilePath)).Value;
                    int tempno = 0;
                    bool parse = int.TryParse(no, out tempno);
                    string formatno = String.Format("({0})", parse ? (tempno + 1) : tempno);
                    string newFileName = String.Concat(fileNameNotExt, formatno, extension);
                    newFilePath = Path.Combine(logDir, newFileName);
                }
                else
                {
                    newFilePath = lastFilePath;
                }
            }
            else
            {
                string newFileName = String.Concat(fileNameNotExt, String.Format("({0})", 0), extension);
                newFilePath = Path.Combine(logDir, newFileName);
            }
            return newFilePath;
        }

        public static void WriteText(string logPath, string logContent)
        {
            lock (_lock)
            {
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    }
                    if (!System.IO.File.Exists(logPath))
                    {
                        System.IO.File.CreateText(logPath).Close();
                    }
                    StreamWriter sw = System.IO.File.AppendText(logPath);
                    sw.Write(logContent);
                    sw.Close();
                }
                catch (Exception ex)
                {

                }
                finally
                {

                }
            }
        }
        #region 接口日志
        /// <summary>
        /// 医保接口日志记录
        /// </summary>
        /// <param name="strSysName">保险系统名称</param>
        /// <param name="strLog">日志内容</param>
        //public static void WriteLog(string strSysName, string strLog, string EventID)
        //{
        //    try
        //    {
        //        string fileName = DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
        //        string m_path = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + strSysName;//存放日志路径
        //        DirectoryInfo m_DirInfo = new DirectoryInfo(m_path);
        //        if (!m_DirInfo.Exists)
        //        {
        //            Directory.CreateDirectory(m_path);//创建文件夹
        //        }
        //        //FileStream fs = new FileStream(m_path + "\\" + fileName, FileMode.Append, FileAccess.Write);
        //        //StreamWriter sw = new StreamWriter(fs);
        //        //sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 【事件ID:" + EventID + "】 " + strLog);
        //        //sw.Close();
        //        //fs.Close();
        //        lock (m_Lock)
        //        {
        //            FileStream fs = new FileStream(m_path + "\\" + fileName, FileMode.Append, FileAccess.Write);
        //            StreamWriter sw = new StreamWriter(fs);
        //            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 【事件ID:" + EventID + "】 " + strLog);
        //            sw.Close();
        //            fs.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw new Exception(ex.ToString());
        //        WriteLog(strSysName, strLog, Guid.NewGuid().ToString());
        //    }
        //}
        #endregion
    }
}
