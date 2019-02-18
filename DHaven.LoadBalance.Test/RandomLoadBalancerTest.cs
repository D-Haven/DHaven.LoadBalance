using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class RandomLoadBalancerTest
    {
        [Fact]
        public void ReturnsNullIfNoResources()
        {
            var balancer = new RandomLoadBalancer<Uri>();

            balancer.GetResource().Should().BeNull();
        }

        [Fact]
        public void ReturnsOnlyItemIfThatIsAllThereIs()
        {
            var balancer = new RandomLoadBalancer<Uri>();
            balancer.Resources.Add(new Uri("http://test.local"));
            var sameAsFirst = balancer.GetResource();

            foreach (var _ in Enumerable.Range(1, 100))
            {
                balancer.GetResource().Should().Be(sameAsFirst);
            }
        }

        [Fact]
        public void ReturnsRandomEntryWhenMultipleExist()
        {
            var balancer = new RandomLoadBalancer<Uri>();
            balancer.Resources.Add(new Uri("http://one.test"));
            balancer.Resources.Add(new Uri("http://two.test"));
            balancer.Resources.Add(new Uri("http://three.test"));

            var list1 = new List<Uri>();
            var list2 = new List<Uri>();

            foreach (var _ in Enumerable.Range(1, 100))
            {
                list1.Add(balancer.GetResource());
                list2.Add(balancer.GetResource());
            }

            var numSame = list1.Select((t, i) => t.ToString() == list2[i].ToString() ? 1 : 0).Sum();

            numSame.Should().BeLessThan(list1.Count);
        }
    }
}