namespace System.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class PriorityQueue<TValue>
    {
        private readonly List<KeyValuePair<double, object>> data;

        public PriorityQueue()
        {
            this.data = new List<KeyValuePair<double, object>>();
        }

        public bool Contains(double priority)
        {
            return this.data.Any<KeyValuePair<double, object>>(v => (v.Key == priority));
        }

        public TValue DequeueByPriority(double priority)
        {
            foreach (KeyValuePair<double, object> pair in this.data)
            {
                if (pair.Key == priority)
                {
                    this.data.Remove(pair);
                    return (TValue) pair.Value;
                }
            }
            throw new InvalidOperationException(System.Spatial.Strings.PriorityQueueDoesNotContainItem(priority));
        }

        public void Enqueue(double priority, TValue value)
        {
            this.data.Add(new KeyValuePair<double, object>(priority, value));
            this.data.Sort((Comparison<KeyValuePair<double, object>>) ((lhs, rhs) => -lhs.Key.CompareTo(rhs.Key)));
        }

        public TValue Peek()
        {
            if (this.data.Count == 0)
            {
                throw new InvalidOperationException(System.Spatial.Strings.PriorityQueueOperationNotValidOnEmptyQueue);
            }
            KeyValuePair<double, object> pair = this.data[0];
            return (TValue) pair.Value;
        }

        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }
    }
}

