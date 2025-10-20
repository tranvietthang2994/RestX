using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Helper;

namespace RestX.WebApp.Services.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICartService cartService;
        public OrderService(IRepository repo, IHttpContextAccessor httpContextAccessor, ICartService cartService) : base(repo, httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.cartService = cartService;
        }

        public async Task<UniversalValue<Guid>> CreatedOrder(CartViewModel model)
        {
            DateTime currentTime = DateTime.Now;
            string customerId = httpContextAccessor.HttpContext.Session.GetString("CustomerId");
            if (customerId.IsNullOrEmpty())
                return UniversalValue<Guid>.Failure("Bạn hãy vui lòng đăng nhập!");
            Order newOrder = null;
            string message = null;
            
            newOrder = new Order()
            {
                CustomerId = Guid.Parse(customerId),
                TableId = model.TableId,
                OwnerId = model.OwnerId,
                OrderStatusId = 1,
                Time = currentTime,
                IsActive = true,
            };

            Guid temp = Guid.Parse((await Repo.CreateAsync(newOrder)).ToString());
            await Repo.SaveAsync();

            model.OrderId = temp;

            UniversalValue<Guid[]> temp2 = await CreatedOrderDetails(model);
            if (!temp2.ErrorMessage.IsNullOrEmpty())
            {
                return UniversalValue<Guid>.Success(temp, "Có lỗi xảy ra! Vui lòng liên hệ nhân viên!");
            }

            return UniversalValue<Guid>.Success(temp, "Đặt đơn hàng thành công!\n Xin cảm ơn quý khách!");
        }

        public async Task<UniversalValue<Guid[]>> CreatedOrderDetails(CartViewModel model)
        {
            model = await cartService.JsonToDishList(model);
            return await CreatedOrderDetails(model.DishList, model.OrderId.Value);
        }

        public async Task<UniversalValue<Guid[]>> CreatedOrderDetails(DishCartViewModel[] modelList, Guid OrderId)
        {
            Guid[] orderDetailIdList = new Guid[modelList.Length];
            int i = 0;

            try
            {
                foreach (DishCartViewModel model in modelList)
                {
                    UniversalValue<Guid> returnGuid = await CreatedOrderDetail(model, OrderId);

                    if (!returnGuid.ErrorMessage.IsNullOrEmpty())
                    {
                        return UniversalValue<Guid[]>.Failure(returnGuid.ErrorMessage);
                    }

                    orderDetailIdList[i] = returnGuid.Data;
                    i++;
                }
            }
            catch (Exception)
            {
                return UniversalValue<Guid[]>.Failure("Ối! Có gì đó không ổn ở " + nameof(CreatedOrderDetails));
            }

            return UniversalValue<Guid[]>.Success(orderDetailIdList, null);
        }

        public async Task<UniversalValue<Guid>> CreatedOrderDetail(DishCartViewModel model, Guid OrderId)
        {
            DateTime currentTime = DateTime.Now;
            string customerId = httpContextAccessor.HttpContext.Session.GetString("CustomerId");
            if (customerId.IsNullOrEmpty())
                return UniversalValue<Guid>.Failure("Bạn hãy vui lòng đăng nhập!");
            OrderDetail newOrderDetail = null;
            Guid temp = Guid.Empty;
            
            newOrderDetail = new OrderDetail()
            {
                OrderId = OrderId,
                DishId = model.DishId,
                Quantity = model.Quantity,
                Price = model.Price,
                IsActive = true,
            };
            try
            {
                temp = Guid.Parse((await Repo.CreateAsync(newOrderDetail, customerId)).ToString());
                await Repo.SaveAsync();
            }
            catch (Exception)
            {
                return UniversalValue<Guid>.Failure("Ối! Có gì đó không ổn ở " + nameof(CreatedOrderDetails));
            }


            return UniversalValue<Guid>.Success(temp, null);
        }

        public async Task<CustomerRequestViewModel> GetCustomerRequestsByStaffAsync(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            
            var orders = await Repo.GetAsync<Order>(
                filter: o => o.OwnerId == ownerId && o.IsActive == true,
                orderBy: q => q.OrderByDescending(o => o.Time),
                includeProperties: "Customer,Table,OrderStatus,OrderDetails,OrderDetails.Dish"
            );

            var customerRequestViewModel = new CustomerRequestViewModel
            {
                Orders = orders.Select(order => new OrderRequestViewModel
                {
                    Id = order.Id,
                    CustomerName = order.Customer?.Name ?? "Unknown",
                    CustomerPhone = order.Customer?.Phone ?? "N/A",
                    TableNumber = order.Table?.TableNumber ?? 0,
                    OrderStatus = order.OrderStatus?.Name ?? "Unknown",
                    OrderTime = order.Time,
                    IsActive = order.IsActive ?? false,
                    OrderDetails = order.OrderDetails?.Where(od => od.IsActive == true).Select(od => new OrderDetailRequestViewModel
                    {
                        Id = od.Id,
                        DishId = od.DishId,
                        DishName = od.Dish?.Name ?? "Unknown",
                        Quantity = od.Quantity,
                        Price = od.Price,
                        IsActive = od.IsActive ?? false
                    }).ToList() ?? new List<OrderDetailRequestViewModel>(),
                    TotalAmount = order.OrderDetails?.Where(od => od.IsActive == true).Sum(od => od.Quantity * od.Price) ?? 0
                }).ToList()
            };

            return customerRequestViewModel;
        }

        public async Task<List<CartViewModel>> GetOrdersByCustomerIdOwnerIdAsync(Guid ownerId, Guid customerId)
        {
            var orders = await Repo.GetAsync<Order>(
                filter: o => o.OwnerId == ownerId && o.CustomerId == customerId && o.IsActive == true,
                includeProperties: "OrderDetails,OrderDetails.Dish,OrderDetails.Dish.File"
            );
            orders = orders.OrderByDescending(o => o.Time);

            List<CartViewModel> cartViewModels = new List<CartViewModel>();

            try
            {
                foreach(var order in orders)
                {
                    List<OrderDetail> orderDetails = order.OrderDetails.ToList();
                    DishCartViewModel[] dishCart = new DishCartViewModel[orderDetails.Count()];
                    int i = 0;

                    foreach (var orderDetail in orderDetails)
                    {
                        dishCart[i] = new DishCartViewModel()
                        {
                            DishId = orderDetail.DishId,
                            DishName = orderDetail.Dish.Name,
                            Quantity = orderDetail.Quantity,
                            Price = orderDetail.Price,
                            ImgUrl = orderDetail.Dish.File.Url,
                        };
                        i++;
                    }

                    cartViewModels.Add(new CartViewModel
                    {
                        OwnerId = order.OwnerId,
                        TableId = order.TableId,
                        OrderId = order.Id,
                        Time = order.Time,
                        DishList = dishCart,
                    });

                }
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return cartViewModels;
        }
    }
}
