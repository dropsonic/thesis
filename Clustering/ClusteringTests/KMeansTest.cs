using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thesis.Clustering;
using System.Diagnostics;

namespace ClusteringTests
{
    [TestClass]
    public class KMeansTest
    {
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
        [Timeout(30000)] //чтобы не уходило в бесконечный цикл
        public void ClusterizeTest()
        {
            var kmeans = new KMeans<Point>(Point.Distance, Point.Mean);
            var clusters = kmeans.Clusterize(4, GetData());
            Assert.AreEqual(4, clusters.Count());

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
            var kmeans = new KMeans<Point>(Point.Distance, Point.Mean);
            var clusters = kmeans.Clusterize(7, GetData());
        }
    }
}
