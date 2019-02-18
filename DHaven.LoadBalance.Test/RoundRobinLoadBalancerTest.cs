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
using System.Linq;
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class RoundRobinLoadBalancerTest
    {
        [Fact]
        public void NothingReturnedWhenNoEntries()
        {
            var balancer = new RoundRobinBalancer<Uri>();

            balancer.GetResource().Should().BeNull();
        }

        [Fact]
        public void OnlyItemReturnedEveryTime()
        {
            var balancer = new RoundRobinBalancer<Uri>();
            balancer.Resources.Add(new Uri("http://test.local"));
            var sameAsFirst = balancer.GetResource();

            foreach (var _ in Enumerable.Range(1, 100))
            {
                balancer.GetResource().Should().Be(sameAsFirst);
            }
        }

        [Fact]
        public void ReturnsItemsInOrderWrappingAroundToFirst()
        {
            var balancer = new RoundRobinBalancer<Uri>();
            balancer.Resources.Add(new Uri("http://one.test"));
            balancer.Resources.Add(new Uri("http://two.test"));
            balancer.Resources.Add(new Uri("http://three.test"));

            balancer.GetResource().Should().Be(new Uri("http://one.test"));
            balancer.GetResource().Should().Be(new Uri("http://two.test"));
            balancer.GetResource().Should().Be(new Uri("http://three.test"));
            balancer.GetResource().Should().Be(new Uri("http://one.test"));
            balancer.GetResource().Should().Be(new Uri("http://two.test"));
            balancer.GetResource().Should().Be(new Uri("http://three.test"));
        }
    }
}