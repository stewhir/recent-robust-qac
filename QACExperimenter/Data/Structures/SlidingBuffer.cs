using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QACExperimenter.Data.Structures
{
    public class SlidingBuffer<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _maxCount;

        public SlidingBuffer(int maxCount)
        {
            _maxCount = maxCount;
            _queue = new Queue<T>(maxCount);
        }

        public void Add(T item)
        {
            if (_queue.Count == _maxCount)
                _queue.Dequeue();
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Retrieve the oldest item in the buffer
        /// </summary>
        /// <returns></returns>
        public T OldestItem()
        {
            return _queue.Peek();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }
    }
}
