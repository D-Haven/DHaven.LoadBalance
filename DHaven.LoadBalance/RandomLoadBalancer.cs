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
    /// Best used when fairness is not a concern and you really don't care which is used.
    /// </summary>
    /// <typeparam name="T">the type of resource to return</typeparam>
    public class RandomLoadBalancer<T> : ILoadBalancer<T>
    {
        private readonly Random random = new Random();

        /// <summary>
        /// Creates a RandomLoadBalancer.
        /// </summary>
        /// <param name="items">the list of resources to balance, if null will create a list</param>
        public RandomLoadBalancer(IList<T> items = null)
        {
            Resources = items ?? new List<T>();
        }

        /// <inheritdoc />
        public IList<T> Resources { get; }

        /// <inheritdoc />
        /// <summary>
        /// Gets the next random resource.  This function is O(1) complexity.
        /// </summary>
        /// <returns>a random entry</returns>
        public T GetResource()
        {
            return Resources.Count == 0 ? default(T) : Resources[random.Next(Resources.Count)];
        }
    }
}