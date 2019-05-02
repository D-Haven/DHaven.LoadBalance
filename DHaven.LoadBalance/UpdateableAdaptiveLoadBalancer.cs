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
using DHaven.LoadBalance.Common;

namespace DHaven.LoadBalance
{
    /// <summary>
    /// Adaptive Load Balancer works best for a small finite number of resources where
    /// you want to spread the load based on a calculation.  The adaptive load balancer
    /// assumes that lower scores mean more capacity.  Assumes any entries are updated
    /// periodically, and adds weight the more an item is requested before an update
    /// happens.
    /// </summary>
    /// <typeparam name="T">The type of item to balance</typeparam>
    public class UpdateableAdaptiveLoadBalancer<T> : ILoadBalancer<T>
    {
        private readonly AdaptiveLoadBalancer<Updateable<T>> adaptiveBalancer;

        /// <summary>
        /// Creates an adaptive load balancer with the function that scores the items in the list.
        /// </summary>
        /// <param name="scorer">the scoring function</param>
        /// <param name="items">the list of items to load balance (if null will create an empty list)</param>
        public UpdateableAdaptiveLoadBalancer(Func<T, int> scorer, IList<T> items = null)
        {
            adaptiveBalancer = new AdaptiveLoadBalancer<Updateable<T>>(Adapt(scorer));
            Resources = new ListAdapter<T, Updateable<T>>(
                external => new Updateable<T>(external),
                intItem => intItem.Item,
                (extItem,intItem) => extItem.Equals(intItem.Item),
                adaptiveBalancer.Resources);

            if (items == null) return;
            
            foreach (var val in items)
            {
                Resources.Add(val);
            }
        }

        public IList<T> Resources { get; }

        public T GetResource()
        {
            var resource = adaptiveBalancer.GetResource();
            if (resource == null) return default(T);
            
            resource.Requests++;
            return resource.Item;
        }

        public void Update(T item)
        {
            var internalItem = ((ListAdapter<T,Updateable<T>>) Resources).GetInternal(item);
            internalItem?.Update();
        }

        private static Func<Updateable<T>, int> Adapt(Func<T, int> inputScorer)
        {
            return updateable => inputScorer(updateable.Item) + updateable.Requests;
        }
    }
}