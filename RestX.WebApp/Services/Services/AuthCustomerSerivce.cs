using Microsoft.EntityFrameworkCore;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Services.Services
{
    public class AuthCustomerService : BaseService, IAuthCustomerService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public AuthCustomerService(IRepository repo, IHttpContextAccessor httpContextAccessor)
            : base(repo, httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Customer> LoginOrCreateAsync(LoginViewModel model, CancellationToken cancellationToken = default)
        {
            ValidateLoginModel(model);

            var customer = await FindExistingCustomerAsync(model.Phone, model.OwnerId, cancellationToken);

            if (customer != null)
            {
                httpContextAccessor.HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
                return await UpdateExistingCustomerAsync(customer, model, cancellationToken);
            }

            return await CreateNewCustomerAsync(model, cancellationToken);
        }

        private static void ValidateLoginModel(LoginViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.Phone))
                throw new ArgumentException("Số điện thoại không được để trống", nameof(model.Phone));

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Tên không được để trống", nameof(model.Name));

            if (model.OwnerId == Guid.Empty)
                throw new ArgumentException("Owner ID không hợp lệ", nameof(model.OwnerId));
        }

        private async Task<Customer?> FindExistingCustomerAsync(string phone, Guid ownerId, CancellationToken cancellationToken)
        {
            return await Repo.GetFirstAsync<Customer>(
                c => c.Phone == phone && c.OwnerId == ownerId
            );
        }

        private async Task<Customer> UpdateExistingCustomerAsync(Customer customer, LoginViewModel model, CancellationToken cancellationToken)
        {
            if (customer.Name != model.Name)
            {
                customer.Name = model.Name;
                customer.ModifiedDate = DateTime.UtcNow;

                Repo.Update<Customer>(customer, customer.Id.ToString());
            }

            return customer;
        }
        public async Task<Customer?> FindCustomerByPhoneAsync(string phone, Guid ownerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phone) || ownerId == Guid.Empty)
                return null;

            return await Repo.GetFirstAsync<Customer>(
                c => c.Phone == phone && c.OwnerId == ownerId && c.IsActive == true
            );
        }
        private async Task<Customer> CreateNewCustomerAsync(LoginViewModel model, CancellationToken cancellationToken)
        {
            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                OwnerId = model.OwnerId,
                Name = model.Name,
                Phone = model.Phone,
                Point = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = null
            };

            await Repo.CreateAsync<Customer>(newCustomer, newCustomer.Id.ToString());
            httpContextAccessor.HttpContext.Session.SetString("CustomerId", newCustomer.Id.ToString());
            return newCustomer;
        }

        public Task<Customer> FindCustomerByPhoneAsync(LoginViewModel model, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}