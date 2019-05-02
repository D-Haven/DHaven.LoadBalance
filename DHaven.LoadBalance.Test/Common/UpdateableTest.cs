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
using DHaven.LoadBalance.Common;
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test.Common
{
    public class UpdateableTest
    {
        [Fact]
        public void UpdateResetsRequestCount()
        {
            var updateable = new Updateable<Uri>(new Uri("http://foo.com"), 100);
            updateable.Requests.Should().Be(100);
            
            updateable.Update();
            updateable.Requests.Should().Be(0);
        }

        [Fact]
        public void UpdateableCanCompareDirectlyToChildItem()
        {
            var rawItem = new Uri("http://google.com");
            var updateable = new Updateable<Uri>(rawItem);

            updateable.Equals(rawItem).Should().BeTrue();
        }

        [Fact]
        public void TwoUpdateableObjectsCanCompareToEachOther()
        {
            var one = new Updateable<string>("item");
            var two = new Updateable<string>("item");

            one.Equals(two).Should().BeTrue();
            (one == two).Should().BeTrue();
            (one != two).Should().BeFalse();
            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void CompareToNullIsFalse()
        {
            var updateable = new Updateable<string>("fore");

            updateable.Equals((string)null).Should().BeFalse();
            updateable.Equals((Updateable<string>) null).Should().BeFalse();
            (updateable == null).Should().BeFalse();
            (updateable != null).Should().BeTrue();

            object indirect = updateable;
            indirect.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void CompareToSelfIsTrue()
        {
            var updateable = new Updateable<Uri>(new Uri("https://[::1]"));

            updateable.Equals(updateable).Should().BeTrue();
            (updateable == updateable).Should().BeTrue();

            object indirect = updateable;
            indirect.Equals(indirect).Should().BeTrue();
        }
    }
}