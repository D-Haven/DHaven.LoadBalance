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
using System.Diagnostics.CodeAnalysis;
using DHaven.LoadBalance.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DHaven.LoadBalance
{
    public static class LoadBalanceExtensions
    {
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
            Action<BindingRegistrar> configure)
        {
            var mapper = new BindingRegistrar();
            configure(mapper);

            services.AddSingleton(mapper.Bindings);
            services.AddTransient<BindingHandler>();
            services.AddHttpClient(Options.DefaultName)
                .AddHttpMessageHandler<BindingHandler>();

            return services;
        }

        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static IServiceCollection AddLoadBalancing(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<Dictionary<string, BalancerOptions>>(options =>
                configuration.GetSection("D-Haven:LocalBalancer").Bind(options));

            services.AddSingleton<BindingMap>();
            services.AddTransient<BindingHandler>();
            services.AddHttpClient(Options.DefaultName)
                .AddHttpMessageHandler<BindingHandler>();

            return services;
        }
    }
}