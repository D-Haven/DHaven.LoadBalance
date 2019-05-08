// Licensed to the D-Haven.org under one or more contributor
// license agreements.  See the LICENSE file distributed with 
// this work for additional information regarding copyright
// ownership.  D-Haven.org licenses this file to you under
// the Apache License, Version 2.0 (the "License"); you may
// not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DHaven.LoadBalance.Common
{
    /// <summary>
    /// Internal class to deal with automatically wrapping the updateable and the item being updated.
    /// </summary>
    /// <typeparam name="TExternal"></typeparam>
    /// <typeparam name="TInternal"></typeparam>
    public class ListAdapter<TExternal,TInternal> : IList<TExternal>
    {
        private readonly IList<TInternal> internalList;
        private readonly Func<TExternal, TInternal> wrapper;
        private readonly Func<TInternal, TExternal> unwrapper;
        private readonly Func<TExternal, TInternal, bool> comparer;

        public ListAdapter(Func<TExternal, TInternal> wrapper,
            Func<TInternal,TExternal> unwrapper, 
            Func<TExternal,TInternal,bool> comparer,
            IList<TInternal> adaptedList)
        {
            this.wrapper = wrapper;
            this.unwrapper = unwrapper;
            this.comparer = comparer;
            internalList = adaptedList;
        }
        
        public int Count => internalList.Count;
        public bool IsReadOnly => internalList.IsReadOnly;
        
        public IEnumerator<TExternal> GetEnumerator()
        {
            return new WrappedEnumerator(unwrapper, internalList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TExternal item)
        {
            internalList.Add(wrapper(item));
        }

        public void Clear()
        {
            internalList.Clear();
        }

        public bool Contains(TExternal item)
        {
            return internalList.Any(internalItem => comparer(item, internalItem));
        }

        public void CopyTo(TExternal[] array, int arrayIndex)
        {
            for(var i = 0; i < array.Length && (arrayIndex + i) < Count; i++)
            {
                array[i] = this[arrayIndex + i];
            }
        }

        public bool Remove(TExternal item)
        {
            return internalList.Remove(wrapper(item));
        }

        public int IndexOf(TExternal item)
        {
            return internalList.IndexOf(wrapper(item));
        }

        public TInternal GetInternal(TExternal item)
        {
            return internalList[IndexOf(item)];
        }

        public void Insert(int index, TExternal item)
        {
            internalList.Insert(index, wrapper(item));
        }

        public void RemoveAt(int index)
        {
            internalList.RemoveAt(index);
        }

        public TExternal this[int index]
        {
            get => unwrapper(internalList[index]);
            set => internalList[index] = wrapper(value);
        }

        private class WrappedEnumerator : IEnumerator<TExternal>
        {
            private readonly IEnumerator<TInternal> internalEnumerator;
            private readonly Func<TInternal, TExternal> unwrapper;

            internal WrappedEnumerator(Func<TInternal,TExternal> unwrapper,
                IEnumerator<TInternal> inEnum)
            {
                internalEnumerator = inEnum;
                this.unwrapper = unwrapper;
            }
            
            public bool MoveNext()
            {
                var success = internalEnumerator.MoveNext();
                Current = success ? unwrapper(internalEnumerator.Current) : default(TExternal);
                return success;
            }

            public void Reset()
            {
                internalEnumerator.Reset();
            }

            public TExternal Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                internalEnumerator.Dispose();
            }
        }
    }
}