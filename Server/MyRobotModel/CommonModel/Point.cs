using System.Collections.Generic;
using System.Globalization;

namespace MyRobotModel.CommonModel
{
    /// <summary>
    /// 点对象
    /// </summary>
    public class Point
    {
        public decimal X;
        public decimal Y;

        public Point()
        {
        }
        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }
        public Point(double x, double y)
        {
            decimal.TryParse(x.ToString(CultureInfo.InvariantCulture), out X);
            decimal.TryParse(y.ToString(CultureInfo.InvariantCulture), out Y);
        }
        public Point(string x, string y)
        {
            decimal.TryParse(x, out X);
            decimal.TryParse(y, out Y);
        }
        public static Point SetPoint(string point)
        {
            string[] arrpoint =
            point.Split(',');
            if (arrpoint.Length == 2)
            {
                return new Point(arrpoint[0], arrpoint[1]);
            }
            return null;
        }

        public static List<Point> SetPoints(string strPoints)
        {
            string[] arrpoint = strPoints.Split(';');
            List<Point> points = new List<Point>();
            foreach (var item in arrpoint)
            {
                var point = SetPoint(item);
                if (point != null)
                {
                    points.Add(point);
                }
            }
            return points;
        }
    }
}
