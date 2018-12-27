/************************************************************************
 * 文件名：ClassHelper
 * 文件功能描述：xx控制层
 * 作    者：  韩俊俊
 * 创建日期：2018/12/6 11:58:13
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * Copyright (c) 2017 Titan.Han . All Rights Reserved. 
 * ***********************************************************************/
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp;
using PropertyAttributes = System.Reflection.PropertyAttributes;

namespace Infrastructure.DynCompile
{
    public class ClassHelper
    {
        #region 公有方法  
        /**//// <summary>  
        /// 防止实例化。  
        /// </summary>  
        private ClassHelper() { }

        /**//// <summary>  
            /// 根据类的类型型创建类实例。  
            /// </summary>  
            /// <param name="t">将要创建的类型。</param>  
            /// <returns>返回创建的类实例。</returns>  
        public static object CreateInstance(Type t)
        {
            return Activator.CreateInstance(t);
        }

        /**//// <summary>  
            /// 根据类的名称,属性列表创建型实例。  
            /// </summary>  
            /// <param name="className">将要创建的类的名称。</param>  
            /// <param name="lcpi">将要创建的类的属性列表。</param>  
            /// <returns>返回创建的类实例</returns>  
        public static object CreateInstance(string className, List<CustPropertyInfo> lcpi)
        {
            Type t = BuildType(className);
            t = AddProperty(t, lcpi);
            return Activator.CreateInstance(t);
        }

        /**//// <summary>  
            /// 根据属性列表创建类的实例,默认类名为DefaultClass,由于生成的类不是强类型,所以类名可以忽略。  
            /// </summary>  
            /// <param name="lcpi">将要创建的类的属性列表</param>  
            /// <returns>返回创建的类的实例。</returns>  
        public static object CreateInstance(List<CustPropertyInfo> lcpi)
        {
            return CreateInstance("DefaultClass", lcpi);
        }

        /**//// <summary>  
            /// 根据类的实例设置类的属性。  
            /// </summary>  
            /// <param name="classInstance">将要设置的类的实例。</param>  
            /// <param name="propertyName">将要设置属性名。</param>  
            /// <param name="propertSetValue">将要设置属性值。</param>  
        public static void SetPropertyValue(object classInstance, string propertyName, object propertSetValue)
        {
            classInstance.GetType().InvokeMember(propertyName, BindingFlags.SetProperty,
                                          null, classInstance, new object[] { Convert.ChangeType(propertSetValue, propertSetValue.GetType()) });
        }

        /**//// <summary>  
            /// 根据类的实例获取类的属性。  
            /// </summary>  
            /// <param name="classInstance">将要获取的类的实例</param>  
            /// <param name="propertyName">将要设置的属性名。</param>  
            /// <returns>返回获取的类的属性。</returns>  
        public static object GetPropertyValue(object classInstance, string propertyName)
        {
            return classInstance.GetType().InvokeMember(propertyName, BindingFlags.GetProperty,
                                                          null, classInstance, new object[] { });
        }

        /**//// <summary>  
            /// 创建一个没有成员的类型的实例,类名为"DefaultClass"。  
            /// </summary>  
            /// <returns>返回创建的类型的实例。</returns>  
        public static Type BuildType()
        {
            return BuildType("DefaultClass");
        }

        /**//// <summary>  
            /// 根据类名创建一个没有成员的类型的实例。  
            /// </summary>  
            /// <param name="className">将要创建的类型的实例的类名。</param>  
            /// <returns>返回创建的类型的实例。</returns>  
        public static Type BuildType(string className)
        {

            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "MyDynamicAssembly";

            //创建一个永久程序集,设置为AssemblyBuilderAccess.RunAndSave。  
            AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName,
                                                            AssemblyBuilderAccess.RunAndSave);

            //创建一个永久单模程序块。  
            ModuleBuilder myModBuilder =
                myAsmBuilder.DefineDynamicModule(myAsmName.Name, myAsmName.Name + ".dll");
            //创建TypeBuilder。  
            TypeBuilder myTypeBuilder = myModBuilder.DefineType(className,
                                                            TypeAttributes.Public);

            //创建类型。  
            Type retval = myTypeBuilder.CreateType();

            //保存程序集,以便可以被Ildasm.exe解析,或被测试程序引用。  
            //myAsmBuilder.Save(myAsmName.Name + ".dll");  
            return retval;
        }

