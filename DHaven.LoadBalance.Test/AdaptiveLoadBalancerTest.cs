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
    public class AdaptiveLoadBalancerTest
    {
        [Fact]
        public void NothingReturnedWhenNoEntries()
        {
            var balancer = new AdaptiveLoadBalancer<Scoreable>(s => (int) (1 - s.PercentMemoryLeft * 100));

            balancer.GetResource().Should().BeNull();
        }

        [Fact]
        public void OnlyItemReturnedEveryTime()
        {
            var balancer = new AdaptiveLoadBalancer<Scoreable>(s => (int) (1 - s.PercentMemoryLeft * 100));
            balancer.Resources.Add(new Scoreable
            {
                Uri = new Uri("http://only.one"),
                PercentMemoryLeft = .25
            });
            
            var sameAsFirst = balancer.GetResource();

            foreach (var _ in Enumerable.Range(1, 100))
            {
                balancer.GetResource().Should().Be(sameAsFirst);
            }
        }

        [Fact]
        public void ReturnsBasedOnScoringFunction()
        {
            var balancer = new AdaptiveLoadBalancer<Scoreable>(s => (int) (1 - s.PercentMemoryLeft * 100));
            balancer.Resources.Add(new Scoreable
            {
                Uri = new Uri("http://base.one"),
                PercentMemoryLeft = .25
            });
            balancer.Resources.Add(new Scoreable
            {
                Uri = new Uri("http://base.two"),
                PercentMemoryLeft = .75
            });
            balancer.Resources.Add(new Scoreable
            {
                Uri = new Uri("http://base.three"),
                PercentMemoryLeft = .5
            });

            var item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.two"));
            item.PercentMemoryLeft = .1;

            item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.three"));

            balancer.GetResource().Should().BeSameAs(item);
        }

        class Scoreable
        {
            public Uri Uri { get; set; }
            public double PercentMemoryLeft { get; set; }
        }
    }
}