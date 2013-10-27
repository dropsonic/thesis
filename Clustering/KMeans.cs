using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Clustering
{
    public class KMeans<T>
    {
        Func<T, T, double> _distanceFunc;

        public KMeans(Func<T, T, double> distanceFunc)
        {
            _distanceFunc = distanceFunc;
        }

        /// <param name="n">Количество кластеров.</param>
        public IList<IList<T>> Clusterize(int n, IList<T> points)
        {
            if (n > points.Count)
                throw new ArgumentException("Количество кластеров не может быть больше количества входных элементов.");

            if (points == null)
                throw new ArgumentNullException("data");



            throw new NotImplementedException();
        }
    }
}
