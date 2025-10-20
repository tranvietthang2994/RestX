using AutoMapper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Helper
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Dish, DishViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : ""))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.File != null ? src.File.Url : ""));
            CreateMap<DataTransferObjects.Dish, Dish>().ReverseMap();
            CreateMap<DataTransferObjects.Category, Category>().ReverseMap();
            CreateMap<CustomerViewModel, Customer>().ReverseMap();
            CreateMap<OwnerProfileViewModel, Owner>().ReverseMap();
            CreateMap<Table, TableViewModel>().ReverseMap();
        }
    }
}
