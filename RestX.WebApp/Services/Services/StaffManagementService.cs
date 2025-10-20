using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using RestX.WebApp.Helper;
using System.Security.Cryptography;
using System.Text;

namespace RestX.WebApp.Services.Services
{
    public class StaffManagementService : BaseService, IStaffManagementService
    {
        private readonly IFileService fileService;
        private readonly IOwnerService ownerService;

        public StaffManagementService(IRepository repo, IHttpContextAccessor httpContextAccessor, IFileService fileService, IOwnerService ownerService)
            : base(repo, httpContextAccessor)
        {
            this.fileService = fileService;
            this.ownerService = ownerService;
        }

        public async Task<StaffManagementViewModel> GetStaffManagementViewModelAsync(Guid ownerId)
        {
            var staffs = await GetStaffsByOwnerIdAsync(ownerId);

            var staffViewModels = staffs.Select(staff => new StaffViewModel
            {
                Id = staff.Id,
                FileId = staff.FileId,
                ImageUrl = staff.File?.Url,
                Name = staff.Name,
                Email = staff.Email,
                Phone = staff.Phone,
                Username = staff.Accounts.FirstOrDefault()?.Username ?? string.Empty,
                Password = string.Empty,
                IsActive = staff.IsActive
            }).ToList();

            return new StaffManagementViewModel
            {
                Staffs = staffViewModels
            };
        }

        public async Task<List<Staff>> GetStaffsByOwnerIdAsync(Guid ownerId)
        {
            var staffs = await Repo.GetAsync<Staff>(
                filter: s => s.OwnerId == ownerId,
                includeProperties: "Accounts,File"
            );
            return staffs.OrderBy(s => s.Name).ToList();
        }

        public async Task<Staff?> GetStaffByIdAsync(Guid id)
        {
            var staffs = await Repo.GetAsync<Staff>(
                filter: s => s.Id == id,
                includeProperties: "Accounts,File"
            );
            return staffs.FirstOrDefault();
        }

        public async Task<StaffViewModel?> GetStaffViewModelByIdAsync(Guid staffId)
        {
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null) return null;

            return new StaffViewModel
            {
                Id = staff.Id,
                FileId = staff.FileId,
                ImageUrl = staff.File?.Url,
                Name = staff.Name,
                Email = staff.Email,
                Phone = staff.Phone,
                Username = staff.Accounts.FirstOrDefault()?.Username ?? string.Empty,
                Password = string.Empty,
                IsActive = staff.IsActive
            };
        }

