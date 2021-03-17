// <copyright file="IBaseRepository.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for handling common operations with entity collection.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Handles adding new entity.
        /// </summary>
        /// <param name="entity">Entity that is being saved to database.</param>
        /// <returns>Returns entity data that was saved to database.</returns>
        T Add(T entity);

        /// <summary>
        /// Handles update of entity.
        /// </summary>
        /// <param name="entity">Entity that is being updated.</param>
        /// <returns>Returns entity data that was updated to database.</returns>
        T Update(T entity);

        /// <summary>
        /// Handles getting entity based on entity identifier.
        /// </summary>
        /// <param name="id">Entity Id that is being used to get entity from database.</param>
        /// <returns>Returns the entity that matches given identifier.</returns>
        Task<T> GetAsync(Guid id);

        /// <summary>
        /// Handles getting all entities from database.
        /// </summary>
        /// <returns>Returns collection of filtered entities using expression.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Handles filtering entity based on expression.
        /// </summary>
        /// <param name="predicate">Expression that is being used to filter entities from database.</param>
        /// <returns>Returns collection of filtered entities using expression.</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Handles delete of entity.
        /// </summary>
        /// <param name="entity">Entity that is being updated.</param>
        /// <returns>Returns entity data that is saved to database.</returns>
        T Delete(T entity);
    }
}