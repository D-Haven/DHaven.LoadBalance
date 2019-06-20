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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DHaven.LoadBalance
{
    public class BindingHandler : DelegatingHandler
    {
        public BindingHandler(BindingMap bindingMap)
        {
            BindingMap = bindingMap;
        }

        private BindingMap BindingMap { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            var endTime = DateTime.Now + BindingMap.MaximumTimeout;

            while (response == null && DateTime.Now < endTime)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var wasLoadBalanced = BindingMap.TryRebindUri(request.RequestUri, out var newUri);
                if (wasLoadBalanced) request.RequestUri = newUri;

                try
                {
                    var timeout = wasLoadBalanced ? BindingMap.RetryTimeout : BindingMap.MaximumTimeout;
                    
                    response = await AttemptSendAsync(request, timeout, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // If we got an operation cancel exception, we are only worried about
                    // this iteration.  It doesn't mean that the whole operation got cancelled.
                    response = null;
                }
            }

            return response ?? new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
        }

        private async Task<HttpResponseMessage> AttemptSendAsync(HttpRequestMessage request, TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            using (var timeoutSource = new CancellationTokenSource(timeout))
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken))
            {
                return await base.SendAsync(request, linkedSource.Token);
            }
        }
    }
}