using AutoMapper;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams;

namespace TransportLogistics.Api.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Driver Mappings
            CreateMap<CreateDriverRequest, Driver>();
            CreateMap<UpdateDriverRequest, Driver>();
            CreateMap<Driver, DriverDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

            // Order Mappings
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (OrderStatus)src.Status));
            CreateMap<UpdateOrderRequest, Order>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (OrderStatus)src.Status));
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
                .ForMember(dest => dest.Driver, opt => opt.MapFrom(src => src.Driver))
                .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
                // Автоматичне мапінг колекції, якщо типи елементів відповідають мапінгам.
                // Оскільки ми додаємо CreateMap<Cargo, CargoRequestDto> нижче,
                // AutoMapper зрозуміє, як мапити ICollection<Cargo> на ICollection<CargoRequestDto>.
                .ForMember(dest => dest.Cargos, opt => opt.MapFrom(src => src.Cargos));

            // Nested DTOs
            CreateMap<Driver, DriverInOrderDto>();
            CreateMap<Vehicle, VehicleInOrderDto>();
            CreateMap<CargoRequestDto, Cargo>(); // Для мапінгу DTO запиту на сутність
            CreateMap<Cargo, CargoRequestDto>(); // !!! ДОДАНО: Для мапінгу сутності на DTO відповіді !!!

            // Інші DTO
        }
    }
}