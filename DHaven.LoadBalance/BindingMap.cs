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
using DHaven.LoadBalance.Config;
using Microsoft.Extensions.Options;

namespace DHaven.LoadBalance
{
    /// <inheritdoc />
    /// <summary>
    ///     BindingMap maps host names to a load balancer with named URIs, so they can be easily remapped.
    /// </summary>
    public class BindingMap : Dictionary<string, ILoadBalancer<Uri>>
    {
        private readonly IOptions<LoadBalanceOptions> options;

        /// <summary>
        ///     Default constructor to create a new BindingMap.
        /// </summary>
        public BindingMap()
        {
        }

        /// <summary>
        ///     Support for auto-configuration
        /// </summary>
        /// <param name="options">the options to configure the binding automatically</param>
        public BindingMap(IOptions<LoadBalanceOptions> options)
        {
            this.options = options;

            foreach (var keyVal in options.Value.Map)
            {
                keyVal.Value.CheckValidity();

                var loadBalancer = keyVal.Value.Type == BalancerType.RoundRobin
                    ? (ILoadBalancer<Uri>) new RoundRobinBalancer<Uri>(keyVal.Value.Uris)
                    : new RandomLoadBalancer<Uri>(keyVal.Value.Uris);

                Add(keyVal.Key, loadBalancer);
            }
        }

        /// <summary>
        ///     The maximum amount of time the user will be waiting to resolve a host.  This is the point where
        ///     there is a complete failure.
        /// </summary>
        public TimeSpan MaximumTimeout => options?.Value.MaximumTimeout ?? TimeSpan.MaxValue;

        /// <summary>
        ///     The amount of time before we attempt ot retry a new host.  This must be before the maximum timeout
        ///     to function.
        /// </summary>
        public TimeSpan RetryTimeout => options?.Value.RetryTimeout ?? TimeSpan.MaxValue;

        /// <summary>
        ///     Transforms the URI based on the current load balancing rules configured in this BindingMap.
        ///     Will look up the load balancer using the Host portion of the URI.  Then it will get the next
        ///     URI from the load balancer to create the new URI.
        ///     If there are no entries that match the Host, the URI is returned unmolested.
        /// </summary>
        /// <param name="uriIn">the URI to transform</param>
        /// <returns>a transformed URI</returns>
        /// <exception cref="ArgumentNullException">if the URI is not provided</exception>
        public bool TryRebindUri(Uri uriIn, out Uri uriOut)
        {
            if (uriIn == null) throw new ArgumentNullException(nameof(uriIn));

            uriOut = uriIn;
            var bound = TryGetValue(uriIn.Host, out var loadBalancer);

            if (bound) uriOut = Join(loadBalancer.GetResource(), uriIn.PathAndQuery);

            return bound;
        }

        /// <summary>
        ///     Joins a base URI that also contains a path with a path and query so
        ///     that the two are completely concatenated.  This allows users to configure
        ///     the load balancer to map to proxied API endpoints.
        ///     The default behavior of the concatenated URI constructor is to ignore any
        ///     path information in the base URI.  We need to respect that information.
        /// </summary>
        /// <param name="baseUri">The base URI to concatenate</param>
        /// <param name="relativePathAndQuery">The relative path and query string to append</param>
        /// <returns>a valid URI with the two combined.</returns>
        private static Uri Join(Uri baseUri, string relativePathAndQuery)
        {
            var root = baseUri.AbsoluteUri;
            if (root.EndsWith("/")) root = root.Substring(0, root.Length - 1);

            if (relativePathAndQuery.StartsWith("/"))
                relativePathAndQuery = relativePathAndQuery.Substring(1, relativePathAndQuery.Length - 1);

            return new Uri(string.Join("/", root, relativePathAndQuery));
        }
    }
}