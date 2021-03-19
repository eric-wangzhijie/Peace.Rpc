using System.Collections.Generic;

namespace Peace.Common
{
    /// <summary>
    /// 线程安全的List
    /// </summary>
    /// <typeparam name="T">列表的类型</typeparam>
    public class ConcurrentList<T>
    {
        /// <summary>
        /// 为了避免内存超出，在这里设置了最大容量，当超过最大容量之后，无法添加进队列，这里是默认的最大容量
        /// </summary>
        public const int DefaultMaxCapacity = 100000;

        private object _lock = new object();
        private List<T> _list = new List<T>();

        /// <summary>
        /// 构造函数，为了避免内存超出，在这里设置了默认的最大容量，当超过最大容量之后，无法添加进队列
        /// </summary>
        public ConcurrentList()
        {
            this.MaxCapacity = DefaultMaxCapacity;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxCapacity">为了避免内存超出，在这里设置了最大容量，当超过最大容量之后，无法添加进队列。如果为0，则表示无限容量</param>
        public ConcurrentList(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
        }

        /// <summary>
        /// 为了避免内存超出，在这里设置了最大容量，当超过最大容量之后，无法添加进队列。如果为0，则表示无限容量
        /// </summary>
        public int MaxCapacity { get; } = 0;

        /// <summary>
        /// 队列中的数量
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _list.Count;
                }
            }
        }

        /// <summary>
        /// 添加一个项
        /// </summary>
        /// <param name="item">要添加的项</param>
        public void Add(T item)
        {
            lock (_lock)
            {
                if (MaxCapacity > 0)
                {
                    if (_list.Count >= MaxCapacity)
                    {
                        return;
                    }
                }
                _list.Add(item);
            }
        }

        /// <summary>
        /// 添加多个项
        /// </summary>
        /// <param name="items">要添加的项目</param>
        public void AddRange(T[] items)
        {
            lock (_lock)
            {
                if (items != null)
                {
                    foreach (T item in items)
                    {
                        if (MaxCapacity > 0)
                        {
                            if (_list.Count >= this.MaxCapacity)
                            {
                                return;
                            }
                        }
                        _list.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 从队列中取出所有的对象，并清空列表
        /// </summary>
        /// <returns>所有的对象</returns>
        public T[] TakeAll()
        {
            lock (_lock)
            {
                T[] array = _list.ToArray();
                _list.Clear();
                return array;
            }
        }

        /// <summary>
        /// 取出头部的一个对象
        /// </summary>
        /// <returns>头部的一个对象</returns>
        public T Take()
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    return default;
                }
                else
                {
                    T t = _list[0];
                    _list.RemoveAt(0);
                    return t;
                }
            }
        }

        /// <summary>
        /// 从队列中取出一定数量的对象
        /// </summary>
        /// <param name="count">要取出的对象的数量</param>
        /// <returns>取出的对象数组</returns>
        public List<T> Take(int count)
        {
            lock (_lock)
            {
                if (_list.Count == 0)
                {
                    return null;
                }
                List<T> result = new List<T>();
                for (int i = 0; i < count; i++)
                {
                    T t = Take();
                    if (t == null)
                    {
                        break;
                    }
                    result.Add(t);
                }
                return result;
            }
        }
    }
}
