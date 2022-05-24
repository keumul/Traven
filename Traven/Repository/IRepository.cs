using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Traven.Repository
{
    interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        IEnumerable<T> Find(Func<T, Boolean> predicate);
        IEnumerable<T> GetWithInclude(params Expression<Func<T, object>>[] includeProperties);

        IEnumerable<T> GetWithInclude(Func<T, bool> predicate,
            params Expression<Func<T, object>>[] includeProperties);
        void Remove(T item);
        void Create(T item);
        void Update(T item);
        void Delete(int id);
    }
}
