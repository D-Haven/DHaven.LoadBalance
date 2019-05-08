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
using System.Collections.Generic;
using DHaven.LoadBalance.Common;
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test.Common
{
    public class ListAdapterTest
    {
        [Fact]
        public void AddingValueWrapsItInOtherList()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                wrappedList)
                {"http://maps.google.com"};


            destList.Count.Should().Be(wrappedList.Count);
            destList.IsReadOnly.Should().Be(((IList<Uri>)wrappedList).IsReadOnly);
            wrappedList[0].Should().BeOfType<Uri>();
            destList[0].Should().Be(wrappedList[0].ToString());
        }

        [Fact]
        public void ChangingValueInListElementChangesOtherList()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                    str => new Uri(str),
                    uri => uri.ToString(),
                    (str, uri) => string.Equals(str, uri?.ToString()),
                    wrappedList)
                {"http://maps.google.com"};

            destList[0] = "https://www.facebook.com";
            
            wrappedList[0].Should().BeEquivalentTo(new Uri("https://www.facebook.com"));
        }

        [Fact]
        public void ClearAffectsBothLists()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                    str => new Uri(str),
                    uri => uri.ToString(),
                    (str, uri) => string.Equals(str, uri?.ToString()),
                    wrappedList)
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            wrappedList.Count.Should().Be(2);
            
            destList.Clear();

            wrappedList.Count.Should().Be(0);
            destList.Count.Should().Be(0);
        }

        [Fact]
        public void RemoveAtRemovesItemByIndex()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                    str => new Uri(str),
                    uri => uri.ToString(),
                    (str, uri) => string.Equals(str, uri?.ToString()),
                    wrappedList)
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            wrappedList.Count.Should().Be(2);
            
            destList.RemoveAt(0);

            wrappedList.Count.Should().Be(1);
            wrappedList[0].ToString().Should().Be("http://www.twitter.com/");
            destList[0].Should().Be(wrappedList[0].ToString());
        }
 
        [Fact]
        public void RemoveRemovesItemByValue()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                wrappedList)
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            wrappedList.Count.Should().Be(2);
            
            destList.Remove("http://www.twitter.com");

            wrappedList.Count.Should().Be(1);
            wrappedList[0].ToString().Should().Be("http://maps.google.com/");
            destList[0].Should().Be(wrappedList[0].ToString());
        }

        [Fact]
        public void ContainsWorksAsExpected()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                wrappedList)
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            destList.Contains("http://www.twitter.com/").Should().BeTrue();
            wrappedList.Contains(new Uri("http://www.twitter.com/")).Should().BeTrue();
            destList.Contains("mailto:null@bitbucket.com").Should().BeFalse();
        }

        [Fact]
        public void CanInsertIntoMiddleOfList()
        {
            var wrappedList = new List<Uri>();
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                wrappedList)
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            wrappedList.Count.Should().Be(2);
            
            destList.Insert(1, "https://drive.google.com");

            wrappedList.Count.Should().Be(3);
            destList.Count.Should().Be(3);
            
            wrappedList[1].Should().BeEquivalentTo(new Uri("https://drive.google.com"));
            wrappedList[2].Should().BeEquivalentTo(new Uri("http://www.twitter.com"));
        }

        [Fact]
        public void CanCopyToArray()
        {
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                new List<Uri>())
            {
                "http://maps.google.com",
                "http://www.twitter.com"
            };

            var array = new string[2];
            destList.CopyTo(array, 0);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].Should().Be(destList[i]);
            }
        }
 
        [Fact]
        public void CopyToArrayRespectsStartIndex()
        {
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                new List<Uri>())
            {
                "http://maps.google.com",
                "http://www.twitter.com",
                "http://localhost/one",
                "http://localhost/two",
                "http://localhost/three"
            };

            var array = new string[3];
            destList.CopyTo(array, 2);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].Should().Be(destList[i + 2]);
            }
        }
  
        [Fact]
        public void CopyToArrayWillStopWhenDestinationIsFull()
        {
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                new List<Uri>())
            {
                "http://maps.google.com",
                "http://www.twitter.com",
                "http://localhost/one",
                "http://localhost/two",
                "http://localhost/three"
            };

            var array = new string[2];
            destList.CopyTo(array, 2);

            for (var i = 0; i < array.Length; i++)
            {
                array[i].Should().Be(destList[i + 2]);
            }
        }
  
        [Fact]
        public void CopyToArrayWillStopWhenNoMoreItemsExist()
        {
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                new List<Uri>())
            {
                "http://maps.google.com",
                "http://www.twitter.com",
                "http://localhost/one",
                "http://localhost/two",
                "http://localhost/three"
            };

            var array = new string[10];
            destList.CopyTo(array, 2);

            for (var i = 0; i < array.Length; i++)
            {
                if (i < 3)
                {
                    array[i].Should().Be(destList[i + 2]);
                }
                else
                {
                    array[i].Should().BeNull();
                }
            }
        }

        [Fact]
        public void CanEnumerateItems()
        {
            var destList = new ListAdapter<string, Uri>(
                str => new Uri(str),
                uri => uri.ToString(),
                (str, uri) => string.Equals(str, uri?.ToString()),
                new List<Uri>())
            {
                "http://maps.google.com",
                "http://www.twitter.com",
                "http://localhost/one",
                "http://localhost/two",
                "http://localhost/three"
            };

            var count = 0;
            foreach (var item in destList)
            {
                destList.Contains(item).Should().BeTrue();
                count++;
            }

            count.Should().Be(destList.Count);
        }
    }
}