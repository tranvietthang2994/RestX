using RestX.Models.Interfaces;

namespace RestX.BLL
{
    public class DbContext
    {
        public object Database { get; internal set; }

        internal IQueryable<TEntity> Set<TEntity>() where TEntity : class, IEntity
        {
            throw new NotImplementedException();
        }
    }
}