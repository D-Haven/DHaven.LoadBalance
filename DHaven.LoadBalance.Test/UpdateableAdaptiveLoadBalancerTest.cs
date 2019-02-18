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
    public class UpdateableAdaptiveLoadBalancerTest
    {
        [Fact]
        public void NothingReturnedWhenNoEntries()
        {
            var balancer = new UpdateableAdaptiveLoadBalancer<Scoreable>(s => (int) (s.PercentMemoryUsec * 100));

            balancer.GetResource().Should().BeNull();
        }

        [Fact]
        public void OnlyItemReturnedEveryTime()
        {
            var balancer = new UpdateableAdaptiveLoadBalancer<Scoreable>(s => (int) (s.PercentMemoryUsec * 100));
            balancer.Resources.Add(new Scoreable(new Uri("http://only.one"))
            {
                PercentMemoryUsec = .75
            });
            
            var sameAsFirst = balancer.GetResource();
            sameAsFirst.Should().NotBeNull();

            foreach (var _ in Enumerable.Range(1, 100))
            {
                balancer.GetResource().Should().Be(sameAsFirst);
            }
        }

        [Fact]
        public void ReturnsBasedOnScoringFunction()
        {
            var balancer = new UpdateableAdaptiveLoadBalancer<Scoreable>(s => (int) (s.PercentMemoryUsec * 100));
            balancer.Resources.Add(new Scoreable(new Uri("http://base.one"))
            {
                PercentMemoryUsec = .25
            });
            balancer.Resources.Add(new Scoreable(new Uri("http://base.two"))
            {
                PercentMemoryUsec = .75
            });
            balancer.Resources.Add(new Scoreable(new Uri("http://base.three"))
            {
                PercentMemoryUsec = .5
            });

            var item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.one"));
            item.PercentMemoryUsec = .8;

            item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.three"));

            balancer.GetResource().Should().BeSameAs(item);
        }

        [Fact]
        public void ReturnsBasedOnRequests()
        {
            var balancer = new UpdateableAdaptiveLoadBalancer<Scoreable>(s => (int) (s.PercentMemoryUsec * 100));
            balancer.Resources.Add(new Scoreable(new Uri("http://base.one"))
            {
                PercentMemoryUsec = .25
            });
            balancer.Resources.Add(new Scoreable(new Uri("http://base.two"))
            {
                PercentMemoryUsec = .25
            });
            balancer.Resources.Add(new Scoreable(new Uri("http://base.three"))
            {
                PercentMemoryUsec = .5
            });

            var item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.one"));

            item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.two"));

            item = balancer.GetResource();
            item.Uri.Should().Be(new Uri("http://base.one"));

            balancer.Update(item);
            balancer.GetResource().Should().BeSameAs(item);
        }
        
        class Scoreable : IEquatable<Scoreable>
        {
            public Scoreable(Uri key)
            {
                Uri = key;
            }
            
            public Uri Uri { get; }
            public double PercentMemoryUsec { get; set; }

            public bool Equals(Scoreable other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Uri.Equals(other.Uri);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is Scoreable other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Uri.GetHashCode();
            }

            public static bool operator ==(Scoreable left, Scoreable right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Scoreable left, Scoreable right)
            {
                return !Equals(left, right);
            }
        }
    }
}