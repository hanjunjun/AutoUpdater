using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.WindowsService
{
    public class WMIHelper
    {
        private string strPath;
        private ManagementClass managementClass;
        public WMIHelper()//:this(".",null,null)
        {
            this.strPath = "\\\\" + "." + "\\root\\cimv2:Win32_Service";
            this.managementClass = new ManagementClass(strPath);
            ConnectionOptions connectionOptions = new ConnectionOptions();
            ManagementScope managementScope = new ManagementScope("//" + "." + "/root/cimv2", connectionOptions);
            try
            {
                managementScope.Connect();
                this.managementClass.Scope = managementScope;
            }
            catch
            {
            }

        }
        public WMIHelper(string host, string userName, string password)
        {
            this.strPath = "\\\\" + host + "\\root\\cimv2:Win32_Service";
            this.managementClass = new ManagementClass(strPath);
            if (userName != null && userName.Length > 0)
            {
                ConnectionOptions connectionOptions = new ConnectionOptions();
                connectionOptions.Username = userName;
                connectionOptions.Password = password;
                ManagementScope managementScope = new ManagementScope("\\\\" + host + "\\root\\cimv2", connectionOptions);
                this.managementClass.Scope = managementScope;
            }
        }
        // 验证是否能连接到远程计算机
        public static bool RemoteConnectValidate(string host, string userName, string password)
        {
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.Username = userName;
            connectionOptions.Password = password;
            ManagementScope managementScope = new ManagementScope("\\\\" + host + "\\root\\cimv2", connectionOptions);
            try
            {
                managementScope.Connect();
            }
            catch (ManagementException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return managementScope.IsConnected;
        }
        // 获取指定服务属性的值
        public object GetServiceValue(string serviceName, string propertyName)
        {
            ManagementObject mo = this.managementClass.CreateInstance();
            mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serviceName + "\"");
            return mo[propertyName];
        }
        // 获取所连接的计算机的所有服务数据
        public string[,] GetServiceList()
        {
            string[,] services = new string[this.managementClass.GetInstances().Count, 4];
            int i = 0;
            foreach (ManagementObject mo in this.managementClass.GetInstances())
            {
                services[i, 0] = (string)mo["Name"];
                services[i, 1] = (string)mo["DisplayName"];
                services[i, 2] = (string)mo["State"];
                services[i, 3] = (string)mo["StartMode"];
                i++;
            }
            return services;
        }
        // 获取所连接的计算机的指定服务数据
        public string[,] GetServiceList(string serverName)
        {
            return GetServiceList(new string[] { serverName });
        }
        // 获取所连接的计算机的的指定服务数据
        public string[,] GetServiceList(string[] serverNames)
        {
            string[,] services = new string[serverNames.Length, 4];
            ManagementObject mo = this.managementClass.CreateInstance();
            for (int i = 0; i < serverNames.Length; i++)
            {
                mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serverNames[i] + "\"");
                services[i, 0] = (string)mo["Name"];
                services[i, 1] = (string)mo["DisplayName"];
                services[i, 2] = (string)mo["State"];
                services[i, 3] = (string)mo["StartMode"];
            }
            return services;
        }
        // 停止指定的服务
        public string StartService(string serviceName)
        {
            string strRst = null;
            ManagementObject mo = this.managementClass.CreateInstance();
            mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serviceName + "\"");
            if ((string)mo["State"] == "Stopped")//!(bool)mo["AcceptStop"]
                mo.InvokeMethod("StartService", null);
            return strRst;
        }
        // 暂停指定的服务
        public string PauseService(string serviceName)
        {
            string strRst = null;
            ManagementObject mo = this.managementClass.CreateInstance();
            mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serviceName + "\"");
            try
            {
                //判断是否可以暂停
                if ((bool)mo["acceptPause"] && (string)mo["State"] == "Running")
                    mo.InvokeMethod("PauseService", null);
            }
            catch (ManagementException e)
            {
                strRst = e.Message;
            }
            return strRst;
        }
        // 恢复指定的服务
        public string ResumeService(string serviceName)
        {
            string strRst = null;
            ManagementObject mo = this.managementClass.CreateInstance();
            mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serviceName + "\"");
            try
            {
                //判断是否可以恢复
                if ((bool)mo["acceptPause"] && (string)mo["State"] == "Paused")
                    mo.InvokeMethod("ResumeService", null);
            }
            catch (ManagementException e)
            {
                strRst = e.Message;
            }
            return strRst;
        }
        // 停止指定的服务
        public string StopService(string serviceName)
        {
            string strRst = null;
            ManagementObject mo = this.managementClass.CreateInstance();
            mo.Path = new ManagementPath(this.strPath + ".Name=\"" + serviceName + "\"");
            //判断是否可以停止
            if ((bool)mo["AcceptStop"])//(string)mo["State"]=="Running"
                mo.InvokeMethod("StopService", null);
            return strRst;
        }
    }
}
