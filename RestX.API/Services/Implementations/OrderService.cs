using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.Entities;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
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
            string? customerId = httpContextAccessor.HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(customerId))
                return UniversalValue<Guid>.Failure("Bạn hãy vui lòng đăng nhập!");

            Order newOrder = new Order()
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
            if (!string.IsNullOrEmpty(temp2.ErrorMessage))
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

        public async Task<UniversalValue<Guid[]>> CreatedOrderDetails(List<DishCartViewModel> modelList, Guid OrderId)
        {
            Guid[] orderDetailIdList = new Guid[modelList.Count()];
            int i = 0;

            try
            {
                foreach (DishCartViewModel model in modelList)
                {
                    UniversalValue<Guid> returnGuid = await CreatedOrderDetail(model, OrderId);

                    if (!string.IsNullOrEmpty(returnGuid.ErrorMessage))   
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
            string? customerId = httpContextAccessor.HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(customerId))
                return UniversalValue<Guid>.Failure("Bạn hãy vui lòng đăng nhập!");

            OrderDetail newOrderDetail = new OrderDetail()
            {
                OrderId = OrderId,
                DishId = model.DishId,
                Quantity = model.Quantity,
                Price = model.Price,
                IsActive = true,
            };

            try
            {
                Guid temp = Guid.Parse((await Repo.CreateAsync(newOrderDetail, customerId)).ToString());
                await Repo.SaveAsync();
                return UniversalValue<Guid>.Success(temp, null);
            }
            catch (Exception)
            {
                return UniversalValue<Guid>.Failure("Ối! Có gì đó không ổn ở " + nameof(CreatedOrderDetails));
            }
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
                    List<DishCartViewModel> dishCart = new List<DishCartViewModel>();

                    foreach (var orderDetail in orderDetails)
                    {
                        dishCart.Add(new DishCartViewModel()
                        {
                            DishId = orderDetail.DishId,
                            DishName = orderDetail.Dish.Name,
                            Quantity = orderDetail.Quantity,
                            Price = orderDetail.Price,
                            ImgUrl = orderDetail.Dish.File.Url,
                        });
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
