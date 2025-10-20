namespace RestX.API.Extensions
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
                // TODO: Remove this fallback when JWT authentication is properly implemented
                // For development/testing purposes, return a default OwnerId
                #if DEBUG
                return Guid.Parse("550e8400-e29b-41d4-a716-446655440040"); // Default test OwnerId from sample data
                #else
                throw new UnauthorizedAccessException("OwnerId claim is missing or invalid.");
                #endif
            }

            return ownerId;
        }

        public static Guid GetCurrentStaffId()
        {
            var user = HttpContextAccessor?.HttpContext?.User;
            var staffIdClaim = user?.FindFirst("StaffId");

            if (staffIdClaim == null || !Guid.TryParse(staffIdClaim.Value, out var staffId))
            {
                // TODO: Remove this fallback when JWT authentication is properly implemented
                // For development/testing purposes, return a default StaffId
                #if DEBUG
                return Guid.Parse("22222222-2222-2222-2222-222222222222"); // Default test StaffId
                #else
                throw new UnauthorizedAccessException("StaffId claim is missing or invalid.");
                #endif
            }

            return staffId;
        }
    }
}
