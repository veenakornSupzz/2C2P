using AutoMapper;
using API.Models;
using API.Dto;

namespace API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Transaction, TransactionResponseDTO>()
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => $"{src.Amount} {src.CurrencyCode}"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatus(src.Status)));
        }

        private string MapStatus(string status)
        {
            return status switch
            {
                "Approved" => "A",
                "Failed" => "R",
                "Finished" => "D",
                "Rejected" => "R",
                "Done" => "D",
                _ => status
            };
        }
    }
}
