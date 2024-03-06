using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using trucki.DatabaseContext;
using trucki.Interfaces.IRepository;

namespace trucki.Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected TruckiDBContext TruckiContext;
        public RepositoryBase(TruckiDBContext repositoryContext)
        => TruckiContext  = repositoryContext;

        public void Create(T entity) => TruckiContext.Set<T>().Add(entity);
        public void Update(T entity) => TruckiContext.Set<T>().Update(entity);
        public void Delete(T entity) => TruckiContext.Set<T>().Remove(entity);

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
        {
            return !trackChanges ? TruckiContext.Set<T>().Where(expression).AsNoTracking() : TruckiContext.Set<T>().Where(expression);
        }

        public IQueryable<T> FindAll(bool trackChanges)
        {
            return !trackChanges ? TruckiContext.Set<T>().AsNoTracking() : TruckiContext.Set<T>();
        }

        public async Task SaveAsync() => await TruckiContext.SaveChangesAsync();
    }
}
