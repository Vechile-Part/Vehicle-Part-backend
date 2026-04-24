using VechilePart.Application.Interfaces;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public abstract class RepositoryBase<T>(AppDbContext dbContext, List<T> set) : IRepositoryBase<T>
{
    protected AppDbContext DbContext { get; } = dbContext;
    protected List<T> Set { get; } = set;

    public virtual Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Set.Add(entity);
        return Task.FromResult(entity);
    }

    public virtual Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<T>)Set);
    }
}
