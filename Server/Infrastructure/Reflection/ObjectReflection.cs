using System;
using System.Collections.Generic;
using System.Reflection;
namespace Infrastructures.Reflection
{
    public class ObjectReflection
    {
        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="typeInfo">类型名称</param>
        /// <returns></returns>
        public static object CreateObject(string typeInfo)
        {
            object obj = null;
            if (!string.IsNullOrEmpty(typeInfo))
            {
                string assemblyInfo = typeInfo;
                if (!string.IsNullOrEmpty(assemblyInfo.Trim()))
                {
                    Type objType = LoadSmartPartType(assemblyInfo.Trim());
                    if (objType != null)
                        obj = Activator.CreateInstance(objType);
                }
            }
            return obj;
        }

        /// <summary>
        /// 加载对象类型
        /// </summary>
        /// <param name="asseblyName">动态链接库名称</param>
        /// <param name="className">类名称</param>
        /// <returns></returns>
        public static Type LoadSmartPartType(string asseblyName)
        {
            Assembly targetAssembly = null;
            string _className = null;
            //string paths = Environment.CurrentDirectory;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            //string BHPath = path.Substring(path.TrimEnd(new char[] { '\\' }).ToString().LastIndexOf(@"\") + 1).TrimEnd(new char[] { '\\' });
            //if (BHPath == "Client")
            //    path = path + "zlInterface\\";
            try
            {
                byte[] b = System.IO.File.ReadAllBytes(path + asseblyName + ".dll");
                targetAssembly = Assembly.Load(b);
                //targetAssembly = Assembly.LoadFrom(path + asseblyName + ".dll");
                //循环医保组件类  一般为Cls_
                foreach (Type clsName in targetAssembly.GetTypes())
                {
                    if (("." + clsName.Name).EndsWith(".ResolveResult"))  // if (("." + clsName.Name + ".").IndexOf(".ResolveResult.") != -1)
                    // if (clsName.Name.IndexOf("ResolveResult") != -1)
                    {
                        _className = asseblyName + "." + clsName.Name;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Type type = null;
            try
            {
                if (targetAssembly == null)
                {
                    type = System.Type.GetType(_className);
                }
                else
                {
                    type = targetAssembly.GetType(_className);
                }
            }
            catch
            {
                type = null;
            }
            return type;
        }

        public static List<string> GetClassName(string FilePath)
        {
            if (!System.IO.File.Exists(FilePath))
            {
                return new List<string>();
            }
            string asseblyName = string.Empty;
            Assembly targetAssembly = null;
            List<string> _className = new List<string>();
            asseblyName = FilePath.Substring(FilePath.LastIndexOf("/") + 1);
            asseblyName = asseblyName.Substring(0, asseblyName.LastIndexOf('.'));
            byte[] b = System.IO.File.ReadAllBytes(FilePath);
            targetAssembly = Assembly.Load(b);
            //循环医保组件类  一般为Cls_
            foreach (Type clsName in targetAssembly.GetTypes())
            {
                if (clsName.FullName.Contains("TimingTask"))
                {
                    _className.Add(asseblyName + "." + clsName.Name);
                }
            }
            return _className;
        }
    }
}
