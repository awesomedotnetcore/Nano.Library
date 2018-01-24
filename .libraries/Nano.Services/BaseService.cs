using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DynamicExpression.Entities;
using DynamicExpression.Extensions;
using DynamicExpression.Interfaces;
using Microsoft.EntityFrameworkCore;
using Nano.Data;
using Nano.Models.Interfaces;
using Nano.Services.Interfaces;
using Z.EntityFramework.Plus;

namespace Nano.Services
{
    /// <inheritdoc />
    public abstract class BaseService<TContext> : IService
        where TContext : BaseDbContext
    {
        /// <summary>
        /// Context.
        /// </summary>
        protected virtual TContext Context { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        protected BaseService(TContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            this.Context = context;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Get<TEntity, TIdentity>(TIdentity key, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return await this.Context
                .Set<TEntity>()
                .FindAsync(new[] { key }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetMany<TEntity, TIdentity>(IEnumerable<TIdentity> keys, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            return await this.Context
                .Set<TEntity>()
                .Where(x => keys.Any(y => y.Equals(x)))
                .ToArrayAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAll<TEntity>(Query query, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return await this.Context
                .Set<TEntity>()
                .Order(query.Order)
                .Limit(query.Paging)
                .ToArrayAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetMany<TEntity, TCriteria>(Query<TCriteria> query, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TCriteria : class, IQueryCriteria, new()
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return await this.Context
                .Set<TEntity>()
                .Where(query.Criteria)
                .Order(query.Order)
                .Limit(query.Paging)
                .ToArrayAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetMany<TEntity>(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return await this.Context
                .Set<TEntity>()
                .Where(expression)
                .ToArrayAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Add<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityCreatable
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = this.Context
                .Add(entity);

            await this.Context
                .SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        /// <inheritdoc />
        public virtual async Task AddMany<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityCreatable
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await this.Context
                .AddRangeAsync(entities, cancellationToken);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> Update<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityUpdatable
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = this.Context
                .Update(entity);

            await this.Context
                .SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        /// <inheritdoc />
        public virtual async Task UpdateMany<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityUpdatable
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            this.Context
                .UpdateRange(entities);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateMany<TEntity, TCriteria>(TCriteria select, TEntity update, CancellationToken cancellationToken = default) 
            where TEntity : class, IEntityUpdatable 
            where TCriteria : class, IQueryCriteria, new()
        {
            if (select == null)
                throw new ArgumentNullException(nameof(select));

            if (update == null)
                throw new ArgumentNullException(nameof(update));

            await this.Context
                .Set<TEntity>()
                .Where(select)
                .UpdateAsync(x => update, cancellationToken);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateMany<TEntity>(Expression<Func<TEntity, bool>> select, Expression<Func<TEntity, TEntity>> update, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityUpdatable
        {
            if (select == null)
                throw new ArgumentNullException(nameof(select));

            if (update == null)
                throw new ArgumentNullException(nameof(update));

            await this.Context
                .Set<TEntity>()
                .Where(select)
                .UpdateAsync(update, cancellationToken);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> AddOrUpdate<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityCreatableAndUpdatable
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entry = this.Context
                .AddOrUpdate(entity);

            await this.Context
                .SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        /// <inheritdoc />
        public virtual async Task AddOrUpdateMany<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityCreatableAndUpdatable
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            this.Context
                .AddOrUpdateMany(entities);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task Delete<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityDeletable
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
    
            this.Context
                .Remove(entity);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteMany<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityDeletable
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            this.Context
                .RemoveRange(entities, cancellationToken);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteMany<TEntity, TCriteria>(TCriteria critiera, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityDeletable
            where TCriteria : class, IQueryCriteria, new()
        {
            if (critiera == null)
                throw new ArgumentNullException(nameof(critiera));

            await this.Context
                .Set<TEntity>()
                .Where(critiera)
                .DeleteAsync(x =>
                {
                    x.BatchSize = this.Context.Options.BulkBatchSize;
                    x.BatchDelayInterval = this.Context.Options.BulkBatchDelay;
                }, cancellationToken);

            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task DeleteMany<TEntity>(Expression<Func<TEntity, bool>> select, CancellationToken cancellationToken = default)
            where TEntity : class, IEntityDeletable
        {
            if (select == null)
                throw new ArgumentNullException(nameof(select));

            await this.Context
                .Set<TEntity>()
                .Where(select)
                .DeleteAsync(x =>
                {
                    x.BatchSize = this.Context.Options.BulkBatchSize;
                    x.BatchDelayInterval = this.Context.Options.BulkBatchDelay;
                }, cancellationToken);
            await this.Context
                .SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Dispose (non-virtual).
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool).
        /// Override in derived classes as needed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this.Context?.Dispose();
        }
    }
}