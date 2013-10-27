using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thesis.Clustering;

namespace ClusteringTests
{
    [TestClass]
    public class KMeansTest
    {
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

        readonly Func<Point, Point, double> _distance = (a, b) =>
                {
                    return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
                };

        readonly Func<IEnumerable<Point>, Point> _mean = (items) =>
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

        Point[] GetData()
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

        [TestMethod]
        public void ClusterizeTest()
        {
            var kmeans = new KMeans<Point>(_distance, _mean);
            var clusters = kmeans.Clusterize(4, GetData());
            Assert.AreEqual(4, clusters.Count);

            Assert.IsTrue(clusters.Any(items => items.Any(p => p.X == 1 && p.Y == 1) 
                && items.Any(p => p.X == 1 && p.Y == 1.1)));

            Assert.IsTrue(clusters.Any(items => items.Any(p => p.X == 3 && p.Y == 1)
                && items.Any(p => p.X == 2.9 && p.Y == 1)));

            Assert.IsTrue(clusters.Any(items => items.Any(p => p.X == 1 && p.Y == 4)));

            Assert.IsTrue(clusters.Any(items => items.Any(p => p.X == 4 && p.Y == 4)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ClusterizeArgTest()
        {
            var kmeans = new KMeans<Point>(_distance, _mean);
            var clusters = kmeans.Clusterize(7, GetData());
        }
    }
}
