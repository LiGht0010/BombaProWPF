using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CiterneProfile : Profile
    {
        public CiterneProfile()
        {
            // Entity to DTO mapping
            CreateMap<Citerne, CiterneDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Nom : null));

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<CiterneDto, Citerne>()
                .ForMember(d => d.Fournisseur, opt => opt.Ignore())
                .ForMember(d => d.Camion, opt => opt.Ignore());
        }
    }
}
