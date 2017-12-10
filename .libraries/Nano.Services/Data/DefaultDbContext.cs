using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Nano.Data;
using Nano.Eventing.Attributes;
using Nano.Eventing.Enums;
using Nano.Eventing.Interfaces;
using Nano.Models.Interfaces;
using Nano.Services.Eventing;

namespace Nano.Services.Data
{
    /// <inheritdoc />
    public class DefaultDbContext : BaseDbContext
    {
        /// <inheritdoc />
        public DefaultDbContext(DbContextOptions options)
            : base(options)
        {

        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges()
        {
            return this.SaveChanges(true);
        }

        /// <summary>
        ///     Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether ChangeTracker AcceptAllChanges is called after the changes have been sent successfully to the database.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return this.SaveChangesAsync(acceptAllChangesOnSuccess).Result;
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return this.SaveChangesAsync(true, cancellationToken);
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether ChangeTracker AcceptAllChanges is called after the changes have been sent successfully to the database.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entities = this.ChangeTracker
                .Entries<IEntityIdentity<Guid>>()
                .Where(x => x.Entity.GetType().GetAttributes<PublishAttribute>().Any())
                .Select(x => x.Entity);

            return base
                .SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                .ContinueWith(x =>
                {
                    var eventing = this.GetService<IEventing>();

                    if (eventing != null)
                        PublishEntityEvent(eventing, entities);

                    return x.Result;
                }, cancellationToken);
        }

        private static void PublishEntityEvent(IEventing eventing, IEnumerable<IEntityIdentity<Guid>> entities)
        {
            if (eventing == null)
                throw new ArgumentNullException(nameof(eventing));

            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                var entityEvent = new EntityEvent
                {
                    Id = entity.Id.ToString(),
                    Name = entity.GetType().Name
                };

                eventing
                    .Publish(entityEvent, Topology.Direct, entity.GetType().Name);
            }
        }
    }
}