using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Clustering
{
    public static class ListExt
    {
        public static IEnumerable<T> Sample<T>(this IList<T> data, int n = 1)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException("n");
            Random rnd = new Random();
            int max = data.Count;
            for (int i = 0; i < n; i++)
                yield return data[rnd.Next(max)];
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
        public IList<IList<T>> Clusterize(int n, IList<T> points)
        {
            if (n > points.Count)
                throw new ArgumentException("Количество кластеров не может быть больше количества входных элементов.");

            if (points == null)
                throw new ArgumentNullException("data");

            //get centers by random
            var centers = points.Sample(n);
            //group by nearest center
            points.GroupBy(p => centers.Min(c => _distanceFunc(c, p)));

            throw new NotImplementedException();
        }
    }
}
