// <copyright file="EnumerableExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for enumerable collections.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Represents the default list split size.
        /// </summary>
        private const short DefaultSplitSize = 40;

        /// <summary>
        /// Indicates whether a collection is null or it has length equal to 0.
        /// </summary>
        /// <typeparam name="T">The type of objects in collection.</typeparam>
        /// <param name="collection">The collection of a specified type.</param>
        /// <returns>Returns true if a collection is null or it has length equal to 0. Else returns false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// This method is to split list into given batch size.
        /// </summary>
        /// <typeparam name="T">T type.</typeparam>
        /// <param name="source">Source list to split.</param>
        /// <param name="nSize">Size value to split the list with 40 as default value.</param>
        /// <returns>A <see cref="IEnumerable{TResult}"/> representing the sub-lists by specified size.</returns>
        public static IEnumerable<List<T>> SplitList<T>(this List<T> source, int nSize = DefaultSplitSize)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));

            for (int i = 0; i < source.Count; i += nSize)
            {
                yield return source.GetRange(i, Math.Min(nSize, source.Count - i));
            }
        }
    }
}