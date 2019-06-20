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
using DHaven.LoadBalance.Config;
using FluentAssertions;
using Xunit;

namespace DHaven.LoadBalance.Test.Config
{
    public class BindingRegistrarTest
    {
        [Fact]
        public void CanRegisterRandomLoadBalancer()
        {
            var registrar = new ExposeBindings();
            var testUri = new Uri("http://localhost");
            
            registrar.RegisterRandom("service", testUri);

            var bindings = registrar.GetBindingMap();
            bindings.Should().ContainKey("service");
            bindings["service"].Should().BeOfType<RandomLoadBalancer<Uri>>();
        }
        
        [Fact]
        public void CanRegisterRoundRobinLoadBalancer()
        {
            var registrar = new ExposeBindings();
            var testUri = new Uri("http://localhost");
            
            registrar.RegisterRoundRobin("service", testUri);

            var bindings = registrar.GetBindingMap();
            bindings.Should().ContainKey("service");
            bindings["service"].Should().BeOfType<RoundRobinBalancer<Uri>>();
        }
        
        [Fact]
        public void CanRegisterAdaptiveLoadBalancer()
        {
            var registrar = new ExposeBindings();
            var testUri = new Uri("http://localhost");
            
            registrar.RegisterAdaptive("service", uri => 100, testUri);

            var bindings = registrar.GetBindingMap();
            bindings.Should().ContainKey("service");
            bindings["service"].Should().BeOfType<AdaptiveLoadBalancer<Uri>>();
        }
          
        [Fact]
        public void CanRegisterUpdateableAdaptiveLoadBalancer()
        {
            var registrar = new ExposeBindings();
            var testUri = new Uri("http://localhost");
            
            registrar.RegisterUpdateableAdaptive("service", uri => 213, testUri);

            var bindings = registrar.GetBindingMap();
            bindings.Should().ContainKey("service");
            bindings["service"].Should().BeOfType<UpdateableAdaptiveLoadBalancer<Uri>>();
        }

        [Fact]
        public void CannotReuseServiceNames()
        {
            var registrar = new BindingRegistrar();
            var serviceName = "repeat-me";
            
            registrar.RegisterRandom(serviceName, new Uri("http://first-time"));
            Action action = () => registrar.RegisterRoundRobin(serviceName, new Uri("http://second-time"));

            action.Should().Throw<ArgumentException>().WithMessage("Service name is already registered*");
        }
      
        [Fact]
        public void HostNameIsRequired()
        {
            var registrar = new BindingRegistrar();
            var testUri = new Uri("http://localhost");
            
            foreach (var action in new Action[]
            {
                () => registrar.RegisterRandom(null, testUri),
                () => registrar.RegisterRoundRobin(null, testUri),
                () => registrar.RegisterAdaptive(null, uri => 0, testUri),
                () => registrar.RegisterUpdateableAdaptive(null, uri => 1, testUri)
            })
            {
                action.Should().Throw<ArgumentNullException>();
            }
        }

        [Fact]
        public void AtLeastOneUriRequired()
        {
            var registrar = new BindingRegistrar();
            
            foreach (var action in new Action[]
            {
                () => registrar.RegisterRandom("random"),
                () => registrar.RegisterRoundRobin("round-robin"),
                () => registrar.RegisterAdaptive("adaptive", uri => 0),
                () => registrar.RegisterUpdateableAdaptive("updateable", uri => 1)
            })
            {
                action.Should().Throw<ArgumentException>().WithMessage("You must provide at least one URI to load balance*");
            }
        }
        
        [Fact]
        public void ScoreFunctionRequiredForAdaptiveBalancers()
        {
            var registrar = new BindingRegistrar();
            var testUri = new Uri("https://not-really/here");
            
            foreach (var action in new Action[]
            {
                () => registrar.RegisterAdaptive("adaptive", null, testUri),
                () => registrar.RegisterUpdateableAdaptive("updateable", null, testUri)
            })
            {
                action.Should().Throw<ArgumentNullException>();
            }
        }
    }
}