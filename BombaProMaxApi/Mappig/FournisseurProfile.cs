using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class FournisseurProfile : Profile
    {
        public FournisseurProfile()
        {
            // Entity to DTO mapping
            CreateMap<Fournisseur, FournisseurDto>();

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<FournisseurDto, Fournisseur>()
                .ForMember(d => d.Achats, opt => opt.Ignore())
                .ForMember(d => d.Camion, opt => opt.Ignore())
                .ForMember(d => d.Citerne, opt => opt.Ignore())
                .ForMember(d => d.Chauffeurs, opt => opt.Ignore());
        }
    }
}
