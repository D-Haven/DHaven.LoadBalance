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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class BindingHandlerTest
    {
        private readonly BindingMap bindingMap = new BindingMap {{"test", 
            new RoundRobinBalancer<Uri>( 
                new[] {new Uri("https://127.0.0.1")})
        }};

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