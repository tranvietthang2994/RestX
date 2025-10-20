using AutoMapper;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Services.Services
{
    public class CustomerService : BaseService, ICustomerService
    {
        private readonly IMapper mapper;

        public CustomerService(IRepository repo, IHttpContextAccessor httpContextAccessor, IMapper mapper)
            : base(repo, httpContextAccessor)
        {
            this.mapper = mapper;
        }

        public async Task<List<CustomerViewModel>> GetCustomersByOwnerIdAsync()
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var customers = await Repo.GetAsync<Customer>(
                filter: c => c.OwnerId == ownerId,
                includeProperties: ""
            );
            return customers.Select(c => mapper.Map<CustomerViewModel>(c)).ToList();
        }

        public async Task<CustomerViewModel?> GetCustomerByIdAsync(Guid id)
        {
            var customer = await Repo.GetByIdAsync<Customer>(id);
            return customer == null ? null : mapper.Map<CustomerViewModel>(customer);
        }

        public async Task<Guid?> UpsertCustomerAsync(CustomerViewModel model)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            Customer? customer = null;
            if (model.Id != Guid.Empty)
            {
                customer = await Repo.GetByIdAsync<Customer>(model.Id);
                if (customer == null) return null;
                mapper.Map(model, customer);
                Repo.Update(customer);
            }
            else
            {
                customer = mapper.Map<Customer>(model);
                customer.Id = Guid.NewGuid();
                customer.OwnerId = ownerId;
                await Repo.CreateAsync(customer);
            }
            await Repo.SaveAsync();
            return customer.Id;
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            var customer = await Repo.GetByIdAsync<Customer>(id);
            if (customer == null) return false;
            Repo.Delete<Customer>(id);
            await Repo.SaveAsync();
            return true;
        }
    }
}