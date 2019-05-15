// Licensed to the D-Haven.org under one or more contributor
// license agreements.  See the LICENSE file distributed with
// this work for additional information regarding copyright
// ownership.  D-Haven.org licenses this file to you under
// the Apache License, Version 2.0 (the "License"); you may
// not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DHaven.LoadBalance.Config
{
    public class BindingRegistrar
    {
        protected internal BindingMap Bindings { get; } = new BindingMap();

        public void RegisterRandom(string service, params Uri[] balancedUris)
        {
            CheckUris(nameof(balancedUris), balancedUris);
            
            Register(service, new RandomLoadBalancer<Uri>(balancedUris));
        }

        public void RegisterRoundRobin(string service, params Uri[] balancedUris)
        {
            CheckUris(nameof(balancedUris), balancedUris);
            
            Register(service, new RoundRobinBalancer<Uri>(balancedUris));
        }
        
        public void RegisterAdaptive(string service, Func<Uri,int> uriScorer, params Uri[] balancedUris)
        {
            if(uriScorer == null) throw new ArgumentNullException(nameof(uriScorer));
            CheckUris(nameof(balancedUris), balancedUris);
            
            Register(service, new AdaptiveLoadBalancer<Uri>(uriScorer, balancedUris));
        }

        public void RegisterUpdateableAdaptive(string service, Func<Uri,int> uriScorer, params Uri[] balancedUris)
        {
            if(uriScorer == null) throw new ArgumentNullException(nameof(uriScorer));
            CheckUris(nameof(balancedUris), balancedUris);

            Register(service, new UpdateableAdaptiveLoadBalancer<Uri>(uriScorer, balancedUris));
        }

        private void Register(string service, ILoadBalancer<Uri> loadBalancer)
        {
            if(string.IsNullOrEmpty(service)) throw new ArgumentNullException(nameof(service));

            if (Bindings.ContainsKey(service))
            {
                throw new ArgumentException($"Service name is already registered: {service}");
            }
            
            Bindings.Add(service, loadBalancer);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void CheckUris(string parameter, Uri[] uris)
        {
            if(uris.Length == 0) throw new ArgumentException("You must provide at least one URI to load balance", parameter);
        }
    }
}