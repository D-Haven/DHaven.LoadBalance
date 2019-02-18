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
    /// <summary>
    /// Adaptive Load Balancer works best for a small finite number of resources where
    /// you want to spread the load based on a calculation.  The adaptive load balancer
    /// assumes that lower scores mean more capacity.
    /// </summary>
    /// <typeparam name="T">The type of item to balance</typeparam>
    public class AdaptiveLoadBalancer<T> : ILoadBalancer<T>
    {
        private readonly Func<T,int> calculateScore;

        /// <summary>
        /// Creates an adaptive load balancer with the function that scores the items in the list.
        /// </summary>
        /// <param name="scorer">the scoring function</param>
        /// <param name="items">the list of items to load balance (if null will create an empty list)</param>
        public AdaptiveLoadBalancer(Func<T,int> scorer, IList<T> items = null)
        {
            calculateScore = scorer;
            Resources = items ?? new List<T>();
        }

        /// <inheritdoc />
        public IList<T> Resources { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the next most available resource.  The algorithm in O(n), since every resource needs
        /// to be scored.  If multiple entries have the same lowest score, the first one is returned.
        /// </summary>
        /// <returns>the most available resource</returns>
        public T GetResource()
        {
            if (Resources.Count == 0) return default(T);

            var lowestScore = int.MaxValue;
            var bestEntry = default(T);

            foreach (var item in Resources)
            {
                var score = calculateScore(item);

                if (score < lowestScore)
                {
                    lowestScore = score;
                    bestEntry = item;
                }
            }

            return bestEntry;
        }
    }
}