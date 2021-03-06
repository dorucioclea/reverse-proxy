// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.ReverseProxy.Core.RuntimeModel;

namespace Microsoft.ReverseProxy.Core.Middleware
{
    /// <summary>
    /// Tracks proxy backend endpoints that are available to handle the current request.
    /// </summary>
    public class AvailableBackendEndpointsFeature : IAvailableBackendEndpointsFeature
    {
        /// <summary>
        /// Backend endpoints that can handle the current request.
        /// </summary>
        public IReadOnlyList<EndpointInfo> Endpoints { get; set; }
    }
}
