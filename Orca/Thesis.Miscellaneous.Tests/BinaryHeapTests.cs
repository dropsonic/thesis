﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thesis.Collections.Tests
{
    [TestClass]
    public class BinaryHeapTest
    {
        /// <summary>
        /// Checks the main property of the binary heap.
        /// </summary>
        private void IsHeap<T>(BinaryHeap<T> heap) where T : IComparable
        {
            for (int i = 1; i < heap.Count / 2; i++)
            {
                int left = 2 * i;
                int right = 2 * i + 1;
                Assert.IsTrue(heap[i - 1].CompareTo(heap[left - 1]) > 0);
                Assert.IsTrue(heap[i - 1].CompareTo(heap[right - 1]) > 0);
            }
        }

        [TestMethod]
        public void BuildHeapTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);

            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 9, 8, 6, 5, 3, 1, 4 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void AddItemTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);
            heap.Push(7);

            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 9, 8, 6, 7, 3, 1, 4, 5 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void AddFirstItemTest()
        {
            BinaryHeap<int> heap = new BinaryHeap<int>();
            heap.Push(7);

            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 7 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void AddManyItemsTest()
        {
            BinaryHeap<int> heap = new BinaryHeap<int>();
            heap.Push(5);
            heap.Push(3);
            heap.Push(1);
            heap.Push(8);
            heap.Push(9);
            heap.Push(6);
            heap.Push(4);

            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 9, 8, 6, 4, 1, 5, 3 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void ChangeItemTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);
            heap[2] = 7;

            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 9, 8, 7, 5, 3, 1, 4 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void RemoveItemTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);
            heap.Remove(1);

            Assert.AreEqual(6, heap.Count);
            IsHeap(heap);
            List<int> correctHeap = new List<int>() { 9, 8, 6, 5, 3, 4 };
            Assert.IsTrue(correctHeap.SequenceEqual(heap));
        }

        [TestMethod]
        public void PopTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);

            int max = heap.Pop();
            Assert.AreEqual(9, max);
            Assert.AreEqual(6, heap.Count);
            IsHeap(heap);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PopEmptyHeapTest()
        {
            BinaryHeap<int> heap = new BinaryHeap<int>();
            int max = heap.Pop();
        }

        [TestMethod]
        public void PeekTest()
        {
            List<int> data = new List<int>() { 5, 3, 1, 8, 9, 6, 4 };
            BinaryHeap<int> heap = new BinaryHeap<int>(data);

            int max = heap.Peek();
            Assert.AreEqual(9, max);
            Assert.AreEqual(7, heap.Count);
            IsHeap(heap);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PeekEmptyHeapTest()
        {
            BinaryHeap<int> heap = new BinaryHeap<int>();
            int max = heap.Peek();
        }
    }
}
