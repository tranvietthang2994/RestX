namespace RestX.WebApp.Models.ViewModels
{
    public class UniversalValue<T>
    {
        public T? Data { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public static UniversalValue<T> Success(T data, string success) => new UniversalValue<T> { Data = data, SuccessMessage = success};
        public static UniversalValue<T> Failure(string error) => new UniversalValue<T> { ErrorMessage = error };
    }
}