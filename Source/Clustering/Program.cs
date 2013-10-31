using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Clustering
{
    class Program
    {
        [DebuggerDisplay("X = {X}, Y = {Y}")]
        struct Point
        {
            public double X { get; set; }
            public double Y { get; set; }
            public Point(double x, double y)
                : this()
            {
                this.X = x;
                this.Y = y;
            }
        }

        static readonly Func<Point, Point, double> _distance = (a, b) =>
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        };

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

        static Point[] GetData()
        {
            return new Point[] 
            { 
                new Point(1, 1),
                new Point(1, 1.1),
                new Point(3, 1),
                new Point(2.9, 1),
                new Point(1, 4),
                new Point(4, 4)
            };
        }

        static void Main(string[] args)
        {
            var kmeans = new KMeans<Point>(_distance, _mean);
            var clusters = kmeans.Clusterize(4, GetData());
            foreach (var cluster in clusters)
            {
                foreach (var item in cluster)
                    Console.WriteLine("{0}, {1}", item.X, item.Y);
                Console.WriteLine();
            }
        }
    }
}
