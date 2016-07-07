//-----------------------------------------------------------------------
// <copyright file="IBaseRepository.cs" company="Integra.Space.Language">
//     Copyright (c) Integra.Space.Language. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Integra.Space.Cache
{
    using System.Collections.Generic;

    /// <summary>
    /// Repository interface.
    /// </summary>
    /// <typeparam name="TEntity">Type of the space object.</typeparam>
    internal interface IBaseRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets all the entities of the repository.
        /// </summary>
        IEnumerable<TEntity> List { get; }

        /// <summary>
        /// Add a new entity to the repository.
        /// </summary>
        /// <param name="entity">Entity to be added.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Delete the specified entity in the repository.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Delete the specified entity in the repository.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">Identifier of the entity.</param>
        /// <returns>The entity with the specified identifier.</returns>
        TEntity FindById(System.Guid id);
    }
}
