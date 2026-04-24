namespace VechilePart.Application.Interfaces;

public interface IRepositoryBase<T>
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
