using AutoMapper;
using RestX.API.Models.Entities;
using RestX.API.Models.DTOs.Request;
using RestX.API.Models.DTOs.Response;
using RestX.API.Models.ViewModels;

namespace RestX.API.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Entity to ViewModel mappings
            CreateMap<Dish, DishViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : ""));

            CreateMap<Customer, CustomerViewModel>();

            // Entity to DTO mappings
            CreateMap<Category, CategoryDto>();

            // Request to Entity mappings
            CreateMap<DishRequest, Dish>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // Add more mappings as needed
        }
    }
}
