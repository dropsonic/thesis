using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace Thesis.Clustering
{
    public static class ListExt
    {
        public static IEnumerable<T> Sample<T>(this IList<T> data, int n = 1)
        {
            if (data == null)
                throw new ArgumentNullException();

            int count = data.Count();
            int i = 0;
            if (n < 1 || n > count)
                throw new ArgumentOutOfRangeException("n");
            Random rnd = new Random();

            double p = (double)n / (double)count;
            foreach (var item in data)
            {
                if (rnd.NextDouble() < p)
                {
                    count--;
                    i++;
                    p = (double)n / (double)count;
                    yield return item;
                }
                if (i == n)
                    yield break;
            }
        }
    }

    public class KMeans<T>
    {
        Func<T, T, double> _distanceFunc;
        Func<IEnumerable<T>, T> _meanFunc;

        public KMeans(Func<T, T, double> distanceFunc, Func<IEnumerable<T>, T> meanFunc)
        {
            if (distanceFunc == null)
                throw new ArgumentNullException("distanceFunc");
            if (meanFunc == null)
                throw new ArgumentNullException("meanFunc");
            _distanceFunc = distanceFunc;
            _meanFunc = meanFunc;
        }

        /// <param name="n">Количество кластеров.</param>
        public IEnumerable<IGrouping<T, T>> Clusterize(int n, IList<T> points)
        {
            if (n > points.Count)
                throw new ArgumentException("Количество кластеров не может быть больше количества входных элементов.");

            if (points == null)
                throw new ArgumentNullException("data");

            //get centers by random
            var newCenters = points.Sample(n);
            IEnumerable<T> centers;
            IEnumerable<IGrouping<T, T>> clusters;
            do
            {
                centers = newCenters;
                //group by nearest center
                clusters = points.GroupBy(p => centers.MinBy(c => _distanceFunc(c, p)));
                //new centers are means of the clusters
                newCenters = clusters.Select(cl => _meanFunc(cl));
            } 
            while (!centers.SequenceEqual(newCenters));

            return clusters;
        }
    }
}
