using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Clustering
{
    /// <summary>
    /// For test puprose only.
    /// </summary>
    [DebuggerDisplay("X = {X}, Y = {Y}")]
    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        static readonly Func<Point, Point, double> _distance = (a, b) =>
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        };

        public static Func<Point, Point, double> Distance
        {
            get { return _distance; }
        }

        static readonly Func<IEnumerable<Point>, Point> _mean = (items) =>
        {
            var result = new Point(0, 0);
            foreach (var x in items)
            {
                result.X += x.X;
                result.Y += x.Y;
            }
            int n = items.Count();
            result.X /= n;
            result.Y /= n;
            return result;
        };

        public static Func<IEnumerable<Point>, Point> Mean
        {
            get { return _mean; }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }
    }
}
