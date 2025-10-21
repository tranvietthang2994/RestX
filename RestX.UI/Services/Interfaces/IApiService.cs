namespace RestX.UI.Services.Interfaces
{
    public interface IApiService
    {
        /// <summary>
        /// Send GET request to API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <returns>Response data</returns>
        Task<T?> GetAsync<T>(string endpoint);

        /// <summary>
        /// Send POST request to API
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <returns>Response data</returns>
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);

        /// <summary>
        /// Send PUT request to API
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <typeparam name="TResponse">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <returns>Response data</returns>
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);

        /// <summary>
        /// Send DELETE request to API
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteAsync(string endpoint);

        /// <summary>
        /// Set JWT authorization token
        /// </summary>
        /// <param name="token">JWT token</param>
        void SetAuthToken(string token);

        /// <summary>
        /// Clear authorization token
        /// </summary>
        void ClearAuthToken();

        /// <summary>
        /// Get response as string
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <returns>Response string</returns>
        Task<string?> GetStringAsync(string endpoint);

        /// <summary>
        /// Post with string response
        /// </summary>
        /// <typeparam name="TRequest">Request type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <returns>Response string</returns>
        Task<string?> PostStringAsync<TRequest>(string endpoint, TRequest data);
    }
}
