/************************************************************************
 * 文件名：DynamicCompile
 * 文件功能描述：xx控制层
 * 作    者：  韩俊俊
 * 创建日期：2018/9/18 10:55:27
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * Copyright (c) 2017 Titan.Han . All Rights Reserved. 
 * ***********************************************************************/
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace Infrastructure.DynCompile
{
    public class DynamicCompile
    {
        public static Dictionary<string, string> Dic = new Dictionary<string, string>();

        public static void DicAdd(string key,string value)
        {
            Dic.Add(key,value);
        }

        public static string DisPlayDic(string key)
        {
            if (Dic.ContainsKey(key))
            {
                return Dic[key];
            }
            else
            {
                return "没找到key！";
            }
        }

        public static int GetDicCount()
        {
            return Dic.Count;
        }

        public static string GetDicListValue()
        {
            var text = "";
            foreach (var item in Dic)
            {
                text += item.Key + "：" + item.Value+"\r\n";
            }

            return text;
        }
    }
}
