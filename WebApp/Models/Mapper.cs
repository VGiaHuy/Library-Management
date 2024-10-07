using AutoMapper;
using WebApp.DTOs;

namespace WebApp.Models
{
    public class Mapper : Profile
    {
        public Mapper() {
            CreateMap<LoginDgDTO, LoginDg>();
        } 

    }
}
