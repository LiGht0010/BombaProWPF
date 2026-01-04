using AutoMapper;
using BombaProMaxApi.DTOs;
using BombaProMaxApi.Models;

namespace BombaProMaxApi.Mappig
{
    public class CamionProfile : Profile
    {
        public CamionProfile()
        {
            // Entity to DTO mapping
            CreateMap<Camion, CamionDto>()
                .ForMember(d => d.FournisseurNom,
                    opt => opt.MapFrom(s => s.Fournisseur != null ? s.Fournisseur.Societe : null))
                .ForMember(d => d.CiterneNumero,
                    opt => opt.MapFrom(s => s.Citerne != null ? s.Citerne.MatriculeCiterne : null));

            // DTO to Entity mapping - ignore navigation properties to prevent creating new entities
            CreateMap<CamionDto, Camion>()
                .ForMember(d => d.Fournisseur, opt => opt.Ignore())
                .ForMember(d => d.Citerne, opt => opt.Ignore())
                .ForMember(d => d.Achats, opt => opt.Ignore());
        }
    }
}
