using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RestX.WebApp.Models;

namespace RestX.WebApp.Filters
{
    public class RestaurantContextFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ownerIdValue = context.RouteData.Values["ownerId"];
            Guid ownerId = Guid.Empty;
            if (ownerIdValue != null && Guid.TryParse(ownerIdValue.ToString(), out var parsedOwnerId))
            {
                ownerId = parsedOwnerId;
            }

            var tableIdValue = context.RouteData.Values["tableId"];
            int tableId = 0;
            if (tableIdValue != null && int.TryParse(tableIdValue.ToString(), out var parsedTableId))
            {
                tableId = parsedTableId;
            }

            var restaurantContext = new RestaurantContext
            {
                OwnerId = ownerId,
                TableId = tableId
            };

            context.HttpContext.Items["RestaurantContext"] = restaurantContext;

            base.OnActionExecuting(context);
        }
    }
}