/************************************************************************
 * 文件名：TimeHelper
 * 文件功能描述：对时间操作的方法集合
 * 作    者：  韩俊俊
 * 创建日期：2017年6月14日15:00:00
 * 修 改 人：
 * 修改日期：
 * 修改原因：
 * Copyright (c) HanJunJun. All Rights Reserved. 
 * ***********************************************************************/
using System;

namespace Infrastructure
{
    public class TimeHelper
    {
        #region 获取自1970-01-01到当前时间经过的毫秒数
        /// <summary>
        /// 获取自1970-01-01到当前时间经过的毫秒数
        /// </summary>
        /// <param name="time">当前时间</param>
        /// <returns></returns>
        public static long GetDateTimeLongFrom1970ToNow(DateTime time)
        {
            try
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                return (long)(time - startTime).TotalMilliseconds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region 把long型毫秒数转换为DateTime
        public static DateTime GetSecondsToDateTime(long totalMilliseconds)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddMilliseconds(totalMilliseconds);
            return dt;
        }
        #endregion

        #region 取得某月的第一天
        /// <summary>
        /// 取得某月的第一天
        /// </summary>
        /// <param name="datetime">要取得月份第一天的时间</param>
        /// <returns></returns>
        private static DateTime FirstDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day);
        }
        #endregion

        #region 取得某月的最后一天
        /// <summary>
        /// 取得某月的最后一天
        /// </summary>
        /// <param name="datetime">要取得月份最后一天的时间</param>
        /// <returns></returns>
        private static DateTime LastDayOfMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(1).AddDays(-1);
        }
        #endregion

        #region 取得上个月第一天
        /// <summary>
        /// 取得上个月第一天
        /// </summary>
        /// <param name="datetime">要取得上个月第一天的当前时间</param>
        /// <returns></returns>
        private static DateTime FirstDayOfPreviousMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddMonths(-1);
        }
        #endregion

        #region 取得上个月的最后一天
        /// <summary>
        /// 取得上个月的最后一天
        /// </summary>
        /// <param name="datetime">要取得上个月最后一天的当前时间</param>
        /// <returns></returns>
        private static DateTime LastDayOfPrdviousMonth(DateTime datetime)
        {
            return datetime.AddDays(1 - datetime.Day).AddDays(-1);
        }
        #endregion

        #region 获取时间段之间的时长
        /// <summary>
        /// 获取时间段之间的时长
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="mode">1：天时分秒 2：天时分</param>
        /// <returns></returns>
        public static string GetTimeLong(DateTime? startTime,DateTime? endTime,int mode)
        {
            string resuleTime = string.Empty;
            if (startTime == null || endTime == null)
            {
                return "";
            }
            double UseTime = (endTime.Value.Ticks - startTime.Value.Ticks) / 10000000;
            var t1 = endTime - startTime;
            if (UseTime >= 0 && UseTime < 60)
            {
                //秒
                resuleTime = Convert.ToString(t1.Value.Seconds) + "秒";
            }
            else if (UseTime >= 60 && UseTime < 3600)
            {
                //分钟
                if (mode == 1)
                {
                    resuleTime = Convert.ToString(t1.Value.Minutes) + "分钟" + Convert.ToString(t1.Value.Seconds) + "秒";
                }
                else if (mode == 2)
                {
                    resuleTime = Convert.ToString(t1.Value.Minutes) + "分钟";
                }
                
            }
            else if (UseTime >= 3600 && UseTime<86400)
            {
                //小时
                if (mode == 1)
                {
                    resuleTime = Convert.ToString(t1.Value.Hours) + "小时" + Convert.ToString(t1.Value.Minutes) + "分钟" + Convert.ToString(t1.Value.Seconds) + "秒"; ;
                }
                else if (mode == 2)
                {
                    resuleTime = Convert.ToString(t1.Value.Hours) + "小时" + Convert.ToString(t1.Value.Minutes) + "分钟";
                }
                
            }
            else if(UseTime>=86400)
            {
                if (mode == 1)
                {
                    resuleTime = Convert.ToString(t1.Value.Days) + "天" + Convert.ToString(t1.Value.Hours) + "小时" + Convert.ToString(t1.Value.Minutes) + "分钟" + Convert.ToString(t1.Value.Seconds) + "秒"; ;
                }
                else if (mode == 2)
                {
                    resuleTime = Convert.ToString(t1.Value.Days) + "天" + Convert.ToString(t1.Value.Hours) + "小时";
                }
                
            }
            return resuleTime;
        }
        #endregion
    }
}
