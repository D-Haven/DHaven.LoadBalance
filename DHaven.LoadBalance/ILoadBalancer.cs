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
    /// Generic resource load balancer.  All load balancers have
    /// a set of resources they manage, but the implementations
    /// control how the resources are provided.
    /// </summary>
    /// <typeparam name="T">the type of resource</typeparam>
    public interface ILoadBalancer<T>
    {
        /// <summary>
        /// Gets the list of resources.  Allows clients to add and remove
        /// entries dynamically.
        /// </summary>
        IList<T> Resources { get; }
        
        /// <summary>
        /// Gets the resource for this call.  The exact behavior depends
        /// on the algorithm to retrieve the resource.
        /// </summary>
        /// <returns>the resource to use</returns>
        T GetResource();
    }
}