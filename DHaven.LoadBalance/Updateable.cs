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
using System.Collections.Generic;

namespace DHaven.LoadBalance
{
    public class Updateable<T> : IEquatable<Updateable<T>>, IEquatable<T>
    {
        public Updateable(T item)
        {
            Item = item;
        }
        
        public T Item { get; private set; }
        
        public int Requests { get; internal set; }

        public void Update(T item)
        {
            Item = item;
            Requests = 0;
        }

        public bool Equals(Updateable<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || EqualityComparer<T>.Default.Equals(Item, other.Item);
        }

        public bool Equals(T other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(Item, other) || EqualityComparer<T>.Default.Equals(Item, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Updateable<T>) obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Item);
        }

        public static bool operator ==(Updateable<T> left, Updateable<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Updateable<T> left, Updateable<T> right)
        {
            return !Equals(left, right);
        }
    }
}