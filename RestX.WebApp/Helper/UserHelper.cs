using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace RestX.WebApp.Helper
{
    public static class UserHelper
    {
        public static IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

        public static Guid GetCurrentOwnerId()
        {
            var user = HttpContextAccessor?.HttpContext?.User;
            var ownerIdClaim = user?.FindFirst("OwnerId");

            if (ownerIdClaim == null || !Guid.TryParse(ownerIdClaim.Value, out var ownerId))
            {
                throw new UnauthorizedAccessException("OwnerId claim is missing or invalid.");
            }

            return ownerId;
        }

        public static Guid GetCurrentStaffId()
        {
            var user = HttpContextAccessor?.HttpContext?.User;
            var staffIdClaim = user?.FindFirst("StaffId");

            if (staffIdClaim == null || !Guid.TryParse(staffIdClaim.Value, out var staffId))
            {
                throw new UnauthorizedAccessException("StaffId claim is missing or invalid.");
            }

            return staffId;
        }
    }
}
