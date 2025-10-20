namespace RestX.API.Models.Interfaces
{
    public interface IEntity<T> : IEntity
    {
        new T Id { get; set; }
    }
}