        /**//// <summary>  
            /// 添加属性到类型的实例,注意:该操作会将其它成员清除掉,其功能有待完善。  
            /// </summary>  
            /// <param name="classType">指定类型的实例。</param>  
            /// <param name="lcpi">表示属性的一个列表。</param>  
            /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddProperty(Type classType, List<CustPropertyInfo> lcpi)
        {
            //合并先前的属性,以便一起在下一步进行处理。  
            MergeProperty(classType, lcpi);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }

        /**//// <summary>  
            /// 添加属性到类型的实例,注意:该操作会将其它成员清除掉,其功能有待完善。  
            /// </summary>  
            /// <param name="classType">指定类型的实例。</param>  
            /// <param name="cpi">表示一个属性。</param>  
            /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddProperty(Type classType, CustPropertyInfo cpi)
        {
            List<CustPropertyInfo> lcpi = new List<CustPropertyInfo>();
            lcpi.Add(cpi);
            //合并先前的属性,以便一起在下一步进行处理。  
            MergeProperty(classType, lcpi);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }

        /**//// <summary>  
            /// 从类型的实例中移除属性,注意:该操作会将其它成员清除掉,其功能有待完善。  
            /// </summary>  
            /// <param name="classType">指定类型的实例。</param>  
            /// <param name="propertyName">要移除的属性。</param>  
            /// <returns>返回处理过的类型的实例。</returns>  
        public static Type DeleteProperty(Type classType, string propertyName)
        {
            List<string> ls = new List<string>();
            ls.Add(propertyName);

            //合并先前的属性,以便一起在下一步进行处理。  
            List<CustPropertyInfo> lcpi = SeparateProperty(classType, ls);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }

        /**//// <summary>  
            /// 从类型的实例中移除属性,注意:该操作会将其它成员清除掉,其功能有待完善。  
            /// </summary>  
            /// <param name="classType">指定类型的实例。</param>  
            /// <param name="ls">要移除的属性列表。</param>  
            /// <returns>返回处理过的类型的实例。</returns>  
        public static Type DeleteProperty(Type classType, List<string> ls)
        {
            //合并先前的属性,以便一起在下一步进行处理。  
            List<CustPropertyInfo> lcpi = SeparateProperty(classType, ls);
            //把属性加入到Type。  
            return AddPropertyToType(classType, lcpi);
        }
        #endregion

        #region 私有方法  
        /**//// <summary>  
            /// 把类型的实例t和lcpi参数里的属性进行合并。  
            /// </summary>  
            /// <param name="t">实例t</param>  
            /// <param name="lcpi">里面包含属性列表的信息。</param>  
        private static void MergeProperty(Type t, List<CustPropertyInfo> lcpi)
        {
            foreach (PropertyInfo pi in t.GetProperties())
            {
                CustPropertyInfo cpi = new CustPropertyInfo(pi.PropertyType.FullName, pi.Name);
                lcpi.Add(cpi);
            }
        }

        /**//// <summary>  
            /// 从类型的实例t的属性移除属性列表lcpi,返回的新属性列表在lcpi中。  
            /// </summary>  
            /// <param name="t">类型的实例t。</param>  
            /// <param name="ls">要移除的属性列表。</param>  
        private static List<CustPropertyInfo> SeparateProperty(Type t, List<string> ls)
        {
            List<CustPropertyInfo> ret = new List<CustPropertyInfo>();
            foreach (PropertyInfo pi in t.GetProperties())
            {
                foreach (string s in ls)
                {
                    if (pi.Name != s)
                    {
                        CustPropertyInfo cpi = new CustPropertyInfo(pi.PropertyType.FullName, pi.Name);
                        ret.Add(cpi);
                    }
                }
            }

            return ret;
        }

