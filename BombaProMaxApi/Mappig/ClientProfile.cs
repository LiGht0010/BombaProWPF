using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            // Entity to DTO mapping
            CreateMap<Client, ClientDto>();

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<ClientDto, Client>()
                .ForMember(d => d.Factures, opt => opt.Ignore())
                .ForMember(d => d.CreditTransactions, opt => opt.Ignore())
                .ForMember(d => d.ReglementsCredit, opt => opt.Ignore())
                .ForMember(d => d.BilanCredit, opt => opt.Ignore())
                .ForMember(d => d.BonsLivraison, opt => opt.Ignore());
        }
    }
}
