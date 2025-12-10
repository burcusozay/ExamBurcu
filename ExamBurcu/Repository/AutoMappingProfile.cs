using AutoMapper;
using ExamBurcu.Data;
using ExamBurcu.Dtos;

namespace WebApplication1.AutoMapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<VaccineScheduleDto, vaccineschedule>().ReverseMap();
            CreateMap<VaccineApplicationDto, vaccineapplication>()
                .ForMember(dest => dest.vaccine, opt => opt.Ignore())
                    .ForMember(dest => dest.child, opt => opt.Ignore())
                    .ForMember(dest => dest.doctor, opt => opt.Ignore())
                    .ReverseMap();
            CreateMap<VaccineDto, vaccine>()
                    .ForMember(dest => dest.vaccineapplications, opt => opt.Ignore())
                    .ForMember(dest => dest.vaccineschedules, opt => opt.Ignore())
                    .ReverseMap();
            CreateMap<DoctorDto, doctor>().ReverseMap();
            CreateMap<ChildDto, child>().ReverseMap();
            // Diğer entity <-> dto eşlemeleri de buraya eklenebilir
        }
    }
}
