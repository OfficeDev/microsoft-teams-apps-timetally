// <copyright file="FakeMemoryCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Fakes
{
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;

    /// <summary>
    /// Fake memory cache.
    /// </summary>
    public class FakeMemoryCache : IMemoryCache
    {
        /// <inheritdoc/>
        public ICacheEntry CreateEntry(object key)
        {
            return Mock.Of<ICacheEntry>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool TryGetValue(object key, out object value)
        {
            value = false;
            return false;
        }
    }
}
