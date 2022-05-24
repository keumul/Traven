using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private Context _context;
        private DbSet<T> _dbSet;

        public Repository(Context context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.AsNoTracking().ToList();

        public T Get(int id) => _dbSet.Find(id);
        public T Get(Guid id) => _dbSet.Find(id);

        public IEnumerable<T> Find(Func<T, bool> predicate) =>
            _dbSet.AsNoTracking().Where(predicate).ToList();

        public void Create(T item)
        {
            _dbSet.Add(item); // TODO async
            _context.SaveChanges();
        }


        public void Update(T item)
        {
            _dbSet.Attach(item);
            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            T item = _dbSet.Find(id);
            if (item != null)
            {
                _dbSet.Remove(item);
                _context.SaveChanges();
            }
        }
        public void Delete(Guid id)
        {
            T item = _dbSet.Find(id);
            if (item != null)
            {
                _dbSet.Remove(item);
                _context.SaveChanges();
            }
        }

        public void Delete(T item)
        {
            _dbSet.Remove(item);
            _context.SaveChanges();
        }
        
        public void Remove(T item)
        {
            //_context.Entry(item).State = EntityState.Deleted;
            //_context.SaveChanges();
            var entry = _context.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }
            _dbSet.Remove(item);
        }

        public IEnumerable<T> GetWithInclude(params Expression<Func<T, object>>[] includeProperties)
        {
            return Include(includeProperties).ToList();
        }

        public IEnumerable<T> GetWithInclude(Func<T, bool> predicate,
            params Expression<Func<T, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return query.AsEnumerable().Where(predicate).ToList();
        }

        private IQueryable<T> Include(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            return includeProperties
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }
        //private Context context;
        //private DbSet<T> dbset;

        //public Repository(Context context)
        //{
        //    this.context = context;
        //    this.dbset = context.Set<T>();
        //}

        //public void Create(T item)
        //{
        //    dbset.Add(item);
        //}


        //public T Get(int id)
        //{
        //    return dbset.Find(id);
        //}

        //public IEnumerable<T> GetAll()
        //{
        //    return dbset;
        //}

        //public T GetT(int id)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Delete(int id)
        //{
        //    dbset.Remove(Get(id));
        //}

        //public void Update(T item)
        //{
        //    context.Entry(item).State = EntityState.Modified;
        //}
    }
}
