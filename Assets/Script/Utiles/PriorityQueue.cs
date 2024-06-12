//==================================
// Author : Cha Kyoung Hoon
// Create : 2015-7-23
// Last Modify : 2015-7-27
// Notes : Made for A* algorithm
//==================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum PRIORITY_SORT_TYPE { ASCENDING, DESCENDING };

public class PriorityQueue<T> : ICollection<T> where T : IComparable<T>
{
    private readonly List<T> items = new List<T>();

    public int Count { get { return items.Count; } }
    public bool IsReadOnly { get { return false; } }
    public PRIORITY_SORT_TYPE SortType { get; private set; }
    public T this[int index] { get { return items[index]; } }

    public PriorityQueue()
    {
        SortType = PRIORITY_SORT_TYPE.ASCENDING;
    }

    public PriorityQueue(PRIORITY_SORT_TYPE sortType)
    {
        SortType = sortType;
    }

    public void Add(T item)
    {
        items.Add(item);

        int index = items.Count - 1;
        bool keepGoing = true;

        while (keepGoing)
        {
            if (index == 0) break;
            int parentIndex = (index - 1) / 2;
            keepGoing = CompareAndSwap(index, parentIndex);
            if (keepGoing) index = parentIndex;
        }
    }

    public void Clear()
    {
        items.Clear();
    }

    public bool Contains(T item)
    {
        return items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        items.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return RemoveAt(items.IndexOf(item));
    }

    public bool RemoveAt(int index)
    {
        if (index < 0) return false;

        if (index == (items.Count - 1))
        {
            items.RemoveAt(index);
            return true;
        }

        items[index] = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);

        bool keepGoing = true;

        while (keepGoing)
        {
            int childIndex = GetPrimaryChildIndex(index);
            if (childIndex < 0) break;
            keepGoing = CompareAndSwap(childIndex, index);
            if (keepGoing) index = childIndex;
        }

        return true;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public T Pop()
    {
        T item = default(T);

        if (items.Count > 0)
        {
            item = items[0];
            RemoveAt(0);
        }

        return item;
    }

    public T Peek()
    {
        return (items.Count > 0) ? items[0] : default(T);
    }

    bool CompareAndSwap(int indexA, int indexB)
    {
        int result = items[indexA].CompareTo(items[indexB]);

        if (SortType == PRIORITY_SORT_TYPE.ASCENDING)
        {
            if (result < 0)
            {
                Swap(indexA, indexB);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (result > 0)
            {
                Swap(indexA, indexB);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    void Swap(int indexA, int indexB)
    {
        T temp = items[indexB];
        items[indexB] = items[indexA];
        items[indexA] = temp;
    }

    int GetPrimaryChildIndex(int parentIndex)
    {
        int primaryChildIndex;
        int leftChildIndex = ((parentIndex + 1) * 2) - 1;
        int rightChildIndex = (parentIndex + 1) * 2;

        if ((leftChildIndex < items.Count) && (rightChildIndex < items.Count))
        {
            int result = items[leftChildIndex].CompareTo(items[rightChildIndex]);

            if (SortType == PRIORITY_SORT_TYPE.ASCENDING)
            {
                if (result < 0)
                {
                    primaryChildIndex = leftChildIndex;
                }
                else
                {
                    primaryChildIndex = rightChildIndex;
                }
            }
            else
            {
                if (result > 0)
                {
                    primaryChildIndex = leftChildIndex;
                }
                else
                {
                    primaryChildIndex = rightChildIndex;
                }
            }
        }
        else
        {
            if (leftChildIndex < items.Count)
            {
                primaryChildIndex = leftChildIndex;
            }
            else if (rightChildIndex < items.Count)
            {
                primaryChildIndex = rightChildIndex;
            }
            else
            {
                primaryChildIndex = -1;
            }
        }

        return primaryChildIndex;
    }
}