namespace RestX.API.Services.Interfaces
{
    public interface IExceptionHandler
    {
        void RaiseException(Exception ex, string customMessage = "");
    }
}
