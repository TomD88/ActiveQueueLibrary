using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ActiveQueueLibrary.SafeCollections
{
    //priority queue that extract the most prioritized element each time
    public class PrioritySafeQueue<T> : IProducerConsumerCollection<T> where T : IPrioritable,IComparable
    {

        private object _lockObject = new object();
        private List<T> _internalList = null;

        public PrioritySafeQueue()
        {
            _internalList = new List<T>();
        }

        public PrioritySafeQueue(IEnumerable<T> collection)
        {
            _internalList = new List<T>(collection);
        }



        public bool TryAdd(T item)
        {
            lock (_lockObject)
            {
                _internalList.Add(item);
                _internalList.Sort();
            }
            return true;
        }

        public bool TryTake(out T item)
        {
            //TryTakeMostPrioritizedItem;
            bool rval = true;
            lock (_lockObject)
            {
                if (_internalList.Count == 0)
                {
                    item = default(T);
                    rval = false;
                }
                else
                {
                    /*
                     int minPriority = _internalList.Min(obj => obj.GetPriority());
                     item = _internalList.Find(obj => obj.GetPriority() == minPriority);
                     _internalList.Remove(item);
                     */

                    //performance improvements with single scanning
                    /*int minIndex = 0;
                    int minValue = Int32.MaxValue;
                    item = _internalList[0];

                    for (int i = 1; i < _internalList.Count; ++i)
                    {
                        if (_internalList[i].GetPriority() < minValue)
                        {
                            minValue = _internalList[i].GetPriority();
                            minIndex = i;
                            item = _internalList[i];
                        }
                    }
                    _internalList.RemoveAt(minIndex);*/

                    item = _internalList[0];
                    _internalList.RemoveAt(0);
                }
            }
            return rval;
        }


        public IEnumerator<T> GetEnumerator()
        {
            List<T> listCopy = null;
            lock (_lockObject) listCopy = new List<T>(_internalList);
            return listCopy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            lock (_lockObject) ((ICollection)_internalList).CopyTo(array, index);
        }


        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { return _lockObject; }
        }

        public int Count
        {
            get { return _internalList.Count; }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (_lockObject) _internalList.CopyTo(array, index);
        }

        public T[] ToArray()
        {
            T[] rval = null;
            lock (_lockObject) rval = _internalList.ToArray();
            return rval;
        }
    }



}
