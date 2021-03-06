using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDB.Abstracts
{
    /// <summary>
    /// A MongoDB data query base class.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public abstract class MongoQuery<TEntity, TKey> : DisposableBase, IMongoQuery<TEntity, TKey>
        where TEntity : class
    {
        private readonly Lazy<IMongoCollection<TEntity>> _collection;
        private readonly IMongoDatabase _mongoDatabase;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoQuery{TEntity, TKey}"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="mongoDatabase"/> is <see langword="null" />.</exception>
        protected MongoQuery(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _collection = new Lazy<IMongoCollection<TEntity>>(CreateCollection);
        }


        /// <summary>
        /// Gets the underling <see cref="IMongoCollection{TEntity}"/> used for queries.
        /// </summary>
        /// <value>
        /// The underling <see cref="IMongoCollection{TEntity}"/>.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public IMongoCollection<TEntity> Collection => _collection.Value;


        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null" />.</exception>
        public TEntity Find(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return Collection
                .Find(KeyExpression(key))
                .FirstOrDefault();
        }

        /// <summary>
        /// Finds the entity with the specified identifier.
        /// </summary>
        /// <param name="key">The entity identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The entity with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null" />.</exception>
        public Task<TEntity> FindAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return Collection
                .Find(KeyExpression(key))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public TEntity FindOne(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return Collection
                .Find(criteria)
                .FirstOrDefault();
        }

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of TEnity that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return Collection
                .Find(criteria)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Find all entities using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return Collection
                .AsQueryable()
                .Where(criteria);
        }

        /// <summary>
        /// Find all entities using the specified <paramref name="criteria" /> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Collection
                .Find(criteria)
                .ToListAsync(cancellationToken);
        }


        /// <summary>
        /// Get all <typeparamref name="TEntity" /> entities as an IQueryable
        /// </summary>
        /// <returns>
        /// IQueryable of <typeparamref name="TEntity" />.
        /// </returns>
        public IQueryable<TEntity> All()
        {
            return Collection.AsQueryable();
        }


        /// <summary>
        /// Gets the number of entities in the collection.
        /// </summary>
        /// <returns></returns>
        public long Count()
        {
            return Collection.CountDocuments(FilterDefinition<TEntity>.Empty);
        }

        /// <summary>
        /// Gets the number of entities in the collection.
        /// </summary>
        /// <returns></returns>
        public Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Collection.CountDocumentsAsync(FilterDefinition<TEntity>.Empty, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets the number of entities in the collection with the specified <paramref name="criteria" />.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public long Count(Expression<Func<TEntity, bool>> criteria)
        {
            return Collection.CountDocuments(criteria);
        }

        /// <summary>
        /// Gets the number of entities in the collection with the specified <paramref name="criteria" />.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<long> CountAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Collection.CountDocumentsAsync(criteria, cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Determines if the specified <paramref name="criteria" /> exists.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns>
        ///   <c>true</c> if criteria expression is found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">criteria</exception>
        public bool Exists(Expression<Func<TEntity, bool>> criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            return Collection
                .AsQueryable()
                .Any(criteria);
        }


        /// <summary>
        /// Gets the key for the specified <paramref name="entity" />.
        /// </summary>
        /// <param name="entity">The entity to get the key from.</param>
        /// <returns>
        /// The key for the specified entity.
        /// </returns>
        public abstract TKey EntityKey(TEntity entity);

        /// <summary>
        /// Gets the key expression with the specified <paramref name="key" />.
        /// </summary>
        /// <param name="key">The key to get expression with.</param>
        /// <returns>
        /// The key expression for the specified key.
        /// </returns>
        /// <example>
        /// <code>
        /// Example expression for an entity key.
        /// <![CDATA[entity => entity.Id == key]]>
        /// </code>
        /// </example>
        protected abstract Expression<Func<TEntity, bool>> KeyExpression(TKey key);


        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <returns></returns>
        protected virtual string CollectionName()
        {
            return typeof(TEntity).Name;
        }

        /// <summary>
        /// Creates the collection.
        /// </summary>
        /// <returns></returns>
        protected virtual IMongoCollection<TEntity> CreateCollection()
        {
            var database = _mongoDatabase;

            string collectionName = CollectionName();
            var mongoCollection = CreateCollection(database, collectionName);

            EnsureIndexes(mongoCollection);

            return mongoCollection;
        }

        /// <summary>
        /// Creates the collection.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        protected virtual IMongoCollection<TEntity> CreateCollection(IMongoDatabase database, string collectionName)
        {
            var mongoCollection = database.GetCollection<TEntity>(collectionName);
            return mongoCollection;
        }

        /// <summary>
        /// Create indexes on the collection.
        /// </summary>
        /// <param name="mongoCollection">The mongo collection.</param>
        protected virtual void EnsureIndexes(IMongoCollection<TEntity> mongoCollection)
        {

        }
    }

}