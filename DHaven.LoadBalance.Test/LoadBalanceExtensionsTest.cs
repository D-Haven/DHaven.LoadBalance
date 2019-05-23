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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DHaven.LoadBalance.Test
{
    public class LoadBalanceExtensionsTest
    {
        [Fact]
        public async Task RegisteringBalancerWillResolveHost()
        {
            var service = new ServiceCollection();
            service.AddLoadBalancing(registrar =>
                {
                    registrar.RegisterRoundRobin("test", new Uri("https://something"));
                });

            var provider = GenerateValidatingProvider(service, new Uri("https://something/one/two/three"));

            var client = provider.GetService<IHttpClientFactory>().CreateClient();
            var response = await client.GetAsync("http://test/one/two/three");
            await response.ShouldMatchUri();
        }

        private static IServiceProvider GenerateValidatingProvider(IServiceCollection services, Uri uri)
        {
            services.AddSingleton(new UriValidatingHandler(uri));
            services.AddHttpClient(Options.DefaultName)
                .AddHttpMessageHandler<UriValidatingHandler>();

            return services.BuildServiceProvider();
        }
    }
}