using RestX.UI.Models.ViewModels;

namespace RestX.UI.Services.Interfaces
{
    public interface ICustomerUIService
    {
        /// <summary>
        /// Get all customers for restaurant
        /// </summary>
        /// <returns>List of customers</returns>
        Task<List<CustomerViewModel>> GetCustomersAsync();

        /// <summary>
        /// Get customer by ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Customer view model</returns>
        Task<CustomerViewModel?> GetCustomerByIdAsync(Guid customerId);

        /// <summary>
        /// Search customers by name or phone
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of customers</returns>
        Task<List<CustomerViewModel>> SearchCustomersAsync(string searchTerm);

        /// <summary>
        /// Create new customer
        /// </summary>
        /// <param name="model">Customer data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> CreateCustomerAsync(CustomerViewModel model);

        /// <summary>
        /// Update customer information
        /// </summary>
        /// <param name="model">Updated customer data</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string? Message)> UpdateCustomerAsync(CustomerViewModel model);

        /// <summary>
        /// Delete customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteCustomerAsync(Guid customerId);

        /// <summary>
        /// Get customer management view model
        /// </summary>
        /// <returns>Customer management view model</returns>
        Task<CustomerManagementViewModel?> GetCustomerManagementAsync();

        /// <summary>
        /// Get customer order history
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer orders</returns>
        Task<List<OrderViewModel>> GetCustomerOrdersAsync(Guid customerId);
    }
}