        /**//// <summary>  
            /// 把lcpi参数里的属性加入到myTypeBuilder中。注意:该操作会将其它成员清除掉,其功能有待完善。  
            /// </summary>  
            /// <param name="myTypeBuilder">类型构造器的实例。</param>  
            /// <param name="lcpi">里面包含属性列表的信息。</param>  
        private static void AddPropertyToTypeBuilder(TypeBuilder myTypeBuilder, List<CustPropertyInfo> lcpi)
        {
            PropertyBuilder custNamePropBldr;
            MethodBuilder custNameGetPropMthdBldr;
            MethodBuilder custNameSetPropMthdBldr;
            MethodAttributes getSetAttr;
            ILGenerator custNameGetIL;
            ILGenerator custNameSetIL;

            // 属性Set和Get方法要一个专门的属性。这里设置为Public。  
            getSetAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig;

            // 添加属性到myTypeBuilder。  
            foreach (CustPropertyInfo cpi in lcpi)
            {
                //定义字段。  
                FieldBuilder customerNameBldr = myTypeBuilder.DefineField(cpi.FieldName,
                                                                          Type.GetType(cpi.Type),
                                                                          FieldAttributes.Private);
                customerNameBldr.SetConstant("11111111");
                //定义属性。  
                //最后一个参数为null,因为属性没有参数。  
                custNamePropBldr = myTypeBuilder.DefineProperty(cpi.PropertyName,
                                                                 PropertyAttributes.HasDefault,
                                                                 Type.GetType(cpi.Type),
                                                                 null);

                custNamePropBldr.SetConstant("111111111");
                //定义Get方法。  
                custNameGetPropMthdBldr =
                    myTypeBuilder.DefineMethod(cpi.GetPropertyMethodName,
                                               getSetAttr,
                                               Type.GetType(cpi.Type),
                                               Type.EmptyTypes);

                custNameGetIL = custNameGetPropMthdBldr.GetILGenerator();

                try
                {
                    custNameGetIL.Emit(OpCodes.Ldarg_0);
                    //custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);  
                    custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);
                    custNameGetIL.Emit(OpCodes.Ret);
                }
                catch (Exception ex)
                {


                }

                //定义Set方法。  
                custNameSetPropMthdBldr =
                    myTypeBuilder.DefineMethod(cpi.SetPropertyMethodName,
                                               getSetAttr,
                                               null,
                                               new Type[] { Type.GetType(cpi.Type) });

                custNameSetIL = custNameSetPropMthdBldr.GetILGenerator();

                custNameSetIL.Emit(OpCodes.Ldarg_0);
                custNameSetIL.Emit(OpCodes.Ldarg_1);
                custNameSetIL.Emit(OpCodes.Stfld, customerNameBldr);
                custNameSetIL.Emit(OpCodes.Ret);
                //custNamePropBldr.SetConstant("ceshi");  
                //把创建的两个方法(Get,Set)加入到PropertyBuilder中。  
                custNamePropBldr.SetGetMethod(custNameGetPropMthdBldr);
                custNamePropBldr.SetSetMethod(custNameSetPropMthdBldr);
            }
        }

        /**//// <summary>  
            /// 把属性加入到类型的实例。  
            /// </summary>  
            /// <param name="classType">类型的实例。</param>  
            /// <param name="lcpi">要加入的属性列表。</param>  
            /// <returns>返回处理过的类型的实例。</returns>  
        public static Type AddPropertyToType(Type classType, List<CustPropertyInfo> lcpi)
        {
            AppDomain myDomain = Thread.GetDomain();
            AssemblyName myAsmName = new AssemblyName();
            myAsmName.Name = "MyDynamicAssembly";

            //创建一个永久程序集,设置为AssemblyBuilderAccess.RunAndSave。  
            AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName,
                                                            AssemblyBuilderAccess.RunAndSave);

            //创建一个永久单模程序块。  
            ModuleBuilder myModBuilder =
                myAsmBuilder.DefineDynamicModule(myAsmName.Name, myAsmName.Name + ".dll");
            //创建TypeBuilder。  
            TypeBuilder myTypeBuilder = myModBuilder.DefineType(classType.FullName,
                                                            TypeAttributes.Public);

            //把lcpi中定义的属性加入到TypeBuilder。将清空其它的成员。其功能有待扩展,使其不影响其它成员。  
            AddPropertyToTypeBuilder(myTypeBuilder, lcpi);

            //创建类型。  
            Type retval = myTypeBuilder.CreateType();

            //保存程序集,以便可以被Ildasm.exe解析,或被测试程序引用。  
            //myAsmBuilder.Save(myAsmName.Name + ".dll");  
            return retval;
        }
        #endregion

        #region 辅助类  
        /**//// <summary>  
            /// 自定义的属性信息类型。  
            /// </summary>  
        public class CustPropertyInfo
        {
            private string propertyName;
            private string type;

            /**//// <summary>  
                /// 空构造。  
                /// </summary>  
            public CustPropertyInfo() { }

            /**//// <summary>  
                /// 根据属性类型名称,属性名称构造实例。  
                /// </summary>  
                /// <param name="type">属性类型名称。</param>  
                /// <param name="propertyName">属性名称。</param>  
            public CustPropertyInfo(string type, string propertyName)
            {
                this.type = type;
                this.propertyName = propertyName;
            }

            /**//// <summary>  
                /// 获取或设置属性类型名称。  
                /// </summary>  
            public string Type
            {
                get { return type; }
                set { type = value; }
            }

            /**//// <summary>  
                /// 获取或设置属性名称。  
                /// </summary>  
            public string PropertyName
            {
                get { return propertyName; }
                set { propertyName = value; }
            }

            /**//// <summary>  
                /// 获取属性字段名称。  
                /// </summary>  
            public string FieldName
            {
                get
                {
                    if (propertyName.Length < 1)
                        return "";
                    return propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
                }
            }

            /**//// <summary>  
                /// 获取属性在IL中的Set方法名。  
                /// </summary>  
            public string SetPropertyMethodName
            {
                get { return "set_" + PropertyName; }
            }

            /**//// <summary>  
                ///  获取属性在IL中的Get方法名。  
                /// </summary>  
            public string GetPropertyMethodName
            {
                get { return "get_" + PropertyName; }
            }
        }
        #endregion

        public static void CreateNewClass()
        {
            //using System.Dynamic;
            //C# 4.0 or higher
            //这个代码只是证明C#具有动态类型的特性——事实上这个特性往往被用于做COM互操作而不是模仿动态语言的编程行为。
            List<dynamic> list = new List<dynamic>();
            dynamic d = new ExpandoObject();
            d.姓名 = "韩俊俊";
            d.年龄 = "18";
            (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("性别", "男"));
            (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("手机", "13337700393"));
            (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("邮箱", "1454055505@qq.com"));
            list.Add(d);
            dynamic c = new ExpandoObject();
            c.姓名 = "张三";
            c.年龄 = "22";
            (c as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("性别", "女"));
            (c as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("手机", "13337700393"));
            (c as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>("邮箱", "2254055505@qq.com"));
            list.Add(c);
            //Console.WriteLine(d.Name + " is " + d.Age + " years old.");
            //Console.WriteLine(d.email);
            //foreach (var item in (d as ExpandoObject).Select(x => x.Key))
            //{
            //    Console.WriteLine("d have property {0}.", item);
            //}
            var objList = new List<object>();
            objList = list;
            var columnListName = new List<string>();//列名
            var dataList = new List<List<string>>();//数据列值
            int i = 0;
            foreach (var item in (objList.FirstOrDefault() as ICollection<KeyValuePair<string, object>>).Select(x=>x.Key))
            {
                columnListName.Add(item);
            }
            foreach (var item in ((ICollection<KeyValuePair<string, object>>)objList.FirstOrDefault()).Select(x => x.Key))
            {
                columnListName.Add(item);
            }
            foreach (var item in objList)
            {
                var dto = new List<string>();
                foreach (var sModel in (item as ICollection<KeyValuePair<string, object>>).Select(x => x.Value))
                {
                    dto.Add(sModel.ToString());
                    
                }
                dataList.Add(dto);
            }

            ToDataTable(list, "table");
        }

        public static DataTable ToDataTable(List<dynamic> list, string tableName)
        {
            //创建属性的集合    
            List<string> cloumnList = new List<string>();
            DataTable dt = new DataTable(tableName);
            //把所有的public属性加入到集合 并添加DataTable的列    
            foreach (var m in (list.FirstOrDefault() as ExpandoObject).Select(x=>x.Key))
            {
                cloumnList.Add(m);
                dt.Columns.Add(m, typeof(string));//新增列
                Console.WriteLine("d have property {0}.", m);
            }

            foreach (var item in list)
            {
                //创建一个DataRow实例    
                DataRow row = dt.NewRow();
                foreach (var m in (item as ExpandoObject))
                {
                    row[m.Key] = m.Value;
                    Console.WriteLine("d have property {0}.", m);
                }
                //加入到DataTable    
                dt.Rows.Add(row);
            }
            return dt;
        }
    }

    public class ModelXML
    {
        public string Value { get; set; }
    }
}
