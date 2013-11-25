using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Collections
{
    public class BinaryHeap<T> : ICollection, IEnumerable<T> where T : IComparable
    {
        private const int _defaultCapacity = 4;

        private List<T> _items;

        public BinaryHeap()
            : this(_defaultCapacity) { }

        public BinaryHeap(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");
            _items = new List<T>(capacity);
        }

        public BinaryHeap(IEnumerable<T> collection)
        {
            _items = new List<T>(collection);
            BuildHeap(_items);
        }

        private static void SwapElements(IList<T> list, int a, int b)
        {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        private static void BuildHeap(IList<T> a)
        {
            for (int i = a.Count / 2; i > 0; i--)
                Heapify(a, i);
        }

        private static void Heapify(IList<T> a, int i)
        {
            int left = 2 * i;
            int right = 2 * i + 1;
            int largest = i;

            if (left <= a.Count && a[left - 1].CompareTo(a[i - 1]) > 0)
                largest = left;
            if (right <= a.Count && a[right - 1].CompareTo(a[largest - 1]) > 0)
                largest = right;

            if (largest != i)
            {
                SwapElements(a, i - 1, largest - 1);
                Heapify(a, largest);
            }
        }

        private static void HeapIncreaseKey(IList<T> a, int i, T key)
        {
            a[i - 1] = key;

            for (int j = i; j > 1 && a[j / 2 - 1].CompareTo(a[j - 1]) < 0; j = j / 2)
                SwapElements(a, j - 1, j / 2 - 1);

            //for (int j = i; j > 0 && a[j].CompareTo(a[j - 1]) > 0; j--)
            //    SwapElements(a, j, j - 1);
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public T this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                HeapIncreaseKey(_items, index + 1, value);
            }
        }

        public void Push(T item)
        {
            _items.Add(item);
            HeapIncreaseKey(_items, _items.Count, item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Removes and returns max element of the heap.
        /// </summary>
        /// <returns>Max element of the heap</returns>
        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty.");
            
            T max = this[0];
            if (Count == 1)
            {
                _items.Remove(max);
                return max;
            }

            T last = _items[_items.Count - 1];
            _items.Remove(last);
            _items[0] = last;
            Heapify(_items, 1);
            return max;
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty.");
            return this[0];
        }

        #region ICollection
        public int Count
        {
            get { return _items.Count; }
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }
        #endregion
    }
}
