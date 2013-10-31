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
            var kmeans = new KMeans<Point>(Point.Distance, Point.Mean);
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
