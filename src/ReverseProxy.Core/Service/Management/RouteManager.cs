﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.ReverseProxy.Core.RuntimeModel;

namespace Microsoft.ReverseProxy.Core.Service.Management
{
    internal sealed class RouteManager : ItemManagerBase<RouteInfo>, IRouteManager
    {
        /// <inheritdoc/>
        protected override RouteInfo InstantiateItem(string itemId)
        {
            return new RouteInfo(itemId);
        }
    }
}
