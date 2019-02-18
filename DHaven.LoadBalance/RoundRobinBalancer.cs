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

using System.Collections.Generic;

namespace DHaven.LoadBalance
{
    /// <summary>
    /// Classic load balancer where fairness is important and there
    /// isn't enough information to be more adaptive.
    /// </summary>
    /// <typeparam name="T">the resource type</typeparam>
    public class RoundRobinBalancer<T> : ILoadBalancer<T>
    {
        private int index = -1;
        
        /// <summary>
        /// Creates a round robin load balancer.
        /// </summary>
        /// <param name="items">the list of resources, creates an empty list if null</param>
        public RoundRobinBalancer(IList<T> items = null)
        {
            Resources = items ?? new List<T>();
        }

        /// <inheritdoc />
        public IList<T> Resources { get; }      
        
        /// <inheritdoc />
        /// <summary>
        /// Gets the next resource in the list.  When the end of the list is reached, will wrap
        /// around to the first item.  This is O(1) complexity.
        /// </summary>
        /// <returns>the next resource</returns>
        public T GetResource()
        {
            if (Resources.Count == 0) return default(T);

            index = (index + 1) % Resources.Count;

            return Resources[index];
        }
    }
}