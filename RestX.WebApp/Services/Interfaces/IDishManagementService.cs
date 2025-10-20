using RestX.WebApp.Models.ViewModels;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IDishManagementService
    {
        Task<DishesManagementViewModel> GetDishesAsync();
    }
}
