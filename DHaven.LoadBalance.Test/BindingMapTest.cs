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
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class BindingMapTest
    {
        [Fact]
        public void WillIgnoreUrisNotConfigured()
        {            
            var bindingMap = new BindingMap();
            
            var startUri = new Uri("http://test/resource");
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(startUri);
        }

        [Fact]
        public void WillReplaceHostNameWhenConfigured()
        {
            var balancer = new RandomLoadBalancer<Uri>();
            balancer.Resources.Add(new Uri("https://127.0.0.1"));
            var bindingMap = new BindingMap {{"test", balancer}};
            
            var startUri = new Uri("http://test/resource");
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://127.0.0.1/resource"));
        }

        [Fact]
        public void WillUseLoadBalancerRulesAndRespectQueries()
        {
            var balancer = new RoundRobinBalancer<Uri>();
            balancer.Resources.Add(new Uri("https://127.0.0.1"));
            balancer.Resources.Add(new Uri("http://[::1]"));
            balancer.Resources.Add(new Uri("https://www.google.com"));
            var bindingMap = new BindingMap {{"round-robin", balancer}};

            var startUri = new Uri("http://round-robin/more-complex/query?foo=bar&baz=1");
            
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://127.0.0.1/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("http://[::1]/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://www.google.com/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://127.0.0.1/more-complex/query?foo=bar&baz=1"));
        }

        [Fact]
        public void CanUseBaseRoute()
        {
            var balancer = new RoundRobinBalancer<Uri>();
            balancer.Resources.Add(new Uri("https://127.0.0.1/api/round-robin"));
            balancer.Resources.Add(new Uri("http://[::1]/rrobin"));
            balancer.Resources.Add(new Uri("https://www.google.com/api/foo/bar/baz"));
            var bindingMap = new BindingMap {{"round-robin", balancer}};

            var startUri = new Uri("http://round-robin/more-complex/query?foo=bar&baz=1");
            
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://127.0.0.1/api/round-robin/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("http://[::1]/rrobin/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://www.google.com/api/foo/bar/baz/more-complex/query?foo=bar&baz=1"));
            bindingMap.RebindUri(startUri).Should().BeEquivalentTo(new Uri("https://127.0.0.1/api/round-robin/more-complex/query?foo=bar&baz=1"));
        }
    }
}