        public async Task<Guid?> UpsertStaffAsync(StaffRequest request, Guid ownerId)
        {
            try
            {
                var ownerId_current = UserHelper.GetCurrentOwnerId(); // Sử dụng UserHelper như DishService
                Staff staff;
                Account account;
                bool isEdit = request.Id.HasValue && request.Id.Value != Guid.Empty;

                if (isEdit)
                {
                    staff = await GetStaffByIdAsync(request.Id.Value);
                    if (staff == null)
                        return null;

                    // Verify ownership
                    if (staff.OwnerId != ownerId_current)
                        throw new UnauthorizedAccessException("You don't have permission to edit this staff.");

                    account = staff.Accounts.FirstOrDefault();
                    if (account == null)
                        return null;

                    if (await IsUsernameExistsAsync(request.Username, account.Id))
                        throw new InvalidOperationException("Username is already taken.");

                    if (await IsEmailExistsAsync(request.Email, staff.Id))
                        throw new InvalidOperationException("Email is already taken.");

                    staff.Name = request.Name;
                    staff.Email = request.Email;
                    staff.Phone = request.Phone;
                    staff.IsActive = request.IsActive;
                    staff.OwnerId = ownerId_current;

                    // Handle file upload - theo pattern DishService
                    if (request.ImageFile != null && request.ImageFile.Length > 0)
                    {
                        var owner = await ownerService.GetOwnerByIdAsync(ownerId_current);
                        var imageUrl = await UploadStaffImageAsync(request.ImageFile, owner.Name, request.Name);
                        var file = await fileService.CreateFileFromUploadAsync(imageUrl, request.ImageFile.FileName, ownerId_current);
                        staff.FileId = file.Id;
                    }

                    account.Username = request.Username;
                    account.Password = HashPassword(request.Password);

                    var ownerName = (await ownerService.GetOwnerByIdAsync(ownerId_current))?.Name;
                    Repo.Update(staff, ownerName);
                    Repo.Update(account, ownerName);
                    await Repo.SaveAsync();
                    return staff.Id;
                }
                else
                {
                    if (await IsUsernameExistsAsync(request.Username))
                        throw new InvalidOperationException("Username is already taken.");

                    if (await IsEmailExistsAsync(request.Email))
                        throw new InvalidOperationException("Email is already taken.");

                    // Tạo staff mới - tương tự như DishService
                    staff = new Staff
                    {
                        Id = Guid.NewGuid(),
                        OwnerId = ownerId_current,
                        Name = request.Name,
                        Email = request.Email,
                        Phone = request.Phone,
                        IsActive = request.IsActive,
                        FileId = Guid.Empty, // Khởi tạo với Guid.Empty trước
                        CreatedDate = DateTime.UtcNow
                    };

                    // Handle file upload - nếu có file thì upload, nếu không thì để Guid.Empty
                    if (request.ImageFile != null && request.ImageFile.Length > 0)
                    {
                        var owner = await ownerService.GetOwnerByIdAsync(ownerId_current);
                        var imageUrl = await UploadStaffImageAsync(request.ImageFile, owner.Name, request.Name);
                        var file = await fileService.CreateFileFromUploadAsync(imageUrl, request.ImageFile.FileName, ownerId_current);
                        staff.FileId = file.Id;
                    }
                    else
                    {
                        // Tạo file mặc định hoặc để Guid.Empty tùy business logic
                        // Nếu FileId là required thì phải tạo default file
                        var defaultFile = await fileService.CreateFileFromUploadAsync("~/images/default-staff.png", "default-staff.png", ownerId_current);
                        staff.FileId = defaultFile.Id;
                    }

                    account = new Account
                    {
                        Id = Guid.NewGuid(),
                        StaffId = staff.Id,
                        Username = request.Username,
                        Password = HashPassword(request.Password),
                        Role = "Staff",
                        CreatedDate = DateTime.UtcNow
                    };

                    var ownerName = (await ownerService.GetOwnerByIdAsync(ownerId_current))?.Name;

                    // Tương tự DishService: create -> save -> return ID
                    var result = await Repo.CreateAsync(staff, ownerName);
                    await Repo.CreateAsync(account, ownerName);
                    await Repo.SaveAsync();
                    return staff.Id;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving staff: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteStaffAsync(Guid staffId)
        {
            try
            {
                var staff = await GetStaffByIdAsync(staffId);
                if (staff == null)
                    return false;

                // Delete related accounts first
                if (staff.Accounts != null && staff.Accounts.Any())
                {
                    var accountsList = staff.Accounts.ToList();
                    foreach (var account in accountsList)
                    {
                        Repo.Delete<Account>(account);
                    }
                }

                // Delete file if exists - tương tự DishService nhưng FileId là non-nullable
                if (staff.FileId != Guid.Empty)
                {
                    try
                    {
                        await fileService.DeleteFileAsync(staff.FileId);
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine($"Warning: Could not delete file: {fileEx.Message}");
                    }
                }

                // Delete staff - sử dụng entity thay vì ID như DishService
                Repo.Delete<Staff>(staff);
                await Repo.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete staff: {ex.Message}", ex);
            }
        }

        private async Task<string> UploadStaffImageAsync(IFormFile file, string ownerName, string staffName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            var fileName = GetStaffImagePath(ownerName, staffName, extension);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", "StaffImages");
            var filePath = Path.Combine(uploadsFolder, fileName);

            Directory.CreateDirectory(uploadsFolder);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"~/Uploads/StaffImages/{fileName}";
        }

        private string GetStaffImagePath(string ownerName, string staffName, string extension)
        {
            var sanitizedOwnerName = SanitizeFileName(ownerName);
            var sanitizedStaffName = SanitizeFileName(staffName);

            return $"Staff-{sanitizedOwnerName}-{sanitizedStaffName}{extension}";
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return sanitized.Replace(" ", "_").Replace("-", "_");
        }

        private async Task<bool> IsUsernameExistsAsync(string username, Guid? excludeAccountId = null)
        {
            var accounts = await Repo.GetAsync<Account>(
                filter: a => a.Username == username && (excludeAccountId == null || a.Id != excludeAccountId.Value)
            );
            return accounts.Any();
        }

        private async Task<bool> IsEmailExistsAsync(string email, Guid? excludeStaffId = null)
        {
            var staffs = await Repo.GetAsync<Staff>(
                filter: s => s.Email == email && (excludeStaffId == null || s.Id != excludeStaffId.Value)
            );
            return staffs.Any();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}