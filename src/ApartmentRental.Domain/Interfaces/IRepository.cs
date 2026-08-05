using System.Linq.Expressions;
using ApartmentRental.Domain.Common;

namespace ApartmentRental.Domain.Interfaces;

// Lives in Domain (not Application) so Domain owns the persistence contract;
// Application and Infrastructure both depend on Domain instead of on each other.
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    // asNoTracking=true (default) for read-only queries. Pass false when the
    // result will be mutated and saved within the same unit of work.
    IQueryable<T> Query(bool asNoTracking = true, bool includeDeleted = false);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    void SoftDelete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
