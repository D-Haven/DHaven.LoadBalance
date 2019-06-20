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
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DHaven.LoadBalance.Config;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class BindingHandlerTest
    {
        private static readonly IOptions<LoadBalanceOptions> Options = new TestOptions<LoadBalanceOptions>(new LoadBalanceOptions
        {
            // NOTE: These timeouts are only so tests don't take so long.  They are not recommended for production.
            MaximumTimeout = TimeSpan.FromSeconds(5),
            RetryTimeout = TimeSpan.FromSeconds(.5),
            Map = 
            {
                { "test", new BalancerOptions
                    {
                        Type = BalancerType.RoundRobin,
                        Uris = {new Uri("https://127.0.0.1"),
                            new Uri("https://[::1]") }
                    }
                }
            }
        });
        private readonly BindingMap bindingMap = new BindingMap(Options);

        [Fact]
        public async Task HandlerWillRemapHandledUris()
        {
            var client = new HttpClient(Create(new Uri("https://127.0.0.1/one/two/three")));

            var response = await client.GetAsync("http://test/one/two/three");
            await response.ShouldMatchUri();
        }

        [Fact]
        public async Task HandlerWillNotRemapUnhandledUris()
        {
            var client = new HttpClient(Create(new Uri("http://something/one/two/three")));

            var response = await client.GetAsync("http://something/one/two/three");
            await response.ShouldMatchUri();
        }

        [Fact]
        public async Task HandlerWillRespectMaxTimeout()
        {
            var delayingHandler = new DelayingHandler(TimeSpan.FromSeconds(120))
            {
                InnerHandler = new UriValidatingHandler(new Uri("https://127.0.0.1/one/two/three"))
            };
            
            var client = new HttpClient(new BindingHandler(bindingMap)
            {
                InnerHandler = delayingHandler
            });

            var watch = new Stopwatch();
            watch.Start();
            var response = await client.GetAsync("http://test/one/two/three");
            watch.Stop();
            
            // NOTE: Task.Delay is not super precise, and errors will compound with retry
            watch.Elapsed.Should().BeCloseTo(bindingMap.MaximumTimeout, 
                (int)bindingMap.RetryTimeout.TotalMilliseconds + 200);
            response.StatusCode.Should().BeEquivalentTo(HttpStatusCode.GatewayTimeout);
            delayingHandler.NumberOfInvocations.Should().BeGreaterThan(1);
        }

        private BindingHandler Create(Uri expectedUri)
        {
            return new BindingHandler(bindingMap)
            {
                InnerHandler = new UriValidatingHandler(expectedUri)
            };
        }
    }

    internal class UriValidatingHandler : DelegatingHandler
    {
        public UriValidatingHandler(Uri expectedUri)
        {
            ExpectedUri = expectedUri;
        }
        
        private Uri ExpectedUri { get; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uriMatches = request.RequestUri.Equals(ExpectedUri);
            return Task.FromResult(new HttpResponseMessage(uriMatches
                ? HttpStatusCode.OK
                : HttpStatusCode.NotFound)
            {
                Content = new StringContent($"Expected URI: {ExpectedUri}\nActual URI: {request.RequestUri}")
            });
        }
    }
}