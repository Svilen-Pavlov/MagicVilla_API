using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDBContext _db;
        internal DbSet<T> _dbSet;
        internal IQueryable<T> _query;

        public Repository(ApplicationDBContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>(); // how we establish which entity type we use
            _query = _dbSet;
        }


        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            //IQueryable<T> _query = _dbSet;
            if (!tracked)
            {
                _query = _query.AsNoTracking();
            }

            if (filter != null)
            {
                _query = _query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _query = _query.Include(includeProp);
                }
            }

            return await _query.FirstOrDefaultAsync(); // deferred execution. ToList() causes immediate execution
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 0, int pageNumber = 1)
        {
            //IQueryable<T> _query = _dbSet;
            if (filter != null)
                _query = _query.Where(filter);

            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                _query = _query.Skip(pageSize * (pageNumber-1)).Take(pageSize);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    _query = _query.Include(includeProp);
                }
            }

            return await _query.ToListAsync(); // deferred execution. ToList() causes immediate execution
        }

        public async Task<int> CountAsync()
        {
            IQueryable<T> query = _dbSet;
            return await query.CountAsync();
        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }


        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
