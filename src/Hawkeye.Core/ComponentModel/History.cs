using System;
using System.Collections.Generic;

namespace Hawkeye.ComponentModel
{
    /// <summary>
    ///     Represents a navigation history list.
    /// </summary>
    internal class History<T>
    {
        private int _index = -1;
        private readonly List<T> _list = new List<T>();

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        /// <exception cref="InvalidOperationException"></exception>
        public T Current
        {
            get
            {
                if (_index == -1)
                {
                    return default(T);
                }

                if (_index >= _list.Count)
                {
                    throw new InvalidOperationException();
                }

                return _list[_index];
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _list.Count;

        /// <summary>
        /// Gets a value indicating whether this instance has previous.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has previous; otherwise, <c>false</c>.
        /// </value>
        public bool HasPrevious => _index > 0;

        /// <summary>
        /// Gets a value indicating whether this instance has next.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has next; otherwise, <c>false</c>.
        /// </value>
        public bool HasNext => _index < _list.Count - 1;

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _list.Clear();
            _index = -1;
        }

        /// <summary>
        /// Pushes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Push(T item)
        {
            _list.Add(item);
            _index = _list.Count - 1;
        }

        /// <summary>
        /// Moves to previous.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void MoveToPrevious()
        {
            if (!HasPrevious)
            {
                throw new InvalidOperationException();
            }

            _index--;
        }

        /// <summary>
        /// Moves to next.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void MoveToNext()
        {
            if (!HasNext)
            {
                throw new InvalidOperationException();
            }

            _index++;
        }
    }
}